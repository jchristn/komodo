using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Metadata about a document stored in Komodo.
    /// </summary>
    public class DocumentMetadata
    {
        #region Public-Members

        /// <summary>
        /// SourceDocument record.
        /// </summary>
        [JsonProperty(Order = -4)]
        public SourceDocument SourceRecord = null;

        /// <summary>
        /// ParsedDocument record.
        /// </summary>
        [JsonProperty(Order = -3)]
        public ParsedDocument ParsedRecord = null;

        /// <summary>
        /// Parse result.
        /// </summary>
        [JsonProperty(Order = -2)]
        public object Parsed = null;

        /// <summary>
        /// Postings result.
        /// </summary>
        [JsonProperty(Order = -1)]
        public PostingsResult Postings = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public DocumentMetadata()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="sourceRecord">SourceDocument.</param>
        /// <param name="parsedRecord">ParsedDocument.</param>
        /// <param name="parsed">ParseResult from parser.</param>
        /// <param name="postings">PostingsResult.</param>
        public DocumentMetadata(SourceDocument sourceRecord, ParsedDocument parsedRecord, object parsed, PostingsResult postings)
        {
            SourceRecord = sourceRecord;
            ParsedRecord = parsedRecord;
            Parsed = parsed;
            Postings = postings;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion

    }
}
