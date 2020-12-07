using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Komodo.Crawler
{
    /// <summary>
    /// HTTP method.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HttpMethod
    {
        /// <summary>
        /// GET.
        /// </summary>
        [EnumMember(Value = "GET")]
        GET,
        /// <summary>
        /// PUT.
        /// </summary>
        [EnumMember(Value = "PUT")]
        PUT,
        /// <summary>
        /// POST.
        /// </summary>
        [EnumMember(Value = "POST")]
        POST,
        /// <summary>
        /// DELETE.
        /// </summary>
        [EnumMember(Value = "DELETE")]
        DELETE,
        /// <summary>
        /// PATCH.
        /// </summary>
        [EnumMember(Value = "PATCH")]
        PATCH
    } 
}
