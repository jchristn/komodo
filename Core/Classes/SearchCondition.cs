using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace KomodoCore
{ 
    /// <summary>
    /// Available conditions for search filters.
    /// </summary> 
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SearchCondition
    {
        [EnumMember(Value = "Equals")]
        Equals,
        [EnumMember(Value = "NotEquals")]
        NotEquals,
        [EnumMember(Value = "GreaterThan")]
        GreaterThan,
        [EnumMember(Value = "GreaterThanOrEqualTo")]
        GreaterThanOrEqualTo,
        [EnumMember(Value = "LessThan")]
        LessThan,
        [EnumMember(Value = "LessThanOrEqualTo")]
        LessThanOrEqualTo,
        [EnumMember(Value = "IsNull")]
        IsNull,
        [EnumMember(Value = "IsNotNull")]
        IsNotNull,
        [EnumMember(Value = "Contains")]
        Contains,
        [EnumMember(Value = "ContainsNot")]
        ContainsNot,
        [EnumMember(Value = "StartsWith")]
        StartsWith,
        [EnumMember(Value = "EndsWith")]
        EndsWith
    }
}
