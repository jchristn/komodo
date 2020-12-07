using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Endpoint type for add metadata document actions.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EndpointType
    {
        /// <summary>
        /// Daemon, memory-resident instance of Komodo.
        /// </summary>
        [EnumMember(Value = "Daemon")]
        Daemon,
        /// <summary>
        /// Standalone Komodo server using RESTful API.
        /// </summary>
        [EnumMember(Value = "Server")]
        Server
    }
}
