using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Identifies credentials that can be used by API requestors.
    /// </summary>
    [Table("apikeys")]
    public class ApiKey
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
        /// Globally-unique identifier of the user.
        /// </summary>
        [Column("userguid", false, DataTypes.Nvarchar, 64, false)]
        public string UserGUID { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Indicates whether or not the API key is able to be used.
        /// </summary>
        [JsonProperty(Order = 990)]
        [Column("active", false, DataTypes.Boolean, false)]
        public bool Active { get; set; } = true;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ApiKey()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="userGuid">Globally-unique identifier of the user.</param>
        /// <param name="active">Indicates whether or not the API key is able to be used.</param>
        public ApiKey(string userGuid, bool active)
        {
            if (String.IsNullOrEmpty(userGuid)) throw new ArgumentNullException(nameof(userGuid));
            UserGUID = userGuid;
            Active = active;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="userGuid">Globally-unique identifier of the user.</param>
        /// <param name="active">Indicates whether or not the API key is able to be used.</param>
        public ApiKey(string guid, string userGuid, bool active)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(userGuid)) throw new ArgumentNullException(nameof(userGuid));
            GUID = guid;
            UserGUID = userGuid;
            Active = active;
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
