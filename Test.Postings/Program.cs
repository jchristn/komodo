using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Komodo.Classes;
using Komodo.Crawler;
using Komodo.Parser;
using Komodo.Postings;

namespace Test.Postings
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Press ENTER to quit");
                string crawlerType = Common.InputString("Crawler type [file|ftp|http|sql|sqlite]:", "file", true);
                if (String.IsNullOrEmpty(crawlerType)) break;

                switch (crawlerType)
                {
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

            DatabaseSettings db = new DatabaseSettings(
                Common.InputString("DB type:", "MySql", false),
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

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.ProcessParseResult(hpr);
            
            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
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

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.ProcessParseResult(jpr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
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

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.ProcessParseResult(spr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
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

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.ProcessParseResult(spr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
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

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.ProcessParseResult(tpr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
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

            PostingsOptions po = new PostingsOptions();
            PostingsGenerator pg = new PostingsGenerator(po);
            PostingsResult pr = pg.ProcessParseResult(xpr);

            Console.WriteLine("");
            Console.WriteLine("--- Postings Result ---");
            Console.WriteLine(Common.SerializeJson(pr, true));
            Console.WriteLine("");
        }
    }
}
