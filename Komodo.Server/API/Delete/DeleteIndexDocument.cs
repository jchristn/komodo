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
using Komodo.Classes;
using Komodo.Crawler; 
using Komodo.IndexClient;
using Komodo.Parser;
using Komodo.Postings;
using Komodo.Server.Classes;
using Index = Komodo.Classes.Index;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task DeleteIndexDocument(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " DeleteIndexDocument ";
             
            string indexName = md.Http.Request.RawUrlEntries[0];
            string sourceGuid = md.Http.Request.RawUrlEntries[1];

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