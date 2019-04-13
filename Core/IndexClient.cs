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

        private bool _Disposed = false;
        private bool _Destroying = false;
        public int _MaxResults = 1000;

        private Index _Index;
        private LoggingModule _Logging;

        private string _RootDirectory;

        private DatabaseWrapper.DatabaseClient _SqlDatabase = null;
        private SqliteWrapper.DatabaseClient _SqliteDatabase = null;
        private IndexQueries _IndexQueries = null;

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

            if (index.Database == null) throw new ArgumentException("Index does not contain database settings.");
            if (index.StorageParsed == null) throw new ArgumentException("Index does not contain storage configuration for parsed documents.");
            if (index.StorageSource == null) throw new ArgumentException("Index does not contain storage configuration for source documents.");
            if (index.Postings == null) throw new ArgumentException("Index does not contain settings for the postings manager.");

            _Index = index;
            Name = index.IndexName;

            _Logging = logging;
            _RootDirectory = index.RootDirectory;
             
            CreateDirectories();
            InitializeDatabase();
            InitializeBlobManager();
            InitializePostingsManager();

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
        /// Parse a document and add to the index.
        /// </summary>
        /// <param name="docType">The type of document.</param>
        /// <param name="sourceData">The source data from the document.</param>
        /// <param name="sourceUrl">The URL from which the content should be retrieved.</param>
        /// <param name="error">Error code associated with the operation.</param>
        /// <param name="masterDocId">Document ID of the added document.</param>
        /// <returns>True if successful.</returns>
        public bool AddDocument(
            DocType docType, 
            byte[] sourceData, 
            string sourceUrl, 
            string name,
            string tags,
            string contentType,
            out ErrorCode error, 
            out string masterDocId)
        {
            error = null;
            masterDocId = null;
            bool cleanupRequired = false;
            IndexedDoc doc = null;

            try
            {
                if (_Destroying)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] AddDocument index is being destroyed");
                    error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                    return false;
                }

                if ((sourceData == null || sourceData.Length < 1) && String.IsNullOrEmpty(sourceUrl))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] AddDocument source URL not supplied");
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
                        _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] AddDocument unable to retrieve data from source " + sourceUrl);
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
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] AddDocument unable to parse source data");
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
                sourceDocVals.Add("Name", name);
                sourceDocVals.Add("Tags", tags);
                sourceDocVals.Add("MasterDocId", doc.MasterDocId);
                sourceDocVals.Add("SourceUrl", sourceUrl);
                sourceDocVals.Add("ContentType", contentType);
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
                    foreach (Posting p in doc.Postings)
                    {
                        if (!_Postings.AddPosting(p))
                        {
                            _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] AddDocument unable to add posting for term " + p.Term + " in document ID " + doc.MasterDocId);
                        }
                    } 
                }

                #endregion

                #region Add-to-Filesystem

                if (!WriteSourceDocument(sourceData, doc))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] AddDocument unable to write source document");
                    error = new ErrorCode(ErrorId.WRITE_ERROR);
                    cleanupRequired = true;
                    return false;
                }

                if (!WriteParsedDocument(doc))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] AddDocument unable to write parsed document");
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
                    _Logging.Log(LoggingModule.Severity.Info, "[" + Name + "] AddDocument starting cleanup due to failed add operation");

                    if (_SqlDatabase != null)
                    {
                        DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("MasterDocId", DatabaseWrapper.Operators.Equals, doc.MasterDocId);
                        _SqlDatabase.Delete("SourceDocuments", e);
                        _SqlDatabase.Delete("ParsedDocuments", e); 
                    }
                    else
                    {
                        SqliteWrapper.Expression e = new SqliteWrapper.Expression("MasterDocId", SqliteWrapper.Operators.Equals, doc.MasterDocId);
                        _SqliteDatabase.Delete("SourceDocuments", e);
                        _SqliteDatabase.Delete("ParsedDocuments", e); 
                    }

                    if (doc.Terms != null && doc.Terms.Count > 0)
                    {
                        _Postings.RemoveDocument(doc.MasterDocId, doc.Terms);
                    }
                }

                #endregion
            }
        }

        /// <summary>
        /// Store a document in the index without parsing.
        /// </summary>
        /// <returns></returns>
        public bool StoreDocument(
            DocType docType,
            byte[] sourceData,
            string sourceUrl,
            string name,
            string tags,
            string contentType,
            out ErrorCode error,
            out string masterDocId)
        {
            error = null;
            masterDocId = null;
            bool cleanupRequired = false; 

            try
            {
                if (_Destroying)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] StoreDocument index is being destroyed");
                    error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                    return false;
                }

                if ((sourceData == null || sourceData.Length < 1) && String.IsNullOrEmpty(sourceUrl))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] StoreDocument source URL not supplied");
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
                        _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] StoreDocument unable to retrieve data from source " + sourceUrl);
                        error = new ErrorCode(ErrorId.RETRIEVE_FAILED, sourceUrl);
                        return false;
                    }
                    sourceData = new byte[data.Length];
                    Buffer.BlockCopy(data, 0, sourceData, 0, data.Length);
                }

                #endregion
                 
                #region Add-to-Database 

                masterDocId = Guid.NewGuid().ToString();

                string ts = null;
                if (_SqlDatabase != null) ts = _SqlDatabase.Timestamp(DateTime.Now.ToUniversalTime());
                else ts = _SqliteDatabase.Timestamp(DateTime.Now.ToUniversalTime());

                // Source documents table
                Dictionary<string, object> sourceDocVals = new Dictionary<string, object>();
                sourceDocVals.Add("IndexName", _Index.IndexName);
                sourceDocVals.Add("Name", name);
                sourceDocVals.Add("Tags", tags);
                sourceDocVals.Add("MasterDocId", masterDocId);
                sourceDocVals.Add("SourceUrl", sourceUrl);
                sourceDocVals.Add("ContentType", contentType);
                sourceDocVals.Add("ContentLength", sourceData.Length);
                sourceDocVals.Add("DocType", docType.ToString());
                sourceDocVals.Add("Created", ts);

                if (_SqlDatabase != null) _SqlDatabase.Insert("SourceDocuments", sourceDocVals);
                else _SqliteDatabase.Insert("SourceDocuments", sourceDocVals);
                 
                #endregion

                #region Add-to-Filesystem

                if (!WriteSourceDocument(sourceData, masterDocId))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] StoreDocument unable to write source document");
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

                if (cleanupRequired)
                {
                    _Logging.Log(LoggingModule.Severity.Info, "[" + Name + "] StoreDocument starting cleanup due to failed add operation");

                    if (_SqlDatabase != null)
                    {
                        DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("MasterDocId", DatabaseWrapper.Operators.Equals, masterDocId);
                        _SqlDatabase.Delete("SourceDocuments", e); 
                    }
                    else
                    {
                        SqliteWrapper.Expression e = new SqliteWrapper.Expression("MasterDocId", SqliteWrapper.Operators.Equals, masterDocId);
                        _SqliteDatabase.Delete("SourceDocuments", e); 
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
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] DocumentExists index is being destroyed");
                error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return false;
            }

            if (String.IsNullOrEmpty(masterDocId))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] DocumentExists document ID not supplied");
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
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] DeleteDocument index is being destroyed");
                error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return false;
            }

            if (String.IsNullOrEmpty(masterDocId))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] DeleteDocument document ID not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "MasterDocId");
                return false;
            }

            _Logging.Log(LoggingModule.Severity.Info, "[" + Name + "] DeleteDocument starting deletion of doc ID " + masterDocId);
             
            IndexedDoc doc = null;
            if (!GetParsedDocument(masterDocId, out doc))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] DeleteDocument unable to read parsed document ID " + masterDocId);
                error = new ErrorCode(ErrorId.RETRIEVE_FAILED, null);
                return false;
            }

            if (_SqlDatabase != null)
            {
                DatabaseWrapper.Expression e = new DatabaseWrapper.Expression("MasterDocId", DatabaseWrapper.Operators.Equals, masterDocId);
                _SqlDatabase.Delete("SourceDocuments", e);
                _SqlDatabase.Delete("ParsedDocuments", e); 
            }
            else
            {
                SqliteWrapper.Expression e = new SqliteWrapper.Expression("MasterDocId", SqliteWrapper.Operators.Equals, masterDocId);
                _SqliteDatabase.Delete("SourceDocuments", e);
                _SqliteDatabase.Delete("ParsedDocuments", e); 
            }

            bool deleteSource = DeleteSourceDocument(masterDocId);
            bool deleteParsed = DeleteParsedDocument(masterDocId);
            bool deletePostings = _Postings.RemoveDocument(doc.MasterDocId, doc.Terms);

            if (!deleteSource || !deleteParsed || !deletePostings)
            {
                error = new ErrorCode(ErrorId.DELETE_ERROR, masterDocId);
                return false;
            }

            _Logging.Log(LoggingModule.Severity.Info, "[" + Name + "] DeleteDocument successfully deleted doc ID " + masterDocId);
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

            #region Setup

            if (_Destroying)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] Search index is being destroyed");
                error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return false;
            }

            if (query == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] Search query not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "Query");
                return false;
            }

            if (query.Required == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] Search required filter not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "Required Filter");
                return false;
            }

            if (query.Required.Terms == null || query.Required.Terms.Count < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] Search required terms not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "Required Terms");
                return false;
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
                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] Search starting async search with POSTback to " + query.PostbackUrl);
                Task.Run(() => SearchTaskWrapper(query));

                result = new SearchResult(query);
                result.Async = true;
                result.IndexName = Name;
                result.MarkStarted();

                return true;
            }
            else
            {
                long indexEnd = 0;
                return SearchInternal(query, out result, out error, out indexEnd);
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
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] Enumerate index is being destroyed");
                error = new ErrorCode(ErrorId.DESTROY_IN_PROGRESS);
                return false;
            }

            if (query == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] Enumerate query not supplied");
                error = new ErrorCode(ErrorId.MISSING_PARAMS, "Query");
                return false;
            }
             
            #endregion

            #region Process

            if (!String.IsNullOrEmpty(query.PostbackUrl))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] Enumeration starting async search with POSTback to " + query.PostbackUrl);
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
                if (_Postings != null) _Postings.Dispose();
            }

            _Disposed = true;
        }

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

        private void InitializeDatabase()
        {
            #region Initialize-Database-Client

            switch (_Index.Database.Type)
            {
                case DatabaseType.MsSql:
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
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
                    break;

                case DatabaseType.SQLite:
                    _SqliteDatabase = new SqliteWrapper.DatabaseClient(
                        _Index.Database.Filename,
                        _Index.Database.Debug);
                    break;

                default:
                    throw new ArgumentException("Unknown database type, use one of: Mssql, Mysql, Pgsql, Sqlite.");
            }

            #endregion

            #region Initialize-Tables

            _IndexQueries = new IndexQueries(_Index, _SqlDatabase, _SqliteDatabase);

            string sourceDocsQuery = _IndexQueries.CreateSourceDocsTable();
            string parsedDocsQuery = _IndexQueries.CreateParsedDocsTable();
            string termsQuery = _IndexQueries.CreateTermsTable();
            string termsMapQuery = _IndexQueries.CreateTermsMapTable();

            switch (_Index.Database.Type)
            {
                case DatabaseType.MsSql:
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    _SqlDatabase.RawQuery(sourceDocsQuery);
                    _SqlDatabase.RawQuery(parsedDocsQuery);
                    _SqlDatabase.RawQuery(termsQuery);
                    _SqlDatabase.RawQuery(termsMapQuery);
                    break;

                case DatabaseType.SQLite:
                    _SqliteDatabase.Query(sourceDocsQuery);
                    _SqliteDatabase.Query(parsedDocsQuery);
                    _SqliteDatabase.Query(termsQuery);
                    _SqliteDatabase.Query(termsMapQuery);
                    break;
            }

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
                _Logging,
                _Index.IndexName,
                _Index.Postings.BaseDirectory,
                _Index.Postings.DatabaseFilename,
                _Index.Postings.DatabaseDebug
                );
        }

        #endregion

        #region Support

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

        #endregion

        #region Source-Documents

        private bool WriteSourceDocument(byte[] data, IndexedDoc doc)
        {
            string filename = doc.MasterDocId + ".source";
            return _BlobSource.Write(filename, false, data).Result;
        }

        private bool WriteSourceDocument(byte[] data, string masterDocId)
        {
            string filename = masterDocId + ".source";
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

        #endregion

        #region Parsed-Documents

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

        #endregion

        #region Filter-Match

        private bool DocMatchesFilters(IndexedDoc doc, SearchQuery query, out decimal filterScore)
        {
            filterScore = 1m;
            
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

            if (!ExcludeFiltersMatch(doc, query))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " one or more exclude filters matched");
                return false;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " matches exclude filters");
            }

            if (!OptionalFiltersMatch(doc, query, out filterScore))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " does not match optional filters");
                return false;
            }
            else
            {
                filterScore = Convert.ToDecimal(filterScore.ToString("N4"));
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + doc.MasterDocId + " matches optional filters [score " + filterScore + "]");
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
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] RequiredFiltersMatch document ID " + doc.MasterDocId + " has no data nodes");
                return false;
            }

            bool result = false;

            foreach (SearchFilter currFilter in query.Required.Filter)
            {
                if (String.IsNullOrEmpty(currFilter.Field))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] RequiredFiltersMatch null key supplied in filter");
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
                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] OptionalFiltersMatch no optional filters found");
                return true;
            }

            if (doc.Text != null || doc.Html != null)
            {
                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] OptionalFiltersMatch document type is text or HTML, skipping");
                return true;  // not appropriate searches
            }

            int filterCount = query.Optional.Filter.Count;
            int matchCount = 0;

            List<DataNode> nodes = DataNodesFromIndexedDoc(doc);
            if (nodes == null || nodes.Count < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] OptionalFiltersMatch document ID " + doc.MasterDocId + " has no data nodes");
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

            _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] OptionalFiltersMatch document ID " + doc.MasterDocId + " [" + filterCount + " filters, " + matchCount + " matches: " + filterScore + " score]");
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
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] RequiredFiltersMatch document ID " + doc.MasterDocId + " has no data nodes");
                return false;
            }

            foreach (SearchFilter currFilter in query.Required.Filter)
            {
                if (String.IsNullOrEmpty(currFilter.Field))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] RequiredFiltersMatch null key supplied in filter");
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
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] FilterMatch unknown condition: " + filter.Condition.ToString());
                    return false;
            } 
        }

        #endregion

        #region Search

        private void SearchTaskWrapper(SearchQuery query)
        {
            SearchResult result = null;
            ErrorCode error = null;

            long indexEnd = 0;
            bool success = SearchInternal(query, out result, out error, out indexEnd);
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
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] SearchTaskWrapper no response from POSTback URL " + query.PostbackUrl);
                return;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchTaskWrapper " + resp.StatusCode + " response from POSTback URL " + query.PostbackUrl);
                return;
            } 
        }

        private bool SearchInternal(SearchQuery query, out SearchResult result, out ErrorCode error, out long indexEnd)
        {
            error = null;
            result = new SearchResult(query);
            result.IndexName = _Index.IndexName;
            result.MatchCount = new MatchCounts();
            result.MarkStarted();

            indexEnd = query.StartIndex;

            try
            {
                #region Variables

                Dictionary<string, decimal> docsMatching = new Dictionary<string, decimal>();
                Dictionary<string, decimal> currDocsMatching = new Dictionary<string, decimal>();

                #endregion

                #region Process

                while (docsMatching.Count < query.MaxResults)
                {
                    bool endOfSearch = false;

                    #region Process-Terms

                    while (true)
                    {
                        _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchInternal retrieving documents matching terms, index " + indexEnd + " max results " + query.MaxResults);

                        _Postings.GetMatchingDocuments(
                            indexEnd,
                            query.MaxResults,
                            query.Required.Terms,
                            query.Optional.Terms,
                            query.Exclude.Terms,
                            out currDocsMatching,
                            out indexEnd);

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
                            _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchInternal adding document " + curr.Key);
                        }

                        if (currDocsMatching.Count >= query.MaxResults)
                        {
                            _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchInternal potential results " + currDocsMatching.Count + " exceeds max results " + query.MaxResults);
                            break;
                        }
                    }

                    if (endOfSearch)
                    {
                        // no more documents
                        _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchInternal reached end of search results");
                        break;
                    }
                    else
                    {
                        // increment limit and offset
                        indexEnd += query.MaxResults;
                    }

                    #endregion

                    #region Process-Filters

                    if ((query.Required.Filter != null && query.Required.Filter.Count > 0)
                        || (query.Optional.Filter != null && query.Optional.Filter.Count > 0)
                        || (query.Exclude.Filter != null && query.Exclude.Filter.Count > 0))
                    {
                        foreach (KeyValuePair<string, decimal> curr in currDocsMatching)
                        {
                            IndexedDoc currParsedDoc = null;
                            if (!ReadParsedDocument(curr.Key, out currParsedDoc))
                            {
                                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] SearchInternal unable to read parsed document ID " + curr.Key);
                                continue;
                            }

                            decimal filterScore = 1m;
                            if (!DocMatchesFilters(currParsedDoc, query, out filterScore))
                            {
                                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchInternal document ID " + curr.Key + " does not match filters, removing");
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
                    List<Document> documents = new List<Document>();

                    foreach (KeyValuePair<string, decimal> curr in docsMatching)
                    {
                        Document currDoc = new Document();
                        currDoc.MasterDocId = curr.Key;
                        currDoc.Score = Convert.ToDecimal(curr.Value.ToString("N4"));
                        currDoc.DocumentType = null;
                        currDoc.Errors = new List<string>();

                        byte[] data = null; 
                        IndexedDoc currIndexedDoc = null;

                        if (query.IncludeParsedDoc || query.IncludeContent)
                        {
                            if (!ReadParsedDocument(curr.Key, out currIndexedDoc))
                            {
                                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] SearchInternal document ID " + curr.Key + " cannot retrieve parsed doc");
                                currDoc.Errors.Add("Unable to retrieve parsed document");
                            }
                            else
                            {
                                currDoc.DocumentType = currIndexedDoc.DocumentType;

                                if (query.IncludeParsedDoc)
                                {
                                    currDoc.Parsed = currIndexedDoc;
                                }

                                if (query.IncludeContent)
                                {
                                    if (!ReadSourceDocument(curr.Key, out data))
                                    {
                                        _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] SearchInternal document ID " + curr + " cannot retrieve source doc");
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
                        }

                        _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchInternal appended doc ID " + curr.Key + " to result");
                        result.Documents.Add(currDoc);
                    } 
                }

                #endregion

                result.SortMatchesByScore();
                return true;
            }
            finally
            {
                result.MarkEnded();
                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchInternal finished (" + result.TotalTimeMs + "ms)");
            }
        }

        #endregion

        #region Enumeration

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
                _Logging.Log(LoggingModule.Severity.Warn, "[" + Name + "] EnumerationTaskWrapper no response from POSTback URL " + query.PostbackUrl);
                return;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] EnumerationTaskWrapper " + resp.StatusCode + " response from POSTback URL " + query.PostbackUrl);
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
                _Logging.Log(LoggingModule.Severity.Debug, "[" + Name + "] SearchInternal finished (" + result.TotalTimeMs + "ms)");
            }
        }

        #endregion

        #endregion
    }
}
