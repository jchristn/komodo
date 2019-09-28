using System;
using System.Collections.Generic;
using System.Text;
using Komodo.Core.Enums;
using DatabaseWrapper;
using SqliteWrapper;

namespace Komodo.Core.Database
{
    /// <summary>
    /// Query builders needed to interact with postings database tables.
    /// </summary>
    public class PostingsQueries
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private Index _Index;
        private DatabaseWrapper.DatabaseClient _SqlDatabase;
        private SqliteWrapper.DatabaseClient _SqliteDatabase;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="sqlDatabase">The database client for non-Sqlite databases.</param>
        /// <param name="sqliteDatabase">The database client for Sqlite databases.</param>
        public PostingsQueries(
            Index index, 
            DatabaseWrapper.DatabaseClient sqlDatabase, 
            SqliteWrapper.DatabaseClient sqliteDatabase)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (index.PostingsDatabase == null) throw new ArgumentException("Index does not contain postings database settings.");
            if (sqlDatabase == null && sqliteDatabase == null) throw new ArgumentException("Only one database client must be supplied.");
            if (sqlDatabase != null && sqliteDatabase != null) throw new ArgumentException("Only one database client must be supplied.");

            _Index = index;
            _SqlDatabase = sqlDatabase;
            _SqliteDatabase = sqliteDatabase;
        }

        #endregion

        #region Public-Methods

        #region TermMap-Table

        /// <summary>
        /// Generate the query to create the terms map table.
        /// </summary>
        /// <returns>String.</returns>
        public string CreateTermsMapTable()
        {
            string query = "";

            switch (_Index.PostingsDatabase.Type)
            {
                case DatabaseType.MsSql:
                    query =
                        "USE " + _Index.PostingsDatabase.DatabaseName + ";" +
                        "IF NOT EXISTS " +
                        "(" +
                        "  SELECT * FROM sysobjects WHERE name = 'TermsMap' AND xtype = 'U' " +
                        ")" +
                        "CREATE TABLE TermsMap " +
                        "(" +
                        "  [Id]            [bigint] IDENTITY(1,1) NOT NULL, " +
                        "  [Term]          [nvarchar] (128) NULL, " +
                        "  [GUID]          [nvarchar] (128) NULL, " +
                        "  [Created]       [datetime2] (7) NULL, " +
                        "  [LastUpdate]    [datetime2] (7) NULL, " +
                        "  CONSTRAINT [PK_TermsMap] PRIMARY KEY CLUSTERED " +
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
                    throw new Exception("Unsupported postings database type: " + _Index.DocumentsDatabase.Type.ToString());
                case DatabaseType.SQLite:
                    query =
                        "CREATE TABLE IF NOT EXISTS TermsMap " +
                        "(" +
                        "  Id                INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "  Term              VARCHAR(128)  COLLATE NOCASE, " +
                        "  GUID              VARCHAR(128)  COLLATE NOCASE, " +
                        "  Created           VARCHAR(32), " +
                        "  LastUpdate        VARCHAR(32) " +
                        ")";
                    return query;
            }

            throw new ArgumentException("Invalid postings database type, use one of: Mssql, Mysql, Pgsql, Sqlite");
        }
        
        /// <summary>
        /// Generate the query to retrieve a term map.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns>String.</returns>
        public string GetTermMap(string term)
        {
            string query =
                "SELECT * FROM TermsMap WHERE Term = '" + Sanitize(term) + "'";
            return query;
        }

        /// <summary>
        /// Generate the query to add a term map.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns>String.</returns>
        public string AddTermMap(string term)
        {
            string ts = Timestamp(DateTime.Now.ToUniversalTime());

            string query =
                "INSERT INTO TermsMap " +
                "(" +
                "  Term, " +
                "  GUID, " +
                "  Created, " +
                "  LastUpdate " +
                ") " +
                "VALUES " +
                "(" +
                "  '" + Sanitize(term) + "', " +
                "  '" + Guid.NewGuid().ToString() + "', " +
                "  '" + ts + "', " +
                "  '" + ts + "' " +
                ")";
            return query;
        }

        /// <summary>
        /// Generate the query to remove a term map.
        /// </summary>
        /// <param name="term">Term.</param>
        /// <returns>String.</returns>
        public string RemoveTermMap(string term)
        {
            string query =
                "DELETE FROM TermsMap WHERE Term = '" + Sanitize(term) + "'";
            return query;
        }

        /// <summary>
        /// Generate the query to retrieve the list of terms.
        /// </summary> 
        /// <returns>String.</returns>
        public string GetTerms()
        {
            string query =
                "SELECT Term FROM TermsMap";
            return query;
        }

        /// <summary>
        /// Generate the query to retrieve a counting of the term maps.
        /// </summary>
        /// <returns>String.</returns>
        public string GetTermsCount()
        {
            string query =
                "SELECT COUNT(*) AS NumTerms FROM TermsMap";
            return query;
        }

        #endregion

        #region Postings-Table

        /// <summary>
        /// Generate the query to create a postings table.
        /// </summary>
        /// <param name="termGuid">Term GUID.</param>
        /// <returns>String.</returns>
        public string CreatePostingsTable(string termGuid)
        {
            string query = "";

            switch (_Index.PostingsDatabase.Type)
            {
                case DatabaseType.MsSql:
                    query =
                        "USE " + _Index.PostingsDatabase.DatabaseName + ";" +
                        "IF NOT EXISTS " +
                        "(" +
                        "  SELECT * FROM sysobjects WHERE name = '" + Sanitize(termGuid) + "' AND xtype = 'U' " +
                        ")" +
                        "CREATE TABLE [" + Sanitize(termGuid) + "] " +
                        "(" +
                        "  [Id]            [bigint] IDENTITY(1,1) NOT NULL, " +
                        "  [DocumentId]    [nvarchar] (128) NULL, " +
                        "  [Frequency]     [bigint] NULL, " +
                        "  [Positions]     [nvarchar] (2048) NULL, " +
                        "  [Created]       [datetime2] (7) NULL, " +
                        "  CONSTRAINT [PK_" + Sanitize(termGuid) + "] PRIMARY KEY CLUSTERED " +
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
                    throw new Exception("Unsupported postings database type: " + _Index.DocumentsDatabase.Type.ToString());
                case DatabaseType.SQLite:
                    query = 
                        "CREATE TABLE IF NOT EXISTS [" + Sanitize(termGuid) + "] " +
                        "(" +
                        "  Id                INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "  DocumentId        VARCHAR(128)  COLLATE NOCASE, " +
                        "  Frequency         BIGINT, " +
                        "  Positions         VARCHAR(2048), " +
                        "  Created           VARCHAR(32) " +
                        ")";
                    return query;
            }

            throw new ArgumentException("Invalid postings database type, use one of: Mssql, Mysql, Pgsql, Sqlite");
        }

        /// <summary>
        /// Generate the query to add a posting.
        /// </summary>
        /// <param name="posting">Posting.</param>
        /// <param name="termGuid">Term GUID.</param>
        /// <returns>String.</returns>
        public string AddPosting(Posting posting, string termGuid)
        {
            string ts = Timestamp(DateTime.Now.ToUniversalTime());

            List<long> trimmedPositions = TrimPositions(posting.Positions);
            string positions = Common.SerializeJson(trimmedPositions, false);

            string query =
                "INSERT INTO [" + Sanitize(termGuid) + "] " +
                "( " +
                "  DocumentId, " + 
                "  Frequency, " +
                "  Positions, " +
                "  Created " +
                ") " +
                "VALUES " +
                "( " +
                "  '" + Sanitize(posting.DocumentId) + "', " + 
                "  '" + posting.Frequency + "', " +
                "  '" + positions + "', " +
                "  '" + ts + "' " +
                ")";
            return query;
        }

        /// <summary>
        /// Generate the query to remove a posting.
        /// </summary>
        /// <param name="documentId">Document ID.</param>
        /// <param name="termGuid">Term GUID.</param>
        /// <returns>String.</returns>
        public string RemovePosting(string documentId, string termGuid)
        {
            string query =
                "DELETE FROM [" + Sanitize(termGuid) + "] WHERE DocumentId = '" + Sanitize(documentId) + "'";
            return query;
        }

        /// <summary>
        /// Generate the query to retrieve the number of postings for a given term.
        /// </summary>
        /// <param name="termGuid">Term GUID.</param>
        /// <returns>String.</returns>
        public string GetPostingsCount(string termGuid)
        {
            string query =
                "SELECT COUNT(*) AS NumPostings FROM [" + Sanitize(termGuid) + "]";
            return query;
        }

        /// <summary>
        /// Generate the query to retrieve postings.
        /// </summary>
        /// <param name="maxResults">Maximum number of results to retrieve.</param>
        /// <param name="startIndex">The index position with which to start.</param>
        /// <param name="termGuid">Term GUID.</param>
        /// <returns>String.</returns>
        public string GetPostings(int maxResults, long startIndex, string termGuid)
        {
            string ret = "";

            switch (_Index.PostingsDatabase.Type)
            {
                case DatabaseType.MsSql:
                    ret =
                        "SELECT * FROM [" + Sanitize(termGuid) + "] " +
                        "ORDER BY Frequency DESC " +
                        "OFFSET " + startIndex + " ROWS " +
                        "FETCH NEXT " + maxResults + " ROWS ONLY";
                    return ret;
                case DatabaseType.MySql:
                case DatabaseType.PgSql:
                    throw new Exception("Unsupported postings database type: " + _Index.DocumentsDatabase.Type.ToString());
                case DatabaseType.SQLite:
                    ret =
                        "SELECT * FROM [" + Sanitize(termGuid) + "]" +
                        "  ORDER BY Frequency DESC " +
                        "  LIMIT " + maxResults +
                        "  OFFSET " + startIndex;
                    return ret;
            }

            throw new ArgumentException("Invalid postings database type, use one of: Mssql, Mysql, Pgsql, Sqlite"); 
        }

        /// <summary>
        /// Generate the query to delete a postings table.
        /// </summary>
        /// <param name="termGuid">Term GUID.</param>
        /// <returns>String.</returns>
        public string DeletePostingsTable(string termGuid)
        {
            string query =
                "DROP TABLE IF EXISTS [" + Sanitize(termGuid) + "]";
            return query;
        }
         
        #endregion

        #endregion

        #region Private-Methods

        private string Sanitize(string str)
        {
            if (String.IsNullOrEmpty(str)) return null;

            if (_SqlDatabase != null) return _SqlDatabase.SanitizeString(str);
            else return (SqliteWrapper.DatabaseClient.SanitizeString(str));
        }

        private string Timestamp(DateTime dt)
        {
            if (_SqlDatabase != null) return _SqlDatabase.Timestamp(dt);
            else return _SqliteDatabase.Timestamp(dt);
        }

        private List<long> TrimPositions(List<long> positions)
        {
            if (positions == null || positions.Count < 1) return positions;

            bool tooLong = true;
            List<long> ret = new List<long>(positions);

            while (tooLong)
            {
                int len = Common.SerializeJson(ret, false).Length;
                if (len <= 2048) tooLong = false;

                int count = ret.Count;
                ret.RemoveAt((count - 1));
            }

            return ret;
        }

        #endregion
    }
}
