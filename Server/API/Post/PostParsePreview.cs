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
        static HttpResponse PostParsePreview(RequestMetadata md)
        {
            #region Get-Values-from-Querystring

            bool pretty = Common.IsTrue(md.CurrRequest.RetrieveHeaderValue("pretty"));
            string docType = md.CurrRequest.RetrieveHeaderValue("type");
            string url = md.CurrRequest.RetrieveHeaderValue("url");
            string filename = md.CurrRequest.RetrieveHeaderValue("filename");
            string dbtype = md.CurrRequest.RetrieveHeaderValue("dbtype");
            string dbserver = md.CurrRequest.RetrieveHeaderValue("dbserver");
            string dbport = md.CurrRequest.RetrieveHeaderValue("dbport");
            string dbuser = md.CurrRequest.RetrieveHeaderValue("dbuser");
            string dbpass = md.CurrRequest.RetrieveHeaderValue("dbpass");
            string dbinstance = md.CurrRequest.RetrieveHeaderValue("dbinstance");
            string dbname = md.CurrRequest.RetrieveHeaderValue("dbname");

            if (String.IsNullOrEmpty(docType))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview no document type supplied");
                return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true), true);
            }

            #endregion

            #region Process

            List<string> errors;
            RestResponse resp = null;
            object parsed = null;
            ParsedHtml html = null;
            ParsedJson json = null;
            ParsedXml xml = null;
            ParsedSql sql = null;
            bool success = false;

            if (!String.IsNullOrEmpty(url))
            {
                #region URL

                switch (docType.ToLower())
                {
                    case "html":
                        success = DocParseHandler.FromUrl(_Config, url, docType, out parsed, out resp, out errors);
                        if (success)
                        {
                            html = (ParsedHtml)parsed;
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(html, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse HTML from supplied URL");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse HTML from supplied URL.", errors).ToJson(true), true);
                        }

                    case "json":
                        success = DocParseHandler.FromUrl(_Config, url, docType, out parsed, out resp, out errors);
                        if (success)
                        {
                            json = (ParsedJson)parsed;
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(json, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse JSON from supplied URL");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse JSON from supplied URL.", errors).ToJson(true), true);
                        }

                    case "xml":
                        success = DocParseHandler.FromUrl(_Config, url, docType, out parsed, out resp, out errors);
                        if (success)
                        {
                            xml = (ParsedXml)parsed;
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(xml, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse XML from supplied URL");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse XML from supplied URL.", errors).ToJson(true), true);
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview invalid document type for processing via URL");
                        return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                            new ErrorResponse(400, "Invalid document type supplied for processing via URL.", null).ToJson(true), true);
                }

                #endregion
            }
            else if (!String.IsNullOrEmpty(filename))
            {
                #region Filename

                switch (docType.ToLower())
                {
                    case "html":
                        success = DocParseHandler.FromHtmlFile(filename, out html, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(html, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse HTML from supplied filename");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse HTML from supplied filename.", errors).ToJson(true), true);
                        }

                    case "json":
                        success = DocParseHandler.FromJsonFile(filename, out json, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(json, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse JSON from supplied filename");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse JSON from supplied filename.", errors).ToJson(true), true);
                        }

                    case "xml":
                        success = DocParseHandler.FromXmlFile(filename, out xml, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(xml, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse XML from supplied filename");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse XML from supplied filename.", null).ToJson(true), true);
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview invalid document type for processing via filename");
                        return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                            new ErrorResponse(400, "Invalid document type supplied for processing via filename.", null).ToJson(true), true);
                }

                #endregion
            }
            else if (docType.ToLower().Equals("sql"))
            {
                #region Query

                if (md.CurrRequest.Data == null || md.CurrRequest.Data.Length < 1)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview no query found in payload");
                    return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unable to find SQL query in request payload.", null), true);
                }

                success = DocParseHandler.FromSqlQuery(dbtype, dbserver, Convert.ToInt32(dbport), dbuser, dbpass, dbname, dbinstance, 
                    Encoding.UTF8.GetString(md.CurrRequest.Data), out sql, out errors);

                if (success)
                {
                    return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(sql, pretty), true);
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse SQL from supplied config and query");
                    return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unable to parse SQL from supplied config and query.", null).ToJson(true), true);
                }
                #endregion
            }
            else if (md.CurrRequest.Data != null && md.CurrRequest.Data.Length > 0)
            {
                #region Supplied-Data

                string data = Encoding.UTF8.GetString(md.CurrRequest.Data);

                switch (docType.ToLower())
                {
                    case "html":
                        success = DocParseHandler.FromHtmlString(data, out html, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(html, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse HTML from supplied data");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse HTML from supplied data.", errors).ToJson(true), true);
                        }

                    case "json":
                        success = DocParseHandler.FromJsonString(data, out json, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(json, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse JSON from supplied data");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse JSON from supplied data.", errors).ToJson(true), true);
                        }

                    case "xml":
                        success = DocParseHandler.FromXmlString(filename, out xml, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(xml, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse XML from supplied data");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse XML from supplied data.", null).ToJson(true), true);
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview invalid document type for processing via data");
                        return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                            new ErrorResponse(400, "Invalid document type supplied for processing via data.", null).ToJson(true), true);
                }

                #endregion
            }
            else
            {
                #region Unknown

                _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to derive data source from request");
                return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "Unable to derive data source from request.", null).ToJson(true), true);

                #endregion
            }

            #endregion
        }
    }
}