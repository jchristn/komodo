using System;
using System.Collections.Generic;
using System.Text;

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
        public SourceDocument SourceRecord = null;

        /// <summary>
        /// ParsedDocument record.
        /// </summary>
        public ParsedDocument ParsedRecord = null;

        /// <summary>
        /// Parse result.
        /// </summary>
        public object Parsed = null;

        /// <summary>
        /// Postings result.
        /// </summary>
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
