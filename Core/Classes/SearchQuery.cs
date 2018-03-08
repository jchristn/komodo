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

namespace KomodoCore
{
    /// <summary>
    /// Object used to search an index.
    /// </summary>
    public class SearchQuery
    {
        #region Public-Members

        /// <summary>
        /// The GUID of the search.
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
        /// Required terms and search filter that must be satisfied to include a document in the results.
        /// </summary>
        public QueryFilter Required { get; set; }

        /// <summary>
        /// Optional terms and search filter that may match on documents but are not required.
        /// </summary>
        public QueryFilter Optional { get; set; }

        /// <summary>
        /// Terms and search filter that must be excluded from the results.
        /// </summary>
        public QueryFilter Exclude { get; set; }

        /// <summary>
        /// Set to true to include the source document content for each match.
        /// </summary>
        public bool IncludeContent { get; set; }

        /// <summary>
        /// Set to true to include the full parsed document object for each match.
        /// </summary>
        public bool IncludeParsedDoc { get; set; }

        /// <summary>
        /// Specify a URL to which the results should be submitted via HTTP POST.
        /// </summary>
        public string PostbackUrl { get; set; }
        
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

        #region Subordinate-Classes
         
        /// <summary>
        /// A filter for the query.
        /// </summary>
        public class QueryFilter
        {
            /// <summary>
            /// List of terms upon which to match.
            /// </summary>
            public List<string> Terms { get; set; }

            /// <summary>
            /// List of filters upon which to match.
            /// </summary>
            public List<SearchFilter> Filter { get; set; }

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public QueryFilter()
            {

            }
        }

        /// <summary>
        /// A search filter.
        /// </summary>
        public class SearchFilter
        {
            /// <summary>
            /// The field upon which to match.
            /// </summary>
            public string Field { get; set; }

            /// <summary>
            /// The condition by which the parsed document's content is evaluated against the supplied value.
            /// </summary>
            public SearchCondition Condition { get; set; }

            /// <summary>
            /// The value to be evaluated using the specified condition against the parsed document's content.
            /// </summary>
            public string Value { get; set; }
        }

        /// <summary>
        /// Available conditions for search filters.
        /// </summary> 
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SearchCondition
        {
            [EnumMember(Value = "Equals")]
            Equals,
            [EnumMember(Value = "NotEquals")]
            NotEquals,
            [EnumMember(Value = "GreaterThan")]
            GreaterThan,
            [EnumMember(Value = "GreaterThanOrEqualTo")]
            GreaterThanOrEqualTo,
            [EnumMember(Value = "LessThan")]
            LessThan,
            [EnumMember(Value = "LessThanOrEqualTo")]
            LessThanOrEqualTo,
            [EnumMember(Value = "IsNull")]
            IsNull,
            [EnumMember(Value = "IsNotNull")]
            IsNotNull,
            [EnumMember(Value = "Contains")]
            Contains,
            [EnumMember(Value = "ContainsNot")]
            ContainsNot,
            [EnumMember(Value = "StartsWith")]
            StartsWith,
            [EnumMember(Value = "EndsWith")]
            EndsWith 
        }

        #endregion
    }
}
