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
        public static HttpResponse PostIndexPreview(RequestMetadata md)
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
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexPreview no document type supplied");
                return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true), true);
            }

            #endregion

            #region Process

            List<string> errors;
            RestResponse resp = null;
            bool success = false;
            IndexedDoc idx = null;

            if (!String.IsNullOrEmpty(url))
            {
                #region URL

                success = DocIndexHandler.FromUrl(_Config, url, docType, out idx, out resp, out errors);
                if (success)
                {
                    return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(idx, pretty), true);
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index HTML from supplied URL");
                    return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unable to parse HTML from supplied URL.", errors).ToJson(true), true);
                }

                #endregion
            }
            else if (!String.IsNullOrEmpty(filename))
            {
                #region Filename

                switch (docType.ToLower())
                {
                    case "html":
                        success = DocIndexHandler.FromHtmlFile(filename, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(idx, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index HTML from supplied filename");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse HTML from supplied filename.", errors).ToJson(true), true);
                        }

                    case "json":
                        success = DocIndexHandler.FromJsonFile(filename, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(idx, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index JSON from supplied filename");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse JSON from supplied filename.", errors).ToJson(true), true);
                        }

                    case "xml":
                        success = DocIndexHandler.FromXmlFile(filename, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(idx, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index XML from supplied filename");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse XML from supplied filename.", null).ToJson(true), true);
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview invalid document type for processing via filename");
                        return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                            new ErrorResponse(400, "Invalid document type supplied for processing via filename.", null).ToJson(true), true);
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
                        success = DocIndexHandler.FromHtmlString(data, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(idx, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index HTML from supplied data");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse HTML from supplied data.", errors).ToJson(true), true);
                        }

                    case "json":
                        success = DocIndexHandler.FromJsonString(data, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(idx, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index JSON from supplied data");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse JSON from supplied data.", errors).ToJson(true), true);
                        }

                    case "xml":
                        success = DocIndexHandler.FromXmlString(filename, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(idx, pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index XML from supplied data");
                            return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse XML from supplied data.", null).ToJson(true), true);
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview invalid document type for processing via data");
                        return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                            new ErrorResponse(400, "Invalid document type supplied for processing via data.", null).ToJson(true), true);
                }

                #endregion
            }
            else if (docType.ToLower().Equals("sql"))
            {
                #region Query

                if (md.CurrRequest.Data == null || md.CurrRequest.Data.Length < 1)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview no query found in payload");
                    return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unable to find SQL query in request payload.", null).ToJson(true), true);
                }

                success = DocIndexHandler.FromSqlQuery(dbtype, dbserver, Convert.ToInt32(dbport), dbuser, dbpass, dbname, dbinstance, 
                    Encoding.UTF8.GetString(md.CurrRequest.Data), out idx, out errors);

                if (success)
                {
                    return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(idx, pretty), true);
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index SQL from supplied config and query");
                    return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unable to parse SQL from supplied config and query.", null).ToJson(true), true);
                }
                #endregion
            }
            else
            {
                #region Unknown

                _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to derive data source from request");
                return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "Unable to derive data source from request.", null).ToJson(true), true);

                #endregion
            }

            #endregion
        }
    }
}