using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestWrapper;
using DatabaseWrapper;
using Komodo.Core.Enums;

namespace Komodo.Core
{
    /// <summary>
    /// Crawler for retrieving data from a file, web URL, or database.
    /// </summary>
    public class Crawler
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private DatabaseClient _Database = null;
        private bool _IsFile = false;
        private bool _IsUrl = false;
        private bool _IsDb = false;
        private string _SourceFile = null; 
        private string _Query = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the Crawler.
        /// </summary>
        /// <param name="sourceUrl">The source URL for the content.</param>
        /// <param name="docType">The DocType of the content.</param>
        public Crawler(string sourceUrl, DocType docType)
        {
            if (String.IsNullOrEmpty(sourceUrl)) throw new ArgumentNullException(nameof(sourceUrl));
             
            _SourceFile = sourceUrl;
             
            if (_SourceFile.StartsWith("http://")
                || _SourceFile.StartsWith("https://"))
            {
                _IsUrl = true;
            }
            else
            {
                _IsFile = true;
            }
        }

        /// <summary>
        /// Instantiates the Crawler.
        /// </summary>
        /// <param name="dbType">The database type, one of: mssql, mysql, pgsql.</param>
        /// <param name="serverHostname">The database server hostname.</param>
        /// <param name="serverPort">The port on which to connect to the database.</param>
        /// <param name="user">The database username.</param>
        /// <param name="pass">The database password.</param>
        /// <param name="instance">The database instance, useful only for mssql databases.</param>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="query">The query to execute against the database.</param>
        public Crawler(DatabaseType dbType, string serverHostname, int serverPort, string user, string pass, string instance, string databaseName, string query)
        {
            // if (String.IsNullOrEmpty(dbType)) throw new ArgumentNullException(nameof(dbType));
            if (String.IsNullOrEmpty(serverHostname)) throw new ArgumentNullException(nameof(serverHostname));
            if (serverPort < 0) throw new ArgumentOutOfRangeException(nameof(serverPort));
            if (String.IsNullOrEmpty(databaseName)) throw new ArgumentNullException(nameof(databaseName));
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            _IsDb = true; 
            _Query = query;

            _Database = new DatabaseClient(dbType.ToString(), serverHostname, serverPort, user, pass, instance, databaseName);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve data from the specified source.
        /// </summary>
        /// <returns>Byte array of data from the specified source.</returns>
        public byte[] RetrieveBytes()
        {
            if (_IsDb) throw new InvalidOperationException("Crawler initialized with database parameters, use RetrieveDataTable instead");
             
            if (_IsFile)
            { 
                return File.ReadAllBytes(_SourceFile);
            }
            else if (_IsUrl)
            { 
                RestRequest req = new RestRequest(
                    _SourceFile,
                    HttpMethod.GET,
                    null,
                    null);

                RestResponse resp = req.Send(); 
                if (resp == null || resp.StatusCode < 200 || resp.StatusCode > 299 || resp.Data == null || resp.ContentLength < 1)
                {
                    throw new IOException("Unable to retrieve a success response with data from the server");
                }

                return Common.StreamToBytes(resp.Data);
            } 
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve data from the database.
        /// </summary>
        /// <returns>DataTable containing rows from the database.</returns>
        public DataTable RerieveDataTable()
        {
            if (!_IsDb) throw new InvalidOperationException("Crawler initialized with file or web parameters, use RetrieveBytes instead");

            DataTable result = _Database.Query(_Query);
            return result;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
