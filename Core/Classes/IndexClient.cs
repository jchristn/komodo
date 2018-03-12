using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqliteWrapper;
using SyslogLogging;
using RestWrapper;

namespace KomodoCore
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

        private LoggingModule _Logging;

        private string _RootDirectory;
        private string _SourceDocumentsDirectory;
        private string _ParsedDocumentsDirectory;
        private string _TermsDirectory;

        private string _DbFilename;
        private bool _DbDebug;
        private IndexOptions _IndexOptions;
        private DatabaseClient _Database;

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
        public IndexClient(string name, string rootDirectory, bool dbDebug, IndexOptions indexOptions, LoggingModule logging)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (String.IsNullOrEmpty(rootDirectory)) throw new ArgumentNullException(nameof(rootDirectory));
            if (indexOptions == null) throw new ArgumentNullException(nameof(indexOptions));
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            Name = name;

            _Logging = logging;
            _RootDirectory = rootDirectory;
            _SourceDocumentsDirectory = rootDirectory + "/SourceDocuments";
            _ParsedDocumentsDirectory = rootDirectory + "/ParsedDocuments";
            _TermsDirectory = rootDirectory + "/Terms";

            _DbFilename = _RootDirectory + "/" + Name + ".db";
            _DbDebug = dbDebug;
            _IndexOptions = indexOptions;
            _DbLock = new object();

            CreateDirectories();

            _Database = new DatabaseClient(_DbFilename, _DbDebug);

            CreateSourceDocsTable();
            CreatedParsedDocsTable();
            CreateTermsTable();

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
        /// Add a document to the index.
        /// </summary>
        /// <param name="docType">The type of document.</param>
        /// <param name="sourceData">The source data from the document.</param>
        /// <param name="sourceUrl">The URL from which the content should be retrieved.</param>
        /// <param name="error">Error code associated with the operation.</param>
        /// <returns>True if successful.</returns>
        public bool AddDocument(DocType docType, byte[] sourceData, string sourceUrl, out ErrorCode error)
        {
            error = null;
            bool cleanupRequired = false;
            IndexedDoc doc = null;

            try
            {
                if ((sourceData == null || sourceData.Length < 1) && String.IsNullOrEmpty(sourceUrl))
                {
                    error = new ErrorCode("MISSING_PARAM", "SourceUrl");
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
                        error = new ErrorCode("RETRIEVE_FAILED", sourceUrl);
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
                    error = new ErrorCode("PARSE_ERROR", sourceUrl);
                    return false;
                }

                #endregion

                #region Add-to-Database 

                string ts = _Database.Timestamp(DateTime.Now.ToUniversalTime());

                Dictionary<string, object> sourceDocVals = new Dictionary<string, object>();
                sourceDocVals.Add("MasterDocId", doc.MasterDocId);
                sourceDocVals.Add("SourceUrl", sourceUrl);
                sourceDocVals.Add("ContentLength", sourceData.Length);
                sourceDocVals.Add("DocType", docType.ToString());
                sourceDocVals.Add("Created", ts);
                _Database.Insert("SourceDocuments", sourceDocVals);

                sourceDocVals.Remove("SourceUrl");
                sourceDocVals.Remove("ContentLength");
                _Database.Insert("ParsedDocuments", sourceDocVals);

                if (doc.Terms != null && doc.Terms.Count > 0)
                {
                    foreach (string currTerm in doc.Terms)
                    {
                        Dictionary<string, object> termsVals = new Dictionary<string, object>();
                        termsVals.Add("MasterDocId", doc.MasterDocId);
                        termsVals.Add("Term", currTerm);
                        termsVals.Add("Created", ts);
                        _Database.Insert("Terms", termsVals);
                    }
                }

                #endregion

                #region Add-to-Filesystem

                if (!WriteSourceDocument(sourceData, doc)
                    || !WriteParsedDocument(doc))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "Index " + Name + " AddDocument unable to write source document");
                    error = new ErrorCode("WRITE_ERROR");
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

                    Expression e = new Expression("MasterDocId", Operators.Equals, doc.MasterDocId);
                    _Database.Delete("SourceDocuments", e);
                    _Database.Delete("ParsedDocuments", e);
                    _Database.Delete("Terms", e);
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
            if (String.IsNullOrEmpty(masterDocId))
            {
                error = new ErrorCode("MISSING_PARAM", "MasterDocId");
                return false; 
            }

            Expression e = new Expression("MasterDocId", Operators.Equals, masterDocId);
            DataTable result = _Database.Select("SourceDocuments", null, null, null, e, null);

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
            if (String.IsNullOrEmpty(masterDocId))
            {
                error = new ErrorCode("MISSING_PARAM", "MasterDocId");
                return false;
            }

            _Logging.Log(LoggingModule.Severity.Info, "Index " + Name + " DeleteDocument starting deletion of doc ID " + masterDocId);

            Expression e = new Expression("MasterDocId", Operators.Equals, masterDocId);
            _Database.Delete("SourceDocuments", e);
            _Database.Delete("ParsedDocuments", e);
            _Database.Delete("Terms", e);

            bool deleteSource = DeleteSourceDocument(masterDocId);
            bool deleteParsed = DeleteParsedDocument(masterDocId);
            if (!deleteSource || !deleteParsed)
            {
                error = new ErrorCode("DELETE_ERROR", masterDocId);
                return false;
            }

            _Logging.Log(LoggingModule.Severity.Info, "Index " + Name + " DeleteDocument successfully deleted doc ID " + masterDocId);
            return true;
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

            if (query == null)
            {
                error = new ErrorCode("MISSING_PARAM", "Query");
                return false;
            }

            if (query.Required == null)
            {
                error = new ErrorCode("MISSING_PARAM", "Required Filter");
                return false;
            }

            if (query.Required.Terms == null || query.Required.Terms.Count < 1)
            {
                error = new ErrorCode("MISSING_PARAM", "Required Terms");
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
                if (_Database != null) _Database.Dispose();
            }

            _Disposed = true;
        }

        private void CreateDirectories()
        { 
            if (!Directory.Exists(_RootDirectory))
            {
                if (!Common.CreateDirectory(_RootDirectory)) throw new IOException("Unable to create index directory.");
            }

            if (!Directory.Exists(_SourceDocumentsDirectory))
            {
                if (!Common.CreateDirectory(_SourceDocumentsDirectory)) throw new IOException("Unable to create source documents directory.");
            }

            if (!Directory.Exists(_ParsedDocumentsDirectory))
            {
                if (!Common.CreateDirectory(_ParsedDocumentsDirectory)) throw new IOException("Unable to create parsed documents directory.");
            }

            if (!Directory.Exists(_TermsDirectory))
            {
                if (!Common.CreateDirectory(_TermsDirectory)) throw new IOException("Unable to create terms directory.");
            }
        }

        private void CreateSourceDocsTable()
        {
            string query =
                "CREATE TABLE IF NOT EXISTS SourceDocuments " +
                "(" +
                "  Id                INTEGER PRIMARY KEY, " + 
                "  MasterDocId       VARCHAR(128), " +
                "  SourceUrl         VARCHAR(256), " +
                "  ContentLength     INTEGER, " +
                "  DocType           VARCHAR(32), " +
                "  Created           VARCHAR(32), " +
                "  Indexed           VARCHAR(32) " +
                ")";

            _Database.Query(query);
        }

        private void CreatedParsedDocsTable()
        {
            string query =
                "CREATE TABLE IF NOT EXISTS ParsedDocuments " +
                "(" +
                "  Id                INTEGER PRIMARY KEY, " +
                "  MasterDocId       VARCHAR(128), " +
                "  DocType           VARCHAR(32), " +
                "  Created           VARCHAR(32), " +
                "  Indexed           VARCHAR(32) " +
                ")";

            _Database.Query(query);
        }

        private void CreateTermsTable()
        {
            string query =
                "CREATE TABLE IF NOT EXISTS Terms " +
                "(" +
                "  Id                INTEGER PRIMARY KEY, " +
                "  MasterDocId       VARCHAR(128), " +
                "  Term              BLOB, " +
                "  Created           VARCHAR(32), " +
                "  Indexed           VARCHAR(32) " +
                ")";

            _Database.Query(query);
        }

        private string Sanitize(string str)
        {
            if (String.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str)); 
            return DatabaseClient.SanitizeString(str);
        }
         
        private IndexedDoc GenerateIndexedDoc(DocType docType, byte[] sourceData, string sourceUrl)
        {
            IndexedDoc doc = null;

            switch (docType)
            {
                case DocType.Html:
                    ParsedHtml html = new ParsedHtml();
                    html.LoadBytes(sourceData, sourceUrl);
                    doc = IndexedDoc.FromHtml(html, _IndexOptions);
                    break;
                case DocType.Json:
                    ParsedJson json = new ParsedJson();
                    json.LoadBytes(sourceData, sourceUrl);
                    doc = IndexedDoc.FromJson(json, _IndexOptions);
                    break;
                case DocType.Text:
                    ParsedText text = new ParsedText();
                    text.LoadBytes(sourceData, sourceUrl);
                    doc = IndexedDoc.FromText(text, _IndexOptions);
                    break;
                case DocType.Xml:
                    ParsedXml xml = new ParsedXml();
                    xml.LoadBytes(sourceData, sourceUrl);
                    doc = IndexedDoc.FromXml(xml, _IndexOptions);
                    break;
                default:
                    throw new ArgumentException("Unknown DocType");
            }

            return doc;
        }

        private bool WriteSourceDocument(byte[] data, IndexedDoc doc)
        {
            return Common.WriteFile(_SourceDocumentsDirectory + "/" + doc.MasterDocId, data);
        }

        private bool DeleteSourceDocument(string masterDocId)
        {
            return Common.DeleteFile(_SourceDocumentsDirectory + "/" + masterDocId);
        }

        private bool ReadSourceDocument(string masterDocId, out byte[] data)
        {
            data = null;

            if (!Common.FileExists(_SourceDocumentsDirectory + "/" + masterDocId))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexClient " + Name + " ReadSourceDocument " + masterDocId + " does not exist");
                return false;
            }

            data = Common.ReadBinaryFile(_SourceDocumentsDirectory + "/" + masterDocId);
            return true;
        }

        private bool WriteParsedDocument(IndexedDoc doc)
        {
            return Common.WriteFile(_ParsedDocumentsDirectory + "/" + doc.MasterDocId, Encoding.UTF8.GetBytes(Common.SerializeJson(doc, true)));
        }
        
        private bool DeleteParsedDocument(string masterDocId)
        {
            return Common.DeleteFile(_ParsedDocumentsDirectory + "/" + masterDocId);
        }

        private bool ReadParsedDocument(string masterDocId, out IndexedDoc doc)
        {
            doc = null;

            if (!Common.FileExists(_ParsedDocumentsDirectory + "/" + masterDocId))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexClient " + Name + " ReadParsedDocument " + masterDocId + " does not exist");
                return false;
            }

            byte[] data = Common.ReadBinaryFile(_ParsedDocumentsDirectory + "/" + masterDocId);

            try
            {
                doc = Common.DeserializeJson<IndexedDoc>(data);
                return true;
            }
            catch (Exception e)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexClient " + Name + " ReadParsedDocument exception while deserializing: " + e.Message);
                return false;
            }
        }

        private List<string> GetMatchingDocIdsByTerms(SearchQuery query)
        {
            List<string> ret = new List<string>();

            #region Build-Query

            string dbQuery =
                "SELECT DISTINCT MasterDocId " +
                "FROM Terms " +
                "WHERE (";

            #region Required-Terms

            // open paren, required terms subclause
            dbQuery += "(";

            dbQuery += "Term IN (";

            int requiredAdded = 0;
            foreach (string currTerm in query.Required.Terms)
            {
                string sanitizedTerm = String.Copy(currTerm);
                if (_IndexOptions.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                sanitizedTerm = Sanitize(sanitizedTerm);

                if (requiredAdded > 0) dbQuery += ",";
                dbQuery += "'" + sanitizedTerm + "'";
                requiredAdded++;
            }

            dbQuery += ")";

            // close paren, required terms subclause
            dbQuery += ")";

            #endregion

            #region Optional-Terms

            if (query.Optional != null && query.Optional.Terms != null && query.Optional.Terms.Count > 0)
            {
                // open paren, optional terms subclause
                dbQuery += " AND (Term IS NOT NULL OR ";

                dbQuery += "Term IN (";

                int optionalAdded = 0;
                foreach (string currTerm in query.Optional.Terms)
                {
                    string sanitizedTerm = String.Copy(currTerm);
                    if (_IndexOptions.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                    sanitizedTerm = Sanitize(sanitizedTerm);

                    if (optionalAdded > 0) dbQuery += ",";
                    dbQuery += "'" + sanitizedTerm + "'";
                    optionalAdded++;
                }

                dbQuery += ")";

                // close paren, optional terms subclause
                dbQuery += ")";
            }

            #endregion

            #region Excluded-Terms

            if (query.Exclude != null && query.Exclude.Terms != null && query.Exclude.Terms.Count > 0)
            {
                // open paren, exclude terms subclause
                dbQuery += " AND (";

                dbQuery += "Term NOT IN (";

                int excludeAdded = 0;
                foreach (string currTerm in query.Exclude.Terms)
                {
                    string sanitizedTerm = String.Copy(currTerm);
                    if (_IndexOptions.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                    sanitizedTerm = Sanitize(sanitizedTerm);

                    if (requiredAdded > 0) dbQuery += ",";
                    dbQuery += "'" + sanitizedTerm + "'";
                    excludeAdded++;
                }

                dbQuery += ")";

                // close paren, exclude terms subclause
                dbQuery += ")";
            }

            #endregion

            dbQuery += ") ";

            #region Pagination

            if (query.MaxResults != null && query.MaxResults > 0 && query.MaxResults <= 100)
            {
                dbQuery += "LIMIT " + query.MaxResults;

                if (query.StartIndex != null && query.StartIndex > 0)
                {
                    dbQuery += ", " + query.StartIndex;
                }
            }
            else
            {
                dbQuery += "LIMIT 10";
            }

            #endregion

            #endregion

            #region Retrieve-and-Respond

            DataTable result = _Database.Query(dbQuery);
            if (result == null || result.Rows.Count < 1) return ret;

            foreach (DataRow currRow in result.Rows)
            {
                ret.Add(currRow["MasterDocId"].ToString());
            }

            return ret;

            #endregion
        }

        private bool DocMatchesFilters(string masterDocId, SearchQuery query, out decimal score)
        {
            score = 1m;

            #region Read-Parsed-Object

            IndexedDoc doc = null;
            if (!ReadParsedDocument(masterDocId, out doc))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexClient " + Name + " DocMatchesFilters unable to read parsed document ID " + masterDocId);
                return false;
            }

            #endregion

            #region Process
             
            if (!RequiredFiltersMatch(doc, query))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + masterDocId + " does not match required filters");
                return false;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + masterDocId + " matches required filters");
            }
             
            if (!OptionalFiltersMatch(doc, query, out score))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + masterDocId + " does not match optional filters");
                return false;
            }
            else
            {
                score = Convert.ToDecimal(score.ToString("N4"));
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + masterDocId + " matches optional filters [score " + score + "]");
            }

            if (!ExcludeFiltersMatch(doc, query))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + masterDocId + " one or more exclude filters matched");
                return false;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Debug, "IndexClient " + Name + " DocMatchesFilters document ID " + masterDocId + " matches exclude filters");
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

            foreach (SearchQuery.SearchFilter currFilter in query.Required.Filter)
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
            score = 0m;
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

            foreach (SearchQuery.SearchFilter currFilter in query.Required.Filter)
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
            score = (decimal)matchCount / filterCount;
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

            foreach (SearchQuery.SearchFilter currFilter in query.Required.Filter)
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
         
        private bool FilterMatch(SearchQuery.SearchFilter filter, DataNode node)
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
                case SearchQuery.SearchCondition.Equals:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return false;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    dataString = node.Data.ToString();
                    if (filter.Value.Equals(dataString)) return true;
                    return false;

                case SearchQuery.SearchCondition.NotEquals:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return true;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    dataString = node.Data.ToString();
                    if (!filter.Value.Equals(dataString)) return true;
                    return false;

                case SearchQuery.SearchCondition.GreaterThan:
                    if (!Decimal.TryParse(filter.Value, out filterDecimal)) return false;
                    if (node.Data == null) return false;
                    if (!Decimal.TryParse(node.Data.ToString(), out dataDecimal)) return false;
                    if (dataDecimal > filterDecimal) return true;
                    return false;

                case SearchQuery.SearchCondition.GreaterThanOrEqualTo:
                    if (!Decimal.TryParse(filter.Value, out filterDecimal)) return false;
                    if (node.Data == null) return false;
                    if (!Decimal.TryParse(node.Data.ToString(), out dataDecimal)) return false;
                    if (dataDecimal >= filterDecimal) return true;
                    return false;

                case SearchQuery.SearchCondition.LessThan:
                    if (!Decimal.TryParse(filter.Value, out filterDecimal)) return false;
                    if (node.Data == null) return false;
                    if (!Decimal.TryParse(node.Data.ToString(), out dataDecimal)) return false;
                    if (dataDecimal < filterDecimal) return true;
                    return false;

                case SearchQuery.SearchCondition.LessThanOrEqualTo:
                    if (!Decimal.TryParse(filter.Value, out filterDecimal)) return false;
                    if (node.Data == null) return false;
                    if (!Decimal.TryParse(node.Data.ToString(), out dataDecimal)) return false;
                    if (dataDecimal <= filterDecimal) return true;
                    return false;

                case SearchQuery.SearchCondition.IsNull:
                    if (node.Data == null) return true;
                    return false;

                case SearchQuery.SearchCondition.IsNotNull:
                    if (node.Data != null) return true;
                    return false;

                case SearchQuery.SearchCondition.Contains:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return false;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    dataString = node.Data.ToString();
                    if (dataString.Contains(filter.Value)) return true;
                    return false;

                case SearchQuery.SearchCondition.ContainsNot:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return true;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    dataString = node.Data.ToString();
                    if (dataString.Contains(filter.Value)) return false;
                    return true; 

                case SearchQuery.SearchCondition.StartsWith:
                    if (String.IsNullOrEmpty(filter.Value) && node.Data == null) return true;
                    if (String.IsNullOrEmpty(filter.Value) && node.Data != null) return false;
                    if (!String.IsNullOrEmpty(filter.Value) && node.Data == null) return false;
                    dataString = node.Data.ToString();
                    if (dataString.StartsWith(filter.Value)) return true;
                    return false;

                case SearchQuery.SearchCondition.EndsWith:
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
            result.MarkStarted();

            try
            {
                #region Check-for-Null-Values

                if (query == null)
                {
                    error = new ErrorCode("MISSING_PARAM", "Query");
                    return false;
                }

                if (query.Required == null)
                {
                    error = new ErrorCode("MISSING_PARAM", "Required Filter");
                    return false;
                }

                if (query.Required.Terms == null || query.Required.Terms.Count < 1)
                {
                    error = new ErrorCode("MISSING_PARAM", "Required Terms");
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

                foreach (string currDocId in docIds)
                {
                    decimal score = 1m;
                    if (!DocMatchesFilters(currDocId, query, out score))
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

                #endregion

                #region Append-Documents

                if (filteredDocIds != null && filteredDocIds.Count > 0)
                {
                    List<SearchResult.Document> documents = new List<SearchResult.Document>();

                    foreach (string currDocId in filteredDocIds)
                    {
                        SearchResult.Document currDoc = new SearchResult.Document();
                        currDoc.MasterDocId = currDocId;
                        currDoc.Score = Convert.ToDecimal(scores[currDocId].ToString("N4"));

                        byte[] data = null;

                        if (query.IncludeParsedDoc)
                        {
                            data = Common.ReadBinaryFile(_ParsedDocumentsDirectory + "/" + currDocId);
                            if (data != null && data.Length > 0)
                            {
                                currDoc.Parsed = Common.DeserializeJson<dynamic>(data);
                            }
                        }

                        if (query.IncludeContent)
                        {
                            data = Common.ReadBinaryFile(_SourceDocumentsDirectory + "/" + currDocId);
                            if (data != null && data.Length > 0)
                            {
                                currDoc.Data = Encoding.UTF8.GetString(data);
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

        #endregion
    }
}
