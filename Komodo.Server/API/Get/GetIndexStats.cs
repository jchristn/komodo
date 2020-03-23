using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Classes;
using Komodo.IndexClient;
using Komodo.IndexManager;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task GetIndexStats(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " GetIndexStats ";
             
            string name = md.Http.Request.RawUrlEntries[0];
            IndicesStats stats = _Indices.Stats(name);
            if (stats == null)
            {
                _Logging.Warn(header + "unable to find index " + name);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                return;
            } 

            md.Http.Response.StatusCode = 200;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(Common.SerializeJson(stats, md.Params.Pretty));
            return; 
        }
    }
}