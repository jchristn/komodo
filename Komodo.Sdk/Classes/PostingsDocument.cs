using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Sdk.Classes
{
    /// <summary>
    /// Postings document created by Komodo.
    /// </summary> 
    public class PostingsDocument
    {
        #region Public-Members

        /// <summary>
        /// Database row ID.
        /// </summary> 
        public int Id { get; set; } = 0;

        /// <summary>
        /// Globally-unique identifier.
        /// </summary> 
        public string GUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Globally-unique identifier of the source document.
        /// </summary> 
        public string SourceDocumentGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Globally-unique identifier of the user that owns the document record.
        /// </summary> 
        public string OwnerGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Globally-unique identifier of the index.
        /// </summary> 
        public string IndexGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The type of document.
        /// </summary> 
        public DocType Type { get; set; } = DocType.Unknown;

        /// <summary>
        /// The content length of the postings document.
        /// </summary> 
        public long ContentLength { get; set; } = 0;

        /// <summary>
        /// The number of terms in the postings document.
        /// </summary> 
        public long Terms { get; set; } = 0;

        /// <summary>
        /// The number of postings in the postings document.
        /// </summary> 
        public long Postings { get; set; } = 0;

        /// <summary>
        /// The timestamp from when the document was indexed.
        /// </summary> 
        public DateTime? Indexed { get; set; } = null;

        /// <summary>
        /// The timestamp from when the entry was created.
        /// </summary> 
        public DateTime Created { get; set; } = DateTime.Now.ToUniversalTime();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public PostingsDocument()
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
