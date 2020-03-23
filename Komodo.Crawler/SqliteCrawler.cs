using System;
using System.Data;
using DatabaseWrapper; 
using Komodo.Classes;

namespace Komodo.Crawler
{
    /// <summary>
    /// Crawler for retrieving data from a Sqlite database file.
    /// </summary>
    public class SqliteCrawler
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
        /// <param name="filename">File containing the Sqlite database.</param>
        /// <param name="query">Query to use for crawling.</param>
        public SqliteCrawler(string filename, string query)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            _DatabaseSettings = new DatabaseSettings(filename);
            _Database = new DatabaseClient(filename);
            _Query = query; 
        }

        #endregion

        #region Public-Methods
          
        /// <summary>
        /// Retrieve data from the database using the supplied query.
        /// </summary>
        /// <returns>DatabaseCrawlResult.</returns>
        public SqliteCrawlResult Get()
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

            SqliteCrawlResult ret = new SqliteCrawlResult();
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
