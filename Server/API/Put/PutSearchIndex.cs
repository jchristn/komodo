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
        static HttpResponse PutSearchIndex(RequestMetadata md)
        {
            HttpResponse resp;

            if (md.CurrRequest.Data == null || md.CurrRequest.Data.Length < 1)
            {
                resp = new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "No request body.", null).ToJson(true), true);
                return resp;
            }

            SearchQuery query = Common.DeserializeJson<SearchQuery>(md.CurrRequest.Data);
             
            string indexName = md.CurrRequest.RawUrlEntries[0];
            Index currIndex = _Index.GetIndexByName(indexName);
            if (currIndex == null || currIndex == default(Index))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutSearchIndex unknown index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index '" + indexName + "'.", null).ToJson(true), true);
            }
             
            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutSearchIndex unable to retrieve client for index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true), true);
            }

            SearchResult result = null;
            ErrorCode error = null;
            if (!currClient.Search(query, out result, out error))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutSearchIndex unable to execute search in index " + indexName);
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