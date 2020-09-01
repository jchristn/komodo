using System;
using System.Data;
using Watson.ORM;
using Watson.ORM.Core;
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

        private DbSettings _DbSettings = null;
        private WatsonORM _ORM = null; 
        private string _Query = null;

        #endregion

        #region Constructors-and-Factories
           
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="settings">Database settings.</param>
        /// <param name="query">Query to use for crawling.</param>
        public SqlCrawler(DbSettings settings, string query)
        { 
            if (settings == null) throw new ArgumentNullException(nameof(settings)); 
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            _DbSettings = settings;
            _ORM = new WatsonORM(_DbSettings.ToDatabaseSettings());
            _ORM.InitializeDatabase();

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
                result = _ORM.Query(_Query);
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
         
        #endregion
    }
}
