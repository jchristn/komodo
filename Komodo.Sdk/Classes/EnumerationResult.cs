using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komodo.Sdk.Classes
{
    /// <summary>
    /// Object returned as the result of an enumeration against an index.
    /// </summary>
    public class EnumerationResult
    {
        #region Public-Members

        /// <summary>
        /// Indicates if the statistics operation was successful.
        /// </summary>
        public bool Success = false;

        /// <summary>
        /// The name of the index that was queried.
        /// </summary>
        public string IndexName = null;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// The enumeration query performed.
        /// </summary>
        public EnumerationQuery Query = null;

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
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
