using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace KomodoCore
{
    /// <summary>
    /// Types of databases supported.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DatabaseType
    {
        [EnumMember(Value = "Mssql")]
        Mssql,
        [EnumMember(Value = "Mysql")]
        Mysql,
        [EnumMember(Value = "Pgsql")]
        Pgsql,
        [EnumMember(Value = "Sqlite")]
        Sqlite
    }
}
