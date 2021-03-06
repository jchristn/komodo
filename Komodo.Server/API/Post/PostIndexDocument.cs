﻿using System;
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
using Komodo;
using Komodo.Crawler; 
using Komodo.IndexClient;
using Komodo.Parser;
using Komodo.Postings;
using Komodo.Server.Classes;
using Index = Komodo.Index;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task PostIndexDocument(RequestMetadata md)
        {
            #region Variables

            string header = "[Komodo.Server] " + md.Http.Request.Source.IpAddress + ":" + md.Http.Request.Source.Port + " PostIndexDocument ";
            string tempFile = _Settings.TempStorage.Disk.Directory + Guid.NewGuid().ToString();
            string indexName = md.Http.Request.Url.Elements[0];
            string sourceGuid = null;
            if (md.Http.Request.Url.Elements.Length == 2) sourceGuid = md.Http.Request.Url.Elements[1];

            #endregion

            #region Check-Index-Existence
             
            Index index = _Daemon.GetIndex(indexName);
            if (index == null)
            {
                _Logging.Warn(header + "index " + indexName + " does not exist");
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                return;
            }

            #endregion

            #region Check-Supplied-GUID

            if (!String.IsNullOrEmpty(sourceGuid))
            {
                if (_Daemon.SourceDocumentExists(indexName, sourceGuid))
                {
                    _Logging.Warn(header + "document " + indexName + "/" + sourceGuid + " already exists");
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
                case "unknown":
                    docType = DocType.Unknown;
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
                CrawlResult crawlResult = null;

                if (!String.IsNullOrEmpty(md.Params.Url) || !String.IsNullOrEmpty(md.Params.Filename))
                {
                    #region Crawl

                    if (!String.IsNullOrEmpty(md.Params.Url))
                    {
                        HttpCrawler httpCrawler = new HttpCrawler(md.Params.Url);
                        crawlResult = httpCrawler.Download(tempFile);
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, crawlResult).ToJson(true));
                            return;
                        }

                        contentLength = crawlResult.ContentLength;
                        md5 = Common.Md5File(tempFile);
                    }
                    else
                    {
                        FileCrawler fileCrawler = new FileCrawler(md.Params.Filename);
                        crawlResult = fileCrawler.Download(tempFile);
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, crawlResult).ToJson(true));
                            return;
                        }

                        contentLength = crawlResult.ContentLength;
                        md5 = Common.Md5(tempFile);
                    }

                    #endregion
                }
                else
                { 
                    using (FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.ReadWrite))
                    {
                        await md.Http.Request.Data.CopyToAsync(fs); 
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
                    sourceGuid,
                    md.User.GUID,
                    index.GUID,
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

                    IndexResult result = await _Daemon.AddDocument(
                        indexName, 
                        src, 
                        Common.ReadBinaryFile(tempFile),
                        !md.Params.Bypass,
                        new PostingsOptions());

                    if (!result.Success)
                    {
                        _Logging.Warn(header + "unable to store document in index " + indexName);
                        md.Http.Response.StatusCode = 500;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(500, "Unable to store document in index '" + indexName + "'.", null, result).ToJson(true));
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
                    result.PostingsResult = null;
                    result.Time = null;

                    Task unawaited = _Daemon.AddDocument(
                        index.Name,
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