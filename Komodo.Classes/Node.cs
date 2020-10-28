using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// System participating in Komodo.
    /// </summary>
    [Table("nodes")]
    public class Node
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
        [JsonProperty(Order = -3)]
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; }

        /// <summary>
        /// The hostname of the node.
        /// </summary>
        [JsonProperty(Order = -2)]
        [Column("hostname", false, DataTypes.Nvarchar, 128, false)]
        public string Hostname { get; set; }

        /// <summary>
        /// The port on which the node is listening for incoming HTTP or HTTPS requests.
        /// </summary>
        [JsonProperty(Order = -1)]
        [Column("port", false, DataTypes.Int, false)]
        public int Port { get; set; }

        /// <summary>
        /// Specifies whether or not SSL is required.
        /// </summary>
        [Column("ssl", false, DataTypes.Boolean, false)]
        public bool Ssl { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Node()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="hostname">The hostname of the node.</param>
        /// <param name="port">The port on which the node is listening for incoming HTTP or HTTPS requests.</param>
        /// <param name="ssl">Specifies whether or not SSL is required.</param>
        public Node(string hostname, int port, bool ssl)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0) throw new ArgumentException("Port must be zero or greater.");

            GUID = Guid.NewGuid().ToString();
            Hostname = hostname;
            Port = port;
            Ssl = ssl;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="hostname">The hostname of the node.</param>
        /// <param name="port">The port on which the node is listening for incoming HTTP or HTTPS requests.</param>
        /// <param name="ssl">Specifies whether or not SSL is required.</param>
        public Node(string guid, string hostname, int port, bool ssl)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0) throw new ArgumentException("Port must be zero or greater.");

            GUID = guid;
            Hostname = hostname;
            Port = port;
            Ssl = ssl;
        } 
    }
}
