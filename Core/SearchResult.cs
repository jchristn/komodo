using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;
using Komodo.Core.Enums;

namespace Komodo.Core
{
    /// <summary>
    /// Object returned as the result of a search against an index.
    /// </summary>
    public class SearchResult
    {
        #region Public-Members

        /// <summary>
        /// Error code associated with the operation.
        /// </summary>
        public ErrorCode Error = new ErrorCode(ErrorId.NONE);

        /// <summary>
        /// The search query performed.
        /// </summary>
        public SearchQuery Query = null;

        /// <summary>
        /// True if the search query had a POSTback URL.
        /// </summary>
        public bool Async = false;

        /// <summary>
        /// The name of the index that was queried.
        /// </summary>
        public string IndexName = null;

        /// <summary>
        /// Timestamp in UTC from the start of the operation.
        /// </summary>
        public DateTime StartTimeUtc = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Timestamp in UTC from the end of the operation.
        /// </summary>
        public DateTime EndTimeUtc = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Time in milliseconds elapsed while searching the index.
        /// </summary>
        public double TotalTimeMs = 0;

        /// <summary>
        /// The next start index to supply to continue the search.
        /// </summary>
        public int NextStartIndex = 0;

        /// <summary>
        /// Counts of documents that matched the query.
        /// </summary>
        public MatchCounts MatchCount = new MatchCounts();

        /// <summary>
        /// List of terms that were not part of the index.
        /// </summary>
        public List<string> TermsNotFound = new List<string>();

        /// <summary>
        /// Documents that matched the query.
        /// </summary>
        public List<MatchingDocument> Documents = new List<MatchingDocument>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public SearchResult()
        { 
        }

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        /// <param name="query">Search query.</param>
        public SearchResult(SearchQuery query)
        {
            Query = query; 
        }

        #endregion

        #region Public-Methods
          
        internal void MarkFinished()
        { 
            EndTimeUtc = DateTime.Now.ToUniversalTime();
            TimeSpan ts = EndTimeUtc - StartTimeUtc;
            TotalTimeMs = ts.TotalMilliseconds;
        }
          
        internal void SortMatchesByScore()
        {
            if (Documents == null || Documents.Count < 1) return;
            Documents = Documents.OrderByDescending(d => d.Score).ToList();
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
