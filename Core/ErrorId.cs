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
    /// Error identification.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ErrorId
    {
        [EnumMember(Value = "UNKNOWN")]
        UNKNOWN,
        DESTROY_IN_PROGRESS,
        MISSING_PARAMS,
        RETRIEVE_FAILED,
        PARSE_ERROR,
        WRITE_ERROR,
        DELETE_ERROR
    }
}
