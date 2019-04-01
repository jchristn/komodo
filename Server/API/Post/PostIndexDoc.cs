using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static HttpResponse PostIndexDoc(RequestMetadata md)
        {
            HttpResponse resp;
             
            #region Retrieve-DocType-from-QS
             
            if (String.IsNullOrEmpty(md.Params.Type))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc no 'type' value found in querystring");
                resp = new HttpResponse(md.Http, false, 400, null, "application/json",
                    new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true), true);
                return resp;
            }

            List<string> matchVals = new List<string> { "json", "xml", "html", "sql", "text" };
            if (!matchVals.Contains(md.Params.Type))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc invalid 'type' value found in querystring: " + md.Params.Type);
                resp = new HttpResponse(md.Http, false, 400, null, "application/json",
                    new ErrorResponse(400, "Invalid 'type' in querystring, use [json/xml/html/sql/text].", null).ToJson(true), true);
                return resp;
            }

            DocType currDocType = DocType.Json;
            switch (md.Params.Type)
            {
                case "json":
                    currDocType = DocType.Json;
                    break;

                case "xml":
                    currDocType = DocType.Xml;
                    break;

                case "html":
                    currDocType = DocType.Html;
                    break;

                case "sql":
                    currDocType = DocType.Sql;
                    break;

                case "text":
                    currDocType = DocType.Text;
                    break;

                default:
                    _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc invalid 'type' value found in querystring: " + md.Params.Type);
                    resp = new HttpResponse(md.Http, false, 400, null, "application/json",
                        new ErrorResponse(400, "Invalid 'type' in querystring, use [json/xml/html].", null).ToJson(true), true);
                    return resp;
            }

            #endregion
            
            #region Set-Stopwatch

            Stopwatch sw = new Stopwatch();
            sw.Start();

            #endregion

            #region Retrieve-Index

            string indexName = md.Http.RawUrlEntries[0];
            Index currIndex = _Index.GetIndexByName(indexName);
            if (currIndex == null || currIndex == default(Index))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc unknown index " + indexName);
                return new HttpResponse(md.Http, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index '" + indexName + "'.", null).ToJson(true), true);
            }

            #endregion

            #region Add-to-Index

            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc unable to retrieve client for index " + indexName);
                return new HttpResponse(md.Http, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true), true); 
            }

            ErrorCode error = null;
            string masterDocId = null;
            if (!currClient.AddDocument(
                currDocType, 
                md.Http.Data, 
                md.Params.Url,
                md.Params.Name,
                md.Params.Tags,
                md.Http.ContentType,
                out error, 
                out masterDocId))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc unable to add document to index " + indexName);
                return new HttpResponse(md.Http, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to add document to index '" + indexName + "'.", error).ToJson(true), true);
            }

            #endregion

            #region Respond

            sw.Stop();

            IndexResponse ret = new IndexResponse(masterDocId, sw.ElapsedMilliseconds);
            resp = new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(ret, true), true);
            return resp;

            #endregion
        }
    }
}