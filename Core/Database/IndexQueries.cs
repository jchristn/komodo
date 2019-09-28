using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;
using SqliteWrapper;
using Komodo.Core.Enums;

namespace Komodo.Core.Database
{
    /// <summary>
    /// Query builders needed to interact with index database tables.
    /// </summary>
    public class IndexQueries
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private Index _Index;
        private DatabaseWrapper.DatabaseClient _SqlDatabase = null;
        private SqliteWrapper.DatabaseClient _SqliteDatabase = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="sqlDatabase">The database client for non-Sqlite databases.</param>
        /// <param name="sqliteDatabase">The database client for Sqlite databases.</param>
        public IndexQueries(
            Index index,
            DatabaseWrapper.DatabaseClient sqlDatabase,
            SqliteWrapper.DatabaseClient sqliteDatabase)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (index.DocumentsDatabase == null) throw new ArgumentException("Index does not contain document database settings.");
            if (sqlDatabase == null && sqliteDatabase == null) throw new ArgumentException("One of the the two database clients must be null.");
            if (sqlDatabase != null && sqliteDatabase != null) throw new ArgumentException("One of the the two database clients must be null.");

            _Index = index;
            _SqlDatabase = sqlDatabase;
            _SqliteDatabase = sqliteDatabase;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Generate the query to create the source documents table.
        /// </summary>
        /// <returns>String.</returns>
        public string CreateSourceDocsTable()
        {
            string query = "";

            switch (_Index.DocumentsDatabase.Type)
            {
                case DatabaseType.MsSql:
                    query =
                        "USE " + _Index.DocumentsDatabase.DatabaseName + ";" +
                        "IF NOT EXISTS " +
                        "(" +
                        "  SELECT * FROM sysobjects WHERE name = 'SourceDocuments' AND xtype = 'U' " +
                        ")" +
                        "CREATE TABLE SourceDocuments " +
                        "(" +
                        "  [Id]            [bigint] IDENTITY(1,1) NOT NULL, " +
                        "  [IndexName]     [nvarchar] (128) NULL, " +
                        "  [DocumentId]    [nvarchar] (128) NULL, " +
                        "  [Name]          [nvarchar] (128) NULL, " +
                        "  [Tags]          [nvarchar] (128) NULL, " +
                        "  [DocumentType]  [nvarchar] (32) NULL, " +
                        "  [SourceUrl]     [nvarchar] (256) NULL, " +
                        "  [Title]         [nvarchar] (128) NULL, " +
                        "  [ContentType]   [nvarchar] (128) NULL, " +
                        "  [ContentLength] [bigint] NULL, " +
                        "  [Md5]           [nvarchar] (64) NULL, " +
                        "  [Created]       [datetime2] (7) NULL, " +
                        "  [Indexed]       [datetime2] (7) NULL, " +
                        "  CONSTRAINT [PK_SourceDocuments] PRIMARY KEY CLUSTERED " +
                        "  ( " +
                        "    [Id] ASC " +
                        "  ) " +
                        "  WITH " +
                        "  (" +
                        "    STATISTICS_NORECOMPUTE = OFF, " +
                        "    IGNORE_DUP_KEY = OFF" +
                        "  ) " +
                        "  ON [PRIMARY]" +
                        ")" +
                        "ON [PRIMARY]";
                    return query;
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    throw new Exception("Unsupported document database type: " + _Index.DocumentsDatabase.Type.ToString());
                case DatabaseType.SQLite:
                    query =
                        "CREATE TABLE IF NOT EXISTS SourceDocuments " +
                        "(" +
                        "  Id                INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "  IndexName         VARCHAR(128)  COLLATE NOCASE, " +
                        "  DocumentId        VARCHAR(128)  COLLATE NOCASE, " +
                        "  Name              VARCHAR(128)  COLLATE NOCASE, " +
                        "  Tags              VARCHAR(128)  COLLATE NOCASE, " +
                        "  DocumentType      VARCHAR(32)   COLLATE NOCASE, " +
                        "  SourceUrl         VARCHAR(256)  COLLATE NOCASE, " +
                        "  Title             VARCHAR(128)  COLLATE NOCASE, " +
                        "  ContentType       VARCHAR(128)  COLLATE NOCASE, " +
                        "  ContentLength     INTEGER, " +
                        "  Md5               VARCHAR(64)  COLLATE NOCASE, " +
                        "  Created           VARCHAR(32), " +
                        "  Indexed           VARCHAR(32) " +
                        ")";
                    return query;
            }

            throw new ArgumentException("Invalid document database type, use one of: Mssql, Mysql, Pgsql, Sqlite");
        }

        /// <summary>
        /// Generate the query to create the parsed documents table.
        /// </summary>
        /// <returns>String.</returns>
        public string CreateParsedDocsTable()
        {
            string query = "";

            switch (_Index.DocumentsDatabase.Type)
            { 
                case DatabaseType.MsSql:
                    query =
                        "USE " + _Index.DocumentsDatabase.DatabaseName + ";" +
                        "IF NOT EXISTS " +
                        "(" +
                        "  SELECT * FROM sysobjects WHERE name = 'ParsedDocuments' AND xtype = 'U' " +
                        ")" +
                        "CREATE TABLE ParsedDocuments " +
                        "(" +
                        "  [Id]                  [bigint] IDENTITY(1,1) NOT NULL, " +
                        "  [IndexName]           [nvarchar] (128) NULL, " +
                        "  [DocumentId]          [nvarchar] (128) NULL, " +
                        "  [DocumentType]        [nvarchar] (32) NULL, " +
                        "  [SourceContentLength] [bigint] NULL, " +
                        "  [ContentLength]       [bigint] NULL, " +
                        "  [Created]             [datetime2] (7) NULL, " +
                        "  [Indexed]             [datetime2] (7) NULL, " +
                        "  CONSTRAINT [PK_ParsedDocuments] PRIMARY KEY CLUSTERED " +
                        "  ( " +
                        "    [Id] ASC " +
                        "  ) " +
                        "  WITH " +
                        "  ( " + 
                        "    STATISTICS_NORECOMPUTE = OFF, " +
                        "    IGNORE_DUP_KEY = OFF " +
                        "  ) " +
                        "  ON [PRIMARY]" +
                        ")" +
                        "ON [PRIMARY]";
                    return query; 
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    throw new Exception("Unsupported database type: " + _Index.DocumentsDatabase.Type.ToString());
                case DatabaseType.SQLite:
                    query =
                        "CREATE TABLE IF NOT EXISTS ParsedDocuments " +
                        "(" +
                        "  Id                   INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "  IndexName            VARCHAR(128)   COLLATE NOCASE, " +
                        "  DocumentId           VARCHAR(128)   COLLATE NOCASE, " +
                        "  DocumentType         VARCHAR(32)    COLLATE NOCASE, " +
                        "  SourceContentLength  INTEGER, " +
                        "  ContentLength        INTEGER, " +
                        "  Created              VARCHAR(32), " +
                        "  Indexed              VARCHAR(32) " +
                        ")";
                    return query;
            }

            throw new ArgumentException("Invalid database type, use one of: Mssql, Mysql, Pgsql, Sqlite");
        }
         
        /// <summary>
        /// Generate the query to retrieve source documents based on supplied enumeration query.
        /// </summary>
        /// <param name="query">Enumeration query.</param>
        /// <returns>String.</returns>
        public string SelectSourceDocumentsByEnumerationQuery(EnumerationQuery query)
        {
            string ret = "";

            switch (_Index.DocumentsDatabase.Type)
            {
                case DatabaseType.MsSql:
                    ret = "SELECT * FROM SourceDocuments ";
                    ret += SearchFilterToWhereClause(query.Filters);
                    ret += " ";
                    ret += "ORDER BY Id ASC ";

                    if (query.StartIndex != null)
                    {
                        ret += "OFFSET " + query.StartIndex + " ROWS ";
                    }

                    if (query.MaxResults != null)
                    {
                        ret += "FETCH NEXT " + query.MaxResults + " ROWS ONLY";
                    }
                    return ret;
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    throw new Exception("Unsupported document database type: " + _Index.DocumentsDatabase.Type.ToString());
                case DatabaseType.SQLite:
                    ret = "SELECT * FROM SourceDocuments ";
                    ret += SearchFilterToWhereClause(query.Filters);

                    if (query.MaxResults != null)
                    {
                        ret += "LIMIT " + query.MaxResults + " ";
                    }

                    if (query.StartIndex != null)
                    {
                        ret += "OFFSET " + query.StartIndex + " ";
                    }
                    return ret;
            }

            throw new ArgumentException("Invalid document database type, use one of: Mssql, Mysql, Pgsql, Sqlite");
        }

        #endregion

        #region Private-Methods

        private string Sanitize(string str)
        {
            if (String.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));
            if (_SqlDatabase != null) return _SqlDatabase.SanitizeString(str);
            else return SqliteWrapper.DatabaseClient.SanitizeString(str);
        }

        private string SearchFilterToWhereClause(List<SearchFilter> filters)
        {
            string ret = "";

            if (filters != null && filters.Count > 0)
            {
                ret += "WHERE ";
                int added = 0;

                foreach (SearchFilter curr in filters)
                {
                    if (String.IsNullOrEmpty(curr.Field)) continue;

                    string currSf = Sanitize(curr.Field);

                    switch (curr.Condition)
                    {
                        case SearchCondition.Contains:
                            currSf += " LIKE '%" + Sanitize(curr.Value) + "%' ";
                            break;
                        case SearchCondition.ContainsNot:
                            currSf += " NOT LIKE '%" + Sanitize(curr.Value) + "%' ";
                            break;
                        case SearchCondition.EndsWith:
                            currSf += " LIKE '%" + Sanitize(curr.Value) + "' ";
                            break;
                        case SearchCondition.Equals:
                            currSf += " = '" + Sanitize(curr.Value) + "' ";
                            break;
                        case SearchCondition.GreaterThan:
                            currSf += " > '" + Sanitize(curr.Value) + "' ";
                            break;
                        case SearchCondition.GreaterThanOrEqualTo:
                            currSf += " >= '" + Sanitize(curr.Value) + "' ";
                            break;
                        case SearchCondition.IsNotNull:
                            currSf += " IS NOT NULL ";
                            break;
                        case SearchCondition.IsNull:
                            currSf += " IS NULL "; 
                            break;
                        case SearchCondition.LessThan:
                            currSf += " < '" + Sanitize(curr.Value) + "' ";
                            break;
                        case SearchCondition.LessThanOrEqualTo:
                            currSf += " <= '" + Sanitize(curr.Value) + "' ";
                            break;
                        case SearchCondition.NotEquals:
                            currSf += " != '" + Sanitize(curr.Value) + "' ";
                            break;
                        case SearchCondition.StartsWith:
                            currSf += " LIKE '" + Sanitize(curr.Value) + "%' ";
                            break;
                        default:
                            continue;
                    }

                    if (added > 0) ret += " AND ";
                    ret += currSf;
                    added++;
                }
            }

            return ret;
        }

        #endregion
    }
}
