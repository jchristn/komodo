using System;
using System.Collections.Generic;
using System.Text;

namespace KomodoCore
{
    public class EnumerationQuery
    {
        #region Public-Members

        /// <summary>
        /// The GUID of the enumeration operation.
        /// </summary>
        public string GUID { get; private set; }

        /// <summary>
        /// Maximum number of results to retrieve.
        /// </summary>
        public int? MaxResults { get; set; }

        /// <summary>
        /// The starting index position for the search.
        /// </summary>
        public int? StartIndex { get; set; }

        /// <summary>
        /// Search filters to apply to enumeration.
        /// </summary>
        public List<SearchFilter> Filters { get; set; }

        /// <summary>
        /// Specify a URL to which the results should be submitted via HTTP POST.
        /// </summary>
        public string PostbackUrl { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

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
