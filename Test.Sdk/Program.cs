using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Komodo.Sdk;
using Komodo.Classes;
using Index = Komodo.Classes.Index;

namespace Test.Sdk
{
    class Program
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
                    InputString("Endpoint:", "http://localhost:9090/", false),
                    InputString("API Key:", "default", false));

                if (!_Sdk.Loopback().Result)
                {
                    Console.WriteLine("Unable to connect to specified endpoint using specified API key");
                    return;
                }

                #endregion

                #region Process

                while (_RunForever)
                {
                    string cmd = InputString("Komodo [? for help]:", null, false);
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
                        case "create":
                            CreateIndex();
                            break;
                        case "delete":
                            DeleteIndex();
                            break;
                        case "add":
                            AddDocument();
                            break;
                        case "store":
                            StoreDocument();
                            break;
                        case "get source":
                            GetSourceDocument();
                            break;
                        case "get parsed":
                            GetDocumentMetadata();
                            break;
                        case "delete doc":
                            DeleteDocument();
                            break;
                        case "search":
                            Search();
                            break;
                        case "enum":
                            Enumerate();
                            break;
                        case "stats":
                            Stats();
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
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░" + Environment.NewLine +
                "░░░█░█░█▀▀█░█▀▄▀█░█▀▀█░█▀▀▄░█▀▀█░░░" + Environment.NewLine +
                "░░░█▀▄░█░░█░█░▀░█░█░░█░█░░█░█░░█░░░" + Environment.NewLine +
                "░░░▀░▀░▀▀▀▀░▀░░░▀░▀▀▀▀░▀▀▀░░▀▀▀▀░░░" + Environment.NewLine +
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░" + Environment.NewLine +
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
            Console.WriteLine(" create        create an index");
            Console.WriteLine(" delete        delete an index");
            Console.WriteLine(" add           add a document to an index and store it");
            Console.WriteLine(" store         store a document without indexing");
            Console.WriteLine(" get source    retrieve source document from an index");
            Console.WriteLine(" get parsed    retrieve parsed document from an index");
            Console.WriteLine(" delete doc    delete a document from an index");
            Console.WriteLine(" search        search an index");
            Console.WriteLine(" enum          enumerate an index");
            Console.WriteLine(" stats         get statistics for an index");
            Console.WriteLine("");
        }

        static void ListIndices()
        {
            List<string> indices = _Sdk.GetIndices().Result;
            if (indices != null && indices.Count > 0)
            {
                foreach (string curr in indices)
                {
                    Console.WriteLine("  " + curr);
                }
            }
        }

        static void CreateIndex()
        {
            string file = InputString("Filename:", "index.json", true);
            if (String.IsNullOrEmpty(file)) return;

            Index index = DeserializeJson<Index>(File.ReadAllBytes(file));

            _Sdk.CreateIndex(index).Wait();
            Console.WriteLine("Success");
        }

        static void DeleteIndex()
        {
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;
            bool cleanup = InputBoolean("Cleanup", true);

            _Sdk.DeleteIndex(indexName, cleanup).Wait();
            Console.WriteLine("Success");
        }

        static void AddDocument()
        {
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string sourceUrl = InputString("Source URL:", null, true);
            DocType docType = GetDocType();
            string title = InputString("Title:", null, true);
            string tagsStr = InputString("Tags CSV:", null, true);
            string sourceFile = InputString("Filename:", "order1.json", true);
            byte[] data = null;

            if (!String.IsNullOrEmpty(sourceFile)) data = File.ReadAllBytes(sourceFile);

            List<string> tags = new List<string>();
            if (!String.IsNullOrEmpty(tagsStr)) tags = Common.CsvToStringList(tagsStr);
            IndexResult resp = _Sdk.AddDocument(indexName, sourceUrl, title, tags, docType, data).Result;
            if (resp != null) Console.WriteLine(SerializeJson(resp, true));
        }

        static void StoreDocument()
        {
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string sourceUrl = InputString("Source URL:", null, true);
            DocType docType = GetDocType();
            string title = InputString("Title:", null, true);
            string tagsStr = InputString("Tags CSV:", null, true);
            string sourceFile = InputString("Filename:", "order1.json", true);
            byte[] data = null;

            if (!String.IsNullOrEmpty(sourceFile)) data = File.ReadAllBytes(sourceFile);

            List<string> tags = new List<string>();
            if (!String.IsNullOrEmpty(tagsStr)) tags = Common.CsvToStringList(tagsStr);

            IndexResult resp = _Sdk.StoreDocument(indexName, sourceUrl, title, tags, docType, data).Result;
            if (resp != null) Console.WriteLine(SerializeJson(resp, true));
        }

        static void GetSourceDocument()
        {
            // GetSourceDocument(string indexName, string docId, out byte[] data)
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string docId = InputString("Document ID:", null, true);
            if (String.IsNullOrEmpty(docId)) return;

            DocumentData obj = _Sdk.GetSourceDocument(indexName, docId).Result;
            if (obj != null)
            {
                Console.WriteLine("[" + obj.ContentLength + " bytes, content type " + obj.ContentType + "]:");
                Console.WriteLine(Encoding.UTF8.GetString(obj.Data));
            }
        }

        static void GetDocumentMetadata()
        {
            // GetParsedDocument(string indexName, string docId, out IndexedDoc doc)
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string docId = InputString("Document ID:", null, true);
            if (String.IsNullOrEmpty(docId)) return;

            DocumentMetadata md = _Sdk.GetDocumentMetadata(indexName, docId).Result;
            if (md != null) Console.WriteLine(SerializeJson(md, true));
        }

        static void DeleteDocument()
        {
            // DeleteDocument(string indexName, string docId)
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string docId = InputString("Document ID:", null, true);
            if (String.IsNullOrEmpty(docId)) return;

            _Sdk.DeleteDocument(indexName, docId).Wait();
            Console.WriteLine("Success");
        }

        static void Search()
        {
            // Search(string indexName, SearchQuery query, out SearchResult result)
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string filename = InputString("Search filename:", "query1.json", true);
            if (String.IsNullOrEmpty(filename)) return;

            SearchQuery query = DeserializeJson<SearchQuery>(File.ReadAllBytes(filename));
            SearchResult result = _Sdk.Search(indexName, query).Result;
            if (result != null) Console.WriteLine(SerializeJson(result, true));
        }

        static void Enumerate()
        {
            // Search(string indexName, SearchQuery query, out SearchResult result)
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            string filename = InputString("Enumeration filename:", "enum1.json", true);
            if (String.IsNullOrEmpty(filename)) return;

            EnumerationQuery query = DeserializeJson<EnumerationQuery>(File.ReadAllBytes(filename));
            EnumerationResult result = _Sdk.Enumerate(indexName, query).Result;
            if (result != null) Console.WriteLine(SerializeJson(result, true));
        }

        static void Stats()
        {
            // Search(string indexName, SearchQuery query, out SearchResult result)
            string indexName = InputString("Index name:", null, true);
            if (String.IsNullOrEmpty(indexName)) return;

            IndexStats stats = _Sdk.GetIndexStats(indexName).Result;
            if (stats != null) Console.WriteLine(SerializeJson(stats, true));
        }

        static DocType GetDocType()
        {
            while (true)
            {
                string docType = InputString("Document type [json/html/text/xml/sql]:", "json", false);
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

        static string InputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

        static bool InputBoolean(string question, bool yesDefault)
        {
            Console.Write(question);

            if (yesDefault) Console.Write(" [Y/n]? ");
            else Console.Write(" [y/N]? ");

            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput))
            {
                if (yesDefault) return true;
                return false;
            }

            userInput = userInput.ToLower();

            if (yesDefault)
            {
                if (
                    (String.Compare(userInput, "n") == 0)
                    || (String.Compare(userInput, "no") == 0)
                   )
                {
                    return false;
                }

                return true;
            }
            else
            {
                if (
                    (String.Compare(userInput, "y") == 0)
                    || (String.Compare(userInput, "yes") == 0)
                   )
                {
                    return true;
                }

                return false;
            }
        }

        static string SerializeJson(object obj, bool pretty)
        {
            if (obj == null) return null;
            string json;

            if (pretty)
            {
                json = JsonConvert.SerializeObject(
                  obj,
                  Newtonsoft.Json.Formatting.Indented,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                  });
            }
            else
            {
                json = JsonConvert.SerializeObject(obj,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc
                  });
            }

            return json;
        }

        static T DeserializeJson<T>(string json)
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine("Exception while deserializing:");
                Console.WriteLine(json);
                Console.WriteLine("");
                throw e;
            }
        }

        static T DeserializeJson<T>(byte[] data)
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
            return DeserializeJson<T>(Encoding.UTF8.GetString(data));
        }

        static byte[] StreamToBytes(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new InvalidOperationException("Input stream is not readable");

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
