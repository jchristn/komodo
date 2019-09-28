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
        private static async Task PostIndexPreview(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";

            #region Process

            if (String.IsNullOrEmpty(md.Params.Type))
            {
                _Logging.Warn(header + "PostIndexPreview no document type supplied");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true));
                return;
            }

            List<string> errors;
            RestResponse resp = null;
            bool success = false;
            IndexedDoc idx = null;

            if (!String.IsNullOrEmpty(md.Params.Url))
            {
                #region URL

                success = DocIndexHandler.FromUrl(_Settings, md.Params.Url, md.Params.Type, out idx, out resp, out errors);
                if (success)
                {
                    md.Http.Response.StatusCode = 200;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
                    return;
                }
                else
                {
                    _Logging.Warn("PostDocIndexPreview unable to index HTML from supplied URL");
                    md.Http.Response.StatusCode = 400;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse HTML.", null).ToJson(true));
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
                        success = DocIndexHandler.FromHtmlFile(md.Params.Filename, out idx, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostDocIndexPreview unable to index HTML from supplied filename");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse HTML.", null).ToJson(true));
                            return;
                        }

                    case "json":
                        success = DocIndexHandler.FromJsonFile(md.Params.Filename, out idx, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostDocIndexPreview unable to index JSON from supplied filename");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse JSON.", null).ToJson(true));
                            return;
                        }

                    case "xml":
                        success = DocIndexHandler.FromXmlFile(md.Params.Filename, out idx, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostDocIndexPreview unable to index XML from supplied filename");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse XML.", null).ToJson(true));
                            return;
                        }

                    default:
                        _Logging.Warn(header + "PostDocIndexPreview invalid document type for processing via filename");
                        md.Http.Response.StatusCode = 400;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(400, "Invalid document type.", null).ToJson(true));
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
                        success = DocIndexHandler.FromHtmlString(data, out idx, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostDocIndexPreview unable to index HTML from supplied data");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse HTML.", null).ToJson(true));
                            return;
                        }

                    case "json":
                        success = DocIndexHandler.FromJsonString(data, out idx, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostDocIndexPreview unable to index JSON from supplied data");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse JSON.", null).ToJson(true));
                            return;
                        }

                    case "xml":
                        success = DocIndexHandler.FromXmlString(md.Params.Filename, out idx, out errors);
                        if (success)
                        {
                            md.Http.Response.StatusCode = 200;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
                            return;
                        }
                        else
                        {
                            _Logging.Warn(header + "PostDocIndexPreview unable to index XML from supplied data");
                            md.Http.Response.StatusCode = 400;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse XML.", null).ToJson(true));
                            return;
                        }

                    default:
                        _Logging.Warn(header + "PostDocIndexPreview invalid document type for processing via data");
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
                    _Logging.Warn(header + "PostDocIndexPreview no query found in payload");
                    md.Http.Response.StatusCode = 400;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "No SQL query supplied.", null).ToJson(true));
                    return;
                }

                success = DocIndexHandler.FromSqlQuery(
                    md.Params.DbType, 
                    md.Params.DbServer, 
                    md.Params.DbPort, 
                    md.Params.DbUser, 
                    md.Params.DbPass, 
                    md.Params.DbName, 
                    md.Params.DbInstance, 
                    Encoding.UTF8.GetString(Common.StreamToBytes(md.Http.Request.Data)), out idx, out errors);

                if (success)
                {
                    md.Http.Response.StatusCode = 200;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
                    return;
                }
                else
                {
                    _Logging.Warn(header + "PostDocIndexPreview unable to index SQL from supplied config and query");
                    md.Http.Response.StatusCode = 400;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Unable to parse SQL query.", null).ToJson(true));
                    return;
                }
                #endregion
            }
            else
            {
                #region Unknown

                _Logging.Warn(header + "PostDocIndexPreview unable to derive data source from request");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Unable to derive data source.", null).ToJson(true));
                return;

                #endregion
            }

            #endregion
        }
    }
}