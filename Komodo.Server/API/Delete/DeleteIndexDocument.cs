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
             
            string name = md.Http.Request.RawUrlEntries[0];
            string docId = md.Http.Request.RawUrlEntries[1];

            KomodoIndex idx = _Indices.Get(name);
            if (idx == null)
            {
                _Logging.Warn(header + "unable to retrieve index " + name);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                return;
            }

            SourceDocument doc = idx.GetSourceDocumentMetadata(docId);
            if (doc == null)
            {
                _Logging.Warn(header + "unable to find document " + name + "/" + docId);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown document.", null, null).ToJson(true));
                return;
            }

            idx.Remove(docId);
            _Logging.Debug(header + "deleted document ID " + docId + " from index " + name);
            md.Http.Response.StatusCode = 204;
            await md.Http.Response.Send();
            return;
        }
    }
}