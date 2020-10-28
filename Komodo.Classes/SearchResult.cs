using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komodo.Classes;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Object returned as the result of a search against an index.
    /// </summary>
    public class SearchResult
    {
        #region Public-Members

        /// <summary>
        /// Indicates if the statistics operation was successful.
        /// </summary>
        [JsonProperty(Order = -3)]
        public bool Success = false;

        /// <summary>
        /// The name of the index that was queried.
        /// </summary>
        [JsonProperty(Order = -2)]
        public string IndexName = null;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        [JsonProperty(Order = -1)]
        public SearchResultTimeComponents Time = new SearchResultTimeComponents();

        /// <summary>
        /// The search query performed.
        /// </summary>
        public SearchQuery Query = null;

        /// <summary>
        /// List of terms not found.
        /// </summary>
        [JsonProperty(Order = 990)]
        public List<string> TermsNotFound = new List<string>();

        /// <summary>
        /// Documents that matched the query.
        /// </summary>
        [JsonProperty(Order = 991)]
        public List<MatchedDocument> Documents = new List<MatchedDocument>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public SearchResult()
        {
        }

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        /// <param name="query">Search query.</param>
        public SearchResult(SearchQuery query)
        {
            Query = query;
        }

        #endregion

        #region Public-Methods
         
        /// <summary>
        /// Sort matches by score.
        /// </summary>
        public void SortMatchesByScore()
        {
            if (Documents == null || Documents.Count < 1) return;
            Documents = Documents.OrderByDescending(d => d.Score).ToList();
        }

        #endregion

        #region Private-Methods

        #endregion

        #region Public-Embedded-Classes

        /// <summary>
        /// Amount of time spent in various aspects of search.
        /// Time elements may not add up to the 'Overall' time as not every element is measured.
        /// </summary>
        public class SearchResultTimeComponents
        {
            /// <summary>
            /// Overall time spent processing the search query.
            /// </summary>
            public Timestamps Overall = new Timestamps();

            /// <summary>
            /// Time spent, in milliseconds, preparing internal data structures for the search.
            /// </summary>
            public double PreparationMs
            {
                get
                {
                    return _PreparationMs;
                }
                set
                {
                    if (value < 0) throw new ArgumentException("Value must be greater than or equal to zero.");
                    _PreparationMs = Math.Round(value, 2);
                }
            }

            /// <summary>
            /// Time spent, in milliseconds, evaluating required and excluded terms.
            /// </summary>
            public double RequiredExcludedTermsMs
            {
                get
                {
                    return _RequiredExcludedTermsMs;
                }
                set
                {
                    if (value < 0) throw new ArgumentException("Value must be greater than or equal to zero.");
                    _RequiredExcludedTermsMs = Math.Round(value, 2);
                }
            }

            /// <summary>
            /// Time spent, in milliseconds, reading parse results from documents.
            /// </summary>
            public double ReadParseResultsMs
            {
                get
                {
                    return _ReadParseResultsMs;
                }
                set
                {
                    if (value < 0) throw new ArgumentException("Value must be greater than or equal to zero.");
                    _ReadParseResultsMs = Math.Round(value, 2);
                }
            }

            /// <summary>
            /// Time spent, in milliseconds, evaluating optional terms.
            /// </summary>
            public double OptionalTermsMs
            {
                get
                {
                    return _OptionalTermsMs;
                }
                set
                {
                    if (value < 0) throw new ArgumentException("Value must be greater than or equal to zero.");
                    _OptionalTermsMs = Math.Round(value, 2);
                }
            }

            /// <summary>
            /// Time spent, in milliseconds, evaluating required and excluded filters.
            /// </summary>
            public double RequiredExcludedFiltersMs
            {
                get
                {
                    return _RequiredExcludedFiltersMs;
                }
                set
                {
                    if (value < 0) throw new ArgumentException("Value must be greater than or equal to zero.");
                    _RequiredExcludedFiltersMs = Math.Round(value, 2);
                }
            }

            /// <summary>
            /// Time spent, in milliseconds, evaluating optional filters.
            /// </summary>
            public double OptionalFiltersMs
            {
                get
                {
                    return _OptionalFiltersMs;
                }
                set
                {
                    if (value < 0) throw new ArgumentException("Value must be greater than or equal to zero.");
                    _OptionalFiltersMs = Math.Round(value, 2);
                }
            }

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public SearchResultTimeComponents()
            {
            }

            private double _PreparationMs = 0;
            private double _RequiredExcludedTermsMs = 0;
            private double _ReadParseResultsMs = 0;
            private double _OptionalTermsMs = 0;
            private double _RequiredExcludedFiltersMs = 0;
            private double _OptionalFiltersMs = 0;
        }

        #endregion
    }
}
