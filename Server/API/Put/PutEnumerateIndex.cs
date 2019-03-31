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
        static HttpResponse PutEnumerateIndex(RequestMetadata md)
        {
            HttpResponse resp;

            if (md.CurrRequest.Data == null || md.CurrRequest.Data.Length < 1)
            {
                resp = new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "No request body.", null).ToJson(true), true);
                return resp;
            }

            EnumerationQuery query = Common.DeserializeJson<EnumerationQuery>(md.CurrRequest.Data);
             
            string indexName = md.CurrRequest.RawUrlEntries[0];
            Index currIndex = _Index.GetIndexByName(indexName);
            if (currIndex == null || currIndex == default(Index))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutEnumerateIndex unknown index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index '" + indexName + "'.", null).ToJson(true), true);
            }
             
            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutEnumerateIndex unable to retrieve client for index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true), true);
            }

            EnumerationResult result = null;
            ErrorCode error = null;
            if (!currClient.Enumerate(query, out result, out error))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutEnumerateIndex unable to execute enumeration in index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to search index '" + indexName + "'.", error).ToJson(true), true);
            }

            if (!String.IsNullOrEmpty(query.PostbackUrl))
            {
                return new HttpResponse(md.CurrRequest, true, 202, null, "application/json", Common.SerializeJson(result, true), true);
            }
            else
            {
                return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(result, true), true);
            }
        }
    }
}