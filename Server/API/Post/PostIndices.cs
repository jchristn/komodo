using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using KomodoCore;
using RestWrapper;

namespace KomodoServer
{
    public partial class KomodoServer
    {
        static HttpResponse PostIndices(RequestMetadata md)
        {
            HttpResponse resp;

            if (md.CurrRequest.Data == null || md.CurrRequest.Data.Length < 1)
            {
                resp = new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "No request body.", null).ToJson(true), true);
                return resp;
            }

            Index req = Common.DeserializeJson<Index>(md.CurrRequest.Data);

            if (_Index.IndexExists(req.IndexName))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndices index " + req.IndexName + " already exists");
                resp = new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "Index already exists.", null).ToJson(true), true);
                return resp;
            }

            _Index.AddIndex(req.IndexName, req.RootDirectory, req.Options);
            _Logging.Log(LoggingModule.Severity.Debug, "PostIndices created index " + req.IndexName);
            resp = new HttpResponse(md.CurrRequest, true, 201, null, "application/json", null, true);
            return resp;
        }
    }
}