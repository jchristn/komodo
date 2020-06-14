using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using SyslogLogging;

using Komodo.Classes;
using Komodo.Crawler;
using Komodo.IndexClient;
using Komodo.IndexManager;

using Index = Komodo.Classes.Index;

namespace Komodo.Daemon
{
    /// <summary>
    /// Komodo daemon; in-process search, storage, and retrieval.
    /// </summary>
    public class KomodoDaemon : IDisposable
    {
        #region Public-Members
         
        public DaemonSettings Settings
        {
            get
            {
                return _Settings;
            }
        }

        #endregion

        #region Private-Members

        private string _Header = "[Komodo] ";
        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private CancellationToken _Token;

        private DaemonSettings _Settings = null;
        private LoggingModule _Logging = null;
        private KomodoIndices _Indices = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate Komodo daemon using default settings, files, and directories.
        /// </summary>
        public KomodoDaemon()
        {
            _Settings = DaemonSettings.Default();
            Initialize(); 
        }

        /// <summary>
        /// Instantiate Komodo daemon using your own settings.
        /// </summary>
        /// <param name="settings">Settings.</param>
        public KomodoDaemon(DaemonSettings settings)
        {
            _Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Initialize();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Public-IndexManager-Methods

        /// <summary>
        /// Retrieve all index names.
        /// </summary>
        /// <returns>List of index names.</returns>
        public List<string> GetIndices()
        {
            List<string> ret = new List<string>();
            List<KomodoIndex> indices = _Indices.Get();
            if (indices != null)
            {
                foreach (KomodoIndex curr in indices)
                {
                    ret.Add(curr.Name);
                }
            }
            return ret;
        }

        /// <summary>
        /// Check if an index exists by name.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <returns>True if exists.</returns>
        public bool IndexExists(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            return _Indices.Exists(indexName);
        }

        /// <summary>
        /// Gather statistics for one or all indices.
        /// </summary>
        /// <param name="indexName">Optional name of the index.</param>
        /// <returns>Indices statistics.</returns>
        public IndicesStats GetIndexStats(string indexName)
        {
            return _Indices.Stats(indexName);
        }

        /// <summary>
        /// Add an index.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <returns>Index.</returns>
        public void AddIndex(Index index)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            _Indices.Add(index);
        }

        /// <summary>
        /// Remove an index, and optionally, destroy it.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="destroy">True if you wish to delete its data.</param>
        public async Task RemoveIndex(string indexName, bool destroy)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            await _Indices.Remove(indexName, destroy); 
        }

        #endregion

        #region Public-Crawler-Methods

        /// <summary>
        /// Crawl the specified file from the filesystem.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Result.</returns>
        public FileCrawlResult CrawlFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            FileCrawler fc = new FileCrawler(filename);
            FileCrawlResult fcr = fc.Get();
            return fcr;
        }

        /// <summary>
        /// Crawl the specified URL using HTTP.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <returns>Result.</returns>
        public HttpCrawlResult CrawlWebpage(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            HttpCrawler hc = new HttpCrawler(url);
            HttpCrawlResult hcr = hc.Get();
            return hcr;
        }

        /// <summary>
        /// Crawl the specified URL using FTP.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <returns>Result.</returns>
        public FtpCrawlResult CrawlFtp(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            FtpCrawler fc = new FtpCrawler(url);
            FtpCrawlResult fcr = fc.Get();
            return fcr;
        }

        #endregion

        #region Public-IndexClient-Methods

        /// <summary>
        /// Check if a source document exists by GUID.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>True if exists.</returns>
        public bool SourceDocumentExists(string indexName, string sourceGuid)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            KomodoIndex idx = GetIndexClient(indexName); 
            return idx.ExistsSource(sourceGuid);
        }

        /// <summary>
        /// Retrieve source document content.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>DocumentContent.</returns>
        public async Task<DocumentData> GetSourceDocumentContent(string indexName, string sourceGuid)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName)); 
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            KomodoIndex idx = GetIndexClient(indexName); 
            return await idx.GetSourceDocumentContent(sourceGuid); 
        }

        /// <summary>
        /// Retrieve source document metadata.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>SourceDocument.</returns>
        public SourceDocument GetSourceDocumentMetadata(string indexName, string sourceGuid)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            KomodoIndex idx = GetIndexClient(indexName); 
            return idx.GetSourceDocumentMetadata(sourceGuid);
        }

        /// <summary>
        /// Check if a parsed document exists by the source document GUID.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>True if exists.</returns>
        public bool ParsedDocumentExists(string indexName, string sourceGuid)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            KomodoIndex idx = GetIndexClient(indexName); 
            return idx.ExistsParsed(sourceGuid);
        }

        /// <summary>
        /// Retrieve a parsed document by the source document GUID.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>ParsedDocument.</returns>
        public ParsedDocument GetParsedDocumentMetadata(string indexName, string sourceGuid)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            KomodoIndex idx = GetIndexClient(indexName); 
            return idx.GetParsedDocument(sourceGuid);
        }

        /// <summary>
        /// Retrieve parse result by source document GUID.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>Parse result.</returns>
        public object GetDocumentParseResult(string indexName, string sourceGuid)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            KomodoIndex idx = GetIndexClient(indexName); 
            return idx.GetParseResult(sourceGuid);
        }

        /// <summary>
        /// Retrieve postings by source document GUID.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>Postings.</returns>
        public PostingsResult GetDocumentPostings(string indexName, string sourceGuid)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            KomodoIndex idx = GetIndexClient(indexName);
            PostingsResult result = idx.GetPostings(sourceGuid); 
            return result;
        }

        /// <summary>
        /// Store or store-and-index a document.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceDoc">Source document.</param>
        /// <param name="data">Byte data.</param>
        /// <param name="parse">True if the document should be parsed and indexed.</param>
        /// <param name="options">Postings options.</param> 
        /// <returns>Index result.</returns>
        public async Task<IndexResult> AddDocument(string indexName, SourceDocument sourceDoc, byte[] data, bool parse, PostingsOptions options)
        {
            return await AddDocument(indexName, sourceDoc, data, parse, options, null);
        }

        /// <summary>
        /// Store or store-and-index a document.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceDoc">Source document.</param>
        /// <param name="data">Byte data.</param>
        /// <param name="parse">True if the document should be parsed and indexed.</param>
        /// <param name="options">Postings options.</param>
        /// <param name="postbackUrl">URL to which results should be POSTed.</param>
        /// <returns>Index result.</returns>
        public async Task<IndexResult> AddDocument(string indexName, SourceDocument sourceDoc, byte[] data, bool parse, PostingsOptions options, string postbackUrl)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));  
            KomodoIndex idx = GetIndexClient(indexName);
            return await idx.Add(sourceDoc, data, parse, options, postbackUrl);
        }

        /// <summary>
        /// Remove a document.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceGuid">Source document GUID.</param>
        public void RemoveDocument(string indexName, string sourceGuid)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            KomodoIndex idx = GetIndexClient(indexName); 
            idx.Remove(sourceGuid);
        }

        /// <summary>
        /// Search the index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">Search query.</param>
        /// <returns>Search result.</returns>
        public SearchResult Search(string indexName, SearchQuery query)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (query == null) throw new ArgumentNullException(nameof(query));
            KomodoIndex idx = GetIndexClient(indexName);
            return idx.Search(query);
        }

        /// <summary>
        /// Enumerate the index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">Enumeration query.</param>
        /// <returns>Enumeraiton result.</returns>
        public EnumerationResult Enumerate(string indexName, EnumerationQuery query)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (query == null) throw new ArgumentNullException(nameof(query));
            KomodoIndex idx = GetIndexClient(indexName);
            return idx.Enumerate(query);
        }
         
        #endregion

        #region Private-Methods

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        /// <param name="disposing">Indicate if child resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // background workers
                _TokenSource.Cancel();
                 
                if (_Indices != null) _Indices.Dispose();
            }
        }

        private void Initialize()
        {
            _Token = _TokenSource.Token;

            _Settings.PrepareFilesAndDirectories();
             
            _Indices = new KomodoIndices(
                _Settings.Database,
                _Settings.SourceDocuments,
                _Settings.ParsedDocuments,
                _Settings.Postings);

            _Logging = new LoggingModule(
                _Settings.Logging.SyslogServerIp,
                _Settings.Logging.SyslogServerPort,
                _Settings.Logging.ConsoleLogging,
                _Settings.Logging.MinimumLevel,
                false, false, true, false, false, false);
             
            if (_Settings.Logging.FileLogging && !String.IsNullOrEmpty(_Settings.Logging.Filename))
            {
                if (String.IsNullOrEmpty(_Settings.Logging.FileDirectory)) _Settings.Logging.FileDirectory = "./";
                while (_Settings.Logging.FileDirectory.Contains("\\")) _Settings.Logging.FileDirectory.Replace("\\", "/");
                if (!Directory.Exists(_Settings.Logging.FileDirectory)) Directory.CreateDirectory(_Settings.Logging.FileDirectory);

                _Logging.FileLogging = FileLoggingMode.FileWithDate;
                _Logging.LogFilename = _Settings.Logging.FileDirectory + _Settings.Logging.Filename;
            } 
        }

        private KomodoIndex GetIndexClient(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            KomodoIndex idx = _Indices.Get(indexName);

            if (idx == null)
            {
                _Logging.Warn(_Header + "index " + indexName + " could not be found");
                throw new KeyNotFoundException("No index with name '" + indexName + "' could be found.");
            }

            return idx;
        }

        #endregion
    }
}
