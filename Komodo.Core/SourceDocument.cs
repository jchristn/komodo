using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Object that has been uploaded to Komodo.
    /// </summary>
    [Table("sourcedocs")]
    public class SourceDocument
    {
        #region Public-Members

        /// <summary>
        /// Database row ID.
        /// </summary>
        [JsonIgnore]
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [JsonProperty(Order = -4)]
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Globally-unique identifier of the user that owns the document record.
        /// </summary>
        [JsonProperty(Order = -3)]
        [Column("ownerguid", false, DataTypes.Nvarchar, 64, false)]
        public string OwnerGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The globally-unique identifier of the index.
        /// </summary>
        [JsonProperty(Order = -2)]
        [Column("indexguid", false, DataTypes.Nvarchar, 64, false)]
        public string IndexGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The name of the object.
        /// </summary>
        [JsonProperty(Order = -1)]
        [Column("name", false, DataTypes.Nvarchar, 128, true)]
        public string Name { get; set; } = null;

        /// <summary>
        /// The title of the object.
        /// </summary>
        [Column("title", false, DataTypes.Nvarchar, 128, true)]
        public string Title { get; set; } = null;

        /// <summary>
        /// The tags associated with the object.
        /// </summary>
        [Column("tags", false, DataTypes.Nvarchar, 256, true)]
        public string Tags { get; set; } = null;

        /// <summary>
        /// The type of document.
        /// </summary>
        [Column("doctype", false, DataTypes.Nvarchar, 16, false)]
        public DocType Type { get; set; } = DocType.Unknown;

        /// <summary>
        /// The URL from which the content was retrieved.
        /// </summary>
        [Column("sourceurl", false, DataTypes.Nvarchar, 256, true)]
        public string SourceURL { get; set; } = null;

        /// <summary>
        /// The content type of the object.
        /// </summary>
        [Column("contenttype", false, DataTypes.Nvarchar, 128, true)]
        public string ContentType { get; set; } = null;

        /// <summary>
        /// The content length of the source document.
        /// </summary>
        [Column("contentlength", false, DataTypes.Long, false)]
        public long ContentLength { get; set; } = 0;

        /// <summary>
        /// The MD5 hash of the source content.
        /// </summary>
        [Column("md5", false, DataTypes.Nvarchar, 64, true)]
        public string Md5 { get; set; } = null;

        /// <summary>
        /// The timestamp from when the document was indexed.
        /// </summary>
        [JsonProperty(Order = 990)]
        [Column("indexed", false, DataTypes.DateTime, true)]
        public DateTime? Indexed { get; set; } = null;

        /// <summary>
        /// The timestamp from when the entry was created.
        /// </summary>
        [JsonProperty(Order = 991)]
        [Column("created", false, DataTypes.DateTime, false)]
        public DateTime Created { get; set; } = DateTime.Now.ToUniversalTime();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SourceDocument()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns the document record.</param>
        /// <param name="indexGuid">The globally-unique identifier of the index.</param>
        /// <param name="name">The name of the object.</param>
        /// <param name="title">The title of the object.</param>
        /// <param name="tags">The tags associated with the object.</param>
        /// <param name="docType">The type of document.</param>
        /// <param name="sourceUrl">The URL from which the content was retrieved.</param>
        /// <param name="contentType">The content type of the object.</param>
        /// <param name="contentLength">The content length of the source document.</param>
        /// <param name="md5">The MD5 hash of the source content.</param>
        public SourceDocument(string ownerGuid, string indexGuid, string name, string title, List<string> tags, DocType docType, string sourceUrl, string contentType, long contentLength, string md5)
        {
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (contentLength < 0) throw new ArgumentException("Content length must be zero or greater.");

            OwnerGUID = ownerGuid;
            IndexGUID = indexGuid;
            Name = name;
            Title = title;

            if (tags != null && tags.Count > 0) Tags = Common.StringListToCsv(tags);
            else Tags = null;

            Type = docType;
            SourceURL = sourceUrl;
            ContentType = contentType;
            ContentLength = contentLength;
            Md5 = md5;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns the document record.</param>
        /// <param name="indexGuid">The globally-unique identifier of the index.</param>
        /// <param name="name">The name of the object.</param>
        /// <param name="title">The title of the object.</param>
        /// <param name="tags">The tags associated with the object.</param>
        /// <param name="docType">The type of document.</param>
        /// <param name="sourceUrl">The URL from which the content was retrieved.</param>
        /// <param name="contentType">The content type of the object.</param>
        /// <param name="contentLength">The content length of the source document.</param>
        /// <param name="md5">The MD5 hash of the source content.</param>
        public SourceDocument(string guid, string ownerGuid, string indexGuid, string name, string title, List<string> tags, DocType docType, string sourceUrl, string contentType, long contentLength, string md5)
        { 
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (contentLength < 0) throw new ArgumentException("Content length must be zero or greater.");

            if (!String.IsNullOrEmpty(guid)) GUID = guid;
            else GUID = Guid.NewGuid().ToString();

            OwnerGUID = ownerGuid;
            IndexGUID = indexGuid;
            Name = name;
            Title = title;

            if (tags != null && tags.Count > 0) Tags = Common.StringListToCsv(tags);
            else Tags = null;

            Type = docType;
            SourceURL = sourceUrl;
            ContentType = contentType;
            ContentLength = contentLength;
            Md5 = md5;
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
