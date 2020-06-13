using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Komodo.Classes;
using Komodo.Crawler;
using Komodo.Daemon;
using Index = Komodo.Classes.Index;

namespace Test.Daemon
{
    class Program
    {
        static KomodoDaemon _Komodo = null;
        static bool _RunForever = true;
        static long _LastContentLength = 0;
        static byte[] _LastData = null;

        static void Main(string[] args)
        {
            Welcome();
            Initialize();

            while (_RunForever)
            {
                string userInput = Common.InputString("Komodo daemon (?/help) >", null, false);
                switch (userInput)
                {
                    #region General

                    case "?":
                        Menu();
                        break;

                    case "q":
                        _RunForever = false;
                        break;

                    case "cls":
                        Console.Clear();
                        break;

                    #endregion

                    #region Index-Management

                    case "get indices":
                        GetIndices();
                        break;

                    case "index exists":
                        IndexExists();
                        break;

                    case "index stats":
                        IndexStats();
                        break;

                    case "add index":
                        AddIndex();
                        break;

                    case "remove index":
                        RemoveIndex();
                        break;

                    #endregion

                    #region Crawlers

                    case "crawl http":
                        CrawlHttp();
                        break;

                    case "crawl ftp":
                        CrawlFtp();
                        break;

                    case "crawl file":
                        CrawlFile();
                        break;

                    #endregion

                    #region Index

                    case "source doc exists":
                        SourceDocExists();
                        break;

                    case "get source doc md":
                        GetSourceDocMetadata();
                        break;

                    case "get source doc data":
                        GetSourceDocData();
                        break;

                    case "parsed doc exists":
                        ParsedDocExists();
                        break;

                    case "get parsed doc md":
                        GetParsedDocMetadata();
                        break;

                    case "get parse result":
                        GetParseResult();
                        break;

                    case "get postings":
                        GetPostings();
                        break;

                    case "add doc":
                        AddDoc();
                        break;

                    case "remove doc":
                        RemoveDoc();
                        break;

                    case "search":
                        Search();
                        break;

                    case "enumerate":
                        Enumerate();
                        break;

                    #endregion

                    default:
                        break;
                }
            }
        }

        static void Welcome()
        {
            Console.WriteLine(Logo());
        }

        static string Logo()
        {
            // thank you https://psfonttk.com/big-text-generator/
            string ret =
                Environment.NewLine +
                Environment.NewLine +
                "oooo                                                    .o8            " + Environment.NewLine +
                "`888                                                    888            " + Environment.NewLine +
                " 888  oooo   .ooooo.  ooo. .oo.  .oo.    .ooooo.   .oooo888   .ooooo.  " + Environment.NewLine +
                " 888 .8P'   d88' `88b `888P'Y88bP'Y88b  d88' `88b d88' `888  d88' `88b " + Environment.NewLine +
                " 888888.    888   888  888   888   888  888   888 888   888  888   888 " + Environment.NewLine +
                " 888 `88b.  888   888  888   888   888  888   888 888   888  888   888 " + Environment.NewLine +
                "o888o o888o `Y8bod8P' o888o o888o o888o `Y8bod8P' `Y8bod88P  `Y8bod8P' " + Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine;
            return ret;
        }

        static void Initialize()
        {
            Console.WriteLine("");

            Console.Write("Initializing Komodo daemon: ");
            _Komodo = new KomodoDaemon();
            Console.WriteLine("[success]");

            Console.WriteLine("");
        }

        static void Menu()
        {
            //                          1         2         3         4         5         6         7         8
            //                 12345678901234567890123456789012345678901234567890123456789012345678901234567890
            Console.WriteLine("|  ");
            Console.WriteLine("General commands:");
            Console.WriteLine("|  ");
            Console.WriteLine("|  ?                        help, i.e. this menu");
            Console.WriteLine("|  q                        quit");
            Console.WriteLine("|  cls                      clear the screen");
            Console.WriteLine("|  ");
            Console.WriteLine("Index management commands:");
            Console.WriteLine("|  ");
            Console.WriteLine("|  get indices              retrieve a list of index names");
            Console.WriteLine("|  index exists             check if index exists by name");
            Console.WriteLine("|  index stats              retrieve stats for all or a specific index");
            Console.WriteLine("|  add index                add an index");
            Console.WriteLine("|  remove index             remove an index");
            Console.WriteLine("|  ");
            Console.WriteLine("Crawler commands:");
            Console.WriteLine("|  ");
            Console.WriteLine("|  crawl http               crawl a URL using HTTP");
            Console.WriteLine("|  crawl ftp                crawl a URL using FTP");
            Console.WriteLine("|  crawl file               crawl a file on the filesystem");
            Console.WriteLine("|  ");
            Console.WriteLine("Index commands:");
            Console.WriteLine("|  ");
            Console.WriteLine("|  source doc exists        check if a source doc exists by GUID");
            Console.WriteLine("|  get source doc md        retrieve a source document metadata");
            Console.WriteLine("|  get source doc data      retrieve a source document contents");
            Console.WriteLine("|  parsed doc exists        check if a parsed doc exists by source doc GUID");
            Console.WriteLine("|  get parsed doc md        retrieve a parsed document metadata");
            Console.WriteLine("|  get parse result         retrieve parse result for a source doc by GUID");
            Console.WriteLine("|  get postings             retrieve postings for a source doc by GUID");
            Console.WriteLine("|  add doc                  add a document to the index");
            Console.WriteLine("|  remove doc               remove a document from the index by GUID");
            Console.WriteLine("|  search                   search an index");
            Console.WriteLine("|  enumerate                enumerate the contents of an index");
            Console.WriteLine("|  ");
        }

        #region Index-Management

        static void GetIndices()
        {
            List<string> indices = _Komodo.GetIndices();
            if (indices == null || indices.Count < 1)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(indices, true));
            }
        }

        static void IndexExists()
        {
            string indexName = Common.InputString("Index name:", "default", false);
            Console.WriteLine("Exists: " + _Komodo.IndexExists(indexName));
        }

        static void IndexStats()
        {
            string indexName = Common.InputString("Index name (null for all indices):", null, true);
            IndicesStats stats = _Komodo.GetIndexStats(indexName);
            if (stats == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(stats, true));
            }
        }
         
        static void AddIndex()
        {
            Index index = new Index(
                Common.InputString("GUID  :", null, false),
                Common.InputString("Owner :", null, false),
                Common.InputString("Name  :", null, false));
            _Komodo.AddIndex(index);
        }

        static void RemoveIndex()
        {
            _Komodo.RemoveIndex(
                Common.InputString("Name    :", null, false),
                Common.InputBoolean("Destroy :", true)).Wait();
        }

        #endregion

        #region Crawlers

        static void CrawlHttp()
        {
            HttpCrawlResult hcr = _Komodo.CrawlWebpage(
                Common.InputString("URL:", null, false));

            if (hcr != null)
            {
                Console.WriteLine(Common.SerializeJson(hcr, true));

                if (hcr.Success)
                {
                    _LastContentLength = hcr.ContentLength;
                    _LastData = new byte[hcr.ContentLength];
                    Buffer.BlockCopy(hcr.Data, 0, _LastData, 0, (int)hcr.ContentLength);

                    Console.WriteLine("Content length and data stored for later use");
                }
            }
        }

        static void CrawlFtp()
        {
            FtpCrawlResult fcr = _Komodo.CrawlFtp(
                Common.InputString("URL:", null, false));

            if (fcr != null)
            {
                Console.WriteLine(Common.SerializeJson(fcr, true));

                if (fcr.Success)
                {
                    _LastContentLength = fcr.ContentLength;
                    _LastData = new byte[fcr.ContentLength];
                    Buffer.BlockCopy(fcr.Data, 0, _LastData, 0, (int)fcr.ContentLength);

                    Console.WriteLine("Content length and data stored for later use");
                }
            }
        }

        static void CrawlFile()
        {
            FileCrawlResult fcr = _Komodo.CrawlFile(
                Common.InputString("Path:", null, false));

            if (fcr != null)
            {
                Console.WriteLine(Common.SerializeJson(fcr, true));

                if (fcr.Success)
                {
                    _LastContentLength = fcr.ContentLength;
                    _LastData = new byte[fcr.ContentLength];
                    Buffer.BlockCopy(fcr.Data, 0, _LastData, 0, (int)fcr.ContentLength);

                    Console.WriteLine("Content length and data stored for later use");
                }
            }
        }
         
        #endregion

        #region Index

        static void SourceDocExists()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            Console.WriteLine("Exists: " + _Komodo.SourceDocumentExists(indexName, sourceGuid));
        }

        static void GetSourceDocMetadata()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            SourceDocument sourceDoc = _Komodo.GetSourceDocumentMetadata(indexName, sourceGuid);
            if (sourceDoc == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(sourceDoc, true));
            }
        }

        static async void GetSourceDocData()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            DocumentData doc = await _Komodo.GetSourceDocumentContent(indexName, sourceGuid);
            if (doc == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(doc, true));
            }
        }

        static void ParsedDocExists()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            Console.WriteLine("Exists: " + _Komodo.ParsedDocumentExists(indexName, sourceGuid));
        }

        static void GetParsedDocMetadata()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            ParsedDocument parsedDoc = _Komodo.GetParsedDocumentMetadata(indexName, sourceGuid);
            if (parsedDoc == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(parsedDoc, true));
            }
        }

        static void GetParseResult()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            object parseResult = _Komodo.GetDocumentParseResult(indexName, sourceGuid);
            if (parseResult == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(parseResult, true));
            }
        }

        static void GetPostings()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            PostingsResult postings = _Komodo.GetDocumentPostings(indexName, sourceGuid);
            if (postings == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(postings, true));
            }
        }

        static async void AddDoc()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            byte[] data = BuildData();
            SourceDocument sourceDoc = BuildSourceDocument();
            bool parse = Common.InputBoolean("Parse:", true);
            PostingsOptions options = new PostingsOptions();

            IndexResult result = await _Komodo.AddDocument(indexName, sourceDoc, data, parse, options);
            if (result == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(result, true));
            }
        }

        static SourceDocument BuildSourceDocument()
        {
            Console.WriteLine("Building source document");
            SourceDocument ret = new SourceDocument(
                Common.InputString("GUID           :", null, false),
                Common.InputString("Owner GUID     :", null, false),
                Common.InputString("Index GUID     :", null, false),
                Common.InputString("Name           :", null, false),
                Common.InputString("Title          :", null, true),
                Common.InputStringList("Tags           :", true), 
                (DocType)(Enum.Parse(typeof(DocType), 
                Common.InputString("DocType [Sql|Html|Json|Xml|Text]:", "Json", false))),
                Common.InputString("Source URL     :", null, true),
                Common.InputString("Content Type   :", "application/json", true),
                Common.InputInteger("Content Length :", (int)_LastContentLength, true, true),
                Common.InputString("MD5            :", null, true));
            return ret;
        }

        static byte[] BuildData()
        {
            if (_LastData != null && _LastData.Length > 0)
            {
                bool reuse = Common.InputBoolean("Reuse last crawl of " + _LastData.Length + " bytes?", true);
                if (reuse) return _LastData;
            }

            return Encoding.UTF8.GetBytes(Common.InputString("Data:", "Hello, world!", false));
        }

        static void RemoveDoc()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            string sourceGuid = Common.InputString("Source GUID :", null, false);
            _Komodo.RemoveDocument(indexName, sourceGuid);
        }

        static void Search()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            SearchQuery query = BuildSearchQuery();
            SearchResult result = _Komodo.Search(indexName, query); 
            if (result == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(result, true));
            }
        }

        static SearchQuery BuildSearchQuery()
        {
            Console.WriteLine("Building search query");
            SearchQuery ret = new SearchQuery();
            ret.IncludeMetadata = true;
            Console.WriteLine("Required filter:");
            ret.Required = BuildFilter(false);
            Console.WriteLine("Excluded filter:");
            ret.Exclude = BuildFilter(true);
            Console.WriteLine("Optional filter:");
            ret.Optional  = BuildFilter(true);
            return ret;
        }

        static QueryFilter BuildFilter(bool allowEmpty)
        {
            QueryFilter ret = new QueryFilter();
            ret.Terms = Common.InputStringList("  Term:", allowEmpty);
            ret.Filter = null;
            return ret;
        }

        static void Enumerate()
        {
            string indexName = Common.InputString("Index name  :", null, false);
            EnumerationQuery query = BuildEnumerationQuery();
            EnumerationResult result = _Komodo.Enumerate(indexName, query);
            if (result == null)
            {
                Console.WriteLine("(none)");
            }
            else
            {
                Console.WriteLine(Common.SerializeJson(result, true));
            }
        }
         
        static EnumerationQuery BuildEnumerationQuery()
        {
            Console.WriteLine("Building enumeration query");
            EnumerationQuery ret = new EnumerationQuery();
            return ret;
        }

        #endregion
    }
}
