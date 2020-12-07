using System;
using System.Collections.Generic;
using System.Text;
using Komodo;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Result from metadata processing.
    /// </summary>
    public class MetadataResult
    {
        #region Public-Members

        /// <summary>
        /// Source document.
        /// </summary>
        public SourceDocument Source { get; set; } = null;

        /// <summary>
        /// Parsed document.
        /// </summary>
        public ParsedDocument Parsed { get; set; } = null;

        /// <summary>
        /// Parse result.
        /// </summary>
        public ParseResult ParseResult { get; set; } = null;

        /// <summary>
        /// List of matching rules.
        /// </summary>
        public List<MetadataRule> MatchingRules { get; set; } = new List<MetadataRule>();

        /// <summary>
        /// Metadata documents.
        /// </summary>
        public List<MetadataDocument> MetadataDocuments { get; set; } = new List<MetadataDocument>();

        /// <summary>
        /// Source documents containing metadata derived from actions.
        /// </summary>
        public List<SourceDocument> DerivedDocuments { get; set; } = new List<SourceDocument>();

        /// <summary>
        /// Data from derived documents.
        /// </summary>
        public List<Dictionary<string, object>> DerivedDocumentsData { get; set; } = new List<Dictionary<string, object>>();

        /// <summary>
        /// Results from indexing derived documents.
        /// </summary>
        public List<IndexResult> DerivedIndexResults { get; set; } = new List<IndexResult>();

        /// <summary>
        /// Status codes from Postback operations.
        /// </summary>
        public Dictionary<string, int> PostbackStatusCodes { get; set; } = new Dictionary<string, int>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataResult()
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
    }
}
