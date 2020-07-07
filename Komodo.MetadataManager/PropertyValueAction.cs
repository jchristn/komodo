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
    /// Property value action for a metadata document action within a metadata rule.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PropertyValueAction
    {
        /// <summary>
        /// Copy the value directly from the source document.
        /// </summary>
        [EnumMember(Value = "CopyFromDocument")]
        CopyFromDocument,
        /// <summary>
        /// Assign a static value.
        /// </summary>
        [EnumMember(Value = "Static")]
        Static
    }
}
