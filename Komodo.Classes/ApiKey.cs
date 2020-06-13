using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;

namespace Komodo.Classes
{
    /// <summary>
    /// Identifies credentials that can be used by API requestors.
    /// </summary>
    [Table("apikeys")]
    public class ApiKey
    { 
        /// <summary>
        /// Database row ID.
        /// </summary>
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set;  }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user.
        /// </summary>
        [Column("userguid", false, DataTypes.Nvarchar, 64, false)]
        public string UserGUID { get; set; }

        /// <summary>
        /// Indicates whether or not the API key is able to be used.
        /// </summary>
        [Column("active", false, DataTypes.Boolean, false)]
        public bool Active { get; set; }
        
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

            GUID = Guid.NewGuid().ToString();
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
    }
}
