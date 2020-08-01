using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using Watson.ORM;
using Watson.ORM.Core;
using WatsonWebserver;
using Webserver = WatsonWebserver.Server;

using Komodo.Classes;
using Komodo.Daemon;
using Komodo.IndexManager;
using Komodo.Server.Classes;
using Common = Komodo.Classes.Common;
using Index = Komodo.Classes.Index;

namespace Komodo.Server
{
    public partial class Program
    {
        private static string _Version;
        private static Settings _Settings;
        private static LoggingModule _Logging;
        private static WatsonORM _ORM;
        private static AuthManager _Auth;
        private static ConnManager _Conn;
        private static DaemonSettings _DaemonSettings;
        private static KomodoDaemon _Daemon; 
        private static ConsoleManager _Console;
        private static Webserver _Webserver;

        public static void Main(string[] args)
        {
            string header = "[Komodo.Server] ";

            try
            { 
                _Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();  

                #region Initial-Setup

                bool initialSetup = false;
                if (args != null && args.Length >= 1)
                {
                    if (String.Compare(args[0], "setup") == 0) initialSetup = true;
                }

                if (!Common.FileExists("System.json")) initialSetup = true;
                if (initialSetup)
                {
                    Setup setup = new Setup();
                }

                _Settings = Settings.FromFile("System.json");

                #endregion
                 
                Welcome(initialSetup);
                InitializeGlobals();
                 
                EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                bool waitHandleSignal = false;
                do
                {
                    waitHandleSignal = waitHandle.WaitOne(1000);
                } while (!waitHandleSignal);

                _Logging.Debug(header + "exiting"); 
            }
            catch (Exception e)
            {
                Console.WriteLine(Common.SerializeJson(e, true));
            }
        }

        private static void Welcome(bool skipLogo)
        {
            ConsoleColor prior = Console.ForegroundColor; 
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (!skipLogo) Console.WriteLine(Logo());
            Console.WriteLine("Komodo | Information search, storage, and retrieval | v" + _Version);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("");

            if (_Settings.Server.ListenerHostname.Equals("localhost") || _Settings.Server.ListenerHostname.Equals("127.0.0.1"))
            {
                //                          1         2         3         4         5         6         7         8
                //                 12345678901234567890123456789012345678901234567890123456789012345678901234567890
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WARNING: Komodo started on '" + _Settings.Server.ListenerHostname + "'");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Komodo can only service requests from the local machine.  If you wish to serve");
                Console.WriteLine("external requests, edit the System.json file and specify a DNS-resolvable");
                Console.WriteLine("hostname in the Server.ListenerHostname field.");
                Console.WriteLine("");
            }

            List<string> adminListeners = new List<string> { "*", "+", "0.0.0.0" };

            if (adminListeners.Contains(_Settings.Server.ListenerHostname))
            {
                //                          1         2         3         4         5         6         7         8
                //                 12345678901234567890123456789012345678901234567890123456789012345678901234567890
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("NOTICE: Komodo listening on a wildcard hostname: '" + _Settings.Server.ListenerHostname + "'");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("Komodo must be run with administrative privileges, otherwise it will not be");
                Console.WriteLine("able to respond to incoming requests.");
                Console.WriteLine("");
            }

            Console.ForegroundColor = prior;
        }

        private static void InitializeGlobals()
        {
            ConsoleColor prior = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.Write("| Initializing logging            : ");
            _Logging = new LoggingModule(
                _Settings.Logging.SyslogServerIp,
                _Settings.Logging.SyslogServerPort,
                _Settings.Logging.ConsoleLogging,
                (Severity)_Settings.Logging.MinimumLevel,
                false,
                true,
                true,
                false,
                false,
                false);

            if (_Settings.Logging.FileLogging && !String.IsNullOrEmpty(_Settings.Logging.Filename))
            {
                if (String.IsNullOrEmpty(_Settings.Logging.FileDirectory)) _Settings.Logging.FileDirectory = "./";
                while (_Settings.Logging.FileDirectory.Contains("\\")) _Settings.Logging.FileDirectory.Replace("\\", "/");
                if (!Directory.Exists(_Settings.Logging.FileDirectory)) Directory.CreateDirectory(_Settings.Logging.FileDirectory);

                _Logging.FileLogging = FileLoggingMode.FileWithDate;
                _Logging.LogFilename = _Settings.Logging.FileDirectory + _Settings.Logging.Filename;
            }
            Console.WriteLine("[success]");

            Console.Write("| Initializing Komodo daemon      : ");
            _DaemonSettings = _Settings.ToDaemonSettings();
            _Daemon = new KomodoDaemon(_DaemonSettings);
            Console.WriteLine("[success]");

            Console.Write("| Initializing database           : ");
            _ORM = new WatsonORM(_Settings.Database.ToDatabaseSettings());
            _ORM.InitializeDatabase();
            _ORM.InitializeTable(typeof(ApiKey));
            _ORM.InitializeTable(typeof(Index));
            _ORM.InitializeTable(typeof(Metadata));
            _ORM.InitializeTable(typeof(MetadataDocument));
            _ORM.InitializeTable(typeof(Node));
            _ORM.InitializeTable(typeof(ParsedDocument));
            _ORM.InitializeTable(typeof(Permission));
            _ORM.InitializeTable(typeof(SourceDocument));
            _ORM.InitializeTable(typeof(TermDoc));
            _ORM.InitializeTable(typeof(TermGuid));
            _ORM.InitializeTable(typeof(User));
            Console.WriteLine("[success]");

            Console.Write("| Initializing authentication     : ");
            _Auth = new AuthManager(_ORM);
            Console.WriteLine("[success]");

            Console.Write("| Initializing connection manager : ");
            _Conn = new ConnManager();
            Console.WriteLine("[success]");

            Console.Write("| Initializing webserver          : ");
            _Webserver = new WatsonWebserver.Server(
                _Settings.Server.ListenerHostname,
                _Settings.Server.ListenerPort,
                _Settings.Server.Ssl,
                RequestReceived);

            _Webserver.ContentRoutes.Add("/Assets/", true);
            _Webserver.AccessControl.Mode = AccessControlMode.DefaultPermit;
            Console.WriteLine("[success]");

            if (_Settings.Server.Ssl)
            {
                Console.WriteLine("| https://" + _Settings.Server.ListenerHostname + ":" + _Settings.Server.ListenerPort);
            }
            else
            { 
                Console.WriteLine("| http://" + _Settings.Server.ListenerHostname + ":" + _Settings.Server.ListenerPort); 
            }

            if (_Settings.EnableConsole) _Console = new ConsoleManager(_Settings, ExitApplication);

            Console.ForegroundColor = prior;
            Console.WriteLine("");
        }

        private static string Logo()
        {
            // thank you https://psfonttk.com/big-text-generator/

            string ret =
                Environment.NewLine +
                Environment.NewLine +
                "oooo                                                    .o8            " + Environment.NewLine +
                "`888                                                    888            " + Environment.NewLine +
                " 888  oooo   .ooooo.  ooo. .oo.  .oo.    .ooooo.   .oooo888   .ooooo.  " + Environment.NewLine +
                " 888 .8P'   d88' `88b `888P'Y88bP'Y88b  d88' `88b d88' `888  d88' `88b " + Environment.NewLine +
                " 888888.    888   888  888   888   888  888   888 888   888  888   888 " + Environment.NewLine +
                " 888 `88b.  888   888  888   888   888  888   888 888   888  888   888 " + Environment.NewLine +
                "o888o o888o `Y8bod8P' o888o o888o o888o `Y8bod8P' `Y8bod88P  `Y8bod8P' " + Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine;
            return ret;
        }

        private static async Task RequestReceived(HttpContext ctx)
        {
            string header = "[Komodo.Server] " + ctx.Request.SourceIp + ":" + ctx.Request.SourcePort + " RequestReceived ";
            DateTime startTime = DateTime.Now;

            try
            {
                #region Base-Handlers

                _Conn.Add(Thread.CurrentThread.ManagedThreadId, ctx);

                if (ctx.Request.Method == HttpMethod.OPTIONS)
                {
                    await OptionsHandler(ctx);
                    return;
                }

                if (ctx.Request.RawUrlEntries == null || ctx.Request.RawUrlEntries.Count == 0)
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "text/html; charset=utf-8";
                    await ctx.Response.Send(RootHtml());
                    return;
                }

                if (ctx.Request.RawUrlEntries != null && ctx.Request.RawUrlEntries.Count > 0)
                {
                    if (ctx.Request.RawUrlEntries.Count == 1)
                    {
                        if (String.Compare(ctx.Request.RawUrlEntries[0].ToLower(), "favicon.ico") == 0)
                        {
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.Send(Common.ReadBinaryFile("Assets/favicon.ico"));
                            return;
                        }

                        if (String.Compare(ctx.Request.RawUrlEntries[0].ToLower(), "robots.txt") == 0)
                        {
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send("User-Agent: *\r\nDisallow:\r\n");
                            return;
                        }

                        if (String.Compare(ctx.Request.RawUrlEntries[0].ToLower(), "loopback") == 0)
                        {
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.Send();
                            return;
                        }

                        if (String.Compare(ctx.Request.RawUrlEntries[0].ToLower(), "version") == 0)
                        {
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send(_Version);
                            return;
                        }
                    }
                }

                #endregion
                 
                #region Admin-API

                string apiKey = ctx.Request.RetrieveHeaderValue(_Settings.Server.HeaderApiKey);

                if (ctx.Request.RawUrlEntries != null 
                    && ctx.Request.RawUrlEntries.Count > 0
                    && ctx.Request.RawUrlEntries[0].Equals("admin"))
                {
                    if (String.IsNullOrEmpty(apiKey))
                    {
                        _Logging.Warn(header + "admin API requested but no API key specified");
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(new ErrorResponse(401, "No API key specified.", null, null).ToJson(true));
                        return;
                    }

                    if (String.Compare(_Settings.Server.AdminApiKey, apiKey) != 0)
                    {
                        _Logging.Warn(header + "admin API requested with invalid API key");
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(new ErrorResponse(401, "Authentication failed.", null, null).ToJson(true));
                        return;
                    }
                         
                    await AdminApiHandler(ctx);
                    return;
                }

                #endregion

                #region Determine-Request-Type

                PermissionType permType = PermissionType.Unknown;

                if (ctx.Request.RawUrlEntries.Count == 1)
                {
                    if (ctx.Request.Method == HttpMethod.GET && ctx.Request.RawUrlEntries[0].Equals("indices"))
                    {
                        // GET /indices, i.e. search
                        permType = PermissionType.Search;
                    }
                    else if (ctx.Request.Method == HttpMethod.GET)
                    {
                        // GET /[index], i.e. search
                        permType = PermissionType.Search;
                    }
                    else if (ctx.Request.Method == HttpMethod.PUT)
                    {
                        // PUT /[index], i.e. search
                        // PUT /[index]?enumerate, i.e. search
                        permType = PermissionType.Search;
                    }
                    else if (ctx.Request.Method == HttpMethod.POST)
                    {
                        if (ctx.Request.RawUrlEntries[0].Equals("_parse"))
                        {
                            // POST /_parse, i.e. parse document (without storing)
                            permType = PermissionType.CreateDocument;
                        }
                        else if (ctx.Request.RawUrlEntries[0].Equals("_postings"))
                        {
                            // POST /_postings, i.e. create postings (without storing)
                            permType = PermissionType.CreateDocument;
                        }
                        else if (ctx.Request.RawUrlEntries[0].Equals("indices"))
                        {
                            // POST /indices, i.e. create index
                            permType = PermissionType.CreateIndex;
                        }
                        else
                        {
                            // POST /[index], i.e. create document
                            permType = PermissionType.CreateDocument;
                        }
                    }
                    else if (ctx.Request.Method == HttpMethod.DELETE)
                    {
                        // DELETE /[index], i.e. delete index
                        permType = PermissionType.DeleteIndex;
                    }
                }
                else if (ctx.Request.RawUrlEntries.Count == 2)
                {
                    if (ctx.Request.Method == HttpMethod.GET)
                    {
                        if (ctx.Request.RawUrlEntries[1].Equals("stats"))
                        {
                            // GET /[index]/stats, i.e. search
                            permType = PermissionType.Search;
                        }
                        else
                        {
                            // GET /[index]/[documentID], i.e. search
                            permType = PermissionType.Search;
                        }
                    }
                    else if (ctx.Request.Method == HttpMethod.POST)
                    {
                        // POST /[index]/[document], i.e. create document with specific GUID
                        permType = PermissionType.CreateDocument;
                    }
                    else if (ctx.Request.Method == HttpMethod.DELETE)
                    {
                        // DELETE /[index]/[document], i.e. delete document
                        permType = PermissionType.DeleteDocument;
                    }
                }
                 
                if (permType == PermissionType.Unknown)
                {
                    _Logging.Warn(header + "unable to verify request type");
                    ctx.Response.StatusCode = 400;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.Send(new ErrorResponse(400, "Unable to discern request type.", null, null).ToJson(true));
                    return;
                }

                #endregion

                #region Authenticate-Request-and-Build-Metadata

                User user = null;
                ApiKey key = null;
                Permission perm = null;

                if (!String.IsNullOrEmpty(apiKey))
                {
                    if (!_Auth.AuthenticateAndAuthorize(apiKey, permType, out user, out key, out perm))
                    {
                        _Logging.Warn(header + "unable to authenticate and authorize request");
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(new ErrorResponse(401, "Unable to authenticate or authorize request.", null, null).ToJson(true));
                        return;
                    } 
                } 
                else
                {
                    _Logging.Warn(header + "authenticated API requested but no authentication material supplied");
                    ctx.Response.StatusCode = 401;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.Send(new ErrorResponse(401, "No authentication material.", null, null).ToJson(true));
                    return;
                }
                 
                RequestMetadata md = new RequestMetadata(ctx, user, key, perm);
                 
                if (!String.IsNullOrEmpty(md.Params.Type))
                {
                    List<string> matchVals = new List<string> { "json", "xml", "html", "sql", "text", "unknown" };
                    if (!matchVals.Contains(md.Params.Type))
                    {
                        _Logging.Warn(header + "invalid 'type' value found in querystring: " + md.Params.Type);
                        ctx.Response.StatusCode = 400;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(new ErrorResponse(400, "Invalid 'type' in querystring, use [json/xml/html/sql/text].", null, null).ToJson(true));
                        return;
                    }
                }

                #endregion

                #region Call-User-API

                await UserApiHandler(md);
                return;

                #endregion
            }
            catch (Exception e)
            {
                _Logging.Exception("RequestReceived", "Outer exception", e);
                ctx.Response.StatusCode = 500;
                ctx.Response.ContentType = "application/json";
                await ctx.Response.Send(new ErrorResponse(500, "Outer exception.", e, null).ToJson(true));
                return;
            }
            finally
            {
                _Conn.Remove(Thread.CurrentThread.ManagedThreadId); 
                _Logging.Debug(header + ctx.Request.Method + " " + ctx.Request.RawUrlWithoutQuery + " " + ctx.Response.StatusCode + " [" + Common.TotalMsFrom(startTime) + "ms]");
            }
        }

        private static async Task OptionsHandler(HttpContext ctx)
        {
            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

            string[] requestedHeaders = null;
            if (ctx.Request.Headers != null)
            {
                foreach (KeyValuePair<string, string> curr in ctx.Request.Headers)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;
                    if (String.IsNullOrEmpty(curr.Value)) continue;
                    if (String.Compare(curr.Key.ToLower(), "access-control-request-headers") == 0)
                    {
                        requestedHeaders = curr.Value.Split(',');
                        break;
                    }
                }
            }

            string headers =
                _Settings.Server.HeaderApiKey;

            if (requestedHeaders != null)
            {
                foreach (string curr in requestedHeaders)
                {
                    headers += ", " + curr;
                }
            }

            responseHeaders.Add("Access-Control-Allow-Methods", "OPTIONS, HEAD, GET, PUT, POST, DELETE");
            responseHeaders.Add("Access-Control-Allow-Headers", "*, Content-Type, X-Requested-With, " + headers);
            responseHeaders.Add("Access-Control-Expose-Headers", "Content-Type, X-Requested-With, " + headers);
            responseHeaders.Add("Access-Control-Allow-Origin", "*");
            responseHeaders.Add("Accept", "*/*");
            responseHeaders.Add("Accept-Language", "en-US, en");
            responseHeaders.Add("Accept-Charset", "ISO-8859-1, utf-8");
            responseHeaders.Add("Connection", "keep-alive");

            if (_Settings.Server.Ssl)
            {
                responseHeaders.Add("Host", "https://" + _Settings.Server.ListenerHostname + ":" + _Settings.Server.ListenerPort);
            }
            else
            {
                responseHeaders.Add("Host", "http://" + _Settings.Server.ListenerHostname + ":" + _Settings.Server.ListenerPort);
            }

            ctx.Response.StatusCode = 200;
            ctx.Response.Headers = responseHeaders;
            await ctx.Response.Send();
            return;
        }

        private static bool ExitApplication()
        {
            _Logging.Info("KomodoServer exiting due to console request");
            Environment.Exit(0);
            return true;
        }

        private static string RootHtml()
        {
            string ret =
                "<html>" +
                "  <head>" +
                "    <title>Komodo Server</title>" +
                "  </head>" +
                "  <body>" +
                "    <div>" +
                "      <img src='Assets/favicon.ico' height='128' width='128'/>" +
                "    </div>" +
                "    <div>" +
                "      <pre>";

            ret += Logo();
            ret += "Komodo Server version " + _Version + Environment.NewLine;
            ret += "Information storage, search, and retrieval platform" + Environment.NewLine;
            ret += Environment.NewLine;
            ret += "Documentation and source code: <a href='https://github.com/jchristn/komodo' target='_blank'>https://github.com/jchristn/komodo</a>" + Environment.NewLine;
            ret +=
                "      </pre>" +
                "    </div>" +
                "  </body>" +
                "</html>";
            return ret;
        }
    }
}
