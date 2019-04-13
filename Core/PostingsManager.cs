using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using SyslogLogging;
using Newtonsoft;
using Newtonsoft.Json;
using SqliteWrapper;
using Caching;

using Komodo.Core.Database;

namespace Komodo.Core
{
    /// <summary>
    /// Postings manager for an index.
    /// </summary>
    public class PostingsManager : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Set the default number of maximum results.  Must be between 1 and 1000.
        /// </summary>
        public int MaxResults
        {
            get
            {
                return _MaxResults;
            }
            set
            {
                if (value < 1) throw new ArgumentException("MaxResults must be one or greater.");
                if (value > 1000) throw new ArgumentException("MaxResults must be one thousand or less.");
                _MaxResults = value;
            }
        }

        #endregion

        #region Private-Members

        private bool _Disposed = false;

        private int _MaxResults = 1000; 

        private LoggingModule _Logging;
        private string _IndexName;
        private string _BaseDirectory;
        private string _DatabaseFilename;
        private bool _DatabaseDebug;
        private DatabaseClient _Database;
        private PostingsQueries _Queries;

        private LRUCache<string, DatabaseClient> _DbClientCache; // GUID, client

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the postings manager.
        /// </summary> 
        /// <param name="logging">Logging instance.</param>
        public PostingsManager(
            LoggingModule logging,
            string indexName,
            string baseDirectory,
            string dbFilename,
            bool dbDebug)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(baseDirectory)) throw new ArgumentNullException(nameof(baseDirectory));
            if (String.IsNullOrEmpty(dbFilename)) throw new ArgumentNullException(nameof(dbFilename));

            _Logging = logging;
            _IndexName = indexName;
            _BaseDirectory = baseDirectory;
            _DatabaseFilename = dbFilename;
            _DatabaseDebug = dbDebug; 

            if (!_BaseDirectory.EndsWith("/") && !_BaseDirectory.EndsWith("\\")) _BaseDirectory = _BaseDirectory + "/";

            if (!Directory.Exists(_BaseDirectory))
                Directory.CreateDirectory(_BaseDirectory);

            _Database = new DatabaseClient(_BaseDirectory + _DatabaseFilename, _DatabaseDebug);
            _Queries = new PostingsQueries(_Database);
            _DbClientCache = new LRUCache<string, DatabaseClient>(100, 20, false);

            InitializeTermDatabase();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Tear down and dispose of resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Add a posting.
        /// </summary>
        /// <param name="posting">Posting.</param>
        /// <returns>True if successful.</returns>
        public bool AddPosting(Posting posting)
        {
            if (posting == null) throw new ArgumentNullException(nameof(posting));

            TermMap map = null;
            DatabaseClient termsDatabase = null;
            string query = null;
            DataTable result = null;

            while (!TermExists(posting.Term, out map))
            {
                _Logging.Log(LoggingModule.Severity.Info, "[" + _IndexName + "] AddPosting creating database for term " + posting.Term);
                query = _Queries.AddTermMap(posting.Term);
                result = _Database.Query(query);
            }

            termsDatabase = new DatabaseClient(GetDatabaseFilename(map.GUID), _DatabaseDebug);
            query = _Queries.CreatePostingsTable();
            result = termsDatabase.Query(query);

            termsDatabase = GetDatabaseClient(map.GUID);
            query = _Queries.AddPosting(posting);
            result = termsDatabase.Query(query);
            return true;
        }

        /// <summary>
        /// Remove a document from a series of terms.
        /// </summary>
        /// <param name="documentId">Document ID.</param>
        /// <param name="terms">List of terms.</param>
        /// <returns>True if successful.</returns>
        public bool RemoveDocument(string documentId, List<string> terms)
        {
            if (String.IsNullOrEmpty(documentId)) throw new ArgumentNullException(nameof(documentId));

            foreach (string currTerm in terms)
            {
                TermMap map = null;
                if (!TermExists(currTerm, out map))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "[" + _IndexName + "] RemoveDocument unable to find term map for term " + currTerm + " while removing document ID " + documentId);
                    continue;
                }

                // remove posting
                DataTable result = null;
                string query = null;
                bool termEmpty = false;

                DatabaseClient termDb = GetDatabaseClient(map.GUID);
                query = _Queries.RemovePosting(documentId);
                result = termDb.Query(query);

                // remove posting if empty
                query = _Queries.GetPostingsCount();
                result = termDb.Query(query);
                if (result != null && result.Rows.Count > 0)
                {
                    if (result.Columns.Contains("NumPostings"))
                    {
                        if (result.Rows[0]["NumPostings"] != DBNull.Value)
                        {
                            long count = Convert.ToInt64(result.Rows[0]["NumPostings"]);
                            if (count == 0)
                            {
                                termEmpty = true;
                                _Logging.Log(LoggingModule.Severity.Info, "[" + _IndexName + "] RemoveDocument term " + currTerm + " is now empty");
                            }
                        }
                    }
                }

                // remove term if empty
                if (termEmpty)
                {
                    RemoveDatabaseClient(map.GUID);

                    try
                    {
                        File.Delete(GetDatabaseFilename(map.GUID));
                    }
                    catch (Exception e)
                    {
                        _Logging.Log(LoggingModule.Severity.Warn, "[" + _IndexName + "] RemoveDocument cannot remove term database " + GetDatabaseFilename(map.GUID) + ": " + e.Message);
                    }

                    query = _Queries.RemoveTermMap(currTerm);
                    result = _Database.Query(query);
                }
            }

            return true;
        }

        /// <summary>
        /// Determine whether or not a term map exists for a specific term.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <param name="map">TermMap.</param>
        /// <returns>True if exists.</returns>
        public bool TermExists(string term, out TermMap map)
        {
            map = null;
            if (String.IsNullOrEmpty(term)) throw new ArgumentNullException(nameof(term));

            string query = _Queries.GetTermMap(term);
            DataTable result = _Database.Query(query);

            if (result != null && result.Rows.Count > 0)
            {
                map = TermMap.FromDataRow(result.Rows[0]);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the list of terms stored.
        /// </summary>
        /// <returns>List of terms.</returns>
        public List<string> GetTerms()
        {
            string query = _Queries.GetTerms();
            DataTable result = _Database.Query(query);

            List<string> ret = new List<string>();
            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow curr in result.Rows)
                {
                    ret.Add(curr["Term"].ToString());
                }
            }

            return ret;
        }

        /// <summary>
        /// Retrieve document IDs that match a series of required, optional, and excluded terms.
        /// </summary>
        /// <param name="startIndex">Start index for each database query.</param>
        /// <param name="maxResults">Maximum number of results to retrieve from each database query.</param>
        /// <param name="requiredTerms">List of required terms.</param>
        /// <param name="optionalTerms">List of optional terms.</param>
        /// <param name="excludeTerms">List of exclude terms.</param>
        /// <param name="matchingDocs">Dictionary containing matching document IDs and their respective scores.</param>
        /// <param name="indexEnd">The value for startIndex that should be used for a subsequent retrieval to continue the search.</param>
        /// <returns></returns>
        public void GetMatchingDocuments(
            long startIndex,
            int maxResults,
            List<string> requiredTerms,
            List<string> optionalTerms,
            List<string> excludeTerms,
            out Dictionary<string, decimal> matchingDocs,
            out long indexEnd
            )
        {
            if (maxResults < 1) throw new ArgumentException("Max results must be greater than zero.");
            if (maxResults > _MaxResults) throw new ArgumentException("Max results must not exceed " + _MaxResults);
            if (requiredTerms == null || requiredTerms.Count < 1) throw new ArgumentNullException(nameof(requiredTerms));

            if (requiredTerms != null) requiredTerms = requiredTerms.Distinct().ToList();
            if (excludeTerms != null) excludeTerms = excludeTerms.Distinct().ToList();
            if (optionalTerms != null) optionalTerms = optionalTerms.Distinct().ToList();
            
            matchingDocs = new Dictionary<string, decimal>();      // document ID, score
            Dictionary<string, int> matchingDocsTemp = new Dictionary<string, int>();        // document ID, optional docs matching

            indexEnd = startIndex;
            DatabaseClient termDb = null;

            #region Gather-and-Process-Matches

            bool endOfRecords = false;
            bool endOfTerms = false;

            while (true)
            {
                _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments index " + indexEnd + " max results " + maxResults);

                #region Required-Terms

                for (int i = 0; i < requiredTerms.Count; i++) 
                {
                    string guid = GuidFromTerm(requiredTerms[i]);
                    if (String.IsNullOrEmpty(guid))
                    {
                        _Logging.Log(LoggingModule.Severity.Warn, "[" + _IndexName + "] GetMatchingDocuments unable to find GUID for term " + requiredTerms[i]);
                        if ((i + 1) >= requiredTerms.Count)
                        {
                            endOfTerms = true;
                            break;
                        }
                        continue;
                    }

                    termDb = GetDatabaseClient(guid);
                    List<Posting> matching = GetMatches(termDb, indexEnd, maxResults * 2);

                    if (matching == null || matching.Count < 1)
                    {
                        _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments no postings for required term " + requiredTerms[i]);
                        endOfRecords = true;
                        break;
                    }
                    else
                    {
                        foreach (Posting currMatch in matching)
                        {
                            if (matchingDocsTemp.Count < 1)
                            {
                                _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments document ID " + currMatch.MasterDocId + " matches required term " + requiredTerms[i]);
                                matchingDocsTemp.Add(currMatch.MasterDocId, 0);
                            }
                            else
                            {
                                if (matchingDocsTemp.ContainsKey(currMatch.MasterDocId))
                                {
                                    _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments skipping document ID " + currMatch.MasterDocId + ", already matches current term " + requiredTerms[i]);
                                }
                                else
                                {
                                    _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments document ID " + currMatch.MasterDocId + " matches required term " + requiredTerms[i]);
                                    matchingDocsTemp.Add(currMatch.MasterDocId, 0);
                                }
                            }
                        }

                        indexEnd += maxResults;
                    }
                }

                #endregion

                #region Excluded-Terms

                if (excludeTerms != null && excludeTerms.Count > 0)
                {
                    for (int i = 0; i < excludeTerms.Count; i++)
                    {
                        string guid = GuidFromTerm(excludeTerms[i]);
                        termDb = GetDatabaseClient(guid);
                        List<Posting> excluded = GetMatches(termDb, indexEnd, maxResults * 2);
                        if (excluded == null || excluded.Count < 1)
                        {
                            break;
                        }
                        else
                        {
                            foreach (Posting currMatch in excluded)
                            {
                                _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments removing document due to excluded term " + excludeTerms[i] + ": " + currMatch.MasterDocId);
                                if (matchingDocsTemp.ContainsKey(currMatch.MasterDocId))
                                    matchingDocsTemp.Remove(currMatch.MasterDocId);
                            }
                        }
                    }
                }

                #endregion

                #region Optional-Terms

                if (optionalTerms != null && optionalTerms.Count > 0)
                {
                    for (int i = 0; i < optionalTerms.Count; i++)
                    {
                        string guid = GuidFromTerm(optionalTerms[i]);
                        termDb = GetDatabaseClient(guid);
                        List<Posting> optional = GetMatches(termDb, indexEnd, maxResults * 2);
                        if (optional == null || optional.Count < 1)
                        {
                            break;
                        }
                        else
                        {
                            foreach (Posting currMatch in optional)
                            {
                                _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments document ID " + currMatch.MasterDocId + " matches optional term " + optionalTerms[i]);

                                if (matchingDocsTemp.ContainsKey(currMatch.MasterDocId))
                                {
                                    int count = matchingDocsTemp[currMatch.MasterDocId];
                                    matchingDocsTemp.Remove(currMatch.MasterDocId);
                                    count++;
                                    matchingDocsTemp.Add(currMatch.MasterDocId, count);
                                }
                            }
                        }
                    }
                }

                #endregion

                #region Check-Result-Count-and-Exit-Flags

                if (matchingDocsTemp.Count > maxResults)
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments matching documents equal to or greater than requested count");
                    break;
                }

                if (endOfRecords)
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments end of records reached");
                    break;
                }

                if (endOfTerms)
                {
                    _Logging.Log(LoggingModule.Severity.Debug, "[" + _IndexName + "] GetMatchingDocuments end of terms reached");
                    break;
                }

                #endregion
            }

            #endregion

            #region Populate-Results

            if (matchingDocsTemp.Count > 0)
            {
                foreach (KeyValuePair<string, int> currMatch in matchingDocsTemp)
                {
                    if (optionalTerms != null && optionalTerms.Count > 0)
                    {
                        if (currMatch.Value == 0)
                        {
                            matchingDocs.Add(currMatch.Key, 0);
                        }
                        else
                        {
                            matchingDocs.Add(currMatch.Key, Convert.ToDecimal(currMatch.Value) / optionalTerms.Count);

                            _Logging.Log(LoggingModule.Severity.Debug, 
                                "[" + _IndexName + "] GetMatchingDocuments" +
                                " document " + currMatch.Key +
                                " score " + currMatch.Value +
                                "/" + optionalTerms.Count +
                                ": " +
                                (Convert.ToDecimal(currMatch.Value) / optionalTerms.Count));
                        }
                    }
                    else
                    {
                        matchingDocs.Add(currMatch.Key, 1);
                    }
                }
            }

            #endregion

            return;
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
                if (_DbClientCache.Count() > 0)
                {
                    List<string> keys = _DbClientCache.GetKeys();
                    foreach (string key in keys)
                    {
                        DatabaseClient db = null;
                        if (_DbClientCache.TryGet(key, out db))
                        {
                            db.Dispose();
                        }
                    }
                }
            }

            _Disposed = true;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void InitializeTermDatabase()
        {
            string query = _Queries.CreateTermMapTable();
            DataTable result = _Database.Query(query);
        }

        private void InitializePostingDatabase(string guid)
        {
            string filename = GetDatabaseFilename(guid);
            DatabaseClient db = new DatabaseClient(filename, _DatabaseDebug);
            string query = _Queries.CreatePostingsTable();
            DataTable result = db.Query(query);
        }

        private bool AddTermMap(string term, out TermMap map)
        {
            if (String.IsNullOrEmpty(term)) throw new ArgumentNullException(nameof(term));

            map = null;
            string query = _Queries.AddTermMap(term);
            DataTable result = _Database.Query(query);

            query = _Queries.GetTermMap(term);
            result = _Database.Query(query);
            if (result != null && result.Rows.Count > 0)
            {
                map = TermMap.FromDataRow(result.Rows[0]);
                return true;
            }
            else
            {
                return false;
            }
        }

        private string GetDatabaseFilename(string termGuid)
        {
            return _BaseDirectory + termGuid + ".db";
        }

        private DatabaseClient GetDatabaseClient(string termGuid)
        {
            DatabaseClient ret = null;
            if (_DbClientCache.TryGet(termGuid, out ret))
            {
                return ret;
            }
            else
            {
                ret = new DatabaseClient(GetDatabaseFilename(termGuid), _DatabaseDebug);
                string query = _Queries.CreatePostingsTable();
                DataTable result = ret.Query(query);
                _DbClientCache.AddReplace(termGuid, ret);
                return ret;
            }
        }

        private void RemoveDatabaseClient(string termGuid)
        {
            DatabaseClient db = null;
            _DbClientCache.TryGet(termGuid, out db);
            if (db != null) db.Dispose();
        }

        private List<Posting> GetMatches(DatabaseClient db, long startIndex, int maxResults)
        {
            List<Posting> ret = new List<Posting>();

            string query = _Queries.GetPostings(maxResults, startIndex);
            DataTable result = db.Query(query);
            if (result == null || result.Rows.Count < 1)
            {
                return ret;
            }

            foreach (DataRow currRow in result.Rows)
            {
                ret.Add(Posting.FromDataRow(currRow)); 
            }

            return ret;
        }

        private string GuidFromTerm(string term)
        {
            string query = _Queries.GetTermMap(term);
            DataTable result = _Database.Query(query);
            if (result != null && result.Rows.Count > 0)
            {
                string guid = result.Rows[0]["GUID"].ToString();
                return guid;
            }
            return null;
        }

        #endregion
    }
}
