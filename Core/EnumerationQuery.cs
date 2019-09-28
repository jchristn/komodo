using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Core
{
    /// <summary>
    /// Query to enumerate matching objects in Komodo.
    /// </summary>
    public class EnumerationQuery
    {
        #region Public-Members

        /// <summary>
        /// The GUID of the enumeration operation.
        /// </summary>
        public string GUID = null;

        /// <summary>
        /// Maximum number of results to retrieve.
        /// </summary>
        public int? MaxResults = 1000;

        /// <summary>
        /// The starting index position for the search.
        /// </summary>
        public int? StartIndex = 0;

        /// <summary>
        /// Search filters to apply to enumeration.
        /// </summary>
        public List<SearchFilter> Filters = new List<SearchFilter>();

        /// <summary>
        /// Specify a URL to which the results should be submitted via HTTP POST.
        /// </summary>
        public string PostbackUrl = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public EnumerationQuery()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
