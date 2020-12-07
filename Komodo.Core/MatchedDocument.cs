using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// A document matching a search query.
    /// </summary>
    public class MatchedDocument
    {
        #region Public-Members

        /// <summary>
        /// Globally-unique identifier for the source document.
        /// </summary>
        [JsonProperty(Order = -2)]
        public string GUID = null;

        /// <summary>
        /// The type of document.
        /// </summary>
        [JsonProperty(Order = -1)]
        public DocType DocumentType = DocType.Unknown;

        /// <summary>
        /// The score of the document, between 0 and 1, over both terms and filters.  Only relevant when optional terms or filters are supplied in the search.
        /// </summary>
        public decimal? Score = null;

        /// <summary>
        /// The terms score of the document, between 0 and 1, when optional terms are supplied.
        /// </summary>
        public decimal? TermsScore = null;

        /// <summary>
        /// The filters score of the document, between 0 and 1, when optional filters are supplied.
        /// </summary>
        public decimal? FiltersScore = null;

        /// <summary>
        /// Source document metadata, if requested.
        /// </summary>
        [JsonProperty(Order = 990)]
        public SourceDocument Metadata = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public MatchedDocument()
        {
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
