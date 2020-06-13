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
        private static async Task PostIndices(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " PostIndices ";

            if (md.Http.Request.Data == null || md.Http.Request.ContentLength < 1)
            {
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "No request body.", null, null).ToJson(true));
                return;
            }

            byte[] reqData = Common.StreamToBytes(md.Http.Request.Data);
            Index req = Common.DeserializeJson<Index>(reqData);
            req.OwnerGUID = md.User.GUID;
            if (String.IsNullOrEmpty(req.GUID)) req.GUID = Guid.NewGuid().ToString();
            
            if (_Indices.Exists(req.Name))
            {
                _Logging.Warn(header + "index " + req.Name + " already exists");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Index already exists.", null, null).ToJson(true));
                return;
            }

            KomodoIndex ki = _Indices.Add(req);
            if (ki == null || ki == default(KomodoIndex))
            {
                _Logging.Warn(header + "unable to add index");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Unable to add index.", null, null).ToJson(true));
                return;
            }

            _Logging.Debug(header + "created index " + req.Name);
            md.Http.Response.StatusCode = 201;
            await md.Http.Response.Send();
            return;
        }
    }
}