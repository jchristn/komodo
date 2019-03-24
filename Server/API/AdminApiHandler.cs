using System;
using System.Net;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using KomodoCore;

namespace KomodoServer
{
    public partial class KomodoServer
    {
        public static HttpResponse AdminApiHandler(HttpRequest req)
        {
            #region Enumerate

            _Logging.Log(LoggingModule.Severity.Debug,
                "AdminApiHandler admin API requested by " +
                req.SourceIp + ":" + req.SourcePort + " " +
                req.Method + " " + req.RawUrlWithoutQuery);

            #endregion
            
            #region Process-Request

            switch (req.Method)
            {
                case HttpMethod.GET: 
                    if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/admin/connections", false))
                    {
                        return new HttpResponse(req, true, 200, null, "application/json", Common.SerializeJson(_Conn.GetActiveConnections(), true), true);
                    }

                    if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/admin/disks", false))
                    {
                        return new HttpResponse(req, true, 200, null, "application/json", Common.SerializeJson(DiskInfo.GetAllDisks(), true), true);
                    }
                    
                    break; 

                case HttpMethod.PUT: 
                    break; 

                case HttpMethod.POST: 
                    break; 

                case HttpMethod.DELETE: 
                    break; 

                case HttpMethod.HEAD: 
                    break; 

                default:
                    _Logging.Log(LoggingModule.Severity.Warn, "AdminApiHandler unknown http method: " + req.Method);
                    return new HttpResponse(req, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unsupported HTTP method.", null).ToJson(true),
                        true);
            }

            _Logging.Log(LoggingModule.Severity.Warn, "AdminApiHandler unknown endpoint URL: " + req.RawUrlWithoutQuery);
            return new HttpResponse(req, false, 400, null, "application/json",
                new ErrorResponse(400, "Unknown endpoint.", null).ToJson(true), true);

            #endregion
        }
    }
}