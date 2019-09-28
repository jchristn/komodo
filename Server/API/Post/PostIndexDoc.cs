using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Core;
using Komodo.Core.Enums;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        private static async Task PostIndexDoc(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";
            string tempFilename = _Settings.Files.TempFiles + Guid.NewGuid().ToString();

            try
            {
                #region Retrieve-DocType-from-QS

                if (String.IsNullOrEmpty(md.Params.Type))
                {
                    _Logging.Warn(header + "PostIndexDoc no 'type' value found in querystring");
                    md.Http.Response.StatusCode = 400;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true));
                    return;
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
                        _Logging.Warn(header + "PostIndexDoc invalid 'type' value found in querystring: " + md.Params.Type);
                        md.Http.Response.StatusCode = 400;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null).ToJson(true));
                        return;
                }

                #endregion

                #region Retrieve-Index

                string indexName = md.Http.Request.RawUrlEntries[0];
                Index currIndex = _Index.GetIndexByName(indexName);
                if (currIndex == null || currIndex == default(Index))
                {
                    _Logging.Warn(header + "PostIndexDoc unknown index " + indexName);
                    md.Http.Response.StatusCode = 404;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null).ToJson(true));
                    return;
                }
                 
                IndexClient currClient = _Index.GetIndexClient(indexName);
                if (currClient == null)
                {
                    _Logging.Warn(header + "PostIndexDoc unable to retrieve client for index " + indexName);
                    md.Http.Response.StatusCode = 500;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(500, "Unable to retrieve client for index '" + indexName + "'.", null).ToJson(true));
                    return;
                }

                #endregion

                #region Write-Temp-File

                if (!String.IsNullOrEmpty(md.Params.Url) && !String.IsNullOrEmpty(md.Params.Type))
                {
                    Crawler crawler = new Crawler(md.Params.Url, (DocType)(Enum.Parse(typeof(DocType), md.Params.Type)));

                    using (FileStream fs = new FileStream(tempFilename, FileMode.Create, FileAccess.ReadWrite))
                    {
                        byte[] crawledData = crawler.RetrieveBytes();
                        await fs.WriteAsync(crawledData, 0, crawledData.Length);
                    }
                }
                else
                {
                    long bytesRemaining = md.Http.Request.ContentLength;
                    byte[] buffer = new byte[65536];

                    using (FileStream fs = new FileStream(tempFilename, FileMode.Create, FileAccess.ReadWrite))
                    {
                        while (bytesRemaining > 0)
                        {
                            int bytesRead = await md.Http.Request.Data.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                fs.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }

                #endregion

                #region Index-or-Store

                IndexResult result = null;
                 
                result = currClient.AddDocument(
                    currDocType,
                    tempFilename,
                    md.Params.Url,
                    md.Params.Name,
                    md.Params.Tags,
                    md.Http.Request.ContentType,
                    md.Params.Title,
                    md.Params.Bypass);

                if (result.Error.Id != ErrorId.NONE)
                {
                    _Logging.Warn(header + "PostIndexDoc unable to store document in index " + indexName);
                    md.Http.Response.StatusCode = 500;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(500, "Unable to store document in index '" + indexName + "'.", result).ToJson(true));
                    return;
                } 

                #endregion

                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(result, md.Params.Pretty));
                return;
            }
            finally
            {
                if (File.Exists(tempFilename)) File.Delete(tempFilename);
            }
        }
    }
}