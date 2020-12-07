using System;
using System.Collections.Generic;
using System.Data;

namespace Komodo.Sdk.Classes
{
    /// <summary>
    /// Object that has been parsed by Komodo.
    /// </summary> 
    public class ParsedDocument
    {
        #region Public-Members

        /// <summary>
        /// Globally-unique identifier.
        /// </summary> 
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the source document.
        /// </summary> 
        public string SourceDocumentGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user that owns the document record.
        /// </summary> 
        public string OwnerGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the index.
        /// </summary> 
        public string IndexGUID { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary> 
        public DocType Type { get; set; }

        /// <summary>
        /// The content length of the parsed document.
        /// </summary> 
        public long ContentLength { get; set; }

        /// <summary>
        /// The number of terms in the parsed document.
        /// </summary> 
        public long Terms { get; set; }

        /// <summary>
        /// The number of postings in the parsed document.
        /// </summary> 
        public long Postings { get; set; }

        /// <summary>
        /// The timestamp from when the entry was created.
        /// </summary> 
        public DateTime Created { get; set; }

        /// <summary>
        /// The timestamp from when the document was indexed.
        /// </summary> 
        public DateTime? Indexed { get; set; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ParsedDocument()
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
