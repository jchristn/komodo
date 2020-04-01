using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DatabaseWrapper;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Classes;
using Komodo.Crawler;
using Komodo.Database; 
using Komodo.IndexClient;
using Komodo.Parser;
using Komodo.Postings;
using Komodo.Server.Classes;
using Index = Komodo.Classes.Index;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task PostIndexDocument(RequestMetadata md)
        {
            #region Variables

            string header = "[Komodo.Server] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " PostIndexDocument ";
            string tempFile = _Settings.TempStorage.Disk.Directory + Guid.NewGuid().ToString();
            string name = md.Http.Request.RawUrlEntries[0];
            string docId = null;
            if (md.Http.Request.RawUrlEntries.Count == 2) docId = md.Http.Request.RawUrlEntries[1];

            #endregion

            #region Get-Index-Client

            KomodoIndex idx = _Indices.Get(name);
            if (idx == null || idx == default(KomodoIndex))
            {
                _Logging.Warn(header + "unknown index " + name);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                return;
            }

            #endregion

            #region Check-Supplied-GUID

            if (!String.IsNullOrEmpty(docId))
            {
                if (idx.ExistsSource(docId))
                {
                    _Logging.Warn(header + "requested source GUID " + docId + " already exists");
                    md.Http.Response.StatusCode = 409;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(409, "Requested GUID already exists.", null, null).ToJson(true));
                    return;
                }
            } 

            #endregion

            #region Retrieve-DocType-from-QS

            if (String.IsNullOrEmpty(md.Params.Type))
            {
                _Logging.Warn(header + "no 'type' value found in querystring");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null, null).ToJson(true));
                return;
            }

            DocType docType = DocType.Json;
            switch (md.Params.Type)
            {
                case "json":
                    docType = DocType.Json;
                    break;

                case "xml":
                    docType = DocType.Xml;
                    break;

                case "html":
                    docType = DocType.Html;
                    break;

                case "sql":
                    docType = DocType.Sql;
                    break;

                case "text":
                    docType = DocType.Text;
                    break;

                default:
                    _Logging.Warn(header + "invalid 'type' value found in querystring: " + md.Params.Type);
                    md.Http.Response.StatusCode = 400;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null, null).ToJson(true));
                    return;
            }

            #endregion
            
            try
            {
                #region Write-Temp-File

                long contentLength = 0;
                string md5 = null;

                if (!String.IsNullOrEmpty(md.Params.Url) || !String.IsNullOrEmpty(md.Params.Filename))
                {
                    #region Crawl

                    if (!String.IsNullOrEmpty(md.Params.Url))
                    {
                        HttpCrawler httpCrawler = new HttpCrawler(md.Params.Url);
                        HttpCrawlResult httpCrawlResult = httpCrawler.Download(tempFile);
                        if (!httpCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, httpCrawlResult).ToJson(true));
                            return;
                        }

                        contentLength = httpCrawlResult.ContentLength;
                        md5 = Common.Md5File(tempFile);
                    }
                    else
                    {
                        FileCrawler fileCrawler = new FileCrawler(md.Params.Filename);
                        FileCrawlResult fileCrawlResult = fileCrawler.Download(tempFile);
                        if (!fileCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, fileCrawlResult).ToJson(true));
                            return;
                        }

                        contentLength = fileCrawlResult.ContentLength;
                        md5 = Common.Md5(tempFile);
                    }

                    #endregion
                }
                else
                {
                    // long bytesRemaining = md.Http.Request.ContentLength;
                    // byte[] buffer = new byte[65536];

                    using (FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        await md.Http.Request.Data.CopyToAsync(fs);
                        /*
                        while (bytesRemaining > 0)
                        {
                            int bytesRead = await md.Http.Request.Data.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                fs.Write(buffer, 0, bytesRead);
                            }
                        }
                        */
                    }

                    contentLength = md.Http.Request.ContentLength;
                    md5 = Common.Md5File(tempFile);
                }

                #endregion

                #region Build-Source-Document

                string sourceUrl = null;
                if (!String.IsNullOrEmpty(md.Params.Url)) sourceUrl = md.Params.Url;
                else if (!String.IsNullOrEmpty(md.Params.Filename)) sourceUrl = md.Params.Filename;
                List<string> tags = null;
                if (!String.IsNullOrEmpty(md.Params.Tags)) tags = Common.CsvToStringList(md.Params.Tags);

                SourceDocument src = new SourceDocument(
                    docId,
                    md.User.GUID,
                    idx.GUID,
                    md.Params.Name,
                    md.Params.Title,
                    tags,
                    docType,
                    sourceUrl,
                    md.Http.Request.ContentType,
                    contentLength,
                    md5);

                #endregion

                if (!md.Params.Async)
                {
                    #region Sync

                    IndexResult result = await idx.Add(
                        src,
                        Common.ReadBinaryFile(tempFile),
                        !md.Params.Bypass,
                        new PostingsOptions());

                    if (!result.Success)
                    {
                        _Logging.Warn(header + "unable to store document in index " + name);
                        md.Http.Response.StatusCode = 500;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(500, "Unable to store document in index '" + name + "'.", null, result).ToJson(true));
                        return;
                    }

                    md.Http.Response.StatusCode = 200;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(Common.SerializeJson(result, md.Params.Pretty));
                    return;

                    #endregion
                }
                else
                {
                    #region Async

                    IndexResult result = new IndexResult();
                    result.Success = true;
                    result.GUID = src.GUID;
                    result.Type = docType;
                    result.ParseResult = null;
                    result.Postings = null;
                    result.Time = null;

                    Task unawaited = idx.Add(
                        src,
                        Common.ReadBinaryFile(tempFile),
                        !md.Params.Bypass,
                        new PostingsOptions());

                    md.Http.Response.StatusCode = 200;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(Common.SerializeJson(result, md.Params.Pretty));
                    return;

                    #endregion
                }
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        } 
    }
}