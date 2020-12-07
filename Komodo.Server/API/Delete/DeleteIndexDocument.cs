using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DatabaseWrapper;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo;
using Komodo.Crawler; 
using Komodo.IndexClient;
using Komodo.Parser;
using Komodo.Postings;
using Komodo.Server.Classes;
using Index = Komodo.Index;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task DeleteIndexDocument(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.Source.IpAddress + ":" + md.Http.Request.Source.Port + " DeleteIndexDocument ";
             
            string indexName = md.Http.Request.Url.Elements[0];
            string sourceGuid = md.Http.Request.Url.Elements[1];

            if (!_Daemon.IndexExists(indexName))
            {
                _Logging.Warn(header + "index " + indexName + " does not exist");
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                return;
            }

            if (!_Daemon.SourceDocumentExists(indexName, sourceGuid))
            {
                _Logging.Warn(header + "document " + indexName + "/" + sourceGuid + " does not exist");
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown document.", null, null).ToJson(true));
                return;
            }

            _Daemon.RemoveDocument(indexName, sourceGuid);
            _Logging.Debug(header + "deleted document " + indexName + "/" + sourceGuid);
            md.Http.Response.StatusCode = 204;
            await md.Http.Response.Send();
            return;
        }
    }
}