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
        private static async Task GetIndex(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";
             
            #region Get-Values
            
            string indexName = md.Http.Request.RawUrlEntries[0];
            Index ret = _Index.GetIndexByName(indexName);
            
            if (ret == null)
            {
                _Logging.Warn(header + "GetIndex unable to retrieve index " + indexName);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null).ToJson(true));
                return;
            }

            md.Http.Response.StatusCode = 200;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(Common.SerializeJson(ret, md.Params.Pretty));
            return; 
            
            #endregion
        }
    }
}