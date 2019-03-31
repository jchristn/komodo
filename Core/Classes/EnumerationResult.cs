using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;

namespace KomodoCore
{
    /// <summary>
    /// Object returned as the result of an enumeration against an index.
    /// </summary>
    public class EnumerationResult
    {
        #region Public-Members

        /// <summary>
        /// The GUID of the enumeration operation.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// The enumeration query performed.
        /// </summary>
        public EnumerationQuery Query { get; set; }

        /// <summary>
        /// True if the enumeration query had a POSTback URL.
        /// </summary>
        public bool Async { get; set; }

        /// <summary>
        /// The name of the index that was enumerated.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// The time that the enumeration started.
        /// </summary>
        public DateTime? StartTimeUtc { get; private set; }

        /// <summary>
        /// The time that the enumeration ended.
        /// </summary>
        public DateTime? EndTimeUtc { get; private set; }

        /// <summary>
        /// The total number of milliseconds that elapsed while handling the enumeration.
        /// </summary>
        public decimal? TotalTimeMs { get; private set; }
         
        /// <summary>
        /// Source documents that matched the query.
        /// </summary>
        public List<SourceDocument> Matches { get; private set; } 

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public EnumerationResult()
        {
            Query = null;
            Async = false;
            IndexName = null;
            StartTimeUtc = null;
            EndTimeUtc = null;
            TotalTimeMs = null;
            Matches = new List<SourceDocument>(); 
        }

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        /// <param name="query">Enumeration query.</param>
        public EnumerationResult(EnumerationQuery query)
        {
            Query = query;
            Async = false;
            StartTimeUtc = DateTime.Now.ToUniversalTime();
            EndTimeUtc = DateTime.Now.ToUniversalTime();
            TotalTimeMs = 0m; 
            Matches = new List<SourceDocument>();
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
        /// Attach matching documents to the results.
        /// </summary>
        /// <param name="documents">List of source documents.</param>
        public void AttachResults(List<SourceDocument> documents)
        {
            if (documents != null)
            {
                foreach (SourceDocument curr in documents)
                {
                    Matches.Add(curr);
                } 
            }
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
