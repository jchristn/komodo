using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Komodo
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
        [JsonProperty(Order = -3)]
        public string GUID = Guid.NewGuid().ToString();

        /// <summary>
        /// Maximum number of results to retrieve.
        /// </summary>
        [JsonProperty(Order = -2)]
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
        [JsonProperty(Order = -1)]
        public int StartIndex
        {
            get
            {
                return _StartIndex;
            }
            set
            {
                if (value < 0) throw new ArgumentException("StartIndex must be zero or greater.");
                _StartIndex = value;
            }
        }
         
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
        private int _StartIndex = 0;
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

        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
