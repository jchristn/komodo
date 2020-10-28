using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Metadata document created during metadata processing of a source and parsed document along with its parse result. 
    /// </summary>
    [Table("metadatadocs")]
    public class MetadataDocument
    { 
        /// <summary>
        /// Database row ID.
        /// </summary>
        [JsonIgnore]
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [JsonProperty(Order = -1)]
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the owner.
        /// </summary>
        [Column("ownerguid", false, DataTypes.Nvarchar, 64, false)]
        public string OwnerGUID { get; set; }

        /// <summary>
        /// The globally-unique identifier of the index.
        /// </summary>
        [Column("indexguid", false, DataTypes.Nvarchar, 64, false)]
        public string IndexGUID { get; set; }
         
        /// <summary>
        /// The globally-unique identifier of the index where the derived document is stored.
        /// Derived documents are stored separately as new source documents.
        /// </summary>
        [Column("targetindexguid", false, DataTypes.Nvarchar, 64, false)]
        public string TargetIndexGUID { get; set; }

        /// <summary>
        /// The globally-unique identifier of the source document.
        /// </summary>
        [Column("sourcedocguid", false, DataTypes.Nvarchar, 64, false)]
        public string SourceDocumentGUID { get; set; }

        /// <summary>
        /// The content type of the derived document.
        /// </summary>
        [Column("contenttype", false, DataTypes.Nvarchar, 128, true)]
        public string ContentType { get; set; }

        /// <summary>
        /// The derived document's type.
        /// </summary>
        [Column("doctype", false, DataTypes.Enum, 16, false)]
        public DocType Type { get; set; }

        /// <summary>
        /// The timestamp from when the entry was created.
        /// </summary>
        [JsonProperty(Order = 990)]
        [Column("created", false, DataTypes.DateTime, false)]
        public DateTime Created { get; set; }
         
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataDocument()
        {
            GUID = Guid.NewGuid().ToString();
            Created = DateTime.Now.ToUniversalTime();
        } 
    }
}
