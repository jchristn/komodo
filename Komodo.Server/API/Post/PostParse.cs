using System;
using System.Collections.Generic;
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
        private static async Task PostParse(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.Source.IpAddress + ":" + md.Http.Request.Source.Port + " PostParse ";
             
            if (String.IsNullOrEmpty(md.Params.Type))
            {
                _Logging.Warn(header + "no document type supplied");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null, null).ToJson(true));
                return;
            }
             
            byte[] data = null;
            CrawlResult crawlResult = null;
            ParseResult parseResult = null; 

            HttpCrawler httpCrawler = null;
            FileCrawler fileCrawler = null;

            if (!String.IsNullOrEmpty(md.Params.Url))
            {
                #region Crawl-URL

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        httpCrawler = new HttpCrawler(md.Params.Url);
                        crawlResult = httpCrawler.Get();
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, crawlResult).ToJson(true));
                            return; 
                        }

                        data = crawlResult.Data;
                        HtmlParser htmlParser = new HtmlParser();
                        parseResult = htmlParser.ParseBytes(data);
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied URL.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    case "json":
                        httpCrawler = new HttpCrawler(md.Params.Url);
                        crawlResult = httpCrawler.Get(); 
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, crawlResult).ToJson(true));
                            return;
                        }

                        data = crawlResult.Data;
                        JsonParser jsonParser = new JsonParser();
                        parseResult = jsonParser.ParseBytes(data);
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied URL.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    case "text":
                        httpCrawler = new HttpCrawler(md.Params.Url);
                        crawlResult = httpCrawler.Get(); 
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, crawlResult).ToJson(true));
                            return;
                        }

                        data = crawlResult.Data;
                        TextParser textParser = new TextParser();
                        parseResult = textParser.ParseBytes(data); 
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied URL.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    case "xml":
                        httpCrawler = new HttpCrawler(md.Params.Url);
                        crawlResult = httpCrawler.Get(); 
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, crawlResult).ToJson(true));
                            return;
                        }

                        data = crawlResult.Data;
                        XmlParser xmlParser = new XmlParser();
                        parseResult = xmlParser.ParseBytes(data); 
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied URL.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    default:
                        _Logging.Warn(header + "invalid document type for processing via URL " + md.Params.Url);
                        md.Http.Response.StatusCode = 400;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(400, "Invalid document type.", null, null).ToJson(true));
                        return;
                }
                  
                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(parseResult, md.Params.Pretty));
                return;

                #endregion
            }
            else if (!String.IsNullOrEmpty(md.Params.Filename))
            {
                #region Filename

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        fileCrawler = new FileCrawler(md.Params.Filename);
                        crawlResult = fileCrawler.Get(); 
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, crawlResult).ToJson(true));
                            return;
                        }

                        data = crawlResult.Data;
                        HtmlParser htmlParser = new HtmlParser();
                        parseResult = htmlParser.ParseBytes(data);
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from file " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied filename.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break; 

                    case "json":
                        fileCrawler = new FileCrawler(md.Params.Filename);
                        crawlResult = fileCrawler.Get(); 
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, crawlResult).ToJson(true));
                            return;
                        }

                        data = crawlResult.Data;
                        JsonParser jsonParser = new JsonParser();
                        parseResult = jsonParser.ParseBytes(data);
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from file " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied filename.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    case "text":
                        fileCrawler = new FileCrawler(md.Params.Filename);
                        crawlResult = fileCrawler.Get(); 
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, crawlResult).ToJson(true));
                            return;
                        }

                        data = crawlResult.Data;
                        TextParser textParser = new TextParser();
                        parseResult = textParser.ParseBytes(data); 
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from file " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied filename.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    case "xml":
                        fileCrawler = new FileCrawler(md.Params.Filename);
                        crawlResult = fileCrawler.Get(); 
                        if (!crawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, crawlResult).ToJson(true));
                            return;
                        }

                        data = crawlResult.Data;
                        XmlParser xmlParser = new XmlParser();
                        parseResult = xmlParser.ParseBytes(data); 
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from file " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied filename.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    default:
                        _Logging.Warn(header + "invalid document type for processing via filename " + md.Params.Filename);
                        md.Http.Response.StatusCode = 400;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(400, "Invalid document type.", null, null).ToJson(true));
                        return;
                }
                 
                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(parseResult, md.Params.Pretty));
                return;

                #endregion
            }
            else if (md.Params.Type.ToLower().Equals("sql"))
            {
                #region Query

                if (md.Http.Request.Data == null || md.Http.Request.ContentLength < 1)
                {
                    _Logging.Warn(header + "no query found in payload");
                    md.Http.Response.StatusCode = 400;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "No SQL query in request payload.", null, null).ToJson(true));
                    return;
                }

                DbSettings dbSettings = new DbSettings(md.Params.DbType, md.Params.DbServer, md.Params.DbPort, md.Params.DbUser, md.Params.DbPass, md.Params.DbInstance, md.Params.DbName);
                SqlCrawler sqlCrawler = new SqlCrawler(dbSettings, Encoding.UTF8.GetString(Common.StreamToBytes(md.Http.Request.Data)));
                crawlResult = sqlCrawler.Get(); 
                if (!crawlResult.Success)
                {
                    _Logging.Warn(header + "failed to crawl database " + md.Params.DbName);
                    md.Http.Response.StatusCode = 500;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl specified database.", null, crawlResult).ToJson(true));
                    return;
                }

                SqlParser sqlParser = new SqlParser();
                parseResult = sqlParser.Parse(crawlResult.DataTable); 
                if (!parseResult.Success)
                {
                    _Logging.Warn(header + "failed to parse data from database " + md.Params.DbName);
                    md.Http.Response.StatusCode = 500;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from specified database.", null, parseResult).ToJson(true));
                    return;
                } 

                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(parseResult, md.Params.Pretty));
                return;
                 
                #endregion
            }
            else if (md.Http.Request.Data != null && md.Http.Request.ContentLength > 0)
            {
                #region Supplied-Data

                data = Common.StreamToBytes(md.Http.Request.Data);

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        HtmlParser htmlParser = new HtmlParser();
                        parseResult = htmlParser.ParseBytes(data); 
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied data.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    case "json": 
                        JsonParser jsonParser = new JsonParser();
                        parseResult = jsonParser.ParseBytes(data); 
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied data.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    case "text":
                        TextParser textParser = new TextParser();
                        parseResult = textParser.ParseBytes(data); 
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied data.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    case "xml":
                        XmlParser xmlParser = new XmlParser();
                        parseResult = xmlParser.ParseBytes(data); 
                        if (!parseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied data.", null, parseResult).ToJson(true));
                            return;
                        } 
                        break;

                    default:
                        _Logging.Warn(header + "invalid document type for processing via data");
                        md.Http.Response.StatusCode = 400;
                        md.Http.Response.ContentType = "application/json";
                        await md.Http.Response.Send(new ErrorResponse(400, "Invalid document type supplied.", null, null).ToJson(true));
                        return;
                }
                 
                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(parseResult, md.Params.Pretty));
                return;

                #endregion
            }
            else
            {
                #region Unknown

                _Logging.Warn(header + "unable to derive data source from request");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Unable to derive data source from request.", null, null).ToJson(true));
                return;

                #endregion
            } 
        }
    }
}