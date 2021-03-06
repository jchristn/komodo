﻿using System;
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
using Komodo.Postings;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task GetIndexDocument(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.Source.IpAddress + ":" + md.Http.Request.Source.Port + " GetIndexDocument ";
             
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

            if (!md.Params.Parsed)
            {
                DocumentData content = await _Daemon.GetSourceDocumentContent(indexName, sourceGuid);
                if (content == null)
                {
                    _Logging.Warn(header + "document " + indexName + "/" + sourceGuid + " does not exist");
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
                SourceDocument source = _Daemon.GetSourceDocumentMetadata(indexName, sourceGuid);
                ParsedDocument parsed = _Daemon.GetParsedDocumentMetadata(indexName, sourceGuid); 
                ParseResult parseResult = _Daemon.GetDocumentParseResult(indexName, sourceGuid);
                PostingsResult postingsResult = _Daemon.GetDocumentPostings(indexName, sourceGuid);

                DocumentMetadata ret = new DocumentMetadata(source, parsed, parseResult, postingsResult);
                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(ret, md.Params.Pretty));
                return;
            } 
        }
    }
}