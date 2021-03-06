﻿using System;
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
using Komodo.IndexClient;
using Komodo.Server.Classes;
using Index = Komodo.Index;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task PutEnumerateIndex(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.Source.IpAddress + ":" + md.Http.Request.Source.Port + " PutEnumerateIndex ";

            if (md.Http.Request.Data == null || md.Http.Request.ContentLength < 1)
            {
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "No request body.", null, null).ToJson(true));
                return;
            }

            string indexName = md.Http.Request.Url.Elements[0];
            if (!_Daemon.IndexExists(indexName))
            {
                _Logging.Warn(header + "index " + indexName + " does not exist");
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                return;
            } 

            EnumerationQuery query = Common.DeserializeJson<EnumerationQuery>(Common.StreamToBytes(md.Http.Request.Data));
            if (query.Filters == null) query.Filters = new List<SearchFilter>();

            EnumerationResult result = _Daemon.Enumerate(indexName, query);

            if (!result.Success)
            {
                _Logging.Warn(header + "failed to execute enumeration in index " + indexName);
                md.Http.Response.StatusCode = 500;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(500, "Unable to enumerate index '" + indexName + "'.", null, result).ToJson(true));
                return;
            }

            md.Http.Response.StatusCode = 200;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(Common.SerializeJson(result, md.Params.Pretty));
            return;
        }
    }
}