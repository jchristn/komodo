using System;
using System.Collections.Generic;
using System.Text;
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

        private DatabaseClient _Database;

        #endregion

        #region Constructors-and-Factories

        public PostingsQueries(DatabaseClient database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            _Database = database;
        }

        #endregion

        #region Public-Methods

        #region TermMap-Table

        public string CreateTermMapTable()
        {
            string query =
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

        public string GetTermMap(string term)
        {
            string query =
                "SELECT * FROM TermsMap WHERE Term = '" + DatabaseClient.SanitizeString(term) + "'";
            return query;
        }

        public string AddTermMap(string term)
        {
            string ts = _Database.Timestamp(DateTime.Now.ToUniversalTime());

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
                "  '" + DatabaseClient.SanitizeString(term) + "', " +
                "  '" + Guid.NewGuid().ToString() + "', " +
                "  '" + ts + "', " +
                "  '" + ts + "' " +
                ")";
            return query;
        }

        public string RemoveTermMap(string term)
        {
            string query =
                "DELETE FROM TermsMap WHERE Term = '" + DatabaseClient.SanitizeString(term) + "'";
            return query;
        }

        public string GetTerms()
        {
            string query =
                "SELECT Term FROM TermsMap";
            return query;
        }

        #endregion

        #region Postings-Table

        public string CreatePostingsTable()
        {
            string query =
                "CREATE TABLE IF NOT EXISTS Postings " +
                "(" +
                "  Id                INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "  MasterDocId       VARCHAR(128)  COLLATE NOCASE, " + 
                "  Frequency         BIGINT, " +
                "  Positions         VARCHAR(4096), " +
                "  Created           VARCHAR(32) " +
                ")";
            return query;
        }

        public string AddPosting(Posting posting)
        {
            string ts = _Database.Timestamp(DateTime.Now.ToUniversalTime());
            string positions = Common.SerializeJson(posting.Positions, false);

            string query =
                "INSERT INTO Postings " +
                "( " +
                "  MasterDocId, " + 
                "  Frequency, " +
                "  Positions, " +
                "  Created " +
                ") " +
                "VALUES " +
                "( " +
                "  '" + DatabaseClient.SanitizeString(posting.MasterDocId) + "', " + 
                "  '" + posting.Frequency + "', " +
                "  '" + positions + "', " +
                "  '" + ts + "' " +
                ")";
            return query;
        }

        public string RemovePosting(string documentId)
        {
            string query =
                "DELETE FROM Postings WHERE MasterDocId = '" + DatabaseClient.SanitizeString(documentId) + "'";
            return query;
        }

        public string GetPostingsCount()
        {
            string query =
                "SELECT COUNT(*) AS NumPostings FROM Postings";
            return query;
        }

        public string GetPostings(int maxResults, long startIndex)
        {
            string query =
                "SELECT * FROM Postings " +
                "  ORDER BY Frequency DESC " +
                "  LIMIT " + maxResults +
                "  OFFSET " + startIndex;
            return query;
        }

        #endregion

        #endregion

        #region Private-Methods

        #endregion
    }
}
