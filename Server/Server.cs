using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatsonWebserver;
using SyslogLogging;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    /// <summary>
    /// Komodo information storage, search, and retrieval server.
    /// </summary>
    public partial class KomodoServer
    {
        private static string _Version; 
        private static Settings _Settings;
        private static LoggingModule _Logging;
        private static ConnectionManager _Conn;
        private static UserManager _User;
        private static ApiKeyManager _ApiKey;
        private static IndexManager _Index;
        private static ConsoleManager _Console;
        private static WatsonWebserver.Server _Server;

        private static void Main(string[] args)
        {
            try
            {
                #region Welcome

                _Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

                Console.WriteLine(Welcome());

                #endregion

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

                #endregion
                
                #region Initialize-Globals

                _Settings = Settings.FromFile("System.json");

                _Logging = new LoggingModule(
                    _Settings.Logging.SyslogServerIp,
                    _Settings.Logging.SyslogServerPort,
                    _Settings.Logging.ConsoleLogging,
                    (LoggingModule.Severity)_Settings.Logging.MinimumLevel,
                    false,
                    true,
                    true,
                    false,
                    false,
                    false);

                _Conn = new ConnectionManager();
                _User = new UserManager(_Logging, UserMaster.FromFile(_Settings.Files.UserMaster));
                _ApiKey = new ApiKeyManager(_Logging, ApiKey.FromFile(_Settings.Files.ApiKey), ApiKeyPermission.FromFile(_Settings.Files.ApiKeyPermission));
                _Index = new IndexManager(_Settings.Files.Indices, _Logging);

                _Server = new WatsonWebserver.Server(
                    _Settings.Server.ListenerHostname, 
                    _Settings.Server.ListenerPort, 
                    _Settings.Server.Ssl, 
                    RequestReceived);

                _Server.ContentRoutes.Add("/SearchApp/", true);
                _Server.ContentRoutes.Add("/Assets/", true);
                _Server.AccessControl.Mode = AccessControlMode.DefaultPermit;
                 
                if (_Settings.EnableConsole) _Console = new ConsoleManager(_Settings, _Index, ExitApplication); 

                #endregion

                #region Wait-for-Server-Thread

                EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, Guid.NewGuid().ToString());
                bool waitHandleSignal = false;
                do
                {
                    waitHandleSignal = waitHandle.WaitOne(1000);
                } while (!waitHandleSignal);

                _Logging.Debug("KomodoServer exiting");

                #endregion 
            }
            catch (Exception e)
            {
                _Logging.Exception("KomodoServer", "Main", e);
            }
        }

        private static string Welcome()
        {
            // thank you https://psfonttk.com/big-text-generator/

            string ret = 
                Environment.NewLine +
                Environment.NewLine +
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░" + Environment.NewLine +
                "░░░█░█░█▀▀█░█▀▄▀█░█▀▀█░█▀▀▄░█▀▀█░░░" + Environment.NewLine +
                "░░░█▀▄░█░░█░█░▀░█░█░░█░█░░█░█░░█░░░" + Environment.NewLine +
                "░░░▀░▀░▀▀▀▀░▀░░░▀░▀▀▀▀░▀▀▀░░▀▀▀▀░░░" + Environment.NewLine +
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░" + Environment.NewLine +
                Environment.NewLine;

            /*
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
                Environment.NewLine;
              */

            return ret;
        }

        private static async Task RequestReceived(HttpContext ctx)
        {
            string header = ctx.Request.SourceIp + ":" + ctx.Request.SourcePort + " ";
            DateTime startTime = DateTime.Now;

            try
            {
                #region Variables

                string apiKey = "";
                string email = "";
                string password = "";

                UserMaster currUserMaster = null;
                ApiKey currApiKey = null;
                ApiKeyPermission currApiKeyPermission = null;
                RequestMetadata md = new RequestMetadata();
                 
                #endregion

                #region Options-Handler

                if (ctx.Request.Method == HttpMethod.OPTIONS)
                { 
                    await OptionsHandler(ctx);
                    return;
                }

                #endregion

                #region Favicon-Robots-Root

                if (ctx.Request.RawUrlEntries == null || ctx.Request.RawUrlEntries.Count == 0)
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "text/html; charset=utf-8";
                    await ctx.Response.Send(RootHtml());
                    return;
                }

                if (ctx.Request.RawUrlEntries != null && ctx.Request.RawUrlEntries.Count > 0)
                {
                    if (String.Compare(ctx.Request.RawUrlEntries[0].ToLower(), "favicon.ico") == 0)
                    {
                        ctx.Response.StatusCode = 200;
                        await ctx.Response.Send();
                        return;
                    } 

                    if (String.Compare(ctx.Request.RawUrlEntries[0].ToLower(), "robots.txt") == 0)
                    {
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "text/plain";
                        await ctx.Response.Send("User-Agent: *\r\nDisallow:\r\n");
                        return;
                    }
                }

                #endregion

                #region Add-Connection

                _Conn.Add(Thread.CurrentThread.ManagedThreadId, ctx);

                #endregion

                #region Unauthenticated-API

                switch (ctx.Request.Method)
                {
                    case HttpMethod.GET: 
                        if (ctx.Request.RawUrlWithoutQuery.Equals("/loopback")) 
                        {
                            ctx.Response.StatusCode = 200;
                            await ctx.Response.Send();
                            return;
                        } 

                        if (ctx.Request.RawUrlWithoutQuery.Equals("/version"))
                        {
                            ctx.Response.StatusCode = 200;
                            ctx.Response.ContentType = "text/plain";
                            await ctx.Response.Send(_Version);
                            return;
                        } 
                        break;  
                }

                #endregion

                #region Retrieve-Authentication

                apiKey = ctx.Request.RetrieveHeaderValue(_Settings.Server.HeaderApiKey);
                email = ctx.Request.RetrieveHeaderValue(_Settings.Server.HeaderEmail);
                password = ctx.Request.RetrieveHeaderValue(_Settings.Server.HeaderPassword);
                
                #endregion

                #region Admin-API

                if (ctx.Request.RawUrlEntries != null && ctx.Request.RawUrlEntries.Count > 0)
                {
                    if (ctx.Request.RawUrlEntries[0].Equals("admin"))
                    {
                        if (String.IsNullOrEmpty(apiKey))
                        {
                            _Logging.Warn(header + "RequestReceived admin API requested but no API key specified");
                            ctx.Response.StatusCode = 401;
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.Send(new ErrorResponse(401, "No API key specified.", null).ToJson(true));
                            return;
                        }

                        if (String.Compare(_Settings.Server.AdminApiKey, apiKey) != 0)
                        {
                            _Logging.Warn(header + "RequestReceived admin API requested with invalid API key");
                            ctx.Response.StatusCode = 401;
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.Send(new ErrorResponse(401, null, null).ToJson(true));
                            return;
                        }

                        await AdminApiHandler(ctx);
                        return;
                    }
                }

                #endregion

                #region Authenticate-Request

                if (!String.IsNullOrEmpty(apiKey))
                {
                    if (!_ApiKey.VerifyApiKey(apiKey, _User, out currUserMaster, out currApiKey, out currApiKeyPermission))
                    {
                        _Logging.Warn(header + "RequestReceived unable to verify API key " + apiKey);
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(new ErrorResponse(401, null, null).ToJson(true));
                        return;
                    }
                }
                else if ((!String.IsNullOrEmpty(email)) && (!String.IsNullOrEmpty(password)))
                {
                    if (!_User.AuthenticateCredentials(email, password, out currUserMaster))
                    {
                        _Logging.Warn(header + "RequestReceived unable to verify credentials for email " + email);
                        ctx.Response.StatusCode = 401;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(new ErrorResponse(401, null, null).ToJson(true));
                        return; 
                    }

                    currApiKeyPermission = ApiKeyPermission.DefaultPermit(currUserMaster);
                }
                else
                {
                    _Logging.Warn(header + "RequestReceived user API requested but no authentication material supplied");
                    ctx.Response.StatusCode = 401;
                    ctx.Response.ContentType = "application/json";
                    await ctx.Response.Send(new ErrorResponse(401, "No authentication material.", null).ToJson(true));
                    return;
                }

                #endregion

                #region Build-and-Validate-Metadata

                md.Http = ctx;
                md.User = currUserMaster;
                md.ApiKey = currApiKey;
                md.Permission = currApiKeyPermission;

                if (ctx.Request.QuerystringEntries != null && ctx.Request.QuerystringEntries.Count > 0)
                {
                    md.Params = RequestParameters.FromDictionary(ctx.Request.QuerystringEntries);
                }
                else
                {
                    md.Params = new RequestParameters();
                }
                
                if (!String.IsNullOrEmpty(md.Params.Type))
                {
                    List<string> matchVals = new List<string> { "json", "xml", "html", "sql", "text" };
                    if (!matchVals.Contains(md.Params.Type))
                    {
                        _Logging.Warn(header + "RequestReceived invalid 'type' value found in querystring: " + md.Params.Type);
                        ctx.Response.StatusCode = 400;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(new ErrorResponse(400, "Invalid 'type' in querystring, use [json/xml/html/sql/text].", null).ToJson(true));
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
                await ctx.Response.Send(new ErrorResponse(500, "Outer exception.", null).ToJson(true));
                return;
            }
            finally
            {
                _Conn.Close(Thread.CurrentThread.ManagedThreadId);

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
                _Settings.Server.HeaderApiKey + ", " +
                _Settings.Server.HeaderEmail + ", " +
                _Settings.Server.HeaderPassword;

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
                "    <pre>";

            ret += Welcome();
            ret += "Komodo Server version " + _Version + Environment.NewLine;
            ret += "Information storage, search, and retrieval platform" + Environment.NewLine;
            ret += Environment.NewLine;
            ret += "Documentation and source code: <a href='https://github.com/jchristn/komodo' target='_blank'>https://github.com/jchristn/komodo</a>" + Environment.NewLine;
            ret +=
                "    </pre>" +
                "  </body>" +
                "</html>";
            return ret;
        }
    }
}
