using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Sdk.Classes
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
        /// PostingsDocument record.
        /// </summary>
        public PostingsDocument PostingsRecord = null;

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
    }
}
