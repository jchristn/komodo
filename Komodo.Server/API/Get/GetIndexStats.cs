using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo;
using Komodo.IndexClient;
using Komodo.IndexManager;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task GetIndexStats(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.Source.IpAddress + ":" + md.Http.Request.Source.Port + " GetIndexStats ";
             
            string indexName = md.Http.Request.Url.Elements[0];
            IndicesStats stats = _Daemon.GetIndexStats(indexName);
            if (stats == null)
            {
                if (String.IsNullOrEmpty(indexName))
                {
                    _Logging.Warn(header + "index " + indexName + " does not exist");
                    md.Http.Response.StatusCode = 404;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                }
                else
                {
                    _Logging.Warn(header + "no indices found");
                    md.Http.Response.StatusCode = 404;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(404, "No indices found.", null, null).ToJson(true));
                }

                return;
            } 

            md.Http.Response.StatusCode = 200;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(Common.SerializeJson(stats, md.Params.Pretty));
            return; 
        }
    }
}