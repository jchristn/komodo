using System;
using System.Collections.Generic;
using System.Data;

namespace Komodo.Sdk.Classes
{
    /// <summary>
    /// Object that has been uploaded to Komodo.
    /// </summary>
    public class SourceDocument
    {
        #region Public-Members

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user that owns the document record.
        /// </summary>
        public string OwnerGUID { get; set; }

        /// <summary>
        /// The globally-unique identifier of the index.
        /// </summary>
        public string IndexGUID { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The title of the object.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The tags associated with the object.
        /// </summary> 
        public string Tags { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary> 
        public DocType Type { get; set; }

        /// <summary>
        /// The URL from which the content was retrieved.
        /// </summary> 
        public string SourceURL { get; set; }

        /// <summary>
        /// The content type of the object.
        /// </summary> 
        public string ContentType { get; set; }

        /// <summary>
        /// The content length of the source document.
        /// </summary> 
        public long ContentLength { get; set; }

        /// <summary>
        /// The MD5 hash of the source content.
        /// </summary> 
        public string Md5 { get; set; }

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
        public SourceDocument()
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
