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
        static HttpResponse PutSearchIndex(RequestMetadata md)
        {
            HttpResponse resp;

            if (md.Http.Data == null || md.Http.Data.Length < 1)
            {
                resp = new HttpResponse(md.Http, 400, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(400, "No request body.", null).ToJson(true)));
                return resp;
            }

            SearchQuery query = Common.DeserializeJson<SearchQuery>(md.Http.Data);
             
            string indexName = md.Http.RawUrlEntries[0];
            Index currIndex = _Index.GetIndexByName(indexName);
            if (currIndex == null || currIndex == default(Index))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutSearchIndex unknown index " + indexName);
                return new HttpResponse(md.Http, 404, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(404, "Unknown index '" + indexName + "'.", null).ToJson(true)));
            }
             
            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutSearchIndex unable to retrieve client for index " + indexName);
                return new HttpResponse(md.Http, 500, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true)));
            }

            SearchResult result = null;
            ErrorCode error = null;
            if (!currClient.Search(query, out result, out error))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PutSearchIndex unable to execute search in index " + indexName);
                return new HttpResponse(md.Http, 500, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(500, "Unable to search index '" + indexName + "'.", error).ToJson(true)));
            }

            if (!String.IsNullOrEmpty(query.PostbackUrl))
            {
                return new HttpResponse(md.Http, 202, null, "application/json",
                    Encoding.UTF8.GetBytes(Common.SerializeJson(result, true)));
            }
            else
            {
                return new HttpResponse(md.Http, 200, null, "application/json",
                    Encoding.UTF8.GetBytes(Common.SerializeJson(result, true)));
            }
        }
    }
}