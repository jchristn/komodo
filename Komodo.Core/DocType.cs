﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Komodo
{
    /// <summary>
    /// Supported document types and data sources.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DocType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,
        /// <summary>
        /// CSV.
        /// </summary>
        [EnumMember(Value = "Csv")]
        Csv,
        /// <summary>
        /// HTML
        /// </summary>
        [EnumMember(Value = "Html")]
        Html,
        /// <summary>
        /// JSON
        /// </summary>
        [EnumMember(Value = "Json")]
        Json,
        /// <summary>
        /// SQL table
        /// </summary>
        [EnumMember(Value = "Sql")]
        Sql,
        /// <summary>
        /// Text
        /// </summary>
        [EnumMember(Value = "Text")]
        Text,
        /// <summary>
        /// XML
        /// </summary>
        [EnumMember(Value = "Xml")]
        Xml
    }
}
