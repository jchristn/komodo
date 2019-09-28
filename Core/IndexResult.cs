using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komodo.Core.Enums;

namespace Komodo.Core
{
    /// <summary>
    /// Response from a request to index a document.
    /// </summary>
    public class IndexResult
    {
        #region Public-Members

        /// <summary>
        /// Document ID.
        /// </summary>
        public string DocumentId = null;

        /// <summary>
        /// Indexed document.
        /// </summary>
        public IndexedDoc Document = null;

        /// <summary>
        /// Error code associated with the operation.
        /// </summary>
        public ErrorCode Error = new ErrorCode(ErrorId.NONE);

        /// <summary>
        /// Timestamp in UTC from the start of the operation.
        /// </summary>
        public DateTime StartTimeUtc = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Timestamp in UTC from the end of the operation.
        /// </summary>
        public DateTime EndTimeUtc = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Time in milliseconds elapsed adding the document.
        /// </summary>
        public double TotalTimeMs = 0;

        /// <summary>
        /// Time in milliseconds spent parsing the document.
        /// </summary>
        public double ParseTimeMs = 0;

        /// <summary>
        /// Time in milliseconds spent adding postings from the parsed document.
        /// </summary>
        public double PostingsTimeMs = 0;

        /// <summary>
        /// Time in milliseconds spent storing the document (i.e. writing to disk).
        /// </summary>
        public double StorageTimeMs = 0;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the IndexResponse.
        /// </summary>
        public IndexResult()
        {

        }
         
        #endregion

        #region Internal-Methods

        internal void MarkFinished()
        {
            EndTimeUtc = DateTime.Now.ToUniversalTime();
            TimeSpan ts = EndTimeUtc - StartTimeUtc;
            TotalTimeMs = ts.TotalMilliseconds;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
