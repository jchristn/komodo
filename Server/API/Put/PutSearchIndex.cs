using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Core;
using Komodo.Core.Enums;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        private static async Task PutSearchIndex(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";
             
            if (md.Http.Request.Data == null || md.Http.Request.ContentLength < 1)
            {
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "No request body.", null).ToJson(true));
                return; 
            }

            SearchQuery query = Common.DeserializeJson<SearchQuery>(Common.StreamToBytes(md.Http.Request.Data));
             
            string indexName = md.Http.Request.RawUrlEntries[0];
            Index currIndex = _Index.GetIndexByName(indexName);
            if (currIndex == null || currIndex == default(Index))
            {
                _Logging.Warn(header + "PutSearchIndex unknown index " + indexName);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null).ToJson(true));
                return;
            }
             
            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Warn(header + "PutSearchIndex unable to retrieve client for index " + indexName);
                md.Http.Response.StatusCode = 500;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true));
                return;
            }

            SearchResult result = currClient.Search(query);

            if (result.Error.Id != ErrorId.NONE)
            {
                _Logging.Warn(header + "PutSearchIndex unable to execute search in index " + indexName);
                md.Http.Response.StatusCode = 500;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(500, "Unable to search index '" + indexName + "'.", result).ToJson(true));
                return;
            }

            if (!String.IsNullOrEmpty(query.PostbackUrl))
            {
                md.Http.Response.StatusCode = 202;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(result, true));
                return;
            }
            else
            {
                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(result, md.Params.Pretty));
                return;
            }
        }
    }
}