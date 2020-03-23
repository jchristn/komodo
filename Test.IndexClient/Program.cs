using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlobHelper;
using Komodo.Classes; 
using Komodo.IndexClient;
using Komodo.Postings;
using EnumerationResult = Komodo.Classes.EnumerationResult;

namespace Test.IndexClient
{
    class Program
    {
        static StorageSettings _SourceDocs = null;
        static StorageSettings _ParsedDocs = null;
        static StorageSettings _Postings = null;
        static DatabaseSettings _DatabaseSettings = null;
        static Komodo.Classes.Index _Index = null;
        static KomodoIndex _IndexClient = null;

        static void Main(string[] args)
        {
            try
            {
                _DatabaseSettings = new DatabaseSettings("test.db");
                _SourceDocs = new StorageSettings(new DiskSettings("./SourceDocs/"));
                _ParsedDocs = new StorageSettings(new DiskSettings("./ParsedDocs/"));
                _Postings = new StorageSettings(new DiskSettings("./Postings/"));
                _Index = new Komodo.Classes.Index("test", "test", "test");
                _IndexClient = new KomodoIndex(_DatabaseSettings, _SourceDocs, _ParsedDocs, _Postings, _Index);

                Console.WriteLine("Test cases");
                Console.WriteLine("  1     Basic indexing and queries");
                Console.WriteLine("  2     Basic indexing and enumeration");
                Console.WriteLine("");
                string testCase = Common.InputString("Selection:", "1", false);
                switch (testCase)
                {
                    case "1":
                        TestCase1();
                        break;

                    case "2":
                        TestCase2();
                        break;

                    default:
                        Console.WriteLine("Unknown test case.");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:" + Environment.NewLine + Common.SerializeJson(e, true));
            }
        }

        static void TestCase1()
        {
            List<string> docsToIndex = DocumentsToIndex("./TestCases/1/");
            List<string> queriesToProcess = QueriesToProcess("./TestCases/1/");
            PostingsOptions options = new PostingsOptions();

            // load documents
            foreach (string curr in docsToIndex)
            {
                byte[] data = Common.ReadBinaryFile(curr);
                SourceDocument src = new SourceDocument("test", "test", curr, curr, null, DocType.Json, null, "application/json", data.Length, Common.Md5(data));
                IndexResult result = _IndexClient.Add(src, data, true, options).Result;
                Console.WriteLine("");
                Console.WriteLine("Add: " + curr);
                Console.WriteLine(Common.SerializeJson(result, true));
            }

            Console.WriteLine("");
            Console.WriteLine("Press ENTER to continue");
            Console.ReadLine();

            // execute queries
            foreach (string curr in queriesToProcess)
            {
                SearchQuery query = Common.DeserializeJson<SearchQuery>(Common.ReadBinaryFile(curr));
                SearchResult result = _IndexClient.Search(query);
                Console.WriteLine("");
                Console.WriteLine("Query: " + curr);
                Console.WriteLine(Common.SerializeJson(result, true));
            }
        }

        static void TestCase2()
        {
            List<string> docsToIndex = DocumentsToIndex("./TestCases/2/");
            List<string> queriesToProcess = QueriesToProcess("./TestCases/2/");
            PostingsOptions options = new PostingsOptions();

            // load documents
            foreach (string curr in docsToIndex)
            {
                byte[] data = Common.ReadBinaryFile(curr);
                SourceDocument src = new SourceDocument("test", "test", curr, curr, null, DocType.Json, null, "application/json", data.Length, Common.Md5(data));
                IndexResult result = _IndexClient.Add(src, data, true, options).Result;
                Console.WriteLine("");
                Console.WriteLine("Add: " + curr);
                Console.WriteLine(Common.SerializeJson(result, true));
            }

            Console.WriteLine("");
            Console.WriteLine("Press ENTER to continue");
            Console.ReadLine();

            // execute queries
            foreach (string curr in queriesToProcess)
            {
                EnumerationQuery query = Common.DeserializeJson<EnumerationQuery>(Common.ReadBinaryFile(curr));
                EnumerationResult result = _IndexClient.Enumerate(query);
                Console.WriteLine("");
                Console.WriteLine("Query: " + curr);
                Console.WriteLine(Common.SerializeJson(result, true));
            }
        }

        static List<string> DocumentsToIndex(string baseDirectory)
        {
            // returns full path, i.e. if baseDirectory is /TestCases/1/, it will return /TestCases/1/index*.*
            return Directory.EnumerateFiles(baseDirectory, "index*.*").ToList();
        }

        static List<string> QueriesToProcess(string baseDirectory)
        {
            // returns full path, i.e. if baseDirectory is /TestCases/1/, it will return /TestCases/1/query*.*
            return Directory.EnumerateFiles(baseDirectory, "query*.*").ToList();
        }
    }
}
