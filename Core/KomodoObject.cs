using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Komodo.Core
{
    /// <summary>
    /// Object stored in Komodo.
    /// </summary>
    public class KomodoObject
    {
        #region Public-Members

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// The document ID.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// The content type of the document.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The length of the document.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// The stream containing the document's data.
        /// </summary>
        [JsonIgnore]
        public Stream Data { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public KomodoObject()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="documentId">The document ID.</param>
        /// <param name="contentType">The content type of the document.</param>
        /// <param name="contentLength">The length of the document.</param>
        /// <param name="data">The stream containing the document's data.</param>
        public KomodoObject(string indexName, string documentId, string contentType, long contentLength, Stream data)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(documentId)) throw new ArgumentNullException(nameof(documentId));

            IndexName = indexName;
            DocumentId = documentId;
            ContentType = contentType;
            ContentLength = contentLength;
            Data = data;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
