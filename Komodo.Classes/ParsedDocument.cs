using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;

namespace Komodo.Classes
{
    /// <summary>
    /// Object that has been parsed by Komodo.
    /// </summary>
    [Table("parseddocs")]
    public class ParsedDocument
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the source document.
        /// </summary>
        [Column("sourcedocguid", false, DataTypes.Nvarchar, 64, false)]
        public string SourceDocumentGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user that owns the document record.
        /// </summary>
        [Column("ownerguid", false, DataTypes.Nvarchar, 64, false)]
        public string OwnerGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the index.
        /// </summary>
        [Column("indexguid", false, DataTypes.Nvarchar, 64, false)]
        public string IndexGUID { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary>
        [Column("doctype", false, DataTypes.Enum, 16, false)]
        public DocType Type { get; set; }
         
        /// <summary>
        /// The content length of the parsed document.
        /// </summary>
        [Column("contentlength", false, DataTypes.Long, false)]
        public long ContentLength { get; set; }

        /// <summary>
        /// The number of terms in the parsed document.
        /// </summary>
        [Column("terms", false, DataTypes.Long, false)]
        public long Terms { get; set; }

        /// <summary>
        /// The number of postings in the parsed document.
        /// </summary>
        [Column("postings", false, DataTypes.Long, false)]
        public long Postings { get; set; }

        /// <summary>
        /// The timestamp from when the entry was created.
        /// </summary>
        [Column("created", false, DataTypes.DateTime, false)]
        public DateTime Created { get; set; }

        /// <summary>
        /// The timestamp from when the document was indexed.
        /// </summary>
        [Column("indexed", false, DataTypes.DateTime, true)]
        public DateTime? Indexed { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ParsedDocument()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="sourceDocGuid">Globally-unique identifier of the source document.</param>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns the document record.</param>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="docType">The type of document.</param>
        /// <param name="contentLength">The content length of the parsed document.</param>
        /// <param name="terms">The number of terms in the parsed document.</param>
        /// <param name="postings">The number of postings in the parsed document.</param>
        public ParsedDocument(string sourceDocGuid, string ownerGuid, string indexGuid, DocType docType, long contentLength, long terms, long postings)
        {
            if (String.IsNullOrEmpty(sourceDocGuid)) throw new ArgumentNullException(nameof(sourceDocGuid));
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (contentLength < 0) throw new ArgumentException("Content length must be zero or greater.");
            if (terms < 0) throw new ArgumentException("Terms count must be zero or greater.");
            if (postings < 0) throw new ArgumentException("Postings count must be zero or greater.");

            GUID = Guid.NewGuid().ToString();
            SourceDocumentGUID = sourceDocGuid;
            OwnerGUID = ownerGuid;
            IndexGUID = indexGuid;
            Type = docType;
            ContentLength = contentLength;
            Terms = terms;
            Postings = postings;
            Created = DateTime.Now.ToUniversalTime(); 
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="sourceDocGuid">Globally-unique identifier of the source document.</param>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns the document record.</param>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="docType">The type of document.</param>
        /// <param name="terms">The number of terms in the parsed document.</param>
        /// <param name="postings">The number of postings in the parsed document.</param>
        /// <param name="contentLength">The content length of the parsed document.</param>
        public ParsedDocument(string guid, string sourceDocGuid, string ownerGuid, string indexGuid, DocType docType, long contentLength, long terms, long postings)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(sourceDocGuid)) throw new ArgumentNullException(nameof(sourceDocGuid));
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (contentLength < 0) throw new ArgumentException("Content length must be zero or greater.");
            if (terms < 0) throw new ArgumentException("Terms count must be zero or greater.");
            if (postings < 0) throw new ArgumentException("Postings count must be zero or greater.");

            GUID = guid;
            SourceDocumentGUID = sourceDocGuid;
            OwnerGUID = ownerGuid;
            IndexGUID = indexGuid;
            Type = docType;
            ContentLength = contentLength;
            Terms = terms;
            Postings = postings;
            Created = DateTime.Now.ToUniversalTime();
        } 
    }
}
