using System;
using System.Collections.Generic;
using System.Text;
using Komodo.Classes;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Result from metadata processing.
    /// </summary>
    public class MetadataResult
    {
        /// <summary>
        /// Source document.
        /// </summary>
        public SourceDocument Source { get; set; }

        /// <summary>
        /// Parsed document.
        /// </summary>
        public ParsedDocument Parsed { get; set; }

        /// <summary>
        /// Parse result.
        /// </summary>
        public object ParseResult { get; set; }

        /// <summary>
        /// List of matching rules.
        /// </summary>
        public List<MetadataRule> MatchingRules = new List<MetadataRule>();

        /// <summary>
        /// Metadata documents.
        /// </summary>
        public List<MetadataDocument> MetadataDocuments = new List<MetadataDocument>();

        /// <summary>
        /// Source documents containing metadata derived from actions.
        /// </summary>
        public List<SourceDocument> DerivedDocuments = new List<SourceDocument>();

        /// <summary>
        /// Data from derived documents.
        /// </summary>
        public List<Dictionary<string, object>> DerivedDocumentsData = new List<Dictionary<string, object>>();

        /// <summary>
        /// Results from indexing derived documents.
        /// </summary>
        public List<IndexResult> DerivedIndexResults = new List<IndexResult>();

        /// <summary>
        /// Status codes from Postback operations.
        /// </summary>
        public Dictionary<string, int> PostbackStatusCodes = new Dictionary<string, int>();

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataResult()
        {

        }
    }
}
