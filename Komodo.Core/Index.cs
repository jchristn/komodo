using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Repository where source and parsed documents are stored.
    /// </summary>
    [Table("indices")]
    public class Index
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
        /// Globally-unique identifier of the user that owns this index.
        /// </summary>
        [Column("ownerguid", false, DataTypes.Nvarchar, 64, false)]
        public string OwnerGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The name of the index.
        /// </summary>
        [Column("name", false, DataTypes.Nvarchar, 64, false)]
        public string Name { get; set; } = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Index()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns this index.</param>
        /// <param name="name">The name of the index.</param> 
        public Index(string ownerGuid, string name)
        {
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            GUID = Guid.NewGuid().ToString();
            OwnerGUID = ownerGuid;
            Name = name;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns this index.</param>
        /// <param name="name">The name of the index.</param> 
        public Index(string guid, string ownerGuid, string name)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            GUID = guid;
            OwnerGUID = ownerGuid;
            Name = name;
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
