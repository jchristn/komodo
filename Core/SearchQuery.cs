using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Komodo.Core
{
    /// <summary>
    /// Object used to search an index.
    /// </summary>
    public class SearchQuery
    {
        #region Public-Members

        /// <summary>
        /// The GUID of the search operation.
        /// </summary>
        public string GUID = Guid.NewGuid().ToString();

        /// <summary>
        /// Maximum number of results to retrieve.
        /// </summary>
        public int MaxResults = 1000;

        /// <summary>
        /// The starting index position for the search.
        /// </summary>
        public int StartIndex = 0;

        /// <summary>
        /// Required terms and search filter that must be satisfied to include a document in the results.
        /// </summary>
        public QueryFilter Required = new QueryFilter();

        /// <summary>
        /// Optional terms and search filter that may match on documents but are not required.
        /// </summary>
        public QueryFilter Optional = new QueryFilter();

        /// <summary>
        /// Terms and search filter that must be excluded from the results.
        /// </summary>
        public QueryFilter Exclude = new QueryFilter();
         
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
        public SearchQuery()
        {
            GUID = Guid.NewGuid().ToString();
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion 
    }
}
