using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using SyslogLogging;
using Newtonsoft;
using Newtonsoft.Json;
using SqliteWrapper; 

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

        /// <summary>
        /// Set the maximum number of terms supported.  Must be between 1 and 32.  
        /// </summary>
        public int MaxTerms
        {
            get
            {
                return _MaxTerms;
            }
            set
            {
                if (value < 1 || value > 32) throw new ArgumentException("MaxTerms must be between 1 and 32.");
                _MaxTerms = value;
            }
        }

        #endregion

        #region Private-Members

        private bool _Disposed = false;

        private int _MaxResults = 1000;
        private int _MaxTerms = 32;

        private Index _Index;
        private LoggingModule _Logging;

        private DatabaseWrapper.DatabaseClient _PostingsDbSql = null;
        private SqliteWrapper.DatabaseClient _PostingsDbSqlite = null;

        private PostingsQueries _Queries;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the postings manager.
        /// </summary> 
        /// <param name="index">Index object.</param> 
        /// <param name="logging">Logging instance.</param>
        /// <param name="sqlDatabase">Database client (if using Microsoft SQL Server, MySQL, or PostgreSQL), otherwise, use null.</param>
        /// <param name="sqliteDatabase">Sqlite database filename (if using Sqlite), otherwise, use null.</param>
        public PostingsManager(
            Index index,
            LoggingModule logging,
            DatabaseWrapper.DatabaseClient sqlDatabase,
            SqliteWrapper.DatabaseClient sqliteDatabase)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            if (sqlDatabase == null && sqliteDatabase == null) throw new ArgumentException("Only one database client must be supplied.");
            if (sqlDatabase != null && sqliteDatabase != null) throw new ArgumentException("Only one database client must be supplied.");

            DateTime ts = DateTime.Now;
            _Logging = logging;
            _Index = index;
            _Logging.Info("PostingsManager starting for index " + _Index.IndexName);
             
            _PostingsDbSql = sqlDatabase;
            _PostingsDbSqlite = sqliteDatabase; 
            _Queries = new PostingsQueries(_Index, _PostingsDbSql, _PostingsDbSqlite);

            _Logging.Info("PostingsManager started for index " + _Index.IndexName + " [" + Common.TotalMsFrom(ts) + "ms]");
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
            string query = null;
            DataTable result = null;

            while (!TermExists(posting.Term, out map))
            {
                _Logging.Debug("[" + _Index.IndexName + "] AddPosting creating table for term " + posting.Term);
                query = _Queries.AddTermMap(posting.Term);
                result = DatabaseQuery(query);
            }
             
            query = _Queries.CreatePostingsTable(map.GUID);
            result = DatabaseQuery(query);
             
            query = _Queries.AddPosting(posting, map.GUID);
            result = DatabaseQuery(query);
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
                    _Logging.Warn("[" + _Index.IndexName + "] RemoveDocument unable to find term map for term " + currTerm + " while removing document ID " + documentId);
                    continue;
                }

                // remove posting
                DataTable result = null;
                string query = null;
                bool termEmpty = false;
                 
                query = _Queries.RemovePosting(documentId, currTerm);
                result = DatabaseQuery(query);

                // remove posting if empty
                query = _Queries.GetPostingsCount(currTerm);
                result = DatabaseQuery(query);
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
                                _Logging.Debug("[" + _Index.IndexName + "] RemoveDocument term " + currTerm + " is now empty");
                            }
                        }
                    }
                }

                // remove term if empty
                if (termEmpty)
                {
                    query = _Queries.DeletePostingsTable(currTerm);
                    DatabaseQuery(query);

                    query = _Queries.RemoveTermMap(currTerm);
                    result = DatabaseQuery(query);
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
            DataTable result = DatabaseQuery(query);

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
            DataTable result = DatabaseQuery(query);

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
        /// Get the count of the number of terms stored.
        /// </summary>
        /// <returns>Long.</returns>
        public long GetTermsCount()
        {
            string query = _Queries.GetTermsCount();
            DataTable result = DatabaseQuery(query);

            if (result != null && result.Rows.Count > 0)
            {
                return Convert.ToInt64(result.Rows[0]["NumTerms"]);
            }

            return 0;
        }

        /// <summary>
        /// Retrieve document IDs that match a series of required, optional, and excluded terms.
        /// </summary>
        /// <param name="startIndex">Start index for each database query.</param>
        /// <param name="maxResults">Maximum number of results to retrieve from each database query.</param>
        /// <param name="requiredTerms">List of required terms.</param>
        /// <param name="optionalTerms">List of optional terms.</param>
        /// <param name="excludeTerms">List of exclude terms.</param>
        /// <param name="termsNotFound">List of terms not found.</param>
        /// <param name="matchingDocs">Dictionary containing matching document IDs and their respective scores.</param>
        /// <param name="indexEnd">The value for startIndex that should be used for a subsequent retrieval to continue the search.</param>
        /// <returns></returns>
        public void GetMatchingDocuments(
            int startIndex,
            int maxResults,
            List<string> requiredTerms,
            List<string> optionalTerms,
            List<string> excludeTerms,
            out List<string> termsNotFound,
            out Dictionary<string, decimal> matchingDocs,
            out int indexEnd
            )
        {
            #region Check-Input-and-Initialize

            if (maxResults < 1) throw new ArgumentException("Max results must be greater than zero.");
            if (maxResults > _MaxResults) throw new ArgumentException("Max results must not exceed " + _MaxResults);
            if (requiredTerms == null || requiredTerms.Count < 1) throw new ArgumentNullException(nameof(requiredTerms));

            if (requiredTerms != null) requiredTerms = requiredTerms.Distinct().ToList();
            if (excludeTerms != null) excludeTerms = excludeTerms.Distinct().ToList();
            if (optionalTerms != null) optionalTerms = optionalTerms.Distinct().ToList();

            if (requiredTerms != null && requiredTerms.Count > _MaxTerms) throw new ArgumentException("Required terms count must not exceed " + _MaxTerms + ".");
            if (excludeTerms != null && excludeTerms.Count > _MaxTerms) throw new ArgumentException("Exclude terms count must not exceed " + _MaxTerms + ".");
            if (optionalTerms != null && optionalTerms.Count > _MaxTerms) throw new ArgumentException("Optional terms count must not exceed " + _MaxTerms + ".");

            termsNotFound = new List<string>();
            matchingDocs = new Dictionary<string, decimal>();      // document ID, score
            Dictionary<string, int> matchingDocsTemp = new Dictionary<string, int>();        // document ID, optional docs matching

            indexEnd = startIndex; 

            #endregion

            #region Get-Term-Maps

            List<TermMap> requiredTermsMaps = new List<TermMap>();
            List<TermMap> excludeTermsMaps = new List<TermMap>();
            List<TermMap> optionalTermsMaps = new List<TermMap>();

            if (requiredTerms != null && requiredTerms.Count > 0)
            {   
                foreach (string curr in requiredTerms)
                {
                    TermMap currTermMap = null;
                    if (!TermExists(curr, out currTermMap))
                    {
                        termsNotFound.Add(curr);
                    }
                    else
                    {
                        if (currTermMap != null && currTermMap != default(TermMap))
                            requiredTermsMaps.Add(currTermMap);
                    }
                }
            }

            if (excludeTerms != null && excludeTerms.Count > 0)
            {
                foreach (string curr in excludeTerms)
                {
                    TermMap currTermMap = null;
                    if (!TermExists(curr, out currTermMap))
                    {
                        termsNotFound.Add(curr);
                    }
                    else
                    {
                        if (currTermMap != null && currTermMap != default(TermMap))
                            excludeTermsMaps.Add(currTermMap);
                    }
                }
            }

            if (optionalTerms != null && optionalTerms.Count > 0)
            {
                foreach (string curr in optionalTerms)
                {
                    TermMap currTermMap = null;
                    if (!TermExists(curr, out currTermMap))
                    {
                        termsNotFound.Add(curr);
                    }
                    else
                    {
                        if (currTermMap != null && currTermMap != default(TermMap))
                            optionalTermsMaps.Add(currTermMap);
                    }
                }
            }

            #endregion

            #region Gather-and-Process-Matches

            bool endOfRecords = false;
            bool endOfTerms = false;

            while (true)
            {
                _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments index " + startIndex + " max results " + maxResults);

                #region Required-Terms

                for (int i = 0; i < requiredTerms.Count; i++) 
                {
                    string guid = GuidFromTerm(requiredTerms[i]);
                    if (String.IsNullOrEmpty(guid))
                    {
                        _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments unable to find GUID for required term " + requiredTerms[i]);
                        termsNotFound.Add(requiredTerms[i]);
                        if ((i + 1) >= requiredTerms.Count)
                        {
                            endOfTerms = true;
                            break;
                        }
                        continue;
                    }

                    List<Posting> matching = GetMatches(guid, startIndex, maxResults * 2);

                    if (matching == null || matching.Count < 1)
                    {
                        _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments no postings for required term " + requiredTerms[i]);
                        endOfRecords = true;
                        break;
                    } 

                    foreach (Posting currMatch in matching)
                    {
                        if (matchingDocsTemp.Count == 0) // first document
                        {
                            _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments document ID " + currMatch.DocumentId + " matches required term " + requiredTerms[i]);
                            matchingDocsTemp.Add(currMatch.DocumentId, 0);
                        }
                        else // subsequent document
                        {
                            if (matchingDocsTemp.ContainsKey(currMatch.DocumentId))
                            {
                                _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments skipping document ID " + currMatch.DocumentId + ", already matched");
                            }
                            else
                            {
                                _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments document ID " + currMatch.DocumentId + " matches required term " + requiredTerms[i]);
                                matchingDocsTemp.Add(currMatch.DocumentId, 0);
                            }
                        }
                    }

                    startIndex += maxResults; 
                }

                #endregion

                #region Excluded-Terms

                if (excludeTerms != null && excludeTerms.Count > 0)
                {
                    for (int i = 0; i < excludeTerms.Count; i++)
                    {
                        string guid = GuidFromTerm(excludeTerms[i]);
                        if (!String.IsNullOrEmpty(guid))
                        {
                            List<Posting> excluded = GetMatches(guid, indexEnd, maxResults * 2);
                            if (excluded == null || excluded.Count == 0) break;

                            foreach (Posting currMatch in excluded)
                            {
                                _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments removing document due to excluded term " + excludeTerms[i] + ": " + currMatch.DocumentId);
                                if (matchingDocsTemp.ContainsKey(currMatch.DocumentId))
                                {
                                    matchingDocsTemp.Remove(currMatch.DocumentId);
                                }
                            }
                        }
                        else
                        {
                            _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments unable to find GUID for exclude term " + excludeTerms[i]);
                            termsNotFound.Add(excludeTerms[i]);
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
                        if (!String.IsNullOrEmpty(guid))
                        {
                            List<Posting> optional = GetMatches(guid, indexEnd, maxResults * 2);
                            if (optional == null || optional.Count == 0) break;

                            foreach (Posting currMatch in optional)
                            {
                                _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments document ID " + currMatch.DocumentId + " matches optional term " + optionalTerms[i]);

                                if (matchingDocsTemp.ContainsKey(currMatch.DocumentId))
                                {
                                    int count = matchingDocsTemp[currMatch.DocumentId];
                                    matchingDocsTemp.Remove(currMatch.DocumentId);
                                    count++;
                                    matchingDocsTemp.Add(currMatch.DocumentId, count);
                                }
                            }
                        }
                        else
                        {
                            _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments unable to find GUID for optional term " + optionalTerms[i]);
                            termsNotFound.Add(optionalTerms[i]);
                        }
                    }
                }

                #endregion

                #region Check-Result-Count-and-Exit-Flags

                if (matchingDocsTemp.Count > maxResults)
                {
                    _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments matching documents equal to or greater than requested count");
                    break;
                }

                if (endOfRecords)
                {
                    _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments end of records reached");
                    break;
                }

                if (endOfTerms)
                {
                    _Logging.Debug("[" + _Index.IndexName + "] GetMatchingDocuments end of terms reached");
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

                            _Logging.Debug(
                                "[" + _Index.IndexName + "] GetMatchingDocuments" +
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

            indexEnd = startIndex;
            termsNotFound = termsNotFound.Distinct().ToList();
            return;
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Tear down and dispose of resources.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_Disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_PostingsDbSql != null) _PostingsDbSql.Dispose();
                if (_PostingsDbSqlite != null) _PostingsDbSqlite.Dispose(); 
            }

            _Disposed = true;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
         
        private bool AddTermMap(string term, out TermMap map)
        {
            if (String.IsNullOrEmpty(term)) throw new ArgumentNullException(nameof(term));

            map = null;
            string query = _Queries.AddTermMap(term);
            DataTable result = DatabaseQuery(query);

            query = _Queries.GetTermMap(term);
            result = DatabaseQuery(query);
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
         
        private List<Posting> GetMatches(string termGuid, long startIndex, int maxResults)
        {
            List<Posting> ret = new List<Posting>();

            string query = _Queries.GetPostings(maxResults, startIndex, termGuid);
            DataTable result = DatabaseQuery(query);
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
            DataTable result = DatabaseQuery(query);
            if (result != null && result.Rows.Count > 0)
            {
                string guid = result.Rows[0]["GUID"].ToString();
                return guid;
            }
            return null;
        }

        private DataTable DatabaseQuery(string query)
        {
            if (_PostingsDbSql != null) return _PostingsDbSql.Query(query);
            else return _PostingsDbSqlite.Query(query);
        }

        #endregion
    }
}
