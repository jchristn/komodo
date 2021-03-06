﻿using System;

namespace Komodo.Sdk.Classes
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
        public int MaxResults
        {
            get
            {
                return _MaxResults;
            }
            set
            {
                if (value < 1) throw new ArgumentException("MaxResults must be greater than zero.");
                if (value > 100) throw new ArgumentException("MaxResults must be one hundred or less.");
                _MaxResults = value;
            }
        }

        /// <summary>
        /// The starting index position for the search.
        /// </summary>
        public int StartIndex = 0;

        /// <summary>
        /// Required terms and search filter that must be satisfied to include a document in the results.
        /// </summary>
        public QueryFilter Required
        {
            get
            {
                return _Required;
            }
            set
            {
                if (value == null) _Required = new QueryFilter();
                else _Required = value;
            }
        }

        /// <summary>
        /// Optional terms and search filter that may match on documents but are not required.
        /// </summary>
        public QueryFilter Optional
        {
            get
            {
                return _Optional;
            }
            set
            {
                if (value == null) _Optional = new QueryFilter();
                else _Optional = value;
            }
        }

        /// <summary>
        /// Terms and search filter that must be excluded from the results.
        /// </summary>
        public QueryFilter Exclude
        {
            get
            {
                return _Exclude;
            }
            set
            {
                if (value == null) _Exclude = new QueryFilter();
                else _Exclude = value;
            }
        }

        /// <summary>
        /// Indicate whether document metadata should be included in the search result.
        /// </summary>
        public bool IncludeMetadata = false;

        #endregion

        #region Private-Members

        private int _MaxResults = 10;

        private QueryFilter _Required = new QueryFilter();
        private QueryFilter _Optional = new QueryFilter();
        private QueryFilter _Exclude = new QueryFilter();

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

        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return KomodoCommon.SerializeJson(this, pretty);
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
