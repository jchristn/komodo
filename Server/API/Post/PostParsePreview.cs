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
        private static async Task PostParsePreview(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";

            #region Process

            if (String.IsNullOrEmpty(md.Params.Type))
            {
                _Logging.Warn(header + "PostParsePreview no document type supplied");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true));
                return;
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
                        success = DocParseHandler.FromUrl(_Settings, md.Params.Url, md.Params.Type, out parsed, out resp, out errors);
                        if (success)
                        {
                            html = (ParsedHtml)parsed;
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(html, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse HTML from supplied URL");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse HTML.", null).ToJson(true));
                            return;
                        }

                    case "json":
                        success = DocParseHandler.FromUrl(_Settings, md.Params.Url, md.Params.Type, out parsed, out resp, out errors);
                        if (success)
                        {
                            json = (ParsedJson)parsed;
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(json, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse JSON from supplied URL");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse JSON.", null).ToJson(true));
                            return;
                        }

                    case "xml":
                        success = DocParseHandler.FromUrl(_Settings, md.Params.Url, md.Params.Type, out parsed, out resp, out errors);
                        if (success)
                        {
                            xml = (ParsedXml)parsed;
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(xml, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse XML from supplied URL");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse XML.", null).ToJson(true));
                            return;
                        }

                    default:
                        _Logging.Warn(header + "PostParsePreview invalid document type for processing via URL");
                        md.Http.Response.StatusCode = 400;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(400, "Invalid document type.", null).ToJson(true));
                        return;
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
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(html, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse HTML from supplied filename");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse HTML.", null).ToJson(true));
                            return;
                        }

                    case "json":
                        success = DocParseHandler.FromJsonFile(md.Params.Filename, out json, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(json, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse JSON from supplied filename");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse JSON.", null).ToJson(true));
                            return;
                        }

                    case "xml":
                        success = DocParseHandler.FromXmlFile(md.Params.Filename, out xml, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(xml, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse XML from supplied filename");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse XML.", null).ToJson(true));
                            return;
                        }

                    default:
                        _Logging.Warn(header + "PostParsePreview invalid document type for processing via filename");
                        md.Http.Response.StatusCode = 400;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(400, "Invalid document type.", null).ToJson(true));
                        return;
                }

                #endregion
            }
            else if (md.Params.Type.ToLower().Equals("sql"))
            {
                #region Query

                if (md.Http.Request.Data == null || md.Http.Request.ContentLength < 1)
                {
                    _Logging.Warn(header + "PostParsePreview no query found in payload");
                    md.Http.Response.StatusCode = 400;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "No SQL query in request payload.", null).ToJson(true));
                    return;
                }

                success = DocParseHandler.FromSqlQuery(
                    md.Params.DbType, 
                    md.Params.DbServer, 
                    md.Params.DbPort, 
                    md.Params.DbUser, 
                    md.Params.DbPass, 
                    md.Params.DbName, 
                    md.Params.DbInstance, 
                    Encoding.UTF8.GetString(Common.StreamToBytes(md.Http.Request.Data)), out sql, out errors);

                if (success)
                {
                    md.Http.Response.StatusCode = 200;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(Common.SerializeJson(sql, md.Params.Pretty));
                    return;
                }
                else
                {
                    _Logging.Warn(header + "PostParsePreview unable to parse SQL from supplied config and query");
                    md.Http.Response.StatusCode = 400;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse SQL.", null).ToJson(true));
                    return;
                }
                #endregion
            }
            else if (md.Http.Request.Data != null && md.Http.Request.ContentLength > 0)
            {
                #region Supplied-Data

                string data = Encoding.UTF8.GetString(Common.StreamToBytes(md.Http.Request.Data));

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        success = DocParseHandler.FromHtmlString(data, out html, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(html, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse HTML from supplied data");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse HTML.", null).ToJson(true));
                            return;
                        }

                    case "json":
                        success = DocParseHandler.FromJsonString(data, out json, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(json, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse JSON from supplied data");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse JSON.", null).ToJson(true));
                            return;
                        }

                    case "xml":
                        success = DocParseHandler.FromXmlString(md.Params.Filename, out xml, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(xml, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostParsePreview unable to parse XML from supplied data");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse XML.", null).ToJson(true));
                            return;
                        }

                    default:
                        _Logging.Warn(header + "PostParsePreview invalid document type for processing via data");
                        md.Http.Response.StatusCode = 400;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(400, "Invalid document type supplied.", null).ToJson(true));
                        return;
                }

                #endregion
            }
            else
            {
                #region Unknown

                _Logging.Warn(header + "PostParsePreview unable to derive data source from request");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Unable to derive data source from request.", null).ToJson(true));
                return;

                #endregion
            }

            #endregion
        }
    }
}