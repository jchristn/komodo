using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KomodoCore;

namespace KomodoTestSdk
{
    class TestSdk
    { 
        static KomodoSdk _Sdk;
        static bool _RunForever = true;

        static void Main(string[] args)
        {
            try
            {
                #region Initialize

                Welcome();
                _Sdk = new KomodoSdk(
                    Common.InputString("Endpoint:", "http://localhost:9090/", false),
                    Common.InputString("API Key:", "default", false));

                if (!_Sdk.Loopback())
                {
                    Console.WriteLine("Unable to connect to specified endpoint using specified API key");
                    return;
                }

                #endregion

                #region Process
                
                while (_RunForever)
                {
                    string cmd = Common.InputString("komodo [? for help]:", null, false);
                    switch (cmd)
                    {
                        case "?":
                            Menu();
                            break;
                        case "c":
                        case "cls":
                        case "clear":
                            Console.Clear();
                            break;
                        case "q":
                        case "quit":
                            _RunForever = false;
                            break;
                        case "list":
                            ListIndices();
                            break;
                        case "create index":
                            CreateIndex();
                            break;
                        case "delete index":
                            DeleteIndex();
                            break;
                        case "add":
                            AddDocument();
                            break;
                        case "get source":
                            GetSourceDocument();
                            break;
                        case "get parsed":
                            GetParsedDocument();
                            break;
                        case "delete":
                            DeleteDocument();
                            break;
                        case "search":
                            Search();
                            break;
                    }
                }

                #endregion
            }
            finally
            {
                Console.WriteLine("Press ENTER to exit");
                Console.ReadLine();
            }
        }

        static void Welcome()
        {
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
                Environment.NewLine;

            Console.WriteLine(ret);
        }

        static void Menu()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine(" ?             help, this menu");
            Console.WriteLine(" q             quit this application");
            Console.WriteLine(" cls           clear the screen");
            Console.WriteLine(" list          list indices");
            Console.WriteLine(" create index  create an index");
            Console.WriteLine(" delete index  delete an index");
            Console.WriteLine(" add           add a document to an index");
            Console.WriteLine(" get source    retrieve source document from an index");
            Console.WriteLine(" get parsed    retrieve parsed document from an index");
            Console.WriteLine(" delete        delete a document from an index");
            Console.WriteLine(" search        search an index");
            Console.WriteLine("");
        }

        static void ListIndices()
        {
            List<string> indices = null;
            if (!_Sdk.GetIndices(out indices))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Success");
                if (indices != null && indices.Count > 0)
                {
                    foreach (string curr in indices)
                    {
                        Console.WriteLine("  " + curr);
                    }
                }
            }
        }

        static void CreateIndex()
        {
            string file = Common.InputString("Filename:", "index.json", true);
            if (String.IsNullOrEmpty(file)) return;

            Index index = Common.DeserializeJson<Index>(Common.ReadBinaryFile(file));

            if (!_Sdk.CreateIndex(index.IndexName, index))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Success");
            }
        }

        static void DeleteIndex()
        {
            string indexName = Common.InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;
            bool cleanup = Common.InputBoolean("Cleanup", true);

            if (!_Sdk.DeleteIndex(indexName, cleanup))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Success");
            }
        }
         
        static void AddDocument()
        { 
            string indexName = Common.InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string sourceUrl = Common.InputString("Source URL:", null, true);
            DocType docType = GetDocType();

            string sourceFile = Common.InputString("Filename:", "order1.json", true);
            byte[] data = null;

            if (!String.IsNullOrEmpty(sourceFile)) data = Common.ReadBinaryFile(sourceFile);

            IndexResponse resp = null;
            if (!_Sdk.AddDocument(indexName, sourceUrl, docType, data, out resp))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Success");
                if (resp != null) Console.WriteLine(Common.SerializeJson(resp, true));
            }
        }

        static void GetSourceDocument()
        {
            // GetSourceDocument(string indexName, string docId, out byte[] data)
            string indexName = Common.InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string docId = Common.InputString("Document ID:", null, true);
            if (String.IsNullOrEmpty(docId)) return;

            byte[] data = null;
            if (!_Sdk.GetSourceDocument(indexName, docId, out data))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Success");
                if (data != null && data.Length > 0) Console.WriteLine(Encoding.UTF8.GetString(data));
            }
        }

        static void GetParsedDocument()
        {
            // GetParsedDocument(string indexName, string docId, out IndexedDoc doc)
            string indexName = Common.InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string docId = Common.InputString("Document ID:", null, true);
            if (String.IsNullOrEmpty(docId)) return;

            IndexedDoc doc = null;
            if (!_Sdk.GetParsedDocument(indexName, docId, out doc))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Success");
                if (doc != null) Console.WriteLine(Common.SerializeJson(doc, true));
            }
        }

        static void DeleteDocument()
        {
            // DeleteDocument(string indexName, string docId)
            string indexName = Common.InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string docId = Common.InputString("Document ID:", null, true);
            if (String.IsNullOrEmpty(docId)) return;

            if (!_Sdk.DeleteDocument(indexName, docId))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Success");
            }
        }

        static void Search()
        {
            // Search(string indexName, SearchQuery query, out SearchResult result)
            string indexName = Common.InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string filename = Common.InputString("Search filename:", "query1.json", true);
            if (String.IsNullOrEmpty(filename)) return;

            SearchQuery query = Common.DeserializeJson<SearchQuery>(Common.ReadBinaryFile(filename));
            SearchResult result = null;

            if (!_Sdk.Search(indexName, query, out result))
            {
                Console.WriteLine("Failed");
            }
            else
            {
                Console.WriteLine("Success");
                if (result != null) Console.WriteLine(Common.SerializeJson(result, true));
            }
        }

        static DocType GetDocType()
        {
            while (true)
            {
                string docType = Common.InputString("Document type [json/html/text/xml/sql]:", "json", false);
                switch (docType)
                {
                    case "json":
                        return DocType.Json;
                    case "html":
                        return DocType.Html;
                    case "text":
                        return DocType.Text;
                    case "xml":
                        return DocType.Xml;
                    case "sql":
                        return DocType.Sql;
                }
            }
        }
    }
}
