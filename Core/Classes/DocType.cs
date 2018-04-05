using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace KomodoCore
{
    /// <summary>
    /// Supported document types and data sources.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DocType
    {
        [EnumMember(Value = "Sql")]
        Sql,
        [EnumMember(Value = "Html")]
        Html,
        [EnumMember(Value = "Json")]
        Json,
        [EnumMember(Value = "Xml")]
        Xml,
        [EnumMember(Value = "Text")]
        Text,
        [EnumMember(Value = "Unknown")]
        Unknown
    }
}
