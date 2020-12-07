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
        private static async Task PostIndices(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.Source.IpAddress + ":" + md.Http.Request.Source.Port + " PostIndices ";

            if (md.Http.Request.Data == null || md.Http.Request.ContentLength < 1)
            {
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "No request body.", null, null).ToJson(true));
                return;
            }

            byte[] reqData = Common.StreamToBytes(md.Http.Request.Data);
            Index index = Common.DeserializeJson<Index>(reqData);
            index.OwnerGUID = md.User.GUID;
            if (String.IsNullOrEmpty(index.GUID)) index.GUID = Guid.NewGuid().ToString();

            if (_Daemon.IndexExists(index.Name))
            {
                _Logging.Warn(header + "index " + index.Name + " already exists");
                md.Http.Response.StatusCode = 409;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(409, "Index already exists.", null, null).ToJson(true));
                return;
            }

            _Daemon.AddIndex(index);
            _Logging.Debug(header + "created index " + index.Name);
            md.Http.Response.StatusCode = 201;
            await md.Http.Response.Send();
            return;
        }
    }
}