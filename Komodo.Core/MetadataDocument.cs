using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Metadata document created during metadata processing of a source and parsed document along with its parse result. 
    /// </summary>
    [Table("metadatadocs")]
    public class MetadataDocument
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
        [JsonProperty(Order = -1)]
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Globally-unique identifier of the owner.
        /// </summary>
        [Column("ownerguid", false, DataTypes.Nvarchar, 64, false)]
        public string OwnerGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The globally-unique identifier of the index.
        /// </summary>
        [Column("indexguid", false, DataTypes.Nvarchar, 64, false)]
        public string IndexGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The globally-unique identifier of the index where the derived document is stored.
        /// Derived documents are stored separately as new source documents.
        /// </summary>
        [Column("targetindexguid", false, DataTypes.Nvarchar, 64, false)]
        public string TargetIndexGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The globally-unique identifier of the source document.
        /// </summary>
        [Column("sourcedocguid", false, DataTypes.Nvarchar, 64, false)]
        public string SourceDocumentGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The content type of the derived document.
        /// </summary>
        [Column("contenttype", false, DataTypes.Nvarchar, 128, true)]
        public string ContentType { get; set; } = null;

        /// <summary>
        /// The derived document's type.
        /// </summary>
        [Column("doctype", false, DataTypes.Enum, 16, false)]
        public DocType Type { get; set; } = DocType.Unknown;

        /// <summary>
        /// The timestamp from when the entry was created.
        /// </summary>
        [JsonProperty(Order = 990)]
        [Column("created", false, DataTypes.DateTime, false)]
        public DateTime Created { get; set; } = DateTime.Now.ToUniversalTime();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataDocument()
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
            return Common.SerializeJson(this, pretty);
        }

        #endregion
    }
}
