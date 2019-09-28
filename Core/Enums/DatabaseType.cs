using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Komodo.Core.Enums
{
    /// <summary>
    /// Types of databases supported.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DatabaseType
    {
        /// <summary>
        /// Microsoft SQL Server
        /// </summary>
        [EnumMember(Value = "MsSql")]
        MsSql,
        /// <summary>
        /// MySQL
        /// </summary>
        [EnumMember(Value = "MySql")]
        MySql,
        /// <summary>
        /// PostgreSQL
        /// </summary>
        [EnumMember(Value = "PgSql")]
        PgSql,
        /// <summary>
        /// Sqlite
        /// </summary>
        [EnumMember(Value = "SQLite")]
        SQLite
    }
}
