using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Object used to enumerate an index.
    /// </summary>
    public class EnumerationQuery
    {
        #region Public-Members

        /// <summary>
        /// The GUID of the enumeration operation.
        /// </summary>
        [JsonProperty(Order = -1)]
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
                if (value > 1000) throw new ArgumentException("MaxResults must be one thousand or less.");
                _MaxResults = value;
            }
        }

        /// <summary>
        /// The starting index position for the enumeration.
        /// </summary>
        public int StartIndex = 0;

        /// <summary>
        /// Search filters to apply to enumeration.
        /// </summary>
        [JsonProperty(Order = 990)]
        public List<SearchFilter> Filters
        {
            get
            {
                return _Filters;
            }
            set
            {
                if (value == null)
                {
                    _Filters = new List<SearchFilter>();
                }
                else
                {
                    _Filters = value;
                }
            }
        }

        #endregion

        #region Private-Members

        private int _MaxResults = 1000;
        private List<SearchFilter> _Filters = new List<SearchFilter>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public EnumerationQuery()
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
