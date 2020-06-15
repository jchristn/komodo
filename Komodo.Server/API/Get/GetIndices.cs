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
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task GetIndices(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " GetIndices ";
            List<string> indexNames = _Daemon.GetIndices();
            md.Http.Response.StatusCode = 200;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(Common.SerializeJson(indexNames, md.Params.Pretty));
            return; 
        }
    }
}