using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BlobHelper;
using Komodo.Classes;
using Komodo.Crawler;
using Komodo.Parser;
using Komodo.Postings;
using DbType = Komodo.Classes.DbType;

namespace Test.Postings
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Press ENTER to quit");

                string crawlerType = Common.InputString("Crawler type [s3|azure|kvp|file|ftp|http|sql|sqlite]:", "file", true);
                if (String.IsNullOrEmpty(crawlerType)) break;

                switch (crawlerType)
                {
                    case "s3":
                        S3Crawler();
                        break;
                    case "azure":
                        AzureCrawler();
                        break;
                    case "kvp":
                        KvpCrawler();
                        break;
                    case "file":
                        FileCrawler();
                        break;
                    case "ftp":
                        FtpCrawler();
                        break;
                    case "http":
                        HttpCrawler();
                        break;
                    case "sql":
                        SqlCrawler();
                        break;
                    case "sqlite":
                        SqliteCrawler();
                        break;
                    default:
                        break;
                }
            }
        }

        static DocType GetDocType()
        {
            return (DocType)(Enum.Parse(typeof(DocType),
                Common.InputString("Document type [Html|Json|Sql|Text|Xml]:", "Html", false)));
        }

        static void S3Crawler()
        {
            string endpoint = Common.InputString("Endpoint:", null, true);
            bool ssl = Common.InputBoolean("SSL:", true);
            string bucket = Common.InputString("Bucket:", null, false);
            string key = Common.InputString("Key:", null, false);
            string accessKey = Common.InputString("Access Key:", null, false);
            string secretKey = Common.InputString("Secret Key:", null, false);
            AwsRegion region = (AwsRegion)(Enum.Parse(typeof(AwsRegion), Common.InputString("Region:", "USWest1", false)));
            string baseUrl = Common.InputString("Base URL:", "http://localhost:8000/{bucket}/{key}", false);

            S3Crawler s3c = null;
            if (!String.IsNullOrEmpty(endpoint)) s3c = new S3Crawler(endpoint, ssl, bucket, key, accessKey, secretKey, region, baseUrl);
            else s3c = new S3Crawler(bucket, key, accessKey, secretKey, region);

            CrawlResult cr = s3c.Get();

            Console.WriteLine("Success        : " + cr.Success);
            Console.WriteLine("Start time     : " + cr.Time.Start.ToString());
            Console.WriteLine("End time       : " + cr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + cr.ContentLength + " bytes");
            Console.WriteLine("Metadata       : " + Common.SerializeJson(cr.Metadata, false));
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(cr.Data));

            Console.WriteLine("");
            if (!cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(cr.Data);
                    break;
                case DocType.Json:
                    ParseJson(cr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(cr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(cr.Data);
                    break;
            }
        }

        static void AzureCrawler()
        {
            string accountName = Common.InputString("Account Name:", null, true);
            string container = Common.InputString("Container:", null, false);
            string endpoint = Common.InputString("Endpoint:", null, false);
            string accessKey = Common.InputString("Access Key:", null, false);
            string key = Common.InputString("Key:", null, false);

            AzureBlobCrawler ac = new AzureBlobCrawler(accountName, container, endpoint, accessKey, key);

            CrawlResult cr = ac.Get();

            Console.WriteLine("Success        : " + cr.Success);
            Console.WriteLine("Start time     : " + cr.Time.Start.ToString());
            Console.WriteLine("End time       : " + cr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + cr.ContentLength + " bytes");
            Console.WriteLine("Metadata       : " + Common.SerializeJson(cr.Metadata, false));
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(cr.Data));

            Console.WriteLine("");
            if (!cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(cr.Data);
                    break;
                case DocType.Json:
                    ParseJson(cr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(cr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(cr.Data);
                    break;
            }
        }

        static void KvpCrawler()
        {
            // string endpoint, string userGuid, string container, string apiKey, string key
            string endpoint = Common.InputString("Endpoint:", null, false);
            string userGuid = Common.InputString("User GUID:", null, true);
            string container = Common.InputString("Container:", null, false);
            string apiKey = Common.InputString("API Key:", null, false);
            string key = Common.InputString("Key:", null, false);

            KvpbaseCrawler kc = new KvpbaseCrawler(endpoint, userGuid, container, apiKey, key);

            CrawlResult cr = kc.Get();

            Console.WriteLine("Success        : " + cr.Success);
            Console.WriteLine("Start time     : " + cr.Time.Start.ToString());
            Console.WriteLine("End time       : " + cr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + cr.ContentLength + " bytes");
            Console.WriteLine("Metadata       : " + Common.SerializeJson(cr.Metadata, false));
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(cr.Data));

            Console.WriteLine("");
            if (!cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(cr.Data);
                    break;
                case DocType.Json:
                    ParseJson(cr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(cr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(cr.Data);
                    break;
            }
        }

        static void FileCrawler()
        {
            string filename = Common.InputString("Filename:", null, true);
            if (String.IsNullOrEmpty(filename)) return;

            FileCrawler fc = new FileCrawler(filename);
            CrawlResult cr = fc.Get();

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success        : " + cr.Success);
            Console.WriteLine("Start time     : " + cr.Time.Start.ToString());
            Console.WriteLine("End time       : " + cr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + cr.ContentLength + " bytes");
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(cr.Data));

            Console.WriteLine("");
            if (!cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(cr.Data);
                    break;
                case DocType.Json:
                    ParseJson(cr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(cr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(cr.Data);
                    break;
            }
        }

        static void FtpCrawler()
        {
            string url = Common.InputString("URL:", "ftp://127.0.0.1/", true);
            if (String.IsNullOrEmpty(url)) return;

            FtpCrawler fc = new FtpCrawler(url);
            fc.Username = Common.InputString("Username:", null, true);
            fc.Password = Common.InputString("Password:", null, true);
            CrawlResult cr = fc.Get();

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success        : " + cr.Success);
            Console.WriteLine("Start time     : " + cr.Time.Start.ToString());
            Console.WriteLine("End time       : " + cr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + cr.ContentLength + " bytes");
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(cr.Data));

            Console.WriteLine("");
            if (!cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(cr.Data);
                    break;
                case DocType.Json:
                    ParseJson(cr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(cr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(cr.Data);
                    break;
            }
        }

        static void HttpCrawler()
        {
            string url = Common.InputString("URL:", "http://127.0.0.1/", true);
            if (String.IsNullOrEmpty(url)) return;

            HttpCrawler hc = new HttpCrawler(url);
            hc.IgnoreCertificateErrors = true;
            hc.Method = (HttpMethod)(Enum.Parse(typeof(HttpMethod), Common.InputString("Method:", "GET", false)));
            hc.Username = Common.InputString("Username:", null, true);
            hc.Password = Common.InputString("Password:", null, true);
            CrawlResult cr = hc.Get();

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success        : " + cr.Success);
            Console.WriteLine("Start time     : " + cr.Time.Start.ToString());
            Console.WriteLine("End time       : " + cr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + cr.ContentLength + " bytes");
            if (cr.Http != null)
            {
                Console.WriteLine("Status code    : " + cr.Http.StatusCode);
                if (cr.Http.Headers != null && cr.Http.Headers.Count > 0)
                {
                    Console.WriteLine("Headers        : ");
                    foreach (KeyValuePair<string, string> curr in cr.Http.Headers)
                        Console.WriteLine("  " + curr.Key + ": " + curr.Value);
                }
            }
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(cr.Data));

            Console.WriteLine("");
            if (!cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(cr.Data);
                    break;
                case DocType.Json:
                    ParseJson(cr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(cr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(cr.Data);
                    break;
            }
        }

        static void SqlCrawler()
        {
            string query = Common.InputString("Query:", null, true);
            if (String.IsNullOrEmpty(query)) return;

            DbSettings db = new DbSettings(
                (DbType)(Enum.Parse(typeof(DbType), Common.InputString("DB type:", "Mysql", false))),
                Common.InputString("Hostname:", "localhost", false),
                Common.InputInteger("Port:", 3306, true, false),
                Common.InputString("Username:", "root", false),
                Common.InputString("Password:", "password", false),
                Common.InputString("Instance:", null, true),
                Common.InputString("Database name:", "dbname", false));

            SqlCrawler sc = new SqlCrawler(db, query);
            CrawlResult cr = sc.Get();

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success    : " + cr.Success);
            Console.WriteLine("Start time : " + cr.Time.Start.ToString());
            Console.WriteLine("End time   : " + cr.Time.End.ToString());
            Console.WriteLine("Total ms   : " + cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Data       : ");
            if (cr.DataTable != null)
            {
                Console.WriteLine(Common.SerializeJson(Common.DataTableToListDynamic(cr.DataTable), true));
            }
            else
            {
                Console.WriteLine("  (null)");
            }

            Console.WriteLine("");
            if (!cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            ParseSql(cr.DataTable);
        }

        static void SqliteCrawler()
        {
            SqliteCrawler sc = new SqliteCrawler(
                Common.InputString("Filename:", null, false),
                Common.InputString("Query:", null, false));
            CrawlResult cr = sc.Get();

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success    : " + cr.Success);
            Console.WriteLine("Start time : " + cr.Time.Start.ToString());
            Console.WriteLine("End time   : " + cr.Time.End.ToString());
            Console.WriteLine("Total ms   : " + cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Data       : ");
            if (cr.DataTable != null)
            {
                Console.WriteLine(Common.SerializeJson(Common.DataTableToListDynamic(cr.DataTable), true));
            }
            else
            {
                Console.WriteLine("  (null)");
            }

            Console.WriteLine("");
            if (!cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            ParseSqlite(cr.DataTable);
        }

        private static void ParseHtml(byte[] data)
        {
            HtmlParser hp = new HtmlParser();
            ParseResult hpr = hp.ParseBytes(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(hpr, true));
            Console.WriteLine("");

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.Process(hpr);
            
            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
            Console.WriteLine("");
        }

        private static void ParseJson(byte[] data)
        {
            JsonParser jp = new JsonParser();
            ParseResult jpr = jp.ParseBytes(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(jpr, true));
            Console.WriteLine("");

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.Process(jpr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
            Console.WriteLine("");
        }

        private static void ParseSql(DataTable data)
        {
            SqlParser sp = new SqlParser();
            ParseResult spr = sp.Parse(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(spr, true));
            Console.WriteLine("");

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.Process(spr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
            Console.WriteLine("");
        }

        private static void ParseSqlite(DataTable data)
        {
            SqliteParser sp = new SqliteParser();
            ParseResult spr = sp.Parse(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(spr, true));
            Console.WriteLine("");

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.Process(spr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
            Console.WriteLine("");
        }

        private static void ParseText(byte[] data)
        {
            TextParser tp = new TextParser();
            ParseResult tpr = tp.ParseBytes(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(tpr, true));
            Console.WriteLine("");

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.Process(tpr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
            Console.WriteLine("");
        }

        private static void ParseXml(byte[] data)
        {
            XmlParser xp = new XmlParser();
            ParseResult xpr = xp.ParseBytes(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(xpr, true));
            Console.WriteLine("");

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.Process(xpr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
            Console.WriteLine("");
        }
    }
}
