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
using Komodo.Postings;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task GetIndexDocument(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " GetIndexDocument ";
             
            string name = md.Http.Request.RawUrlEntries[0];
            string docId = md.Http.Request.RawUrlEntries[1];

            KomodoIndex idx = _Indices.Get(name);
            if (idx == null)
            {
                _Logging.Warn(header + "unable to find index " + name);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                return;
            }
             
            if (!md.Params.Parsed)
            {
                DocumentData content = await idx.GetSourceDocumentContent(docId);
                if (content == null)
                {
                    _Logging.Warn(header + "unable to find document " + name + "/" + docId);
                    md.Http.Response.StatusCode = 404;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(404, "Unknown document.", null, null).ToJson(true));
                    return;
                }
                else
                {
                    md.Http.Response.StatusCode = 200;
                    md.Http.Response.ContentType = content.ContentType;
                    await md.Http.Response.Send(content.ContentLength, content.DataStream);
                    return;
                } 
            }
            else
            {
                SourceDocument source = idx.GetSourceDocumentMetadata(docId);
                ParsedDocument parsed = idx.GetParsedDocument(docId);
                object parseResult = idx.GetParseResult(docId);
                PostingsResult postingsResult = idx.GetPostings(docId);

                DocumentMetadata ret = new DocumentMetadata(source, parsed, parseResult, postingsResult);
                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(ret, md.Params.Pretty));
                return;
            } 
        }
    }
}