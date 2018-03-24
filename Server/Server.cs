using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KomodoCore;
using WatsonWebserver;
using SyslogLogging;

namespace KomodoServer
{
    public partial class KomodoServer
    {
        public static Config _Config { get; set; }
        public static LoggingModule _Logging { get; set; }
        public static ConnectionManager _Conn { get; set; }
        public static UserManager _User { get; set; }
        public static ApiKeyManager _ApiKey { get; set; } 
        public static IndexManager _Index { get; set; }
        public static ConsoleManager _Console { get; set; }
        public static Server _Server { get; set; }

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(Welcome());
                
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
                
                #region Initialize-Global-Variables

                _Config = Config.FromFile("System.json");

                _Logging = new LoggingModule(
                    _Config.Logging.SyslogServerIp,
                    _Config.Logging.SyslogServerPort,
                    Common.IsTrue(_Config.Logging.ConsoleLogging),
                    LoggingModule.Severity.Debug,
                    false,
                    true,
                    true,
                    false,
                    false,
                    false);

                _Conn = new ConnectionManager();
                _User = new UserManager(_Logging, UserMaster.FromFile(_Config.Files.UserMaster));
                _ApiKey = new ApiKeyManager(_Logging, ApiKey.FromFile(_Config.Files.ApiKey), ApiKeyPermission.FromFile(_Config.Files.ApiKeyPermission));
                _Index = new IndexManager(_Config.Files.IndicesDatabase, Common.IsTrue(_Config.Debug.Database), _Logging);

                _Server = new Server(_Config.Server.ListenerHostname, _Config.Server.ListenerPort, Common.IsTrue(_Config.Server.Ssl), RequestReceived, true);
                _Server.AddContentRoute("/SearchApp/", true);
                _Server.AddContentRoute("/Assets/", true);

                #endregion
                 
                #region Console

                if (Common.IsTrue(_Config.EnableConsole))
                {
                    _Console = new ConsoleManager(_Config, ExitApplication);
                }

                #endregion

                #region Wait-for-Server-Thread

                EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, Guid.NewGuid().ToString());
                bool waitHandleSignal = false;
                do
                {
                    waitHandleSignal = waitHandle.WaitOne(1000);
                } while (!waitHandleSignal);

                _Logging.Log(LoggingModule.Severity.Debug, "KomodoServer exiting");

                #endregion 
            }
            catch (Exception e)
            {
                LoggingModule.ConsoleException("KomodoServer", "Main", e);
            }
        }

        public static string Welcome()
        {
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

            return ret;
        }

        public static void Usage()
        {
            //                 1234567890123456789012345678901234567890123456789012345678901234567890123456789");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("  C:\\> KomodoServer [-c=<configfile>]");
            Console.WriteLine("");
            Console.WriteLine("Where:");
            Console.WriteLine("  -c=<configfile>       Configuration file to load");
            Console.WriteLine("");
        }

        public static HttpResponse RequestReceived(HttpRequest req)
        {
            HttpResponse resp = new HttpResponse(req, false, 500, null, "application/json",
                new ErrorResponse(500, "Outer exception.", null).ToJson(true),
                true);

            try
            {
                #region Variables

                DateTime startTime = DateTime.Now;
                
                string apiKey = "";
                string email = "";
                string password = "";
                string version = "";

                UserMaster currUserMaster = null;
                ApiKey currApiKey = null;
                ApiKeyPermission currApiKeyPermission = null;
                RequestMetadata md = new RequestMetadata();

                if (Common.IsTrue(_Config.Logging.LogHttpRequests))
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "RequestReceived request received: " + Environment.NewLine + req.ToString());
                }

                #endregion

                #region Options-Handler

                if (req.Method.ToLower().Trim().Contains("option"))
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "RequestReceived " + Thread.CurrentThread.ManagedThreadId + ": OPTIONS request received");
                    resp = OptionsHandler(req);
                    return resp;
                }

                #endregion

                #region Favicon-Robots-Root

                if (req.RawUrlEntries != null && req.RawUrlEntries.Count > 0)
                {
                    if (String.Compare(req.RawUrlEntries[0].ToLower(), "favicon.ico") == 0)
                    {
                        resp = new HttpResponse(req, true, 200, null, null, null, true);
                        return resp;
                    }
                }

                if (req.RawUrlEntries != null && req.RawUrlEntries.Count > 0)
                {
                    if (String.Compare(req.RawUrlEntries[0].ToLower(), "robots.txt") == 0)
                    {
                        resp = new HttpResponse(req, true, 200, null, "text/plain", "User-Agent: *\r\nDisallow:\r\n", true);
                        return resp;
                    }
                }

                if (req.RawUrlEntries == null || req.RawUrlEntries.Count == 0)
                {
                    _Logging.Log(LoggingModule.Severity.Info, "RequestReceived null raw URL list detected, redirecting to documentation page");
                    resp = new HttpResponse(req, true, 200, null, "text/html", RootHtml(), true);
                    return resp;
                }

                #endregion

                #region Add-Connection

                _Conn.Add(Thread.CurrentThread.ManagedThreadId, req);

                #endregion

                #region Unauthenticated-API

                switch (req.Method.ToLower())
                {
                    case "get":
                        #region get

                        #region loopback

                        if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/loopback", false))
                        {
                            resp = new HttpResponse(req, true, 200, null, "text/plain", "Hello from Komodo!", true);
                            return resp;
                        }

                        #endregion
                        
                        #region version

                        if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/version", false))
                        {
                            resp = new HttpResponse(req, true, 200, null, "text/plain", _Config.ProductVersion, true);
                            return resp;
                        }

                        #endregion

                        break;

                    #endregion
                         
                    default:
                        break;
                }

                #endregion

                #region Retrieve-Auth-Parameters

                apiKey = req.RetrieveHeaderValue(_Config.Server.HeaderApiKey);
                email = req.RetrieveHeaderValue(_Config.Server.HeaderEmail);
                password = req.RetrieveHeaderValue(_Config.Server.HeaderPassword);
                version = req.RetrieveHeaderValue(_Config.Server.HeaderVersion);

                #endregion

                #region Admin-API

                if (req.RawUrlEntries != null && req.RawUrlEntries.Count > 0)
                {
                    if (String.Compare(req.RawUrlEntries[0], "admin") == 0)
                    {
                        if (String.IsNullOrEmpty(apiKey))
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "RequestReceived admin API requested but no API key specified");
                            resp = new HttpResponse(req, false, 401, null, "application/json",
                                new ErrorResponse(401, "No API key specified.", null).ToJson(true),
                                true);
                            return resp;
                        }

                        if (String.Compare(_Config.Server.AdminApiKey, apiKey) != 0)
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "RequestReceived admin API requested but invalid API key specified");
                            resp = new HttpResponse(req, false, 401, null, "application/json",
                                new ErrorResponse(401, null, null).ToJson(true),
                                true);
                            return resp;
                        }

                        resp = AdminApiHandler(req);
                        return resp;
                    }
                }

                #endregion

                #region Authenticate-Request

                if (!String.IsNullOrEmpty(apiKey))
                {
                    if (!_ApiKey.VerifyApiKey(apiKey, _User, out currUserMaster, out currApiKey, out currApiKeyPermission))
                    {
                        _Logging.Log(LoggingModule.Severity.Warn, "RequestReceived unable to verify API key " + apiKey);
                        resp = new HttpResponse(req, false, 401, null, "application/json",
                           new ErrorResponse(401, null, null).ToJson(true),
                           true);
                        return resp;
                    }
                }
                else if ((!String.IsNullOrEmpty(email)) && (!String.IsNullOrEmpty(password)))
                {
                    if (!_User.AuthenticateCredentials(email, password, out currUserMaster))
                    {
                        _Logging.Log(LoggingModule.Severity.Warn, "RequestReceived unable to verify credentials for email " + email);
                        resp = new HttpResponse(req, false, 401, null, "application/json",
                            new ErrorResponse(401, null, null).ToJson(true),
                            true);
                        return resp;
                    }

                    currApiKeyPermission = ApiKeyPermission.DefaultPermit(currUserMaster);
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "RequestReceived user API requested but no authentication material supplied");
                    resp = new HttpResponse(req, false, 401, null, "application/json",
                        new ErrorResponse(401, "No authentication material.", null).ToJson(true),
                        true);
                    return resp;
                }

                #endregion

                #region Build-Metadata

                md.CurrRequest = req;
                md.CurrUser = currUserMaster;
                md.CurrApiKey = currApiKey;
                md.CurrPerm = currApiKeyPermission;

                #endregion
                 
                #region Call-User-API

                resp = UserApiHandler(md);
                return resp;

                #endregion
            }
            catch (Exception e)
            {
                _Logging.LogException("RequestReceived", "Outer exception", e);
                return resp;
            }
            finally
            {
                _Conn.Close(Thread.CurrentThread.ManagedThreadId);

                if (Common.IsTrue(_Config.Logging.LogHttpRequests))
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "RequestReceived sending response: " + Environment.NewLine + resp.ToString());
                } 
            }
        }

        public static HttpResponse OptionsHandler(HttpRequest req)
        {
            _Logging.Log(LoggingModule.Severity.Debug, "OptionsHandler " + Thread.CurrentThread.ManagedThreadId + ": processing options request");

            Dictionary<string, string> responseHeaders = new Dictionary<string, string>();

            string[] requestedHeaders = null;
            if (req.Headers != null)
            {
                foreach (KeyValuePair<string, string> curr in req.Headers)
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
                _Config.Server.HeaderApiKey + ", " +
                _Config.Server.HeaderEmail + ", " +
                _Config.Server.HeaderPassword + ", " +
                _Config.Server.HeaderVersion;

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

            if (Common.IsTrue(_Config.Server.Ssl))
            {
                responseHeaders.Add("Host", "https://" + _Config.Server.ListenerHostname + ":" + _Config.Server.ListenerPort);
            }
            else
            {
                responseHeaders.Add("Host", "http://" + _Config.Server.ListenerHostname + ":" + _Config.Server.ListenerPort);
            }

            _Logging.Log(LoggingModule.Severity.Debug, "OptionsHandler " + Thread.CurrentThread.ManagedThreadId + ": exiting successfully from OptionsHandler");
            return new HttpResponse(req, true, 200, responseHeaders, null, null, true);
        }

        public static bool ExitApplication()
        {
            _Logging.Log(LoggingModule.Severity.Info, "KomodoServer exiting due to console request");
            Environment.Exit(0);
            return true;
        }

        public static string RootHtml()
        {
            string ret =
                "<html>" +
                "  <head>" +
                "    <title>Komodo Server</title>" +
                "  </head>" +
                "  <body>" +
                "    <pre>";

            ret += Welcome();
            ret += "Komodo Server version " + _Config.ProductVersion + Environment.NewLine;

            ret +=
                "    </pre>" +
                "  </body>" +
                "</html>";
            return ret;
        }
    }
}
