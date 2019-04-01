using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;
using SqliteWrapper;

namespace Komodo.Core
{
    /// <summary>
    /// Query builders needed to interact with database tables.
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
            if (index.Database == null) throw new ArgumentException("Index does not contain database settings.");
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

            switch (_Index.Database.Type)
            {
                case DatabaseType.Mssql:
                    query =
                        "USE " + _Index.Database.DatabaseName + ";" +
                        "IF NOT EXISTS " +
                        "(" +
                        "  SELECT * FROM sysobjects WHERE name = 'SourceDocuments' AND xtype = 'U' " +
                        ")" +
                        "CREATE TABLE SourceDocuments " +
                        "(" +
                        "  [Id]            [bigint] IDENTITY(1,1) NOT NULL, " +
                        "  [IndexName]     [nvarchar] (128) NULL, " +
                        "  [MasterDocId]   [nvarchar] (128) NULL, " +
                        "  [DocType]       [nvarchar] (32) NULL, " +
                        "  [SourceUrl]     [nvarchar] (256) NULL, " +
                        "  [ContentLength] [bigint] NULL, " +
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
                case DatabaseType.Mysql:
                case DatabaseType.Pgsql:
                case DatabaseType.Sqlite:
                    query =
                        "CREATE TABLE IF NOT EXISTS SourceDocuments " +
                        "(" +
                        "  Id                INTEGER PRIMARY KEY, " +
                        "  IndexName         VARCHAR(128), " +
                        "  MasterDocId       VARCHAR(128), " +
                        "  DocType           VARCHAR(32), " +
                        "  SourceUrl         VARCHAR(256), " +
                        "  ContentLength     INTEGER, " +
                        "  Created           VARCHAR(32), " +
                        "  Indexed           VARCHAR(32) " +
                        ")";
                    return query;
            }

            throw new ArgumentException("Invalid database type, use one of: Mssql, Mysql, Pgsql, Sqlite");
        }

        /// <summary>
        /// Generate the query to create the parsed documents table.
        /// </summary>
        /// <returns>String.</returns>
        public string CreateParsedDocsTable()
        {
            string query = "";

            switch (_Index.Database.Type)
            { 
                case DatabaseType.Mssql:
                    query =
                        "USE " + _Index.Database.DatabaseName + ";" +
                        "IF NOT EXISTS " +
                        "(" +
                        "  SELECT * FROM sysobjects WHERE name = 'ParsedDocuments' AND xtype = 'U' " +
                        ")" +
                        "CREATE TABLE ParsedDocuments " +
                        "(" +
                        "  [Id]                  [bigint] IDENTITY(1,1) NOT NULL, " +
                        "  [IndexName]           [nvarchar] (128) NULL, " +
                        "  [MasterDocId]         [nvarchar] (128) NULL, " +
                        "  [DocType]             [nvarchar] (32) NULL, " +
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
                case DatabaseType.Mysql:
                case DatabaseType.Pgsql:
                case DatabaseType.Sqlite:
                    query =
                        "CREATE TABLE IF NOT EXISTS ParsedDocuments " +
                        "(" +
                        "  Id                   INTEGER PRIMARY KEY, " +
                        "  IndexName            VARCHAR(128), " +
                        "  MasterDocId          VARCHAR(128), " +
                        "  DocType              VARCHAR(32), " +
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
        /// Generate the query to create the terms table.
        /// </summary>
        /// <returns>String.</returns>
        public string CreateTermsTable()
        {
            string query = "";

            switch (_Index.Database.Type)
            {
                case DatabaseType.Mssql:
                    query =
                        "USE " + _Index.Database.DatabaseName + ";" +
                        "IF NOT EXISTS " +
                        "(" +
                        "  SELECT * FROM sysobjects WHERE name = 'Terms' AND xtype = 'U' " +
                        ")" +
                        "CREATE TABLE Terms " +
                        "(" +
                        "  [Id]                  [bigint] IDENTITY(1,1) NOT NULL, " +
                        "  [IndexName]           [nvarchar] (128) NULL, " +
                        "  [MasterDocId]         [nvarchar] (128) NULL, " +
                        "  [Term]                [nvarchar] (128) NULL, " +
                        "  [Created]             [datetime2] (7) NULL, " +
                        "  [Indexed]             [datetime2] (7) NULL, " +
                        "  CONSTRAINT [PK_Terms] PRIMARY KEY CLUSTERED " +
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
                case DatabaseType.Mysql:
                case DatabaseType.Pgsql:
                case DatabaseType.Sqlite:
                    query =
                        "CREATE TABLE IF NOT EXISTS Terms " +
                        "(" +
                        "  Id                INTEGER PRIMARY KEY, " +
                        "  IndexName         VARCHAR(128), " +
                        "  MasterDocId       VARCHAR(128), " +
                        "  Term              BLOB, " +
                        "  Created           VARCHAR(32), " +
                        "  Indexed           VARCHAR(32) " +
                        ")";
                    return query;
            }

            throw new ArgumentException("Invalid database type, use one of: Mssql, Mysql, Pgsql, Sqlite");
        }
         
        /// <summary>
        /// Generate the query to retrieve document IDs based on query terms match.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <returns>String.</returns>
        public string SelectDocIdsByTerms(SearchQuery query)
        {
            string dbQuery = "";
            int requiredAdded = 0; 
            int excludeAdded = 0;
            int optionalAdded = 0;
            
            switch (_Index.Database.Type)
            {
                case DatabaseType.Mssql:
                    #region Mssql
                     
                    dbQuery =
                        "SELECT DISTINCT MasterDocId " +
                        "FROM Terms " +
                        "WHERE ("; 

                    #region Required-Terms

                    // open paren, required terms subclause
                    dbQuery += "(";

                    dbQuery += "Term IN (";
                     
                    foreach (string currTerm in query.Required.Terms)
                    {
                        string sanitizedTerm = String.Copy(currTerm);
                        if (_Index.Options.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                        sanitizedTerm = Sanitize(sanitizedTerm);

                        if (requiredAdded > 0) dbQuery += ",";
                        dbQuery += "'" + sanitizedTerm + "'";
                        requiredAdded++;
                    }

                    dbQuery += ")";

                    // close paren, required terms subclause
                    dbQuery += ")";

                    #endregion

                    #region Optional-Terms

                    if (query.Optional != null && query.Optional.Terms != null && query.Optional.Terms.Count > 0)
                    {
                        // open paren, optional terms subclause
                        dbQuery += " AND (Term IS NOT NULL OR ";

                        dbQuery += "Term IN (";

                        foreach (string currTerm in query.Optional.Terms)
                        {
                            string sanitizedTerm = String.Copy(currTerm);
                            if (_Index.Options.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                            sanitizedTerm = Sanitize(sanitizedTerm);

                            if (optionalAdded > 0) dbQuery += ",";
                            dbQuery += "'" + sanitizedTerm + "'";
                            optionalAdded++;
                        }

                        dbQuery += ")";

                        // close paren, optional terms subclause
                        dbQuery += ")";
                    }

                    #endregion

                    #region Excluded-Terms

                    if (query.Exclude != null && query.Exclude.Terms != null && query.Exclude.Terms.Count > 0)
                    {
                        // open paren, exclude terms subclause
                        dbQuery += " AND (";

                        dbQuery += "Term NOT IN (";

                        foreach (string currTerm in query.Exclude.Terms)
                        {
                            string sanitizedTerm = String.Copy(currTerm);
                            if (_Index.Options.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                            sanitizedTerm = Sanitize(sanitizedTerm);

                            if (requiredAdded > 0) dbQuery += ",";
                            dbQuery += "'" + sanitizedTerm + "'";
                            excludeAdded++;
                        }

                        dbQuery += ")";

                        // close paren, exclude terms subclause
                        dbQuery += ")";
                    }

                    #endregion

                    dbQuery += ") ";

                    #region Pagination

                    if (query.MaxResults != null && query.MaxResults > 0 && query.MaxResults <= 100)
                    {
                        if (query.StartIndex != null && query.StartIndex > 0)
                        {
                            dbQuery +=
                                " ORDER BY MasterDocId OFFSET " + query.StartIndex + " ROWS " +
                                " FETCH NEXT " + query.MaxResults + " ROWS ONLY";
                        }
                        else
                        {
                            dbQuery +=
                                " ORDER BY MasterDocId OFFSET 0 ROWS " +
                                " FETCH NEXT " + query.MaxResults + " ROWS ONLY";
                        }
                    }
                    else
                    {
                        dbQuery +=
                            " ORDER BY MasterDocId OFFSET 0 ROWS " +
                            " FETCH NEXT 10 ROWS ONLY";
                    }

                    #endregion

                    return dbQuery;

                    #endregion

                case DatabaseType.Mysql:
                case DatabaseType.Pgsql:
                case DatabaseType.Sqlite:
                    #region Sqlite

                    dbQuery =
                        "SELECT DISTINCT MasterDocId " +
                        "FROM Terms " +
                        "WHERE (";

                    #region Required-Terms

                    // open paren, required terms subclause
                    dbQuery += "(";

                    dbQuery += "Term IN (";

                    foreach (string currTerm in query.Required.Terms)
                    {
                        string sanitizedTerm = String.Copy(currTerm);
                        if (_Index.Options.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                        sanitizedTerm = Sanitize(sanitizedTerm);

                        if (requiredAdded > 0) dbQuery += ",";
                        dbQuery += "'" + sanitizedTerm + "'";
                        requiredAdded++;
                    }

                    dbQuery += ")";

                    // close paren, required terms subclause
                    dbQuery += ")";

                    #endregion

                    #region Optional-Terms

                    if (query.Optional != null && query.Optional.Terms != null && query.Optional.Terms.Count > 0)
                    {
                        // open paren, optional terms subclause
                        dbQuery += " AND (Term IS NOT NULL OR ";

                        dbQuery += "Term IN (";
                         
                        foreach (string currTerm in query.Optional.Terms)
                        {
                            string sanitizedTerm = String.Copy(currTerm);
                            if (_Index.Options.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                            sanitizedTerm = Sanitize(sanitizedTerm);

                            if (optionalAdded > 0) dbQuery += ",";
                            dbQuery += "'" + sanitizedTerm + "'";
                            optionalAdded++;
                        }

                        dbQuery += ")";

                        // close paren, optional terms subclause
                        dbQuery += ")";
                    }

                    #endregion

                    #region Excluded-Terms

                    if (query.Exclude != null && query.Exclude.Terms != null && query.Exclude.Terms.Count > 0)
                    {
                        // open paren, exclude terms subclause
                        dbQuery += " AND (";

                        dbQuery += "Term NOT IN (";
                         
                        foreach (string currTerm in query.Exclude.Terms)
                        {
                            string sanitizedTerm = String.Copy(currTerm);
                            if (_Index.Options.NormalizeCase) sanitizedTerm = sanitizedTerm.ToLower();
                            sanitizedTerm = Sanitize(sanitizedTerm);

                            if (requiredAdded > 0) dbQuery += ",";
                            dbQuery += "'" + sanitizedTerm + "'";
                            excludeAdded++;
                        }

                        dbQuery += ")";

                        // close paren, exclude terms subclause
                        dbQuery += ")";
                    }

                    #endregion

                    dbQuery += ") ";

                    #region Pagination

                    if (query.MaxResults != null && query.MaxResults > 0 && query.MaxResults <= 100)
                    {
                        dbQuery += "LIMIT " + query.MaxResults;

                        if (query.StartIndex != null && query.StartIndex > 0)
                        {
                            dbQuery += ", " + query.StartIndex;
                        }
                    }
                    else
                    {
                        dbQuery += "LIMIT 10";
                    }

                    #endregion

                    return dbQuery;

                    #endregion
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
            string ret = "SELECT * FROM SourceDocuments ";

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
