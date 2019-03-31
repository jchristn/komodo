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
        public static HttpResponse PostIndexPreview(RequestMetadata md)
        { 
            #region Process

            if (String.IsNullOrEmpty(md.Params.Type))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "PostIndexPreview no document type supplied");
                return new HttpResponse(md.Http, false, 400, null, "application/json",
                    new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true), true);
            }

            List<string> errors;
            RestResponse resp = null;
            bool success = false;
            IndexedDoc idx = null;

            if (!String.IsNullOrEmpty(md.Params.Url))
            {
                #region URL

                success = DocIndexHandler.FromUrl(_Config, md.Params.Url, md.Params.Type, out idx, out resp, out errors);
                if (success)
                {
                    return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(idx, md.Params.Pretty), true);
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index HTML from supplied URL");
                    return new HttpResponse(md.Http, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unable to parse HTML from supplied URL.", errors).ToJson(true), true);
                }

                #endregion
            }
            else if (!String.IsNullOrEmpty(md.Params.Filename))
            {
                #region Filename

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        success = DocIndexHandler.FromHtmlFile(md.Params.Filename, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(idx, md.Params.Pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index HTML from supplied filename");
                            return new HttpResponse(md.Http, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse HTML from supplied filename.", errors).ToJson(true), true);
                        }

                    case "json":
                        success = DocIndexHandler.FromJsonFile(md.Params.Filename, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(idx, md.Params.Pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index JSON from supplied filename");
                            return new HttpResponse(md.Http, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse JSON from supplied filename.", errors).ToJson(true), true);
                        }

                    case "xml":
                        success = DocIndexHandler.FromXmlFile(md.Params.Filename, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(idx, md.Params.Pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index XML from supplied filename");
                            return new HttpResponse(md.Http, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse XML from supplied filename.", null).ToJson(true), true);
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview invalid document type for processing via filename");
                        return new HttpResponse(md.Http, false, 400, null, "application/json",
                            new ErrorResponse(400, "Invalid document type supplied for processing via filename.", null).ToJson(true), true);
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
                        success = DocIndexHandler.FromHtmlString(data, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(idx, md.Params.Pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index HTML from supplied data");
                            return new HttpResponse(md.Http, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse HTML from supplied data.", errors).ToJson(true), true);
                        }

                    case "json":
                        success = DocIndexHandler.FromJsonString(data, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(idx, md.Params.Pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index JSON from supplied data");
                            return new HttpResponse(md.Http, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse JSON from supplied data.", errors).ToJson(true), true);
                        }

                    case "xml":
                        success = DocIndexHandler.FromXmlString(md.Params.Filename, out idx, out errors);
                        if (success)
                        {
                            return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(idx, md.Params.Pretty), true);
                        }
                        else
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index XML from supplied data");
                            return new HttpResponse(md.Http, false, 400, null, "application/json",
                                new ErrorResponse(400, "Unable to parse XML from supplied data.", null).ToJson(true), true);
                        }

                    default:
                        _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview invalid document type for processing via data");
                        return new HttpResponse(md.Http, false, 400, null, "application/json",
                            new ErrorResponse(400, "Invalid document type supplied for processing via data.", null).ToJson(true), true);
                }

                #endregion
            }
            else if (md.Params.Type.ToLower().Equals("sql"))
            {
                #region Query

                if (md.Http.Data == null || md.Http.Data.Length < 1)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview no query found in payload");
                    return new HttpResponse(md.Http, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unable to find SQL query in request payload.", null).ToJson(true), true);
                }

                success = DocIndexHandler.FromSqlQuery(
                    md.Params.DbType, 
                    md.Params.DbServer, 
                    md.Params.DbPort, 
                    md.Params.DbUser, 
                    md.Params.DbPass, 
                    md.Params.DbName, 
                    md.Params.DbInstance, 
                    Encoding.UTF8.GetString(md.Http.Data), out idx, out errors);

                if (success)
                {
                    return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(idx, md.Params.Pretty), true);
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to index SQL from supplied config and query");
                    return new HttpResponse(md.Http, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unable to parse SQL from supplied config and query.", null).ToJson(true), true);
                }
                #endregion
            }
            else
            {
                #region Unknown

                _Logging.Log(LoggingModule.Severity.Warn, "PostDocIndexPreview unable to derive data source from request");
                return new HttpResponse(md.Http, false, 400, null, "application/json",
                    new ErrorResponse(400, "Unable to derive data source from request.", null).ToJson(true), true);

                #endregion
            }

            #endregion
        }
    }
}