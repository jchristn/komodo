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
        private static async Task DeleteIndex(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";
             
            #region Get-Values
             
            string indexName = md.Http.Request.RawUrlEntries[0];             
            Index currIndex = _Index.GetIndexByName(indexName); 
            if (currIndex == null)
            {
                _Logging.Warn(header + "DeleteIndex unable to retrieve index " + indexName);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null).ToJson(true));
                return;
            }

            _Index.RemoveIndex(indexName, md.Params.Cleanup);
            md.Http.Response.StatusCode = 204;
            await md.Http.Response.Send();
            return; 
            
            #endregion
        }
    }
}