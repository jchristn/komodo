using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BlobHelper;
using Komodo;
using Komodo.IndexClient;
using Komodo.IndexManager;
using Komodo.MetadataManager;
using Index = Komodo.Index;

namespace Test.MetadataManager
{
    class Program
    {
        static DbSettings _Database = new DbSettings("./index.db");
        static Index _Index1 = null;
        static Index _Index2 = null;
        static KomodoIndices _Indices = null;
        static KomodoIndex _IndexClient1 = null;
        static KomodoIndex _IndexClient2 = null;
        static Blobs _Blobs = null;
        static MetadataPolicy _Policy = null;
        static MetadataProcessor _Metadata = null;
        static MetadataResult _Result1 = null;
        static MetadataResult _Result2 = null;
        static MetadataResult _Result3 = null;

        static void Main(string[] args)
        {
            #region Index-Manager

            Console.WriteLine("Initializing index manager");
            _Indices = new KomodoIndices(
                new DbSettings("./indices.db"),
                new StorageSettings(new DiskSettings("./source/")),
                new StorageSettings(new DiskSettings("./parsed/")),
                new StorageSettings(new DiskSettings("./postings/")));

            #endregion

            #region Indices

            Console.WriteLine("Initializing indices");
            _Index1 = new Index("default", "default", "default");
            _Index2 = new Index("metadata", "default", "metadata");
            _Indices.Add(_Index1);
            _Indices.Add(_Index2);

            #endregion

            #region Adding-Documents

            _IndexClient1 = _Indices.GetIndexClient("default");
            _IndexClient2 = _Indices.GetIndexClient("metadata");

            byte[] doc1 = File.ReadAllBytes("person1.json");
            SourceDocument sd1 = new SourceDocument(
                "default",
                "default",
                "Person 1",
                "Person 1",
                null,
                DocType.Json,
                null,
                "application/json",
                doc1.Length,
                Common.Md5(doc1));

            byte[] doc2 = File.ReadAllBytes("person2.json");
            SourceDocument sd2 = new SourceDocument(
                "default",
                "default",
                "Person 2",
                "Person 2",
                null,
                DocType.Json,
                null,
                "application/json",
                doc2.Length,
                Common.Md5(doc2));

            byte[] doc3 = File.ReadAllBytes("person3.json");
            SourceDocument sd3 = new SourceDocument(
                "default",
                "default",
                "Person 3",
                "Person 3",
                null,
                DocType.Json,
                null,
                "application/json",
                doc3.Length,
                Common.Md5(doc3));

            IndexResult r1 = _IndexClient1.Add(sd1, doc1, true, new PostingsOptions()).Result;
            IndexResult r2 = _IndexClient1.Add(sd2, doc2, true, new PostingsOptions()).Result;
            IndexResult r3 = _IndexClient1.Add(sd3, doc3, true, new PostingsOptions()).Result;

            #endregion

            #region Blobs

            _Blobs = new Blobs(new DiskSettings("./Metadata/"));
            if (!Directory.Exists("./Metadata/")) Directory.CreateDirectory("./Metadata/");

            #endregion

            #region Policy
             
            byte[] bytes = File.ReadAllBytes("./policy.json"); 
            _Policy = Common.DeserializeJson<MetadataPolicy>(File.ReadAllBytes("./policy.json")); 

            #endregion

            #region Initialize-Metadata

            Console.WriteLine("Initializing metadata processor");

            _Metadata = new MetadataProcessor(_Policy, _Indices);

            #endregion

            #region Apply-Metadata

            Console.WriteLine("Processing metadata");

            _Result1 = _Metadata.ProcessDocument(
                r1.SourceDocument,
                r1.ParsedDocument,
                r1.ParseResult).Result;

            // Console.WriteLine("Document 1: " + Environment.NewLine + Common.SerializeJson(_Result1, true));

            _Result2 = _Metadata.ProcessDocument(
                r2.SourceDocument,
                r2.ParsedDocument,
                r2.ParseResult).Result;

            // Console.WriteLine("Document 2: " + Environment.NewLine + Common.SerializeJson(_Result2, true));

            _Result3 = _Metadata.ProcessDocument(
                r3.SourceDocument,
                r3.ParsedDocument,
                r3.ParseResult).Result;

            Console.WriteLine("Document 3: " + Environment.NewLine + Common.SerializeJson(_Result3, true));
             
            #endregion
        }
    }
}
