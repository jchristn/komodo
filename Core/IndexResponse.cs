using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komodo.Core
{
    public class IndexResponse
    {
        #region Public-Members

        /// <summary>
        /// Document ID.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// Time in milliseconds elapsed adding the document.
        /// </summary>
        public long AddTimeMs { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the IndexResponse.
        /// </summary>
        public IndexResponse()
        {

        }

        /// <summary>
        /// Instantiates the IndexResponse.
        /// </summary>
        /// <param name="docId">Document ID.</param>
        /// <param name="addTimeMs">Time in milliseconds elapsed adding the document.</param>
        public IndexResponse(string docId, long addTimeMs)
        {
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));
            if (addTimeMs < 0) throw new ArgumentException("addTimeMs must be 0 or greater.");

            DocumentId = docId;
            AddTimeMs = addTimeMs;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
