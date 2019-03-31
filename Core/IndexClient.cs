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

        #endregion

        #region Private-Members

        private bool _Disposed = false;
        private bool _Destroying = false;

        private Index _Index;
        private LoggingModule _Logging;

        private string _RootDirectory;

        private DatabaseWrapper.DatabaseClient _SqlDatabase = null;
        private SqliteWrapper.DatabaseClient _SqliteDatabase = null;
        private IndexQueries _IndexQueries = null;

        private Blobs _BlobSource;
        private Blobs _BlobParsed;

        private readonly object _DbLock;

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
        /// <param name="name">The name of the index.</param>
        /// <param name="rootDirectory">The root directory of the index.</param>
        /// <param name="dbDebug">Enable or disable database debugging.</param>
        /// <param name="indexOptions">Options for the index.</param>
        /// <param name="logging">Logging module.</param>
        public IndexClient(Index index, LoggingModule logging)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (String.IsNullOrEmpty(index.RootDirectory)) throw new ArgumentException("Index does not contain a root directory.");
            if (String.IsNullOrEmpty(index.IndexName)) throw new ArgumentException("Index does not contain a name.");
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            if (index.Database == null) throw new ArgumentException("Index does not contain database settings.");
            if (index.StorageParsed == null) throw new ArgumentException("Index does not contain storage configuration for parsed documents.");
            if (index.StorageSource == null) throw new ArgumentException("Index does not contain storage configuration for source documents.");

            _Index = index;
            Name = index.IndexName;

            _Logging = logging;
            _RootDirectory = index.RootDirectory;
            
            _DbLock = new object();

            CreateDirectories();
            InitializeDatabase();

            _IndexQueries = new IndexQueries(_Index, _SqlDatabase, _SqliteDatabase);
            InitializeDatabaseTables();
            InitializeBlobManager();

            _Logging.Log(LoggingModule.Severity.Info, "IndexClient started for index " + Name);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Tear down the index client and release resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Delete all data associated with the index.
        /// </summary>
        public void Destroy()
        {
            _Destroying = true;
             
            if (_SqlDatabase != null)
            {
                #region Not-Sqlite

                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression(
                    new DatabaseWrapper.Expression("Id", DatabaseWrapper.Operators.GreaterThan, 0),
                    DatabaseWrapper.Operators.And,
                    new DatabaseWrapper.Expression("IndexName", DatabaseWrapper.Operators.Equals, _Index.IndexName));

                DataTable sourceResult = _SqlDatabase.Select("SourceDocuments", null, null, null, e, null);
                _SqlDatabase.Delete("SourceDocuments", e);

                if (sourceResult != null && sourceResult.Rows.Count > 0)
                {
                    foreach (DataRow currRow in sourceResult.Rows)
                    {
                        string masterDocId = currRow["MasterDocId"].ToString();
                        DeleteSourceDocument(masterDocId);
                    }
                }

                DataTable parsedResult = _SqlDatabase.Select("ParsedDocuments", null, null, null, e, null);
                _SqlDatabase.Delete("ParsedDocuments", e);

                if (parsedResult != null && parsedResult.Rows.Count > 0)
                {
                    foreach (DataRow currRow in parsedResult.Rows)
                    {
                        string masterDocId = currRow["MasterDocId"].ToString();
                        DeleteParsedDocument(masterDocId);
                    }
                }

                _SqlDatabase.Delete("Terms", e);

                #endregion
            }
            else
            {
                #region Sqlite

                SqliteWrapper.Expression e = new SqliteWrapper.Expression(
                    new SqliteWrapper.Expression("Id", SqliteWrapper.Operators.GreaterThan, 0),
                    SqliteWrapper.Operators.And,
                    new SqliteWrapper.Expression("IndexName", SqliteWrapper.Operators.Equals, _Index.IndexName));

                DataTable sourceResult = _SqliteDatabase.Select("SourceDocuments", null, null, null, e, null);
                _SqliteDatabase.Delete("SourceDocuments", e);

                if (sourceResult != null && sourceResult.Rows.Count > 0)
                {
                    foreach (DataRow currRow in sourceResult.Rows)
                    {
                        string masterDocId = currRow["MasterDocId"].ToString();
                        DeleteSourceDocument(masterDocId);
                    }
                }

                DataTable parsedResult = _SqliteDatabase.Select("ParsedDocuments", null, null, null, e, null);
                _SqliteDatabase.Delete("ParsedDocuments", e);

                if (parsedResult != null && parsedResult.Rows.Count > 0)
                {
                    foreach (DataRow currRow in parsedResult.Rows)
                    {
                        string masterDocId = currRow["MasterDocId"].ToString();
                        DeleteParsedDocument(masterDocId);
                    }
                }

                _SqliteDatabase.Delete("Terms", e);

                #endregion
            } 
        }

        /// <summary>
        /// Add a document to the index.
        /// </summary>
        /// <param name="docType">The type of document.</param>
        /// <param name="sourceData">The source data from the document.</param>
        /// <param name="sourceUrl">The URL from which the content should be retrieved.</param>
        /// <param name="error">Error code associated with the operation.</param>
        /// <param name="masterDocId">Document ID of the added document.</param>
        /// <returns>True if successful.</returns>
        public bool AddDocument(DocType docType, byte[] sourceData, string sourceUrl, out ErrorCode error, out string masterDocId)
        {
            error = null;
            masterDocId = null;
            bool cleanupRequired = false;
            IndexedDoc doc = null;

            try
            {
                if (_Destroying)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " AddDocument index is being destroyed");
                    error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                    return false;
                }

                if ((sourceData == null || sourceData.Length < 1) && String.IsNullOrEmpty(sourceUrl))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " AddDocument source URL not supplied");
                    error = new ErrorCode(ErrorId.MISSING_PARAMS, "SourceUrl");
                    return false;
                }

                #region Retrieve

                if (!String.IsNullOrEmpty(sourceUrl))
                {
                    Crawler crawler = new Crawler(sourceUrl, docType);
                    byte[] data = crawler.RetrieveBytes();
                    if (data == null || data.Length < 1)
                    {
                        _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " AddDocument unable to retrieve data from source " + sourceUrl);
                        error = new ErrorCode(ErrorId.RETRIEVE_FAILED, sourceUrl);
                        return false;
                    }
                    sourceData = new byte[data.Length];
                    Buffer.BlockCopy(data, 0, sourceData, 0, data.Length);
                }

                #endregion

                #region Parse-and-Create-IndexedDoc

                doc = GenerateIndexedDoc(docType, sourceData, sourceUrl);
                if (doc == null)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " AddDocument unable to parse source data");
                    error = new ErrorCode(ErrorId.PARSE_ERROR, sourceUrl);
                    return false;
                }

                masterDocId = doc.MasterDocId;

                #endregion

                #region Add-to-Database 

                string ts = null;
                if (_SqlDatabase != null) ts = _SqlDatabase.Timestamp(DateTime.Now.ToUniversalTime());
                else ts = _SqliteDatabase.Timestamp(DateTime.Now.ToUniversalTime());

                // Source documents table
                Dictionary<string, object> sourceDocVals = new Dictionary<string, object>();
                sourceDocVals.Add("IndexName", _Index.IndexName);
                sourceDocVals.Add("MasterDocId", doc.MasterDocId);
                sourceDocVals.Add("SourceUrl", sourceUrl);
                sourceDocVals.Add("ContentLength", sourceData.Length);
                sourceDocVals.Add("DocType", docType.ToString());
                sourceDocVals.Add("Created", ts);

                if (_SqlDatabase != null) _SqlDatabase.Insert("SourceDocuments", sourceDocVals);
                else _SqliteDatabase.Insert("SourceDocuments", sourceDocVals);

                // Parsed documents table
                Dictionary<string, object> parsedDocVals = new Dictionary<string, object>();
                parsedDocVals.Add("IndexName", _Index.IndexName);
                parsedDocVals.Add("MasterDocId", doc.MasterDocId);
                parsedDocVals.Add("DocType", docType.ToString());
                parsedDocVals.Add("SourceContentLength", sourceData.Length);
                parsedDocVals.Add("ContentLength", Common.SerializeJson(doc, false).Length);
                parsedDocVals.Add("Created", ts);

                if (_SqlDatabase != null) _SqlDatabase.Insert("ParsedDocuments", parsedDocVals);
                else _SqliteDatabase.Insert("ParsedDocuments", parsedDocVals);

                if (doc.Terms != null && doc.Terms.Count > 0)
                {
                    foreach (string currTerm in doc.Terms)
                    {
                        Dictionary<string, object> termsVals = new Dictionary<string, object>();
                        termsVals.Add("IndexName", _Index.IndexName);
                        termsVals.Add("MasterDocId", doc.MasterDocId);
                        termsVals.Add("Term", currTerm);
                        termsVals.Add("Created", ts);

                        if (_SqlDatabase != null) _SqlDatabase.Insert("Terms", termsVals);
                        else _SqliteDatabase.Insert("Terms", termsVals);
                    }
                }

                #endregion

                #region Add-to-Filesystem

                if (!WriteSourceDocument(sourceData, doc)
                    || !WriteParsedDocument(doc))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " AddDocument unable to write source document");
                    error = new ErrorCode(ErrorId.WRITE_ERROR);
                    cleanupRequired = true;
                    return false;
                }

                #endregion

                return true;
            }
            finally
            {
                #region Cleanup

                if (cleanupRequired && doc != null)
                {
                    _Logging.Log(LoggingModule.Severity.Info, "Index " + Name + " AddDocument starting cleanup due to failed add operation");

                    if (_SqlDatabase != null)
                    {
                        DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("MasterDocId", DatabaseWrapper.Operators.Equals, doc.MasterDocId);
                        _SqlDatabase.Delete("SourceDocuments", e);
                        _SqlDatabase.Delete("ParsedDocuments", e);
                        _SqlDatabase.Delete("Terms", e); 
                    }
                    else
                    {
                        SqliteWrapper.Expression e = new SqliteWrapper.Expression("MasterDocId", SqliteWrapper.Operators.Equals, doc.MasterDocId);
                        _SqliteDatabase.Delete("SourceDocuments", e);
                        _SqliteDatabase.Delete("ParsedDocuments", e);
                        _SqliteDatabase.Delete("Terms", e);
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// Check if a document exists in the index.
        /// </summary>
        /// <param name="masterDocId">The ID of the document.</param>
        /// <param name="error">Error code associated with the operation.</param>
        /// <returns>True if the document exists.</returns>
        public bool DocumentExists(string masterDocId, out ErrorCode error)
        {
            error = null;

            if (_Destroying)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " DocumentExists index is being destroyed");
                error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return false;
            }

            if (String.IsNullOrEmpty(masterDocId))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " DocumentExists document ID not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "MasterDocId");
                return false;
            }

            DataTable result = null;

            if (_SqlDatabase != null)
            {
                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("MasterDocId", DatabaseWrapper.Operators.Equals, masterDocId);
                result = _SqlDatabase.Select("SourceDocuments", null, null, null, e, null);
            }
            else
            {
                SqliteWrapper.Expression e = new SqliteWrapper.Expression("MasterDocId", SqliteWrapper.Operators.Equals, masterDocId);
                result = _SqliteDatabase.Select("SourceDocuments", null, null, null, e, null);
            }

            if (result != null && result.Rows.Count > 0) return true;
            return false;
        }

        /// <summary>
        /// Delete a document from the index.
        /// </summary>
        /// <param name="masterDocId">The ID of the document.</param>
        /// <param name="error">Error code associated with the operation.</param>
        /// <returns>True if successful.</returns>
        public bool DeleteDocument(string masterDocId, out ErrorCode error)
        {
            error = null;

            if (_Destroying)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " DeleteDocument index is being destroyed");
                error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return false;
            }

            if (String.IsNullOrEmpty(masterDocId))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " DeleteDocument document ID not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "MasterDocId");
                return false;
            }

            _Logging.Log(LoggingModule.Severity.Info, "Index " + Name + " DeleteDocument starting deletion of doc ID " + masterDocId);

            if (_SqlDatabase != null)
            {
                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("MasterDocId", DatabaseWrapper.Operators.Equals, masterDocId);
                _SqlDatabase.Delete("SourceDocuments", e);
                _SqlDatabase.Delete("ParsedDocuments", e);
                _SqlDatabase.Delete("Terms", e);
            }
            else
            {
                SqliteWrapper.Expression e = new SqliteWrapper.Expression("MasterDocId", SqliteWrapper.Operators.Equals, masterDocId);
                _SqliteDatabase.Delete("SourceDocuments", e);
                _SqliteDatabase.Delete("ParsedDocuments", e);
                _SqliteDatabase.Delete("Terms", e);
            }

            bool deleteSource = DeleteSourceDocument(masterDocId);
            bool deleteParsed = DeleteParsedDocument(masterDocId);
            if (!deleteSource || !deleteParsed)
            {
                error = new ErrorCode(ErrorId.DELETE_ERROR, masterDocId);
                return false;
            }

            _Logging.Log(LoggingModule.Severity.Info, "Index " + Name + " DeleteDocument successfully deleted doc ID " + masterDocId);
            return true;
        }

        /// <summary>
        /// Retrieve a source document by ID from storage.
        /// </summary>
        /// <param name="masterDocId">The ID of the document.</param>
        /// <param name="sourceData">The source data from the document.</param>
        /// <returns>True if successful.</returns>
        public bool GetSourceDocument(string masterDocId, out byte[] sourceData)
        {
            sourceData = null;
            return ReadSourceDocument(masterDocId, out sourceData);
        }

        /// <summary>
        /// Retrieve a parsed document by ID from storage.
        /// </summary>
        /// <param name="masterDocId">The ID of the document.</param>
        /// <param name="doc">The parsed document.</param>
        /// <returns>True if successful.</returns>
        public bool GetParsedDocument(string masterDocId, out IndexedDoc doc)
        {
            doc = null;
            return ReadParsedDocument(masterDocId, out doc);
        }

        /// <summary>
        /// Search the index.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <param name="result">The result of the search.</param>
        /// <param name="error">Error code associated with the operation.</param>
        /// <returns>True if successful.</returns>
        public bool Search(SearchQuery query, out SearchResult result, out ErrorCode error)
        {
            error = null;
            result = new SearchResult(query);
            result.MarkStarted();

            #region Check-for-Null-Values

            if (_Destroying)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " Search index is being destroyed");
                error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return false;
            }

            if (query == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " Search query not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "Query");
                return false;
            }

            if (query.Required == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " Search required filter not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "Required Filter");
                return false;
            }

            if (query.Required.Terms == null || query.Required.Terms.Count < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " Search required terms not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "Required Terms");
                return false;
            }

            #endregion

            #region Process

            if (!String.IsNullOrEmpty(query.PostbackUrl))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " Search starting async search with POSTback to " + query.PostbackUrl);
                Task.Run(() => SearchTaskWrapper(query));

                result = new SearchResult(query);
                result.Async = true;
                result.IndexName = Name;
                result.MarkStarted();

                return true;
            }
            else
            {
                return SearchInternal(query, out result, out error);
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

            #region Source-Docs-Stats

            string sourceDocsQuery =
                "SELECT " +
                "  COUNT(*) AS NumDocs, " +
                "  SUM(ContentLength) AS SizeBytes " +
                "FROM SourceDocuments";

            DataTable sourceDocsResult = null;
            if (_SqlDatabase != null) sourceDocsResult = _SqlDatabase.RawQuery(sourceDocsQuery);
            else sourceDocsResult = _SqliteDatabase.Query(sourceDocsQuery);
            
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
            if (_SqlDatabase != null) parsedDocsResult = _SqlDatabase.RawQuery(parsedDocsQuery);
            else parsedDocsResult = _SqliteDatabase.Query(parsedDocsQuery);
             
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
        /// <param name="result">The result of the enumeration.</param>
        /// <param name="error">Error code associated with the operation.</param>
        /// <returns>True if successful.</returns>
        public bool Enumerate(EnumerationQuery query, out EnumerationResult result, out ErrorCode error)
        {
            error = null;
            result = new EnumerationResult(query);
            result.MarkStarted();

            #region Check-for-Null-Values

            if (_Destroying)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " Enumerate index is being destroyed");
                error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return false;
            }

            if (query == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " Enumerate query not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "Query");
                return false;
            }
             
            #endregion

            #region Process

            if (!String.IsNullOrEmpty(query.PostbackUrl))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " Enumeration starting async search with POSTback to " + query.PostbackUrl);
                Task.Run(() => EnumerationTaskWrapper(query));

                result = new EnumerationResult(query);
                result.Async = true;
                result.IndexName = Name;
                result.MarkStarted();

                return true;
            }
            else
            {
                return EnumerationInternal(query, out result, out error);
            }

            #endregion
        }

        #endregion

        #region Private-Methods

        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_SqliteDatabase != null) _SqliteDatabase.Dispose();
            }

            _Disposed = true;
        }

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

        private void InitializeDatabase()
        {
            switch (_Index.Database.Type)
            {
                case DatabaseType.Mssql:
                case DatabaseType.Mysql:
                case DatabaseType.Pgsql:
                    _SqlDatabase = new DatabaseWrapper.DatabaseClient(
                        DatabaseTypeToString(_Index.Database.Type),
                        _Index.Database.Hostname,
                        _Index.Database.Port,
                        _Index.Database.Username,
                        _Index.Database.Password,
                        _Index.Database.InstanceName,
                        _Index.Database.DatabaseName);
                    _SqlDatabase.DebugRawQuery = _Index.Database.Debug;
                    _SqlDatabase.DebugResultRowCount = _Index.Database.Debug;
                    return;

                case DatabaseType.Sqlite:
                    _SqliteDatabase = new SqliteWrapper.DatabaseClient(
                        _Index.Database.Filename,
                        _Index.Database.Debug);
                    return;
            }

            throw new ArgumentException("Unknown database type, use one of: Mssql, Mysql, Pgsql, Sqlite.");
        }

        private string DatabaseTypeToString(DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.Mssql:
                    return "mssql";
                case DatabaseType.Mysql:
                    return "mysql";
                case DatabaseType.Pgsql:
                    return "pgsql";
                case DatabaseType.Sqlite:
                    return "sqlite";
            }

            throw new ArgumentException("Unknown database type, use one of: Mssql, Mysql, Pgsql, Sqlite.");
        }

        private void InitializeDatabaseTables()
        {
            string sourceDocsQuery = _IndexQueries.CreateSourceDocsTable();
            string parsedDocsQuery = _IndexQueries.CreateParsedDocsTable();
            string termsQuery = _IndexQueries.CreateTermsTable();
             
            switch (_Index.Database.Type)
            {
                case DatabaseType.Mssql:
                case DatabaseType.Mysql:
                case DatabaseType.Pgsql:
                    _SqlDatabase.RawQuery(sourceDocsQuery);
                    _SqlDatabase.RawQuery(parsedDocsQuery);
                    _SqlDatabase.RawQuery(termsQuery);
                    return;
                case DatabaseType.Sqlite:
                    _SqliteDatabase.Query(sourceDocsQuery);
                    _SqliteDatabase.Query(parsedDocsQuery);
                    _SqliteDatabase.Query(termsQuery);
                    return;
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

        private string Sanitize(string str)
        {
            if (String.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));
            if (_SqlDatabase != null) return _SqlDatabase.SanitizeString(str);
            else return SqliteWrapper.DatabaseClient.SanitizeString(str);
        }
         
        private IndexedDoc GenerateIndexedDoc(DocType docType, byte[] sourceData, string sourceUrl)
        {
            IndexedDoc doc = null;

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
         
        private bool WriteSourceDocument(byte[] data, IndexedDoc doc)
        {
            string filename = doc.MasterDocId + ".source";
            return _BlobSource.Write(filename, false, data).Result;
        }

        private bool DeleteSourceDocument(string masterDocId)
        {
            string filename = masterDocId + ".source";
            return _BlobSource.Delete(filename).Result;
        }

        private bool ReadSourceDocument(string masterDocId, out byte[] data)
        {
            string filename = masterDocId + ".source";
            data = null;

            try
            {
                data = _BlobSource.Get(filename).Result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool WriteParsedDocument(IndexedDoc doc)
        {
            string filename = doc.MasterDocId + ".parsed.json";
            return _BlobParsed.Write(filename, false, Encoding.UTF8.GetBytes(Common.SerializeJson(doc, false))).Result;
        }
        
        private bool DeleteParsedDocument(string masterDocId)
        {
            string filename = masterDocId + ".parsed.json";
            return _BlobParsed.Delete(filename).Result;
        }

        private bool ReadParsedDocument(string masterDocId, out IndexedDoc doc)
        {
            byte[] data;
            doc = null;
            string filename = masterDocId + ".parsed.json";

            try
            {
                data = _BlobParsed.Get(filename).Result;
                doc = Common.DeserializeJson<IndexedDoc>(data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private List<string> GetMatchingDocIdsByTerms(SearchQuery query)
        {
            List<string> ret = new List<string>();

            string dbQuery = _IndexQueries.SelectDocIdsByTerms(query);

            DataTable result = null;
            if (_SqlDatabase != null) result = _SqlDatabase.RawQuery(dbQuery);
            else result = _SqliteDatabase.Query(dbQuery);

            if (result == null || result.Rows.Count < 1) return ret;

            foreach (DataRow currRow in result.Rows)
            {
                ret.Add(currRow["MasterDocId"].ToString());
            }

            return ret; 
        }

        private bool DocMatchesFilters(IndexedDoc doc, SearchQuery query, out decimal score)
        {
            score = 1m;
            
            #region Process
             
            if (!RequiredFiltersMatch(doc, query))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " does not match required filters");
                return false;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " matches required filters");
            }
             
            if (!OptionalFiltersMatch(doc, query, out score))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " does not match optional filters");
                return false;
            }
            else
            {
                score = Convert.ToDecimal(score.ToString("N4"));
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " matches optional filters [score " + score + "]");
            }

            if (!ExcludeFiltersMatch(doc, query))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " one or more exclude filters matched");
                return false;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " matches exclude filters");
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
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " RequiredFiltersMatch document ID " + doc.MasterDocId + " has no data nodes");
                return false;
            }

            foreach (SearchFilter currFilter in query.Required.Filter)
            {
                if (String.IsNullOrEmpty(currFilter.Field))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " RequiredFiltersMatch null key supplied in filter");
                    continue;
                }

                foreach (DataNode currNode in nodes)
                {
                    if (currNode.Key.Equals(currFilter.Field))
                    {
                        if (!FilterMatch(currFilter, currNode)) return false;
                    }
                }
            }

            return true;
        }

        private bool OptionalFiltersMatch(IndexedDoc doc, SearchQuery query, out decimal score)
        {
            score = 1m;
            if (query.Optional == null || query.Optional.Filter == null || query.Optional.Filter.Count < 1)
            {
                _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " OptionalFiltersMatch no optional filters found");
                return true;
            }

            if (doc.Text != null || doc.Html != null)
            {
                _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " OptionalFiltersMatch document type is text or HTML, skipping");
                return true;  // not appropriate searches
            }

            int filterCount = query.Optional.Filter.Count;
            int matchCount = 0;

            List<DataNode> nodes = DataNodesFromIndexedDoc(doc);
            if (nodes == null || nodes.Count < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " OptionalFiltersMatch document ID " + doc.MasterDocId + " has no data nodes");
                return false;
            }

            foreach (SearchFilter currFilter in query.Required.Filter)
            {
                if (String.IsNullOrEmpty(currFilter.Field))
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " OptionalFiltersMatch null key supplied in filter");
                    continue;
                }

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

            _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " OptionalFiltersMatch document ID " + doc.MasterDocId + " [" + filterCount + " filters, " + matchCount + " matches: " + score + " score]");
            if (matchCount > 0 && filterCount > 0) score = (decimal)matchCount / filterCount;
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
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " RequiredFiltersMatch document ID " + doc.MasterDocId + " has no data nodes");
                return false;
            }

            foreach (SearchFilter currFilter in query.Required.Filter)
            {
                if (String.IsNullOrEmpty(currFilter.Field))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " RequiredFiltersMatch null key supplied in filter");
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
                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " FilterMatch unknown condition: " + filter.Condition.ToString());
                    return false;
            } 
        }

        private void SearchTaskWrapper(SearchQuery query)
        {
            SearchResult result = null;
            ErrorCode error = null;

            bool success = SearchInternal(query, out result, out error);
            byte[] data = null;

            if (success) data = Encoding.UTF8.GetBytes(Common.SerializeJson(result, true));
            else data = Encoding.UTF8.GetBytes(Common.SerializeJson(error, true));
    
            RestResponse resp = RestRequest.SendRequestSafe(
                query.PostbackUrl,
                "application/json",
                "POST",
                null, null, false, true, null,
                data);

            if (resp == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " SearchTaskWrapper no response from POSTback URL " + query.PostbackUrl);
                return;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " SearchTaskWrapper " + resp.StatusCode + " response from POSTback URL " + query.PostbackUrl);
                return;
            } 
        }

        private bool SearchInternal(SearchQuery query, out SearchResult result, out ErrorCode error)
        {
            error = null;
            result = new SearchResult(query);
            result.GUID = query.GUID;
            result.IndexName = _Index.IndexName;
            result.MarkStarted();

            try
            {
                #region Check-for-Null-Values

                if (query == null)
                {
                    error = new ErrorCode(ErrorId.MISSING_PARAMS, "Query");
                    return false;
                }

                if (query.Required == null)
                {
                    error = new ErrorCode(ErrorId.MISSING_PARAMS, "Required Filter");
                    return false;
                }

                if (query.Required.Terms == null || query.Required.Terms.Count < 1)
                {
                    error = new ErrorCode(ErrorId.MISSING_PARAMS, "Required Terms");
                    return false;
                }

                #endregion

                #region Process-Terms

                List<string> docIds = GetMatchingDocIdsByTerms(query);
                if (docIds == null || docIds.Count < 1)
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " SearchInternal found no results");
                    return true;
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " SearchInternal found " + docIds.Count + " results");
                }

                result.SetTermsMatchCount(docIds.Count);

                #endregion

                #region Process-Filters

                List<string> filteredDocIds = new List<string>();
                Dictionary<string, decimal> scores = new Dictionary<string, decimal>();

                if ((query.Required.Filter != null && query.Required.Filter.Count > 0)
                    || (query.Optional.Filter != null && query.Optional.Filter.Count > 0)
                    || (query.Exclude.Filter != null && query.Exclude.Filter.Count > 0))
                { 
                    foreach (string currDocId in docIds)
                    {
                        IndexedDoc currParsedDoc = null;
                        if (!ReadParsedDocument(currDocId, out currParsedDoc))
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "IndexClient " + Name + " SearchInternal unable to read parsed document ID " + currDocId);
                            continue;
                        }

                        decimal score = 1m;
                        if (!DocMatchesFilters(currParsedDoc, query, out score))
                        {
                            _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " SearchInternal document ID " + currDocId + " does not match filters");
                            continue;
                        }
                        else
                        {
                            scores.Add(currDocId, score);
                            filteredDocIds.Add(currDocId);
                        }
                    } 
                }

                #endregion

                #region Append-Documents

                if (filteredDocIds != null && filteredDocIds.Count > 0)
                {
                    List<Document> documents = new List<Document>();

                    foreach (string currDocId in filteredDocIds)
                    {
                        Document currDoc = new Document();
                        currDoc.MasterDocId = currDocId;
                        currDoc.Score = Convert.ToDecimal(scores[currDocId].ToString("N4"));

                        byte[] data = null; 
                        IndexedDoc currIndexedDoc = null;

                        if (query.IncludeParsedDoc || query.IncludeContent)
                        { 
                            if (!ReadParsedDocument(currDocId, out currIndexedDoc))
                            {
                                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " SearchInternal document ID " + currDocId + " cannot retrieve parsed doc");
                                currDoc.Errors.Add("Unable to retrieve parsed document");
                            }

                            currDoc.DocumentType = currIndexedDoc.DocumentType;

                            if (query.IncludeParsedDoc)
                            {
                                currDoc.Parsed = currIndexedDoc;
                            }

                            if (query.IncludeContent)
                            {
                                if (!ReadSourceDocument(currDocId, out data))
                                {
                                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " SearchInternal document ID " + currDocId + " cannot retrieve source doc");
                                    currDoc.Errors.Add("Unable to retrieve source document");
                                }
                                else
                                {
                                    if (currIndexedDoc != null && currIndexedDoc.DocumentType == DocType.Json)
                                    {
                                        JToken jsonData = JToken.Parse(Encoding.UTF8.GetString(data));
                                        currDoc.Data = jsonData;
                                    }
                                    else
                                    {
                                        currDoc.Data = Encoding.UTF8.GetString(data);
                                    }
                                }
                            } 
                        }

                        _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " SearchInternal appended doc ID " + currDocId + " to result");
                        documents.Add(currDoc);
                    }

                    result.AttachResults(documents);
                }

                result.SortMatchesByScore();

                #endregion

                return true;
            }
            finally
            {
                result.MarkEnded();
                _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " SearchInternal finished (" + result.TotalTimeMs + "ms)");
            }
        }

        private DocType DocTypeFromString(string val)
        {
            if (String.IsNullOrEmpty(val)) throw new ArgumentNullException(nameof(val));

            switch (val.ToLower())
            {
                case "html":
                    return DocType.Html;
                case "json":
                    return DocType.Json;
                case "xml":
                    return DocType.Xml;
                case "text":
                    return DocType.Text;
                case "sql":
                    return DocType.Sql;
                default:
                    return DocType.Unknown;
            } 
        }

        private void EnumerationTaskWrapper(EnumerationQuery query)
        {
            EnumerationResult result = null;
            ErrorCode error = null;

            bool success = EnumerationInternal(query, out result, out error);
            byte[] data = null;

            if (success) data = Encoding.UTF8.GetBytes(Common.SerializeJson(result, true));
            else data = Encoding.UTF8.GetBytes(Common.SerializeJson(error, true));

            RestResponse resp = RestRequest.SendRequestSafe(
                query.PostbackUrl,
                "application/json",
                "POST",
                null, null, false, true, null,
                data);

            if (resp == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " EnumerationTaskWrapper no response from POSTback URL " + query.PostbackUrl);
                return;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " EnumerationTaskWrapper " + resp.StatusCode + " response from POSTback URL " + query.PostbackUrl);
                return;
            }
        }

        private bool EnumerationInternal(EnumerationQuery query, out EnumerationResult result, out ErrorCode error)
        {
            error = null;
            result = new EnumerationResult(query);
            result.GUID = query.GUID;
            result.IndexName = _Index.IndexName;
            result.MarkStarted();

            try
            {
                #region Check-for-Null-Values

                if (query == null)
                {
                    error = new ErrorCode(ErrorId.MISSING_PARAMS, "Query");
                    return false;
                }

                if (query.MaxResults == null || query.MaxResults > 5000) query.MaxResults = 5000;

                #endregion

                #region Execute-Query

                string dbQuery = _IndexQueries.SelectSourceDocumentsByEnumerationQuery(query);

                DataTable dbResult = null;
                if (_SqlDatabase != null) dbResult = _SqlDatabase.RawQuery(dbQuery);
                else dbResult = _SqliteDatabase.Query(dbQuery);

                List<SourceDocument> sourceDocs = new List<SourceDocument>();
                foreach (DataRow row in dbResult.Rows)
                {
                    sourceDocs.Add(SourceDocument.FromDataRow(row));
                }
                
                result.AttachResults(sourceDocs);
                
                #endregion
                 
                return true;
            }
            finally
            {
                result.MarkEnded();
                _Logging.Log(LoggingModule.Severity.Debug, "Index " + Name + " SearchInternal finished (" + result.TotalTimeMs + "ms)");
            }
        }

        #endregion
    }
}
