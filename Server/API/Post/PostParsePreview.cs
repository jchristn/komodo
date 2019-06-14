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
        static HttpResponse PostParsePreview(RequestMetadata md)
        {
            #region Process

            if (String.IsNullOrEmpty(md.Params.Type))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview no document type supplied");
                return new HttpResponse(md.Http, 400, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true)));
            }

            List<string> errors;
            RestResponse resp = null;
            object parsed = null;
            ParsedHtml html = null;
            ParsedJson json = null;
            ParsedXml xml = null;
            ParsedSql sql = null;
            bool success = false;

            if (!String.IsNullOrEmpty(md.Params.Url))
            {
                #region URL

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        success = DocParseHandler.FromUrl(_Config, md.Params.Url, md.Params.Type, out parsed, out resp, out errors);
                        if (success)
                        {
                            html = (ParsedHtml)parsed;
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(html, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse HTML from supplied URL");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse HTML from supplied URL.", errors).ToJson(true)));
                        }

                    case "json":
                        success = DocParseHandler.FromUrl(_Config, md.Params.Url, md.Params.Type, out parsed, out resp, out errors);
                        if (success)
                        {
                            json = (ParsedJson)parsed;
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(json, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse JSON from supplied URL");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse JSON from supplied URL.", errors).ToJson(true)));
                        }

                    case "xml":
                        success = DocParseHandler.FromUrl(_Config, md.Params.Url, md.Params.Type, out parsed, out resp, out errors);
                        if (success)
                        {
                            xml = (ParsedXml)parsed;
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(xml, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse XML from supplied URL");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse XML from supplied URL.", errors).ToJson(true)));
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview invalid document type for processing via URL");
                        return new HttpResponse(md.Http, 400, null, "application/json",
                            Encoding.UTF8.GetBytes(new ErrorResponse(400, "Invalid document type supplied for processing via URL.", null).ToJson(true)));
                }

                #endregion
            }
            else if (!String.IsNullOrEmpty(md.Params.Filename))
            {
                #region Filename

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        success = DocParseHandler.FromHtmlFile(md.Params.Filename, out html, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(html, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse HTML from supplied filename");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse HTML from supplied filename.", errors).ToJson(true)));
                        }

                    case "json":
                        success = DocParseHandler.FromJsonFile(md.Params.Filename, out json, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(json, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse JSON from supplied filename");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse JSON from supplied filename.", errors).ToJson(true)));
                        }

                    case "xml":
                        success = DocParseHandler.FromXmlFile(md.Params.Filename, out xml, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(xml, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse XML from supplied filename");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse XML from supplied filename.", null).ToJson(true)));
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview invalid document type for processing via filename");
                        return new HttpResponse(md.Http, 400, null, "application/json",
                            Encoding.UTF8.GetBytes(new ErrorResponse(400, "Invalid document type supplied for processing via filename.", null).ToJson(true)));
                }

                #endregion
            }
            else if (md.Params.Type.ToLower().Equals("sql"))
            {
                #region Query

                if (md.Http.Data == null || md.Http.Data.Length < 1)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview no query found in payload");
                    return new HttpResponse(md.Http, 400, null, "application/json",
                        Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to find SQL query in request payload.", null).ToJson(md.Params.Pretty)));
                }

                success = DocParseHandler.FromSqlQuery(
                    md.Params.DbType, 
                    md.Params.DbServer, 
                    md.Params.DbPort, 
                    md.Params.DbUser, 
                    md.Params.DbPass, 
                    md.Params.DbName, 
                    md.Params.DbInstance, 
                    Encoding.UTF8.GetString(md.Http.Data), out sql, out errors);

                if (success)
                {
                    return new HttpResponse(md.Http, 200, null, "application/json",
                        Encoding.UTF8.GetBytes(Common.SerializeJson(sql, md.Params.Pretty)));
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse SQL from supplied config and query");
                    return new HttpResponse(md.Http, 400, null, "application/json",
                        Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse SQL from supplied config and query.", null).ToJson(true)));
                }
                #endregion
            }
            else if (md.Http.Data != null && md.Http.Data.Length > 0)
            {
                #region Supplied-Data

                string data = Encoding.UTF8.GetString(md.Http.Data);

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        success = DocParseHandler.FromHtmlString(data, out html, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(html, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse HTML from supplied data");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse HTML from supplied data.", errors).ToJson(true)));
                        }

                    case "json":
                        success = DocParseHandler.FromJsonString(data, out json, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(json, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse JSON from supplied data");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse JSON from supplied data.", errors).ToJson(true)));
                        }

                    case "xml":
                        success = DocParseHandler.FromXmlString(md.Params.Filename, out xml, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, 200, null, "application/json",
                                Encoding.UTF8.GetBytes(Common.SerializeJson(xml, md.Params.Pretty)));
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to parse XML from supplied data");
                            return new HttpResponse(md.Http, 400, null, "application/json",
                                Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to parse XML from supplied data.", null).ToJson(true)));
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview invalid document type for processing via data");
                        return new HttpResponse(md.Http, 400, null, "application/json",
                            Encoding.UTF8.GetBytes(new ErrorResponse(400, "Invalid document type supplied for processing via data.", null).ToJson(true)));
                }

                #endregion
            }
            else
            {
                #region Unknown

                _Logging.Log(LoggingModule.Severity.Warn, "PostParsePreview unable to derive data source from request");
                return new HttpResponse(md.Http, 400, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unable to derive data source from request.", null).ToJson(true)));

                #endregion
            }

            #endregion
        }
    }
}