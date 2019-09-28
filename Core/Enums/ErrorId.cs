using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Komodo.Core.Enums
{
    /// <summary>
    /// Error identification.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ErrorId
    {
        /// <summary>
        /// No error.
        /// </summary>
        [EnumMember(Value = "NONE")]
        NONE,
        /// <summary>
        /// The resource is being destroyed.
        /// </summary>
        [EnumMember(Value = "DESTROY_IN_PROGRESS")]
        DESTROY_IN_PROGRESS,
        /// <summary>
        /// Missing parameters.
        /// </summary>
        [EnumMember(Value = "MISSING_PARAMS")]
        MISSING_PARAMS,
        /// <summary>
        /// Failed to retrieve.
        /// </summary>
        [EnumMember(Value = "RETRIEVE_FAILED")]
        RETRIEVE_FAILED,
        /// <summary>
        /// Parsing error.
        /// </summary>
        [EnumMember(Value = "PARSE_ERROR")]
        PARSE_ERROR,
        /// <summary>
        /// Error writing data to underlying storage.
        /// </summary>
        [EnumMember(Value = "WRITE_ERROR")]
        WRITE_ERROR,
        /// <summary>
        /// Error reading from the underlying storage.
        /// </summary>
        [EnumMember(Value = "READ_ERROR")]
        READ_ERROR,
        /// <summary>
        /// Error deleting requested resource.
        /// </summary>
        [EnumMember(Value = "DELETE_ERROR")]
        DELETE_ERROR
    }
}
