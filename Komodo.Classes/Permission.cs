using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Specifies API permissions associated with API keys. 
    /// </summary>
    [Table("permissions")]
    public class Permission
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
        [JsonProperty(Order = -4)]
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the index.
        /// </summary>
        [JsonProperty(Order = -3)]
        [Column("indexguid", false, DataTypes.Nvarchar, 64, false)]
        public string IndexGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user.
        /// </summary>
        [JsonProperty(Order = -2)]
        [Column("userguid", false, DataTypes.Nvarchar, 64, false)]
        public string UserGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the API key.
        /// </summary>
        [JsonProperty(Order = -1)]
        [Column("apikeyguid", false, DataTypes.Nvarchar, 64, false)]
        public string ApiKeyGUID { get; set; }

        /// <summary>
        /// Allow or disallow search.
        /// </summary>
        [Column("allowsearch", false, DataTypes.Boolean, false)]
        public bool AllowSearch { get; set; }

        /// <summary>
        /// Allow or disallow document creation.
        /// </summary>
        [Column("allowcreatedoc", false, DataTypes.Boolean, false)]
        public bool AllowCreateDocument { get; set; }

        /// <summary>
        /// Allow or disallow document deletion.
        /// </summary>
        [Column("allowdeletedoc", false, DataTypes.Boolean, false)]
        public bool AllowDeleteDocument { get; set; }

        /// <summary>
        /// Allow or disallow index creation.
        /// </summary>
        [Column("allowcreateindex", false, DataTypes.Boolean, false)]
        public bool AllowCreateIndex { get; set; }

        /// <summary>
        /// Allow or disallow index deletion.
        /// </summary>
        [Column("allowdeleteindex", false, DataTypes.Boolean, false)]
        public bool AllowDeleteIndex { get; set; }
         
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Permission()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="indexGuid">Index GUID.</param>
        /// <param name="userGuid">User GUID.</param>
        /// <param name="apiKeyGuid">API key GUID.</param>
        /// <param name="allowSearch">Allow search.</param>
        /// <param name="allowCreateDoc">Allow document creation.</param>
        /// <param name="allowDeleteDoc">Allow document deletion.</param>
        /// <param name="allowCreateIndex">Allow index creation.</param>
        /// <param name="allowDeleteIndex">Allow index deletion.</param>
        public Permission(string indexGuid, string userGuid, string apiKeyGuid, bool allowSearch, bool allowCreateDoc, bool allowDeleteDoc, bool allowCreateIndex, bool allowDeleteIndex)
        {
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(userGuid)) throw new ArgumentNullException(nameof(userGuid));
            if (String.IsNullOrEmpty(apiKeyGuid)) throw new ArgumentNullException(nameof(apiKeyGuid));

            GUID = Guid.NewGuid().ToString();
            IndexGUID = indexGuid;
            UserGUID = userGuid;
            ApiKeyGUID = apiKeyGuid;

            AllowSearch = allowSearch;
            AllowCreateDocument = allowCreateDoc;
            AllowDeleteDocument = allowDeleteDoc;
            AllowCreateIndex = allowCreateIndex;
            AllowDeleteIndex = allowDeleteIndex; 
        } 
    }
}
