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
        private static async Task PostParse(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " PostParse ";
             
            if (String.IsNullOrEmpty(md.Params.Type))
            {
                _Logging.Warn(header + "no document type supplied");
                md.Http.Response.StatusCode = 400;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(400, "Supply 'type' [json/xml/html/sql/text] in querystring.", null, null).ToJson(true));
                return;
            }
             
            byte[] data = null;
            object crawlResult = null;
            object parseResult = null;
            PostingsOptions options = new PostingsOptions();
            PostingsGenerator postingsGen = null;
            PostingsResult postings = null;

            HttpCrawler httpCrawler = null;
            HttpCrawlResult httpCrawlResult = null;
            FileCrawler fileCrawler = null;
            FileCrawlResult fileCrawlResult = null;

            if (!String.IsNullOrEmpty(md.Params.Url))
            {
                #region Crawl-URL

                switch (md.Params.Type.ToLower())
                {
                    case "html":
                        httpCrawler = new HttpCrawler(md.Params.Url);
                        httpCrawlResult = httpCrawler.Get();
                        crawlResult = httpCrawlResult;
                        if (!httpCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, httpCrawlResult).ToJson(true));
                            return; 
                        }

                        data = httpCrawlResult.Data;
                        HtmlParser htmlParser = new HtmlParser();
                        HtmlParseResult htmlParseResult = htmlParser.ParseBytes(data);
                        parseResult = htmlParseResult;
                        if (!htmlParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied URL.", null, htmlParseResult).ToJson(true));
                            return;
                        }
                         
                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied URL.", null, postings).ToJson(true));
                            return;
                        }
                        break;

                    case "json":
                        httpCrawler = new HttpCrawler(md.Params.Url);
                        httpCrawlResult = httpCrawler.Get();
                        crawlResult = httpCrawlResult;
                        if (!httpCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, httpCrawlResult).ToJson(true));
                            return;
                        }

                        data = httpCrawlResult.Data;
                        JsonParser jsonParser = new JsonParser();
                        JsonParseResult jsonParseResult = jsonParser.ParseBytes(data);
                        parseResult = jsonParseResult;
                        if (!jsonParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied URL.", null, jsonParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied URL.", null, postings).ToJson(true));
                            return;
                        }
                        break;

                    case "text":
                        httpCrawler = new HttpCrawler(md.Params.Url);
                        httpCrawlResult = httpCrawler.Get();
                        crawlResult = httpCrawlResult;
                        if (!httpCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, httpCrawlResult).ToJson(true));
                            return;
                        }

                        data = httpCrawlResult.Data;
                        TextParser textParser = new TextParser();
                        TextParseResult textParseResult = textParser.ParseBytes(data);
                        parseResult = textParseResult;
                        if (!textParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied URL.", null, textParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied URL.", null, postings).ToJson(true));
                            return;
                        }
                        break;

                    case "xml":
                        httpCrawler = new HttpCrawler(md.Params.Url);
                        httpCrawlResult = httpCrawler.Get();
                        crawlResult = httpCrawlResult;
                        if (!httpCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied URL.", null, httpCrawlResult).ToJson(true));
                            return;
                        }

                        data = httpCrawlResult.Data;
                        XmlParser xmlParser = new XmlParser();
                        XmlParseResult xmlParseResult = xmlParser.ParseBytes(data);
                        parseResult = xmlParseResult;
                        if (!xmlParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied URL.", null, xmlParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from URL " + md.Params.Url);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied URL.", null, postings).ToJson(true));
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
                 
                Dictionary<string, object> ret = new Dictionary<string, object>();
                ret.Add("CrawlResult", crawlResult);
                ret.Add("ParseResult", parseResult);
                ret.Add("Postings", postings);

                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(ret, md.Params.Pretty));
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
                        fileCrawlResult = fileCrawler.Get();
                        crawlResult = fileCrawlResult;
                        if (!fileCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, fileCrawlResult).ToJson(true));
                            return;
                        }

                        data = fileCrawlResult.Data;
                        HtmlParser htmlParser = new HtmlParser();
                        HtmlParseResult htmlParseResult = htmlParser.ParseBytes(data);
                        parseResult = htmlParseResult;
                        if (!htmlParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from file " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied filename.", null, htmlParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied filename.", null, postings).ToJson(true));
                            return;
                        }
                        break; 

                    case "json":
                        fileCrawler = new FileCrawler(md.Params.Filename);
                        fileCrawlResult = fileCrawler.Get();
                        crawlResult = fileCrawlResult;
                        if (!fileCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, fileCrawlResult).ToJson(true));
                            return;
                        }

                        data = fileCrawlResult.Data;
                        JsonParser jsonParser = new JsonParser();
                        JsonParseResult jsonParseResult = jsonParser.ParseBytes(data);
                        parseResult = jsonParseResult;
                        if (!jsonParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from file " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied filename.", null, jsonParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied filename.", null, postings).ToJson(true));
                            return;
                        }
                        break;

                    case "text":
                        fileCrawler = new FileCrawler(md.Params.Filename);
                        fileCrawlResult = fileCrawler.Get();
                        crawlResult = fileCrawlResult;
                        if (!fileCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, fileCrawlResult).ToJson(true));
                            return;
                        }

                        data = fileCrawlResult.Data;
                        TextParser textParser = new TextParser();
                        TextParseResult textParseResult = textParser.ParseBytes(data);
                        parseResult = textParseResult;
                        if (!textParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from file " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied filename.", null, textParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied filename.", null, postings).ToJson(true));
                            return;
                        }
                        break;

                    case "xml":
                        fileCrawler = new FileCrawler(md.Params.Filename);
                        fileCrawlResult = fileCrawler.Get();
                        crawlResult = fileCrawlResult;
                        if (!fileCrawlResult.Success)
                        {
                            _Logging.Warn(header + "failed to crawl filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl supplied filename.", null, fileCrawlResult).ToJson(true));
                            return;
                        }

                        data = fileCrawlResult.Data;
                        XmlParser xmlParser = new XmlParser();
                        XmlParseResult xmlParseResult = xmlParser.ParseBytes(data);
                        parseResult = xmlParseResult;
                        if (!xmlParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from file " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied filename.", null, xmlParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from filename " + md.Params.Filename);
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied filename.", null, postings).ToJson(true));
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

                Dictionary<string, object> ret = new Dictionary<string, object>();
                ret.Add("CrawlResult", crawlResult);
                ret.Add("ParseResult", parseResult);
                ret.Add("Postings", postings);

                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(ret, md.Params.Pretty));
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

                DatabaseSettings dbSettings = new DatabaseSettings(md.Params.DbType, md.Params.DbServer, md.Params.DbPort, md.Params.DbUser, md.Params.DbPass, md.Params.DbInstance, md.Params.DbName);
                SqlCrawler sqlCrawler = new SqlCrawler(dbSettings, Encoding.UTF8.GetString(Common.StreamToBytes(md.Http.Request.Data)));
                SqlCrawlResult sqlCrawlResult = sqlCrawler.Get();
                crawlResult = sqlCrawlResult;
                if (!sqlCrawlResult.Success)
                {
                    _Logging.Warn(header + "failed to crawl database " + md.Params.DbName);
                    md.Http.Response.StatusCode = 500;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Failed to crawl specified database.", null, fileCrawlResult).ToJson(true));
                    return;
                }

                SqlParser sqlParser = new SqlParser();
                SqlParseResult sqlParseResult = sqlParser.Parse(sqlCrawlResult.DataTable);
                parseResult = sqlParseResult;
                if (!sqlParseResult.Success)
                {
                    _Logging.Warn(header + "failed to parse data from database " + md.Params.DbName);
                    md.Http.Response.StatusCode = 500;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from specified database.", null, sqlParseResult).ToJson(true));
                    return;
                }

                postingsGen = new PostingsGenerator(options);
                postings = postingsGen.ProcessParseResult(parseResult);
                if (!postings.Success)
                {
                    _Logging.Warn(header + "failed to parse data from database " + md.Params.DbName);
                    md.Http.Response.StatusCode = 500;
                    md.Http.Response.ContentType = "application/json";
                    await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from specified database.", null, postings).ToJson(true));
                    return;
                }

                Dictionary<string, object> ret = new Dictionary<string, object>();
                ret.Add("CrawlResult", crawlResult);
                ret.Add("ParseResult", parseResult);
                ret.Add("Postings", postings);

                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(ret, md.Params.Pretty));
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
                        HtmlParseResult htmlParseResult = htmlParser.ParseBytes(data);
                        parseResult = htmlParseResult;
                        if (!htmlParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied data.", null, htmlParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to generate postings from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied data.", null, postings).ToJson(true));
                            return;
                        }
                        break;

                    case "json": 
                        JsonParser jsonParser = new JsonParser();
                        JsonParseResult jsonParseResult = jsonParser.ParseBytes(data);
                        parseResult = jsonParseResult;
                        if (!jsonParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied data.", null, jsonParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to generate postings from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied data.", null, postings).ToJson(true));
                            return;
                        }
                        break;

                    case "text":
                        TextParser textParser = new TextParser();
                        TextParseResult textParseResult = textParser.ParseBytes(data);
                        parseResult = textParseResult;
                        if (!textParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied data.", null, textParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to generate postings from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied data.", null, postings).ToJson(true));
                            return;
                        }
                        break;

                    case "xml":
                        XmlParser xmlParser = new XmlParser();
                        XmlParseResult xmlParseResult = xmlParser.ParseBytes(data);
                        parseResult = xmlParseResult;
                        if (!xmlParseResult.Success)
                        {
                            _Logging.Warn(header + "failed to parse data from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to parse data from supplied data.", null, xmlParseResult).ToJson(true));
                            return;
                        }

                        postingsGen = new PostingsGenerator(options);
                        postings = postingsGen.ProcessParseResult(parseResult);
                        if (!postings.Success)
                        {
                            _Logging.Warn(header + "failed to generate postings from supplied data");
                            md.Http.Response.StatusCode = 500;
                            md.Http.Response.ContentType = "application/json";
                            await md.Http.Response.Send(new ErrorResponse(400, "Failed to generate postings from data from supplied data.", null, postings).ToJson(true));
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

                Dictionary<string, object> ret = new Dictionary<string, object>();
                ret.Add("CrawlResult", crawlResult);
                ret.Add("ParseResult", parseResult);
                ret.Add("Postings", postings);

                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(ret, md.Params.Pretty));
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