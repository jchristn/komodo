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
    /// Object returned as the result of an enumeration against an index.
    /// </summary>
    public class EnumerationResult
    {
        #region Public-Members

        /// <summary>
        /// Error code associated with the enumeration request.
        /// </summary>
        public ErrorCode Error = new ErrorCode(ErrorId.NONE, null);

        /// <summary>
        /// The GUID of the enumeration operation.
        /// </summary>
        public string GUID = null;

        /// <summary>
        /// The enumeration query performed.
        /// </summary>
        public EnumerationQuery Query = null;

        /// <summary>
        /// True if the enumeration query had a POSTback URL.
        /// </summary>
        public bool Async = false;

        /// <summary>
        /// The name of the index that was enumerated.
        /// </summary>
        public string IndexName = null;

        /// <summary>
        /// The time that the enumeration started.
        /// </summary>
        public DateTime StartTimeUtc = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// The time that the enumeration ended.
        /// </summary>
        public DateTime EndTimeUtc = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// The total number of milliseconds that elapsed while handling the enumeration.
        /// </summary>
        public decimal TotalTimeMs = 0m;

        /// <summary>
        /// Source documents that matched the query.
        /// </summary>
        public List<SourceDocument> Matches = new List<SourceDocument>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public EnumerationResult()
        { 
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
        /// Mark the query as having ended.
        /// </summary>
        public void MarkEnded()
        {
            DateTime ts = DateTime.Now.ToUniversalTime();
            EndTimeUtc = ts;
            TimeSpan span = Convert.ToDateTime(EndTimeUtc) - Convert.ToDateTime(StartTimeUtc);
            TotalTimeMs = Convert.ToDecimal(span.TotalMilliseconds);
        }
         
        #endregion

        #region Private-Methods

        #endregion 
    }
}
