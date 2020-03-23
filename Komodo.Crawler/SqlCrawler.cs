using System;
using System.Data;
using DatabaseWrapper; 
using Komodo.Classes;

namespace Komodo.Crawler
{
    /// <summary>
    /// Crawler for retrieving data from a SQL database (SQL Server, MySQL, or PostgreSQL).
    /// </summary>
    public class SqlCrawler
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private DatabaseSettings _DatabaseSettings = null;
        private DatabaseClient _Database = null; 
        private string _Query = null;

        #endregion

        #region Constructors-and-Factories
           
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="database">Database settings.</param>
        /// <param name="query">Query to use for crawling.</param>
        public SqlCrawler(DatabaseSettings database, string query)
        { 
            if (database == null) throw new ArgumentNullException(nameof(database)); 
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            _DatabaseSettings = database;
            _Database = InitializeDatabase(_DatabaseSettings);
            _Query = query; 
        }

        #endregion

        #region Public-Methods
          
        /// <summary>
        /// Retrieve data from the database using the supplied query.
        /// </summary>
        /// <returns>DatabaseCrawlResult.</returns>
        public SqlCrawlResult Get()
        {
            bool success = false;
            DataTable result = null;

            try
            {
                result = _Database.Query(_Query);
                success = true;
            }
            catch (Exception)
            {

            }

            SqlCrawlResult ret = new SqlCrawlResult();
            ret.DataTable = result;
            ret.Success = success;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        #endregion

        #region Private-Methods

        private DatabaseClient InitializeDatabase(DatabaseSettings settings)
        {
            return new DatabaseClient(
               settings.Type,
               settings.Hostname,
               settings.Port,
               settings.Username,
               settings.Password,
               settings.Instance,
               settings.DatabaseName);
        }
         
        #endregion
    }
}
