using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        public static HttpResponse DeleteIndexDoc(RequestMetadata md)
        {
            #region Check-Input

            if (md.Http.RawUrlEntries.Count != 2)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DeleteIndexDoc raw URL entries does not contain exactly two items");
                return new HttpResponse(md.Http, false, 400, null, "application/json",
                    new ErrorResponse(400, "URL must contain exactly two elements.", null).ToJson(true), true);
            }

            #endregion

            #region Get-Values

            string indexName = md.Http.RawUrlEntries[0];
            string docId = md.Http.RawUrlEntries[1]; 

            Index currIndex = _Index.GetIndexByName(indexName); 
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DeleteIndexDoc unable to retrieve index " + indexName);
                return new HttpResponse(md.Http, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index.", null).ToJson(true), true);
            }

            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DeleteIndexDoc unable to retrieve client for index " + indexName);
                return new HttpResponse(md.Http, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true), true);
            }

            ErrorCode error;
            if (!currClient.DeleteDocument(docId, out error))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DeleteIndexDoc unable to delete document ID " + docId + " from index " + indexName);
                return new HttpResponse(md.Http, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to delete document from index '" + indexName + "'.", error).ToJson(true), true);
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "DeleteIndexDoc deleted document ID " + docId + " from index " + indexName);
                return new HttpResponse(md.Http, true, 204, null, "application/json", null, true);
            }
            
            #endregion
        }
    }
}