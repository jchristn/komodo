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
        public static HttpResponse GetIndexDocument(RequestMetadata md)
        {
            #region Check-Input

            if (md.Http.RawUrlEntries.Count != 2)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexDocument raw URL entries does not contain exactly two items");
                return new HttpResponse(md.Http, 400, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(400, "URL must contain exactly two elements.", null).ToJson(true)));
            }

            #endregion

            #region Get-Values

            string indexName = md.Http.RawUrlEntries[0];
            string documentId = md.Http.RawUrlEntries[1];
            
            Index currIndex = _Index.GetIndexByName(indexName);
            
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexDocument unable to retrieve index " + indexName);
                return new HttpResponse(md.Http, 404, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(404, "Unknown index.", null).ToJson(true)));
            }

            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexDocument unable to retrieve client for index " + indexName);
                return new HttpResponse(md.Http, 500, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true)));
            }

            if (!md.Params.Parsed)
            {
                byte[] data = null;
                if (!currClient.GetSourceDocument(documentId, out data))
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "GetIndexDocument unable to find source document ID " + documentId + " in index " + indexName);
                    return new HttpResponse(md.Http, 404, null, "application/json",
                        Encoding.UTF8.GetBytes(new ErrorResponse(404, "Unable to find document.", null).ToJson(true)));
                }
                else
                {
                    return new HttpResponse(md.Http, 200, null, "application/octet-stream", data);
                }
            }
            else
            {
                IndexedDoc doc = null;
                if (!currClient.GetParsedDocument(documentId, out doc))
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "GetIndexDocument unable to find parsed document ID " + documentId + " in index " + indexName);
                    return new HttpResponse(md.Http, 404, null, "application/json",
                        Encoding.UTF8.GetBytes(new ErrorResponse(404, "Unable to find document.", null).ToJson(true)));
                }
                else
                {
                    return new HttpResponse(md.Http, 200, null, "application/json",
                        Encoding.UTF8.GetBytes(Common.SerializeJson(doc, md.Params.Pretty)));
                }
            }
            
            #endregion
        }
    }
}