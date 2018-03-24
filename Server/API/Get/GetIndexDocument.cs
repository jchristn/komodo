using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using KomodoCore;
using RestWrapper;

namespace KomodoServer
{
    public partial class KomodoServer
    {
        public static HttpResponse GetIndexDocument(RequestMetadata md)
        {
            #region Check-Input

            if (md.CurrRequest.RawUrlEntries.Count != 2)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexDocument raw URL entries does not contain exactly two items");
                return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "URL must contain exactly two elements.", null).ToJson(true), true);
            }

            #endregion

            #region Get-Values

            bool pretty = Common.IsTrue(md.CurrRequest.RetrieveHeaderValue("pretty"));
            string indexName = md.CurrRequest.RawUrlEntries[0];
            string documentId = md.CurrRequest.RawUrlEntries[1];
            bool parsed = Common.IsTrue(md.CurrRequest.RetrieveHeaderValue("parsed"));

            Index currIndex = _Index.GetIndexByName(indexName);
            
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexDocument unable to retrieve index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index.", null).ToJson(true), true);
            }

            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexDocument unable to retrieve client for index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true), true);
            }

            if (!parsed)
            {
                byte[] data = null;
                if (!currClient.GetSourceDocument(documentId, out data))
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "GetIndexDocument unable to find source document ID " + documentId + " in index " + indexName);
                    return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                        new ErrorResponse(404, "Unable to find document.", null).ToJson(true), true);
                }
                else
                {
                    return new HttpResponse(md.CurrRequest, true, 200, null, "application/octet-stream", data, true);
                }
            }
            else
            {
                IndexedDoc doc = null;
                if (!currClient.GetParsedDocument(documentId, out doc))
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "GetIndexDocument unable to find parsed document ID " + documentId + " in index " + indexName);
                    return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                        new ErrorResponse(404, "Unable to find document.", null).ToJson(true), true);
                }
                else
                {
                    return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(doc, pretty), true);
                }
            }
            
            #endregion
        }
    }
}