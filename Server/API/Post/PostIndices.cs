using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        static HttpResponse PostIndices(RequestMetadata md)
        {
            HttpResponse resp;

            if (md.Http.Data == null || md.Http.Data.Length < 1)
            {
                resp = new HttpResponse(md.Http, 400, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(400, "No request body.", null).ToJson(true)));
                return resp;
            }

            Index req = Common.DeserializeJson<Index>(md.Http.Data);

            if (_Index.IndexExists(req.IndexName))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndices index " + req.IndexName + " already exists");
                resp = new HttpResponse(md.Http, 400, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(400, "Index already exists.", null).ToJson(true)));
                return resp;
            }
            
            if (!_Index.AddIndex(req))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndices unable to add index");
                resp = new HttpResponse(md.Http, 400, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to create index.", null).ToJson(true)));
            }

            _Logging.Log(LoggingModule.Severity.Debug, "PostIndices created index " + req.IndexName);
            resp = new HttpResponse(md.Http, 201, null, "application/json", null);
            return resp;
        }
    }
}