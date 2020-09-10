using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using BlobHelper;
using Komodo.Classes;
using Komodo.Crawler;
using Komodo.Parser;
using DbType = Komodo.Classes.DbType;

namespace Test.Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Press ENTER to quit");

                string crawlerType = Common.InputString("Crawler type [s3|azure|kvp|komodo|file|ftp|http|sql|sqlite]:", "file", true);
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
                    case "komodo":
                        KomodoCrawler();
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
                Common.InputString("Document type [Html|Json|Sql|Sqlite|Text|Xml]:", "Html", false)));
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

            S3CrawlResult s3cr = s3c.Get();

            Console.WriteLine("Success        : " + s3cr.Success);
            Console.WriteLine("Start time     : " + s3cr.Time.Start.ToString());
            Console.WriteLine("End time       : " + s3cr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + s3cr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + s3cr.ContentLength + " bytes");
            Console.WriteLine("Metadata       : " + Common.SerializeJson(s3cr.Metadata, false));
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(s3cr.Data));

            Console.WriteLine("");
            if (!s3cr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }
             
            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(s3cr.Data);
                    break;
                case DocType.Json:
                    ParseJson(s3cr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(s3cr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(s3cr.Data);
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

            AzureBlobCrawlResult ar = ac.Get();

            Console.WriteLine("Success        : " + ar.Success);
            Console.WriteLine("Start time     : " + ar.Time.Start.ToString());
            Console.WriteLine("End time       : " + ar.Time.End.ToString());
            Console.WriteLine("Total ms       : " + ar.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + ar.ContentLength + " bytes");
            Console.WriteLine("Metadata       : " + Common.SerializeJson(ar.Metadata, false));
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(ar.Data));

            Console.WriteLine("");
            if (!ar.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }
             
            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(ar.Data);
                    break;
                case DocType.Json:
                    ParseJson(ar.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(ar.Data);
                    break;
                case DocType.Xml:
                    ParseXml(ar.Data);
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

            KvpbaseCrawlResult kcr = kc.Get();

            Console.WriteLine("Success        : " + kcr.Success);
            Console.WriteLine("Start time     : " + kcr.Time.Start.ToString());
            Console.WriteLine("End time       : " + kcr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + kcr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + kcr.ContentLength + " bytes");
            Console.WriteLine("Metadata       : " + Common.SerializeJson(kcr.Metadata, false));
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(kcr.Data));

            Console.WriteLine("");
            if (!kcr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(kcr.Data);
                    break;
                case DocType.Json:
                    ParseJson(kcr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(kcr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(kcr.Data);
                    break;
            }
        }

        static void KomodoCrawler()
        {
            // string endpoint, string indexGuid, string apiKey, string key
            string endpoint = Common.InputString("Endpoint:", null, false);
            string indexGuid = Common.InputString("Index GUID:", null, true);
            string apiKey = Common.InputString("API Key:", null, false);
            string key = Common.InputString("Key:", null, false);

            KomodoCrawler kc = new KomodoCrawler(endpoint, indexGuid, apiKey, key);

            KomodoCrawlResult kcr = kc.Get();

            Console.WriteLine("Success        : " + kcr.Success);
            Console.WriteLine("Start time     : " + kcr.Time.Start.ToString());
            Console.WriteLine("End time       : " + kcr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + kcr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + kcr.ContentLength + " bytes");
            Console.WriteLine("Metadata       : " + Common.SerializeJson(kcr.Metadata, false));
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(kcr.Data));

            Console.WriteLine("");
            if (!kcr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(kcr.Data);
                    break;
                case DocType.Json:
                    ParseJson(kcr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(kcr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(kcr.Data);
                    break;
            }
        }

        static void FileCrawler()
        {
            string filename = Common.InputString("Filename:", null, true);
            if (String.IsNullOrEmpty(filename)) return;

            FileCrawler fc = new FileCrawler(filename);
            FileCrawlResult fcr = fc.Get();  
            
            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success        : " + fcr.Success);
            Console.WriteLine("Start time     : " + fcr.Time.Start.ToString());
            Console.WriteLine("End time       : " + fcr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + fcr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + fcr.ContentLength + " bytes");
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(fcr.Data));

            Console.WriteLine("");
            if (!fcr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }
             
            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(fcr.Data);
                    break;
                case DocType.Json:
                    ParseJson(fcr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(fcr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(fcr.Data);
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
            FtpCrawlResult fcr = fc.Get(); 

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success        : " + fcr.Success);
            Console.WriteLine("Start time     : " + fcr.Time.Start.ToString());
            Console.WriteLine("End time       : " + fcr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + fcr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + fcr.ContentLength + " bytes");
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(fcr.Data));

            Console.WriteLine("");
            if (!fcr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }
             
            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(fcr.Data);
                    break;
                case DocType.Json:
                    ParseJson(fcr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(fcr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(fcr.Data);
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
            HttpCrawlResult hcr = hc.Get(); 

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success        : " + hcr.Success);
            Console.WriteLine("Start time     : " + hcr.Time.Start.ToString());
            Console.WriteLine("End time       : " + hcr.Time.End.ToString());
            Console.WriteLine("Total ms       : " + hcr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Content length : " + hcr.ContentLength + " bytes");
            Console.WriteLine("Headers        : ");
            if (hcr.Headers != null && hcr.Headers.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in hcr.Headers)
                    Console.WriteLine("  " + curr.Key + ": " + curr.Value);
            }
            Console.WriteLine("Data           :" + Environment.NewLine + Encoding.UTF8.GetString(hcr.Data));

            Console.WriteLine("");
            if (!hcr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            DocType docType = GetDocType();

            switch (docType)
            {
                case DocType.Html:
                    ParseHtml(hcr.Data);
                    break;
                case DocType.Json:
                    ParseJson(hcr.Data);
                    break;
                case DocType.Sql:
                    // ParseSql();
                    break;
                case DocType.Text:
                    ParseText(hcr.Data);
                    break;
                case DocType.Xml:
                    ParseXml(hcr.Data);
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
            SqlCrawlResult scr = sc.Get();

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success    : " + scr.Success);
            Console.WriteLine("Start time : " + scr.Time.Start.ToString());
            Console.WriteLine("End time   : " + scr.Time.End.ToString());
            Console.WriteLine("Total ms   : " + scr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Data       : ");
            if (scr.DataTable != null)
            {
                Console.WriteLine(Common.SerializeJson(Common.DataTableToListDynamic(scr.DataTable), true));
            }
            else
            {
                Console.WriteLine("  (null)");
            }

            Console.WriteLine("");
            if (!scr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            ParseSql(scr.DataTable);
        }

        static void SqliteCrawler()
        {
            SqliteCrawler sc = new SqliteCrawler(
                Common.InputString("Filename:", null, false),
                Common.InputString("Query:", null, false));
            SqliteCrawlResult scr = sc.Get(); 

            Console.WriteLine("--- Crawl Result ---");
            Console.WriteLine("Success    : " + scr.Success);
            Console.WriteLine("Start time : " + scr.Time.Start.ToString());
            Console.WriteLine("End time   : " + scr.Time.End.ToString());
            Console.WriteLine("Total ms   : " + scr.Time.TotalMs.ToString() + "ms");
            Console.WriteLine("Data       : ");
            if (scr.DataTable != null)
            {
                Console.WriteLine(Common.SerializeJson(Common.DataTableToListDynamic(scr.DataTable), true));
            }
            else
            {
                Console.WriteLine("  (null)");
            }

            Console.WriteLine("");
            if (!scr.Success)
            {
                Console.WriteLine("Failure status reported");
                Console.WriteLine("");
                return;
            }

            ParseSqlite(scr.DataTable);
        }

        private static void ParseHtml(byte[] data)
        {
            HtmlParser hp = new HtmlParser();
            HtmlParseResult hpr = hp.ParseBytes(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(hpr, true));
            Console.WriteLine("");
        }

        private static void ParseJson(byte[] data)
        {
            JsonParser jp = new JsonParser();
            JsonParseResult jpr = jp.ParseBytes(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(jpr, true));
            Console.WriteLine("");
        }

        private static void ParseSql(DataTable data)
        {
            SqlParser sp = new SqlParser();
            SqlParseResult spr = sp.Parse(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(spr, true));
            Console.WriteLine("");
        }

        private static void ParseSqlite(DataTable data)
        {
            SqliteParser sp = new SqliteParser();
            SqliteParseResult spr = sp.Parse(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(spr, true));
            Console.WriteLine("");
        }

        private static void ParseText(byte[] data)
        {
            TextParser tp = new TextParser();
            TextParseResult tpr = tp.ParseBytes(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(tpr, true));
            Console.WriteLine("");
        }

        private static void ParseXml(byte[] data)
        {
            XmlParser xp = new XmlParser();
            XmlParseResult xpr = xp.ParseBytes(data);

            Console.WriteLine("");
            Console.WriteLine("--- Parse Result ---");
            Console.WriteLine(Common.SerializeJson(xpr, true));
            Console.WriteLine("");
        }
    }
}
