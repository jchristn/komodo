using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;

namespace KomodoCore
{
    /// <summary>
    /// Object returned as the result of a search against an index.
    /// </summary>
    public class SearchResult
    {
        #region Public-Members

        /// <summary>
        /// The GUID of the search operation.
        /// </summary>
        public string GUID { get; set; }

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
        public MatchCounts MatchCount { get; private set; } 

        /// <summary>
        /// Documents that matched the query.
        /// </summary>
        public List<Document> Matches { get; private set; } 

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
            Matches = new List<Document>(); 
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
            Matches = new List<Document>(); 
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
        /// Set the number of documents that matched the supplied terms.
        /// </summary>
        /// <param name="count"></param>
        public void SetTermsMatchCount(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            MatchCount.TermsMatch = count;
        }

        /// <summary>
        /// Attach matching documents to the results.
        /// </summary>
        /// <param name="documents">List of documents.</param>
        public void AttachResults(List<Document> documents)
        {
            if (documents != null)
            {
                foreach (Document curr in documents)
                {
                    Matches.Add(curr);
                }

                MatchCount.FilterMatch = documents.Count;
            }
        }

        /// <summary>
        /// In-place descending sort of matching documents by the score assigned to each.
        /// </summary>
        public void SortMatchesByScore()
        {
            if (Matches == null || Matches.Count < 1) return;
            Matches = Matches.OrderByDescending(d => d.Score).ToList();
        }

        #endregion

        #region Private-Methods

        #endregion

        #region Subordinate-Classes
         
        /// <summary>
        /// A document matching the search query.
        /// </summary>
        public class Document
        {
            /// <summary>
            /// Master document ID.
            /// </summary>
            public string MasterDocId { get; set; }

            /// <summary>
            /// The type of document.
            /// </summary>
            public DocType DocumentType { get; set; }

            /// <summary>
            /// The score of the document, between 0 and 1.  Only relevant when optional filters are supplied in the search.
            /// </summary>
            public decimal Score { get; set; }

            /// <summary>
            /// Error description strings, if any.
            /// </summary>
            public List<string> Errors { get; set; }

            /// <summary>
            /// The document's data, if requested in the search query.
            /// </summary>
            public object Data { get; set; }

            /// <summary>
            /// The parsed document, if requested in the search query.
            /// </summary>
            public object Parsed { get; set; }

            /// <summary>
            /// Instantiates the object.
            /// </summary>
            public Document()
            {
                MasterDocId = null;
                DocumentType = DocType.Unknown;
                Score = 0m;
                Errors = new List<string>();
                Data = null;
                Parsed = null;
            }
        }

        /// <summary>
        /// Provides details about how many documents matched the supplied search.
        /// </summary>
        public class MatchCounts
        {
            /// <summary>
            /// The number of documents that matched the specified terms.
            /// </summary>
            public int TermsMatch { get; set; }

            /// <summary>
            /// The number of documents that matchd the specified terms and the specified filters.
            /// </summary>
            public int FilterMatch { get; set; }

            /// <summary>
            /// Instantiates the object.
            /// </summary>
            public MatchCounts()
            {
                TermsMatch = 0;
                FilterMatch = 0;
            }
        }

        #endregion
    }
}
