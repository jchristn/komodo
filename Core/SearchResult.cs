using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;

namespace Komodo.Core
{
    /// <summary>
    /// Object returned as the result of a search against an index.
    /// </summary>
    public class SearchResult
    {
        #region Public-Members
        
        /// <summary>
        /// The search query performed.
        /// </summary>
        public SearchQuery Query { get; set; }

        /// <summary>
        /// True if the search query had a POSTback URL.
        /// </summary>
        public bool Async { get; set; }

        /// <summary>
        /// The name of the index that was queried.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// The time that the search started.
        /// </summary>
        public DateTime? StartTimeUtc { get; private set; }

        /// <summary>
        /// The time that the search ended.
        /// </summary>
        public DateTime? EndTimeUtc { get; private set; }

        /// <summary>
        /// The total number of milliseconds that elapsed while handling the search.
        /// </summary>
        public decimal? TotalTimeMs { get; private set; }

        /// <summary>
        /// Counts of documents that matched the query.
        /// </summary>
        public MatchCounts MatchCount { get; set; }

        /// <summary>
        /// List of terms that were not part of the index.
        /// </summary>
        public List<string> TermsNotFound { get; set; }

        /// <summary>
        /// Documents that matched the query.
        /// </summary>
        public List<Document> Documents { get; private set; } 

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public SearchResult()
        {
            Query = null;
            Async = false;
            IndexName = null;
            StartTimeUtc = null;
            EndTimeUtc = null;
            TotalTimeMs = null;
            MatchCount = null;
            TermsNotFound = new List<string>();
            Documents = new List<Document>();
        }

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        /// <param name="query">Search query.</param>
        public SearchResult(SearchQuery query)
        {
            Query = query;
            Async = false;
            StartTimeUtc = DateTime.Now.ToUniversalTime();
            EndTimeUtc = DateTime.Now.ToUniversalTime();
            TotalTimeMs = 0m;
            MatchCount = new MatchCounts();
            Documents = new List<Document>(); 
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Mark the query as having started.
        /// </summary>
        public void MarkStarted()
        {
            DateTime ts = DateTime.Now.ToUniversalTime();
            StartTimeUtc = ts;
            EndTimeUtc = null; 
            TotalTimeMs = null;
        }

        /// <summary>
        /// Mark the query as having ended.
        /// </summary>
        public void MarkEnded()
        {
            DateTime ts = DateTime.Now.ToUniversalTime();
            EndTimeUtc = ts;
            TimeSpan span = Convert.ToDateTime(EndTimeUtc) - Convert.ToDateTime(StartTimeUtc);
            TotalTimeMs = Convert.ToDecimal(span.TotalMilliseconds);
        }
         
        /// <summary>
        /// In-place descending sort of matching documents by the score assigned to each.
        /// </summary>
        public void SortMatchesByScore()
        {
            if (Documents == null || Documents.Count < 1) return;
            Documents = Documents.OrderByDescending(d => d.Score).ToList();
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
