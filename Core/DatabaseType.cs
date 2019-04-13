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
    /// Types of databases supported.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DatabaseType
    {
        [EnumMember(Value = "MsSql")]
        MsSql,
        [EnumMember(Value = "MySql")]
        MySql,
        [EnumMember(Value = "PgSql")]
        PgSql,
        [EnumMember(Value = "SQLite")]
        SQLite
    }
}
