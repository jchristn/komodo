using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlobHelper;
using DatabaseWrapper;
using SqliteWrapper;
using SyslogLogging;
using RestWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Komodo.Core.Enums;
using Komodo.Core.Database;

namespace Komodo.Core
{
    /// <summary>
    /// The client for interacting with the Index.
    /// </summary>
    public class IndexClient : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Maximum results to be returned by a search.
        /// </summary>
        public int MaxResults
        {
            get
            {
                return _MaxResults;
            }
            set
            {
                if (value > 1000) throw new ArgumentException("Max results must be one thousand or less.");
                if (value < 1) throw new ArgumentException("Max results must be at least one.");
                _MaxResults = value;
            }
        }

        #endregion

        #region Private-Members

        private bool _Destroying = false;
        private int _MaxResults = 1000; 
        private Index _Index;
        private LoggingModule _Logging;

        private string _RootDirectory;

        private DatabaseWrapper.DatabaseClient _DocsDbSql = null;
        private SqliteWrapper.DatabaseClient _DocsDbSqlite = null;

        private DatabaseWrapper.DatabaseClient _PostingsDbSql = null;
        private SqliteWrapper.DatabaseClient _PostingsDbSqlite = null;

        private IndexQueries _IndexQueries = null;
        private PostingsQueries _PostingsQueries = null;

        private Blobs _BlobSource;
        private Blobs _BlobParsed; 

        private PostingsManager _Postings;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the Index.
        /// </summary>
        public IndexClient()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Instantiate the Index.
        /// </summary>
        /// <param name="index">Index object.</param> 
        /// <param name="logging">Logging module.</param>
        public IndexClient(Index index, LoggingModule logging)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (String.IsNullOrEmpty(index.RootDirectory)) throw new ArgumentException("Index does not contain a root directory.");
            if (String.IsNullOrEmpty(index.IndexName)) throw new ArgumentException("Index does not contain a name.");
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            if (index.DocumentsDatabase == null) throw new ArgumentException("Index does not contain documents database settings.");
            if (index.PostingsDatabase == null) throw new ArgumentException("Index does not contain postings database settings.");
            if (index.StorageParsed == null) throw new ArgumentException("Index does not contain storage configuration for parsed documents.");
            if (index.StorageSource == null) throw new ArgumentException("Index does not contain storage configuration for source documents.");

            DateTime ts = DateTime.Now;

            _Index = index;
            Name = index.IndexName;

            _Logging = logging;
            _RootDirectory = index.RootDirectory;
             
            _Logging.Info("IndexClient creating directories for index " + Name);
            CreateDirectories();

            _Logging.Info("IndexClient initializing database clients for index " + Name);
            InitializeDatabases();

            _Logging.Info("IndexClient initializing storage client for index " + Name);
            InitializeBlobManager();

            _Logging.Info("IndexClient initializing postings manager for index " + Name);
            InitializePostingsManager();

            _Logging.Info("IndexClient started for index " + Name + " [" + Common.TotalMsFrom(ts) + "ms]");
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Tear down the index client and release resources.
        /// </summary>
        public void Dispose()
        {
            if (_Postings != null)
            {
                _Postings.Dispose();
                _Postings = null;
            }

            if (_DocsDbSqlite != null)
            {
                _DocsDbSqlite.Dispose();
                _DocsDbSqlite = null;
            }

            if (_DocsDbSql != null)
            {
                _DocsDbSql.Dispose();
                _DocsDbSql = null;
            } 

            if (_PostingsDbSqlite != null)
            {
                _PostingsDbSqlite.Dispose();
                _PostingsDbSqlite = null;
            }

            if (_PostingsDbSql != null)
            {
                _PostingsDbSql.Dispose();
                _PostingsDbSql = null;
            }

            if (_BlobSource != null)
            {
                _BlobSource = null;
            }

            if (_BlobParsed != null)
            {
                _BlobParsed = null;
            }
        }

        /// <summary>
        /// Delete all data associated with the index.
        /// </summary>
        public void Destroy()
        {
            _Destroying = true;

            #region Gather-Records

            DataTable sourceResult = null;
            DataTable parsedResult = null;

            if (_DocsDbSql != null)
            {
                #region Sql

                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression(
                    new DatabaseWrapper.Expression("Id", DatabaseWrapper.Operators.GreaterThan, 0),
                    DatabaseWrapper.Operators.And,
                    new DatabaseWrapper.Expression("IndexName", DatabaseWrapper.Operators.Equals, _Index.IndexName));

                sourceResult = _DocsDbSql.Select("SourceDocuments", null, null, null, e, null);
                _DocsDbSql.Delete("SourceDocuments", e); 

                parsedResult = _DocsDbSql.Select("ParsedDocuments", null, null, null, e, null);
                _DocsDbSql.Delete("ParsedDocuments", e);

                _DocsDbSql.Delete("Terms", e);

                #endregion
            }
            else
            {
                #region Sqlite

                SqliteWrapper.Expression e = new SqliteWrapper.Expression(
                    new SqliteWrapper.Expression("Id", SqliteWrapper.Operators.GreaterThan, 0),
                    SqliteWrapper.Operators.And,
                    new SqliteWrapper.Expression("IndexName", SqliteWrapper.Operators.Equals, _Index.IndexName));

                sourceResult = _DocsDbSqlite.Select("SourceDocuments", null, null, null, e, null);
                _DocsDbSqlite.Delete("SourceDocuments", e);
                 
                parsedResult = _DocsDbSqlite.Select("ParsedDocuments", null, null, null, e, null);
                _DocsDbSqlite.Delete("ParsedDocuments", e);
                 
                _DocsDbSqlite.Delete("Terms", e);

                #endregion
            }

            #endregion

            #region Delete-Records

            if (sourceResult != null && sourceResult.Rows.Count > 0)
            {
                foreach (DataRow currRow in sourceResult.Rows)
                {
                    string documentId = currRow["DocumentId"].ToString();
                    DeleteSourceDocument(documentId).Wait();
                }
            }

            if (parsedResult != null && parsedResult.Rows.Count > 0)
            {
                foreach (DataRow currRow in parsedResult.Rows)
                {
                    string documentId = currRow["DocumentId"].ToString();
                    DeleteParsedDocument(documentId).Wait();
                }
            } 

            #endregion
        }

        /// <summary>
        /// Parse a document and add to the index.
        /// </summary>
        /// <param name="docType">The type of document.</param>
        /// <param name="filename">The file to add to the index and store.</param>
        /// <param name="sourceUrl">The URL from which the content should be retrieved.</param>
        /// <param name="name">The name of the document.</param>
        /// <param name="tags">Document tags in a comma-separated list.</param>
        /// <param name="contentType">Content type of the document.</param>
        /// <param name="title">Document title.</param>
        /// <param name="storeOnly">Indicate if the document should be indexed and stored (false) or only stored (true).</param>
        /// <returns>Index result.</returns>
        public IndexResult AddDocument(
            DocType docType, 
            string filename,
            string sourceUrl, 
            string name,
            string tags,
            string contentType,
            string title,
            bool storeOnly)
        {
            IndexResult result = new IndexResult();
            bool cleanupRequired = false; 

            try
            {
                #region Check-Input

                if (_Destroying)
                {
                    _Logging.Warn("[" + Name + "] AddDocument index is being destroyed");
                    result.Error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                    return result;
                }

                if (String.IsNullOrEmpty(filename))
                {
                    _Logging.Warn("[" + Name + "] AddDocument source filename not supplied");
                    result.Error = new ErrorCode(ErrorId.MISSING_PARAMS, "Source filename not supplied.");
                    return result;
                }
                  
                if (!File.Exists(filename))
                {
                    _Logging.Warn("[" + Name + "] AddDocument source filename " + filename + " does not exist");
                    result.Error = new ErrorCode(ErrorId.READ_ERROR, "Source file does not exist.");
                    return result;
                }

                #endregion

                #region Parse-and-Create-IndexedDoc

                if (!storeOnly)
                {
                    result.Document = GenerateIndexedDoc(docType, filename, sourceUrl);
                    if (result.Document == null)
                    {
                        _Logging.Warn("[" + Name + "] AddDocument unable to parse source data");
                        result.Error = new ErrorCode(ErrorId.PARSE_ERROR, sourceUrl);
                        return result;
                    }
                    result.DocumentId = result.Document.DocumentId;
                }
                else
                {
                    result.Document = null;
                    result.DocumentId = Guid.NewGuid().ToString();
                }

                #endregion

                #region Calculate-MD5-and-Length

                string md5 = null;
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    md5 = Common.Md5(fs);
                }

                long contentLength = new FileInfo(filename).Length;

                #endregion

                #region Add-to-Database 

                SourceDocument sourceDoc = new SourceDocument();
                sourceDoc.IndexName = _Index.IndexName;
                sourceDoc.DocumentId = result.DocumentId;
                sourceDoc.Name = name;
                sourceDoc.Title = title;
                sourceDoc.Tags = tags;
                sourceDoc.SourceUrl = sourceUrl;
                sourceDoc.ContentType = contentType;
                sourceDoc.ContentLength = contentLength;
                sourceDoc.Md5 = md5;
                sourceDoc.DocumentType = docType;
                sourceDoc.Created = DateTime.Now.ToUniversalTime();
                sourceDoc.Indexed = sourceDoc.Created;

                Dictionary<string, object> sourceDocVals = sourceDoc.ToInsertDictionary();
                if (_DocsDbSql != null) _DocsDbSql.Insert("SourceDocuments", sourceDocVals);
                else _DocsDbSqlite.Insert("SourceDocuments", sourceDocVals);

                if (!storeOnly)
                {
                    DateTime parseStart = DateTime.Now;

                    ParsedDocument parsedDoc = new ParsedDocument();
                    parsedDoc.IndexName = _Index.IndexName;
                    parsedDoc.DocumentId = result.DocumentId;
                    parsedDoc.DocumentType = docType;
                    parsedDoc.SourceContentLength = contentLength;
                    parsedDoc.ContentLength = Common.SerializeJson(result.Document, false).Length;
                    parsedDoc.Created = DateTime.Now.ToUniversalTime();

                    Dictionary<string, object> parsedDocVals = parsedDoc.ToInsertDictionary();
                    if (_DocsDbSql != null) _DocsDbSql.Insert("ParsedDocuments", parsedDocVals);
                    else _DocsDbSqlite.Insert("ParsedDocuments", parsedDocVals);

                    result.ParseTimeMs = (DateTime.Now - parseStart).TotalMilliseconds;

                    DateTime postingsStart = DateTime.Now;

                    if (result.Document.Terms != null && result.Document.Terms.Count > 0)
                    {
                        _Logging.Debug("[" + Name + "] document ID " + result.Document.DocumentId + " contains " + result.Document.Postings.Count + " postings");

                        foreach (Posting p in result.Document.Postings)
                        {
                            if (!_Postings.AddPosting(p))
                            {
                                _Logging.Warn(
                                    "[" + Name + "] " +
                                    "AddDocument unable to add posting for term " +
                                    p.Term + " in document ID " + result.Document.DocumentId);
                            }
                        }
                    }

                    result.PostingsTimeMs = (DateTime.Now - postingsStart).TotalMilliseconds;
                }

                #endregion

                #region Add-to-Filesystem

                DateTime storageStart = DateTime.Now;

                WriteSourceDocument(filename, result.Document).Wait();

                if (!storeOnly)
                {
                    WriteParsedDocument(result.Document).Wait();
                }

                result.StorageTimeMs = (DateTime.Now - storageStart).TotalMilliseconds;

                #endregion

                result.MarkFinished();
                return result;
            }
            finally
            {
                #region Cleanup

                if (cleanupRequired && !String.IsNullOrEmpty(result.DocumentId)) 
                {
                    _Logging.Info("[" + Name + "] AddDocument starting cleanup due to failed add operation");

                    if (_DocsDbSql != null)
                    {
                        DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("DocumentId", DatabaseWrapper.Operators.Equals, result.DocumentId);
                        _DocsDbSql.Delete("SourceDocuments", e);
                        _DocsDbSql.Delete("ParsedDocuments", e); 
                    }
                    else
                    {
                        SqliteWrapper.Expression e = new SqliteWrapper.Expression("DocumentId", SqliteWrapper.Operators.Equals, result.DocumentId);
                        _DocsDbSqlite.Delete("SourceDocuments", e);
                        _DocsDbSqlite.Delete("ParsedDocuments", e); 
                    }

                    if (result.Document.Terms != null && result.Document.Terms.Count > 0)
                    {
                        _Postings.RemoveDocument(result.DocumentId, result.Document.Terms);
                    }
                }

                #endregion
            }
        }
         
        /// <summary>
        /// Check if a document exists in the index.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>True if the document exists.</returns>
        public bool DocumentExists(string documentId)
        {
            if (String.IsNullOrEmpty(documentId)) throw new ArgumentNullException(nameof(documentId));

            if (_Destroying)
            {
                _Logging.Warn("[" + Name + "] DocumentExists index is being destroyed");
                return false;
            }

            DataTable result = null;

            if (_DocsDbSql != null)
            {
                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("DocumentId", DatabaseWrapper.Operators.Equals, documentId);
                result = _DocsDbSql.Select("SourceDocuments", null, null, null, e, null);
            }
            else
            {
                SqliteWrapper.Expression e = new SqliteWrapper.Expression("DocumentId", SqliteWrapper.Operators.Equals, documentId);
                result = _DocsDbSqlite.Select("SourceDocuments", null, null, null, e, null);
            }

            if (result != null && result.Rows.Count > 0) return true;
            return false;
        }

        /// <summary>
        /// Delete a document from the index.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param> 
        /// <returns>True if successful.</returns>
        public bool DeleteDocument(string documentId)
        {
            if (String.IsNullOrEmpty(documentId)) throw new ArgumentNullException(nameof(documentId));

            if (_Destroying)
            {
                _Logging.Warn("[" + Name + "] DeleteDocument index is being destroyed");
                return false;
            }

            _Logging.Info("[" + Name + "] DeleteDocument starting deletion of doc ID " + documentId);

            IndexedDoc doc = GetParsedDocument(documentId).Result;

            if (_DocsDbSql != null)
            {
                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("DocumentId", DatabaseWrapper.Operators.Equals, documentId);
                _DocsDbSql.Delete("SourceDocuments", e);
                _DocsDbSql.Delete("ParsedDocuments", e); 
            }
            else
            {
                SqliteWrapper.Expression e = new SqliteWrapper.Expression("DocumentId", SqliteWrapper.Operators.Equals, documentId);
                _DocsDbSqlite.Delete("SourceDocuments", e);
                _DocsDbSqlite.Delete("ParsedDocuments", e); 
            }

            bool deleteSource = DeleteSourceDocument(documentId).Result;
            bool deleteParsed = DeleteParsedDocument(documentId).Result;
            bool deletePostings = _Postings.RemoveDocument(doc.DocumentId, doc.Terms);

            if (!deleteSource || !deleteParsed || !deletePostings) return false;
            _Logging.Info("[" + Name + "] DeleteDocument successfully deleted doc ID " + documentId);

            return true;
        }

        /// <summary>
        /// Retrieve the source document entry from the database.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param> 
        /// <returns>Source document.</returns>
        public SourceDocument GetSourceDocument(string documentId)
        {
            if (String.IsNullOrEmpty(documentId)) throw new ArgumentNullException(nameof(documentId));

            DataTable result = null;

            if (_DocsDbSql != null)
            {
                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("DocumentId", DatabaseWrapper.Operators.Equals, documentId);
                result = _DocsDbSql.Select("SourceDocuments", null, null, null, e, null);
            }
            else
            {
                SqliteWrapper.Expression e = new SqliteWrapper.Expression("DocumentId", SqliteWrapper.Operators.Equals, documentId);
                result = _DocsDbSqlite.Select("SourceDocuments", null, null, null, e, null);
            }

            if (result != null && result.Rows.Count > 0)
            {
                return SourceDocument.FromDataRow(result.Rows[0]);
            }

            return null;
        }

        /// <summary>
        /// Retrieve a series of source documents using a list of document IDs.
        /// </summary>
        /// <param name="documentIds">List of document IDs.</param>
        /// <returns>List of source documents.</returns>
        public List<SourceDocument> GetSourceDocuments(List<string> documentIds)
        {
            // Implement and then update the end of SearchInternal to use this rather
            // than retrieving each source document individually to update doctype

            if (documentIds == null || documentIds.Count < 1) return new List<SourceDocument>();

            DataTable result = null;

            if (_DocsDbSql != null)
            {
                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("DocumentId", DatabaseWrapper.Operators.In, documentIds);
                result = _DocsDbSql.Select("SourceDocuments", null, null, null, e, null);
            }
            else
            {
                SqliteWrapper.Expression e = new SqliteWrapper.Expression("DocumentId", SqliteWrapper.Operators.In, documentIds);
                result = _DocsDbSqlite.Select("SourceDocuments", null, null, null, e, null);
            }

            List<SourceDocument> ret = new List<SourceDocument>();

            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow curr in result.Rows)
                {
                    ret.Add(SourceDocument.FromDataRow(curr));
                }
            } 

            return ret;
        }

        /// <summary>
        /// Retrieve a source document by ID from storage.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>Komodo object.</returns>
        public async Task<KomodoObject> GetSourceDocumentData(string documentId)
        {
            if (String.IsNullOrEmpty(documentId)) throw new ArgumentNullException(nameof(documentId));
            return await ReadSourceDocument(documentId);
        }

        /// <summary>
        /// Retrieve a parsed document by ID from storage.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>Indexed document.</returns>
        public async Task<IndexedDoc> GetParsedDocument(string documentId)
        {
            if (String.IsNullOrEmpty(documentId)) throw new ArgumentNullException(nameof(documentId));
            return await ReadParsedDocument(documentId);
        }
         
        /// <summary>
        /// Search the index.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>Search result.</returns>
        public SearchResult Search(SearchQuery query)
        {
            SearchResult result = new SearchResult(query); 

            #region Setup

            if (_Destroying)
            {
                _Logging.Warn("[" + Name + "] Search index is being destroyed");
                result.Error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return result;
            }

            if (query == null)
            {
                _Logging.Warn("[" + Name + "] Search query not supplied");
                result.Error = new ErrorCode(ErrorId.MISSING_PARAMS, "Query");
                return result;
            }

            if (query.Required == null)
            {
                _Logging.Warn("[" + Name + "] Search required filter not supplied");
                result.Error = new ErrorCode(ErrorId.MISSING_PARAMS, "Required Filter");
                return result;
            }

            if (query.Required.Terms == null || query.Required.Terms.Count < 1)
            {
                _Logging.Warn("[" + Name + "] Search required terms not supplied");
                result.Error = new ErrorCode(ErrorId.MISSING_PARAMS, "Required Terms");
                return result;
            }
              
            if (query.Optional == null || query.Optional.Terms == null)
            {
                query.Optional = new QueryFilter();
                query.Optional.Terms = new List<string>();
            }

            if (query.Exclude == null || query.Exclude.Terms == null)
            {
                query.Exclude = new QueryFilter();
                query.Exclude.Terms = new List<string>();
            }

            if (query.StartIndex < 0) query.StartIndex = 0;

            if (query.MaxResults > _MaxResults) query.MaxResults = _MaxResults;
            if (query.MaxResults < 1) query.MaxResults = 1;

            #endregion

            #region Process

            if (!String.IsNullOrEmpty(query.PostbackUrl))
            {
                _Logging.Debug("[" + Name + "] Search starting async search with POSTback to " + query.PostbackUrl);
                Task.Run(() => SearchTaskWrapper(query));

                result = new SearchResult(query);
                result.Async = true;
                result.IndexName = Name; 
                return result;
            }
            else
            { 
                return SearchInternal(query);
            }

            #endregion
        }

        /// <summary>
        /// Retrieve statistics for the index.
        /// </summary>
        /// <returns>Index statistics.</returns>
        public IndexStats GetIndexStats()
        {
            IndexStats stats = new IndexStats();
            stats.SourceDocuments = new IndexStats.SourceDocumentStats();
            stats.SourceDocuments.Count = 0;
            stats.SourceDocuments.SizeBytes = 0;
            
            stats.ParsedDocuments = new IndexStats.ParsedDocumentStats();
            stats.ParsedDocuments.Count = 0;
            stats.ParsedDocuments.SizeBytesParsed = 0;
            stats.ParsedDocuments.SizeBytesSource = 0;

            stats.IndexName = _Index.IndexName;
            stats.TermsCount = _Postings.GetTermsCount();

            #region Source-Docs-Stats

            string sourceDocsQuery =
                "SELECT " +
                "  COUNT(*) AS NumDocs, " +
                "  SUM(ContentLength) AS SizeBytes " +
                "FROM SourceDocuments";

            DataTable sourceDocsResult = null;
            if (_DocsDbSql != null) sourceDocsResult = _DocsDbSql.Query(sourceDocsQuery);
            else sourceDocsResult = _DocsDbSqlite.Query(sourceDocsQuery);
            
            if (sourceDocsResult != null && sourceDocsResult.Rows.Count > 0)
            {
                foreach (DataRow currRow in sourceDocsResult.Rows)
                {
                    if (currRow["NumDocs"] != null && currRow["NumDocs"] != DBNull.Value)
                        stats.SourceDocuments.Count = Convert.ToInt64(currRow["NumDocs"]);
                    if (currRow["SizeBytes"] != null && currRow["SizeBytes"] != DBNull.Value)
                        stats.SourceDocuments.SizeBytes = Convert.ToInt64(currRow["SizeBytes"]);
                }
            }

            #endregion

            #region Parsed-Docs-Stats

            string parsedDocsQuery = 
                "SELECT " +
                "  COUNT(*) AS NumDocs, " +
                "  SUM(SourceContentLength) AS SizeBytesSource, " +
                "  SUM(ContentLength) AS SizeBytesParsed " +
                "FROM ParsedDocuments";

            DataTable parsedDocsResult = null;
            if (_DocsDbSql != null) parsedDocsResult = _DocsDbSql.Query(parsedDocsQuery);
            else parsedDocsResult = _DocsDbSqlite.Query(parsedDocsQuery);
             
            if (parsedDocsResult != null && parsedDocsResult.Rows.Count > 0)
            {
                foreach (DataRow currRow in parsedDocsResult.Rows)
                {
                    if (currRow["NumDocs"] != null && currRow["NumDocs"] != DBNull.Value)
                        stats.ParsedDocuments.Count = Convert.ToInt64(currRow["NumDocs"]);
                    if (currRow["SizeBytesSource"] != null && currRow["SizeBytesSource"] != DBNull.Value)
                        stats.ParsedDocuments.SizeBytesSource = Convert.ToInt64(currRow["SizeBytesSource"]);
                    if (currRow["SizeBytesParsed"] != null && currRow["SizeBytesParsed"] != DBNull.Value)
                        stats.ParsedDocuments.SizeBytesParsed = Convert.ToInt64(currRow["SizeBytesParsed"]);
                }
            }

            #endregion

            return stats;
        }

        /// <summary>
        /// Enumerate source documents held by the index.
        /// </summary>
        /// <param name="query">The query to execute.</param> 
        /// <returns>Enumeration result.</returns>
        public EnumerationResult Enumerate(EnumerationQuery query)
        {
            EnumerationResult result = new EnumerationResult(query);

            #region Check-Input

            if (_Destroying)
            {
                _Logging.Warn("[" + Name + "] Enumerate index is being destroyed");
                result.Error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return result;
            }

            if (query == null)
            {
                _Logging.Warn("[" + Name + "] Enumerate query not supplied");
                result.Error = new ErrorCode(ErrorId.MISSING_PARAMS, "Query");
                return result;
            }

            if (query.MaxResults == null || query.MaxResults > 5000) query.MaxResults = 5000;

            #endregion

            #region Process

            if (!String.IsNullOrEmpty(query.PostbackUrl))
            {
                _Logging.Debug("[" + Name + "] Enumeration starting async search with POSTback to " + query.PostbackUrl);
                Task.Run(() => EnumerationTask(query, result));
                 
                result.Async = true; 
                return result;
            }
            else
            {
                return EnumerationInternal(query, result);
            }

            #endregion
        }

        #endregion

        #region Private-Methods
         
        #region Initialization

        private void CreateDirectories()
        { 
            if (!Directory.Exists(_RootDirectory))
            {
                if (!Common.CreateDirectory(_RootDirectory)) throw new IOException("Unable to create index directory.");
            }

            if (_Index.StorageSource.Disk != null)
            {
                if (!Directory.Exists(_Index.StorageSource.Disk.Directory))
                {
                    if (!Common.CreateDirectory(_Index.StorageSource.Disk.Directory))
                        throw new IOException("Unable to create source documents directory.");
                }
            }

            if (_Index.StorageParsed.Disk != null)
            {
                if (!Directory.Exists(_Index.StorageParsed.Disk.Directory))
                {
                    if (!Common.CreateDirectory(_Index.StorageParsed.Disk.Directory))
                        throw new IOException("Unable to create parsed documents directory.");
                }
            } 
        }

        private void InitializeDatabases()
        {
            #region Documents-Database

            #region Initialize-Database-Client

            switch (_Index.DocumentsDatabase.Type)
            {
                case DatabaseType.MsSql:
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    _DocsDbSql = new DatabaseWrapper.DatabaseClient(
                        DatabaseTypeToString(_Index.DocumentsDatabase.Type),
                        _Index.DocumentsDatabase.Hostname,
                        _Index.DocumentsDatabase.Port,
                        _Index.DocumentsDatabase.Username,
                        _Index.DocumentsDatabase.Password,
                        _Index.DocumentsDatabase.InstanceName,
                        _Index.DocumentsDatabase.DatabaseName);
                    _DocsDbSql.DebugRawQuery = _Index.DocumentsDatabase.Debug;
                    _DocsDbSql.DebugResultRowCount = _Index.DocumentsDatabase.Debug;
                    break;

                case DatabaseType.SQLite:
                    _DocsDbSqlite = new SqliteWrapper.DatabaseClient(
                        _Index.DocumentsDatabase.Filename,
                        _Index.DocumentsDatabase.Debug);
                    break;

                default:
                    throw new ArgumentException("Unknown documents database type, use one of: Mssql, Mysql, Pgsql, Sqlite.");
            }

            #endregion

            #region Initialize-Tables

            _IndexQueries = new IndexQueries(_Index, _DocsDbSql, _DocsDbSqlite);

            string sourceDocsQuery = _IndexQueries.CreateSourceDocsTable();
            string parsedDocsQuery = _IndexQueries.CreateParsedDocsTable();

            switch (_Index.DocumentsDatabase.Type)
            {
                case DatabaseType.MsSql:
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    _DocsDbSql.Query(sourceDocsQuery);
                    _DocsDbSql.Query(parsedDocsQuery);
                    break;

                case DatabaseType.SQLite:
                    _DocsDbSqlite.Query(sourceDocsQuery);
                    _DocsDbSqlite.Query(parsedDocsQuery);
                    break;
            }

            #endregion

            #endregion

            #region Postings-Database

            #region Initialize-Database-Client

            switch (_Index.PostingsDatabase.Type)
            {
                case DatabaseType.MsSql:
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    _PostingsDbSql = new DatabaseWrapper.DatabaseClient(
                        DatabaseTypeToString(_Index.PostingsDatabase.Type),
                        _Index.PostingsDatabase.Hostname,
                        _Index.PostingsDatabase.Port,
                        _Index.PostingsDatabase.Username,
                        _Index.PostingsDatabase.Password,
                        _Index.PostingsDatabase.InstanceName,
                        _Index.PostingsDatabase.DatabaseName);
                    _PostingsDbSql.DebugRawQuery = _Index.PostingsDatabase.Debug;
                    _PostingsDbSql.DebugResultRowCount = _Index.PostingsDatabase.Debug;
                    break;

                case DatabaseType.SQLite:
                    _PostingsDbSqlite = new SqliteWrapper.DatabaseClient(
                        _Index.PostingsDatabase.Filename,
                        _Index.PostingsDatabase.Debug);
                    break;

                default:
                    throw new ArgumentException("Unknown postings database type, use one of: Mssql, Mysql, Pgsql, Sqlite.");
            }

            #endregion

            #region Initialize-Tables

            _PostingsQueries = new PostingsQueries(_Index, _PostingsDbSql, _PostingsDbSqlite);

            string termsTableQuery = _PostingsQueries.CreateTermsMapTable();

            switch (_Index.DocumentsDatabase.Type)
            {
                case DatabaseType.MsSql:
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    _PostingsDbSql.Query(termsTableQuery); 
                    break;

                case DatabaseType.SQLite:
                    _PostingsDbSqlite.Query(termsTableQuery); 
                    break;
            }

            #endregion 

            #endregion
        }

        private string DatabaseTypeToString(DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.MsSql:
                    return "mssql";
                case DatabaseType.MySql:
                    return "mysql";
                case DatabaseType.PgSql:
                    return "pgsql";
                case DatabaseType.SQLite:
                    return "sqlite";
            }

            throw new ArgumentException("Unknown database type, use one of: Mssql, Mysql, Pgsql, Sqlite.");
        }
         
        private void InitializeBlobManager()
        { 
            switch (_Index.StorageSource.Type)
            {
                case StorageType.AwsS3:
                    _BlobSource = new Blobs(_Index.StorageSource.AwsS3);
                    break;
                case StorageType.Azure:
                    _BlobSource = new Blobs(_Index.StorageSource.Azure);
                    break;
                case StorageType.Disk:
                    _BlobSource = new Blobs(_Index.StorageSource.Disk);
                    break;
                case StorageType.Kvpbase:
                    _BlobSource = new Blobs(_Index.StorageSource.Kvpbase);
                    break;
                default:
                    throw new ArgumentException("Unknown storage type in index " + _Index.IndexName);
            }

            switch (_Index.StorageParsed.Type)
            {
                case StorageType.AwsS3:
                    _BlobParsed = new Blobs(_Index.StorageParsed.AwsS3);
                    break;
                case StorageType.Azure:
                    _BlobParsed = new Blobs(_Index.StorageParsed.Azure);
                    break;
                case StorageType.Disk:
                    _BlobParsed = new Blobs(_Index.StorageParsed.Disk);
                    break;
                case StorageType.Kvpbase:
                    _BlobParsed = new Blobs(_Index.StorageParsed.Kvpbase);
                    break;
                default:
                    throw new ArgumentException("Unknown storage type in index " + _Index.IndexName);
            } 
        }
         
        private void InitializePostingsManager()
        {
            _Postings = new PostingsManager(
                _Index,
                _Logging, 
                _PostingsDbSql,
                _PostingsDbSqlite
                );
        }

        #endregion

        #region Support

        private string Sanitize(string str)
        {
            if (String.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));
            if (_DocsDbSql != null) return _DocsDbSql.SanitizeString(str);
            else return SqliteWrapper.DatabaseClient.SanitizeString(str);
        }
         
        private IndexedDoc GenerateIndexedDoc(DocType docType, string filename, string sourceUrl)
        {
            IndexedDoc doc = null;

            byte[] sourceData = File.ReadAllBytes(filename);

            switch (docType)
            {
                case DocType.Html:
                    ParsedHtml html = new ParsedHtml();
                    html.LoadBytes(sourceData, sourceUrl);
                    doc = IndexedDoc.FromHtml(html, _Index.Options);
                    break;
                case DocType.Json:
                    ParsedJson json = new ParsedJson();
                    json.LoadBytes(sourceData, sourceUrl);
                    doc = IndexedDoc.FromJson(json, _Index.Options);
                    break;
                case DocType.Text:
                    ParsedText text = new ParsedText();
                    text.LoadBytes(sourceData, sourceUrl);
                    doc = IndexedDoc.FromText(text, _Index.Options);
                    break;
                case DocType.Xml:
                    ParsedXml xml = new ParsedXml();
                    xml.LoadBytes(sourceData, sourceUrl);
                    doc = IndexedDoc.FromXml(xml, _Index.Options);
                    break;
                default:
                    throw new ArgumentException("Unknown DocType");
            } 

            return doc;
        }

        #endregion

        #region Source-Documents

        private async Task WriteSourceDocument(string filename, IndexedDoc doc)
        {
            string targetFilename = doc.DocumentId + ".source"; 
            long contentLength = new FileInfo(filename).Length; 
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                await _BlobSource.Write(targetFilename, null, contentLength, fs);
            }
        }
         
        private async Task<bool> DeleteSourceDocument(string documentId)
        {
            string filename = documentId + ".source";
            return await _BlobSource.Delete(filename);
        }

        private async Task<KomodoObject> ReadSourceDocument(string documentId)
        {
            string filename = documentId + ".source";

            SourceDocument sourceDoc = GetSourceDocument(documentId);
            if (sourceDoc == null) return null;

            BlobData blob = await _BlobSource.GetStream(filename);
            if (blob == null) return null;

            KomodoObject ret = new KomodoObject(_Index.IndexName, documentId, sourceDoc.ContentType, blob.ContentLength, blob.Data);
            return ret;
        }

        #endregion

        #region Parsed-Documents

        private async Task WriteParsedDocument(IndexedDoc doc)
        {
            string filename = doc.DocumentId + ".parsed.json";
            await _BlobParsed.Write(filename, "application/json", Encoding.UTF8.GetBytes(Common.SerializeJson(doc, false)));
        }
        
        private async Task<bool> DeleteParsedDocument(string documentId)
        {
            string filename = documentId + ".parsed.json";
            return await _BlobParsed.Delete(filename);
        }

        private async Task<IndexedDoc> ReadParsedDocument(string documentId)
        {
            byte[] data; 
            string filename = documentId + ".parsed.json";

            data = await _BlobParsed.Get(filename);
            return Common.DeserializeJson<IndexedDoc>(data);
        }

        #endregion

        #region Filter-Match

        private bool DocMatchesFilters(IndexedDoc doc, SearchQuery query, out decimal filterScore)
        {
            filterScore = 1m;
            
            #region Process
             
            if (!RequiredFiltersMatch(doc, query))
            {
                _Logging.Debug("IndexClient " + Name + " DocMatchesFilters document ID " + doc.DocumentId + " does not match required filters");
                return false;
            }
            else
            {
                _Logging.Debug("IndexClient " + Name + " DocMatchesFilters document ID " + doc.DocumentId + " matches required filters");
            }

            if (!ExcludeFiltersMatch(doc, query))
            {
                _Logging.Debug("IndexClient " + Name + " DocMatchesFilters document ID " + doc.DocumentId + " one or more exclude filters matched");
                return false;
            }
            else
            {
                _Logging.Debug("IndexClient " + Name + " DocMatchesFilters document ID " + doc.DocumentId + " matches exclude filters");
            }

            if (!OptionalFiltersMatch(doc, query, out filterScore))
            {
                _Logging.Debug("IndexClient " + Name + " DocMatchesFilters document ID " + doc.DocumentId + " does not match optional filters");
                return false;
            }
            else
            {
                filterScore = Convert.ToDecimal(filterScore.ToString("N4"));
                _Logging.Debug("IndexClient " + Name + " DocMatchesFilters document ID " + doc.DocumentId + " matches optional filters [score " + filterScore + "]");
            }

            return true;

            #endregion
        }

        private bool RequiredFiltersMatch(IndexedDoc doc, SearchQuery query)
        {
            if (query.Required == null || query.Required.Filter == null || query.Required.Filter.Count < 1) return true;
            if (doc == null) return false;
            if (doc.Text != null || doc.Html != null) return true;  // not appropriate searches

            List<DataNode> nodes = DataNodesFromIndexedDoc(doc);
            if (nodes == null || nodes.Count < 1)
            {
                _Logging.Warn("[" + Name + "] RequiredFiltersMatch document ID " + doc.DocumentId + " has no data nodes");
                return false;
            }

            bool result = false;

            foreach (SearchFilter currFilter in query.Required.Filter)
            {
                if (String.IsNullOrEmpty(currFilter.Field))
                {
                    _Logging.Warn("[" + Name + "] RequiredFiltersMatch null key supplied in filter");
                    continue;
                }

                foreach (DataNode currNode in nodes)
                {
                    if (currNode.Key.Equals(currFilter.Field))
                    {
                        if (FilterMatch(currFilter, currNode)) result = true;
                        break;
                    }
                }
            }

            return result;
        }

        private bool OptionalFiltersMatch(IndexedDoc doc, SearchQuery query, out decimal filterScore)
        {
            filterScore = 1m;
            if (query.Optional == null || query.Optional.Filter == null || query.Optional.Filter.Count < 1)
            {
                _Logging.Debug("[" + Name + "] OptionalFiltersMatch no optional filters found");
                return true;
            }

            if (doc.Text != null || doc.Html != null)
            {
                _Logging.Debug("[" + Name + "] OptionalFiltersMatch document type is text or HTML, skipping");
                return true;  // not appropriate searches
            }

            int filterCount = query.Optional.Filter.Count;
            int matchCount = 0;

            List<DataNode> nodes = DataNodesFromIndexedDoc(doc);
            if (nodes == null || nodes.Count < 1)
            {
                _Logging.Warn("[" + Name + "] OptionalFiltersMatch document ID " + doc.DocumentId + " has no data nodes");
                return false;
            }
             
            foreach (SearchFilter currFilter in query.Required.Filter)
            {
                if (String.IsNullOrEmpty(currFilter.Field)) continue;
                bool matchFound = false;

                foreach (DataNode currNode in nodes)
                {
                    if (currNode.Key.Equals(currFilter.Field))
                    {
                        matchFound = FilterMatch(currFilter, currNode);
                    }
                }

                if (matchFound)
                {
                    if (matchCount < filterCount) matchCount++;
                }
            }

            _Logging.Debug("[" + Name + "] OptionalFiltersMatch document ID " + doc.DocumentId + " [" + filterCount + " filters, " + matchCount + " matches: " + filterScore + " score]");
            if (matchCount > 0 && filterCount > 0) filterScore = (decimal)matchCount / filterCount;
            return true;
        }

        private bool ExcludeFiltersMatch(IndexedDoc doc, SearchQuery query)
        {
            if (query.Exclude == null || query.Exclude.Filter == null || query.Exclude.Filter.Count < 1) return true;
            if (doc == null) return false;
            if (doc.Text != null || doc.Html != null) return true;  // not appropriate searches

            List<DataNode> nodes = DataNodesFromIndexedDoc(doc);
            if (nodes == null || nodes.Count < 1)
            {
                _Logging.Warn("[" + Name + "] RequiredFiltersMatch document ID " + doc.DocumentId + " has no data nodes");
                return false;
            }

            foreach (SearchFilter currFilter in query.Required.Filter)
            {
                if (String.IsNullOrEmpty(currFilter.Field))
                {
                    _Logging.Warn("[" + Name + "] RequiredFiltersMatch null key supplied in filter");
                    continue;
                }

                foreach (DataNode currNode in nodes)
                {
                    if (currNode.Key.Equals(currFilter.Field))
                    {
                        if (FilterMatch(currFilter, currNode)) return false;
                    }
                }
            }

            return true;
        }

        private List<DataNode> DataNodesFromIndexedDoc(IndexedDoc doc)
        {
            if (doc == null) return null;
            if (doc.Text != null || doc.Html != null) return null;
            if (doc.Json != null && doc.Json.Flattened != null) return doc.Json.Flattened;
            if (doc.Sql != null && doc.Sql.Flattened != null) return doc.Sql.Flattened;
            if (doc.Xml != null && doc.Xml.Flattened != null) return doc.Xml.Flattened;
            return null;
        }
         
        private bool FilterMatch(SearchFilter filter, DataNode node)
        {
            if (filter == null) return false;
            if (node == null) return false;
            if (String.IsNullOrEmpty(filter.Field)) return false;
            if (String.IsNullOrEmpty(node.Key)) return false;

            string dataString = null;

            decimal filterDecimal = 0m;
            decimal dataDecimal = 0m;

            switch (filter.Condition)
            {
                case SearchCondition.Equals:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return false;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    dataString = node.Data.ToString();
                    if (filter.Value.Equals(dataString)) return true;
                    return false;

                case SearchCondition.NotEquals:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return true;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    dataString = node.Data.ToString();
                    if (!filter.Value.Equals(dataString)) return true;
                    return false;

                case SearchCondition.GreaterThan:
                    if (!Decimal.TryParse(filter.Value, out filterDecimal)) return false;
                    if (node.Data == null) return false;
                    if (!Decimal.TryParse(node.Data.ToString(), out dataDecimal)) return false;
                    if (dataDecimal > filterDecimal) return true;
                    return false;

                case SearchCondition.GreaterThanOrEqualTo:
                    if (!Decimal.TryParse(filter.Value, out filterDecimal)) return false;
                    if (node.Data == null) return false;
                    if (!Decimal.TryParse(node.Data.ToString(), out dataDecimal)) return false;
                    if (dataDecimal >= filterDecimal) return true;
                    return false;

                case SearchCondition.LessThan:
                    if (!Decimal.TryParse(filter.Value, out filterDecimal)) return false;
                    if (node.Data == null) return false;
                    if (!Decimal.TryParse(node.Data.ToString(), out dataDecimal)) return false;
                    if (dataDecimal < filterDecimal) return true;
                    return false;

                case SearchCondition.LessThanOrEqualTo:
                    if (!Decimal.TryParse(filter.Value, out filterDecimal)) return false;
                    if (node.Data == null) return false;
                    if (!Decimal.TryParse(node.Data.ToString(), out dataDecimal)) return false;
                    if (dataDecimal <= filterDecimal) return true;
                    return false;

                case SearchCondition.IsNull:
                    if (node.Data == null) return true;
                    return false;

                case SearchCondition.IsNotNull:
                    if (node.Data != null) return true;
                    return false;

                case SearchCondition.Contains:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return false;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    dataString = node.Data.ToString();
                    if (dataString.Contains(filter.Value)) return true;
                    return false;

                case SearchCondition.ContainsNot:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return true;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    dataString = node.Data.ToString();
                    if (dataString.Contains(filter.Value)) return false;
                    return true; 

                case SearchCondition.StartsWith:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return false;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    dataString = node.Data.ToString();
                    if (dataString.StartsWith(filter.Value)) return true;
                    return false;

                case SearchCondition.EndsWith:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return false;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    dataString = node.Data.ToString();
                    if (dataString.EndsWith(filter.Value)) return true;
                    return false;

                default:
                    _Logging.Warn("[" + Name + "] FilterMatch unknown condition: " + filter.Condition.ToString());
                    return false;
            } 
        }

        #endregion

        #region Search

        private void SearchTaskWrapper(SearchQuery query)
        {
            SearchResult result = SearchInternal(query);
            byte[] data = Encoding.UTF8.GetBytes(Common.SerializeJson(result, true));

            RestRequest req = new RestRequest(
                query.PostbackUrl,
                HttpMethod.POST,
                null,
                "application/json");
             
            RestResponse resp = req.Send(data); 
            if (resp == null)
            {
                _Logging.Warn("[" + Name + "] SearchTaskWrapper no response from POSTback URL " + query.PostbackUrl);
                return;
            }
            else
            {
                _Logging.Debug("[" + Name + "] SearchTaskWrapper " + resp.StatusCode + " response from POSTback URL " + query.PostbackUrl);
                return;
            } 
        }

        private SearchResult SearchInternal(SearchQuery query)
        { 
            SearchResult result = new SearchResult(query);
            result.IndexName = _Index.IndexName;
            result.MatchCount = new MatchCounts();

            try
            {
                #region Variables

                Dictionary<string, decimal> docsMatching = new Dictionary<string, decimal>();
                Dictionary<string, decimal> currDocsMatching = new Dictionary<string, decimal>();
                List<string> termsNotFound = new List<string>();

                #endregion

                #region Process

                int startIndex = query.StartIndex;
                if (startIndex < 0) startIndex = 0;

                while (docsMatching.Count < query.MaxResults)
                {
                    bool endOfSearch = false;

                    #region Process-Terms

                    while (true)
                    {
                        DateTime startTime = DateTime.Now;
                        _Logging.Debug("[" + Name + "] SearchInternal calling GetMatchingDocuments [index " + query.StartIndex + " max " + query.MaxResults + "]");

                        List<string> currTermsNotFound = new List<string>();

                        _Postings.GetMatchingDocuments(
                            startIndex,
                            query.MaxResults,
                            query.Required.Terms,
                            query.Optional.Terms,
                            query.Exclude.Terms,
                            out currTermsNotFound,
                            out currDocsMatching,
                            out result.NextStartIndex);

                        _Logging.Debug("[" + Name + "] SearchInternal returned from GetMatchingDocuments after " + Common.TotalMsFrom(startTime) + "ms [index " + query.StartIndex + " max results " + query.MaxResults + " next " + result.NextStartIndex + "]");

                        startIndex = result.NextStartIndex;

                        if (currTermsNotFound != null && currTermsNotFound.Count > 0)
                        {
                            foreach (string curr in currTermsNotFound)
                            {
                                termsNotFound.Add(curr);
                            } 
                        }

                        if (currDocsMatching == null || currDocsMatching.Count < 1)
                        {
                            // no more documents
                            endOfSearch = true;
                            break; 
                        }
                        else
                        {
                            result.MatchCount.TermsMatch += currDocsMatching.Count;
                        }

                        foreach (KeyValuePair<string, decimal> curr in currDocsMatching)
                        {
                            if (!docsMatching.ContainsKey(curr.Key)) docsMatching.Add(curr.Key, curr.Value);
                            _Logging.Debug("[" + Name + "] SearchInternal adding document " + curr.Key);
                        }

                        if (currDocsMatching.Count >= query.MaxResults)
                        {
                            _Logging.Debug("[" + Name + "] SearchInternal potential results " + currDocsMatching.Count + " exceeds max results " + query.MaxResults);
                            break;
                        }
                    }

                    if (endOfSearch)
                    {
                        // no more documents
                        _Logging.Debug("[" + Name + "] SearchInternal reached end of search results");
                        break;
                    }
                    else
                    {
                        // increment limit and offset
                        result.NextStartIndex += query.MaxResults;
                    }

                    #endregion

                    #region Process-Filters

                    if ((query.Required.Filter != null && query.Required.Filter.Count > 0)
                        || (query.Optional.Filter != null && query.Optional.Filter.Count > 0)
                        || (query.Exclude.Filter != null && query.Exclude.Filter.Count > 0))
                    {
                        foreach (KeyValuePair<string, decimal> curr in currDocsMatching)
                        {
                            IndexedDoc currParsedDoc = ReadParsedDocument(curr.Key).Result;
                            decimal filterScore = 1m;
                            if (!DocMatchesFilters(currParsedDoc, query, out filterScore))
                            {
                                _Logging.Debug("[" + Name + "] SearchInternal document ID " + curr.Key + " does not match filters, removing");
                                if (docsMatching.ContainsKey(curr.Key)) docsMatching.Remove(curr.Key);
                                currDocsMatching.Remove(curr.Key);
                                continue;
                            }
                            else
                            {
                                result.MatchCount.FilterMatch = result.MatchCount.FilterMatch + 1;
                                docsMatching[curr.Key] = (curr.Value + filterScore) / 2;
                            }
                        }
                    }

                    #endregion
                }

                if (docsMatching.Count > query.MaxResults)
                {
                    docsMatching = docsMatching.Take(query.MaxResults).ToDictionary(pair => pair.Key, pair => pair.Value);
                }

                #endregion

                #region Append-Documents

                if (docsMatching != null && docsMatching.Count > 0)
                {
                    List<SourceDocument> sourceDocs = GetSourceDocuments(new List<string>(docsMatching.Keys));

                    foreach (KeyValuePair<string, decimal> curr in docsMatching)
                    {
                        MatchingDocument currDoc = new MatchingDocument();
                        currDoc.DocumentId = curr.Key;
                        currDoc.Score = Convert.ToDecimal(curr.Value.ToString("N4"));

                        if (sourceDocs.Exists(d => d.DocumentId.Equals(curr.Key)))
                        {
                            currDoc.DocumentType = sourceDocs.Where(d => d.DocumentId.Equals(curr.Key)).FirstOrDefault().DocumentType;
                        }
                        
                        _Logging.Debug("[" + Name + "] SearchInternal appended doc ID " + curr.Key + " to result");
                        result.Documents.Add(currDoc);
                    } 
                }

                #endregion

                result.TermsNotFound = termsNotFound.Distinct().ToList();
                result.SortMatchesByScore();
                result.MarkFinished();
                return result;
            }
            finally
            { 
                _Logging.Debug("[" + Name + "] SearchInternal finished (" + result.TotalTimeMs + "ms)");
            }
        }

        #endregion

        #region Enumeration

        private async Task EnumerationTask(EnumerationQuery query, EnumerationResult result)
        {
            EnumerationResult final = EnumerationInternal(query, result);
            
            RestRequest req = new RestRequest(
                query.PostbackUrl,
                HttpMethod.POST,
                null,
                "application/json");

            RestResponse resp = await req.SendAsync(Common.SerializeJson(result, true));
            if (resp == null)
            {
                _Logging.Warn("[" + Name + "] EnumerationTask no response from POSTback URL " + query.PostbackUrl);
                return;
            }
            else
            {
                _Logging.Debug("[" + Name + "] EnumerationTask " + resp.StatusCode + " response from POSTback URL " + query.PostbackUrl);
                return;
            }
        }

        private EnumerationResult EnumerationInternal(EnumerationQuery query, EnumerationResult result)
        {
            result.GUID = query.GUID;

            try
            { 
                #region Execute-Query

                string dbQuery = _IndexQueries.SelectSourceDocumentsByEnumerationQuery(query);
                DataTable dbResult = null;
                if (_DocsDbSql != null) dbResult = _DocsDbSql.Query(dbQuery);
                else dbResult = _DocsDbSqlite.Query(dbQuery);
                 
                foreach (DataRow row in dbResult.Rows)
                {
                    result.Matches.Add(SourceDocument.FromDataRow(row));
                }

                #endregion

                return result;
            }
            finally
            {
                result.MarkEnded();
                _Logging.Debug("[" + Name + "] SearchInternal finished (" + result.TotalTimeMs + "ms)");
            }
        }

        #endregion

        #endregion
    }
}
