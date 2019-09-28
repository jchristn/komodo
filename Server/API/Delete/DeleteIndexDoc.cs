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
        private static async Task DeleteIndexDoc(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";
             
            #region Get-Values

            string indexName = md.Http.Request.RawUrlEntries[0];
            string docId = md.Http.Request.RawUrlEntries[1]; 

            Index currIndex = _Index.GetIndexByName(indexName); 
            if (currIndex == null)
            {
                _Logging.Warn(header + "DeleteIndexDoc unable to retrieve index " + indexName);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null).ToJson(true));
                return;
            }

            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Warn(header + "DeleteIndexDoc unable to retrieve client for index " + indexName);
                md.Http.Response.StatusCode = 500;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true));
                return;
            }
             
            if (!currClient.DeleteDocument(docId))
            {
                _Logging.Warn(header + "DeleteIndexDoc unable to delete document ID " + docId + " from index " + indexName);
                md.Http.Response.StatusCode = 500;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(500, "Unable to delete document from index '" + indexName + "'.", null).ToJson(true));
                return;
            }
            else
            {
                _Logging.Debug(header + "DeleteIndexDoc deleted document ID " + docId + " from index " + indexName);
                md.Http.Response.StatusCode = 204;
                await md.Http.Response.Send();
                return;
            }
            
            #endregion
        }
    }
}