using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        static HttpResponse PostIndexDoc(RequestMetadata md)
        {
            HttpResponse resp;
             
            #region Retrieve-DocType-from-QS

            string indexTypeStr = null;
            if (md.CurrRequest.QuerystringEntries.ContainsKey("type")) indexTypeStr = md.CurrRequest.QuerystringEntries["type"];
            if (String.IsNullOrEmpty(indexTypeStr))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc no 'type' value found in querystring");
                resp = new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true), true);
                return resp;
            }

            List<string> matchVals = new List<string> { "json", "xml", "html", "sql", "text" };
            if (!matchVals.Contains(indexTypeStr))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc invalid 'type' value found in querystring: " + indexTypeStr);
                resp = new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "Invalid 'type' in querystring, use [json/xml/html/sql/text].", null).ToJson(true), true);
                return resp;
            }

            DocType currDocType = DocType.Json;
            switch (indexTypeStr)
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
                    _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc invalid 'type' value found in querystring: " + indexTypeStr);
                    resp = new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                        new ErrorResponse(400, "Invalid 'type' in querystring, use [json/xml/html].", null).ToJson(true), true);
                    return resp;
            }

            #endregion

            #region Retrieve-Other-Params-from-QS

            string sourceUrl = null;
            if (md.CurrRequest.QuerystringEntries.ContainsKey("url")) sourceUrl = md.CurrRequest.QuerystringEntries["url"];

            #endregion

            #region Set-Stopwatch

            Stopwatch sw = new Stopwatch();
            sw.Start();

            #endregion

            #region Retrieve-Index

            string indexName = md.CurrRequest.RawUrlEntries[0];
            Index currIndex = _Index.GetIndexByName(indexName);
            if (currIndex == null || currIndex == default(Index))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc unknown index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index '" + indexName + "'.", null).ToJson(true), true);
            }

            #endregion

            #region Add-to-Index

            IndexClient currClient = _Index.GetIndexClient(indexName);
            if (currClient == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc unable to retrieve client for index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true), true); 
            }

            ErrorCode error = null;
            string masterDocId = null;
            if (!currClient.AddDocument(currDocType, md.CurrRequest.Data, sourceUrl, out error, out masterDocId))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexDoc unable to add document to index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to add document to index '" + indexName + "'.", error).ToJson(true), true);
            }

            #endregion

            #region Respond

            sw.Stop();

            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("DocumentId", masterDocId);
            ret.Add("AddTimeMs", sw.ElapsedMilliseconds);
            resp = new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(ret, true), true);
            return resp;

            #endregion
        }
    }
}