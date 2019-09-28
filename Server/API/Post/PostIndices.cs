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
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        private static async Task PostIndices(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";
             
            if (md.Http.Request.Data == null || md.Http.Request.ContentLength < 1)
            {
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "No request body.", null).ToJson(true));
                return;
            }

            byte[] reqData = Common.StreamToBytes(md.Http.Request.Data);
            Index req = Common.DeserializeJson<Index>(reqData);

            if (_Index.IndexExists(req.IndexName))
            {
                _Logging.Warn(header + "PostIndices index " + req.IndexName + " already exists");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Index already exists.", null).ToJson(true));
                return; 
            }
            
            if (!_Index.AddIndex(req))
            {
                _Logging.Warn(header + "PostIndices unable to add index");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Unable to create index.", null).ToJson(true));
                return;
            }

            _Logging.Debug(header + "PostIndices created index " + req.IndexName);
            md.Http.Response.StatusCode = 201;
            await md.Http.Response.Send();
            return; 
        }
    }
}