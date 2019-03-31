using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Komodo.Core
{
    /// <summary>
    /// Types of data supported.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataType
    {
        [EnumMember(Value = "Object")]
        Object,
        [EnumMember(Value = "Array")]
        Array,
        [EnumMember(Value = "Timestamp")]
        Timestamp,
        [EnumMember(Value = "Integer")]
        Integer,
        [EnumMember(Value = "Long")]
        Long,
        [EnumMember(Value = "Decimal")]
        Decimal,
        [EnumMember(Value = "String")]
        String,
        [EnumMember(Value = "Boolean")]
        Boolean,
        [EnumMember(Value = "Null")]
        Null
    }
}
