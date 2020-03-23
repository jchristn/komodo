using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Komodo.Classes
{
    /// <summary>
    /// Types of data supported.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DataType
    {
        /// <summary>
        /// Object
        /// </summary>
        [EnumMember(Value = "Object")]
        Object,
        /// <summary>
        /// Array
        /// </summary>
        [EnumMember(Value = "Array")]
        Array,
        /// <summary>
        /// Timestamp
        /// </summary>
        [EnumMember(Value = "Timestamp")]
        Timestamp,
        /// <summary>
        /// Integer
        /// </summary>
        [EnumMember(Value = "Integer")]
        Integer,
        /// <summary>
        /// Long
        /// </summary>
        [EnumMember(Value = "Long")]
        Long,
        /// <summary>
        /// Decimal
        /// </summary>
        [EnumMember(Value = "Decimal")]
        Decimal,
        /// <summary>
        /// String
        /// </summary>
        [EnumMember(Value = "String")]
        String,
        /// <summary>
        /// Boolean
        /// </summary>
        [EnumMember(Value = "Boolean")]
        Boolean,
        /// <summary>
        /// Null
        /// </summary>
        [EnumMember(Value = "Null")]
        Null
    }
}
