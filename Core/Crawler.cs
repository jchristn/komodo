using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestWrapper;
using DatabaseWrapper;

namespace Komodo.Core
{
    /// <summary>
    /// Crawler for retrieving data.
    /// </summary>
    public class Crawler
    {
        #region Public-Members

        /// <summary>
        /// Indicates if the target is a file.
        /// </summary>
        public bool IsFile = false;

        /// <summary>
        /// Indicates if the target is a URL.
        /// </summary>
        public bool IsUrl = false;

        /// <summary>
        /// Indicates if the target is a database.
        /// </summary>
        public bool IsDb = false;

        /// <summary>
        /// The source file containing the desired data.
        /// </summary>
        public string SourceFile = null;

        /// <summary>
        /// The content type of the data.
        /// </summary>
        public string ContentType = null;

        /// <summary>
        /// The type of database, one of: mssql, mysql, pgsql
        /// </summary>
        public string DbType = "mssql";

        /// <summary>
        /// The database server hostname.
        /// </summary>
        public string DbServerHostname = null;

        /// <summary>
        /// The port on which to connect to the database.
        /// </summary>
        public int DbServerPort = 0;

        /// <summary>
        /// The database username.
        /// </summary>
        public string DbUser = null;

        /// <summary>
        /// The database password.
        /// </summary>
        public string DbPass = null;

        /// <summary>
        /// The database instance, useful only for mssql databases.
        /// </summary>
        public string DbInstance = null;

        /// <summary>
        /// The name of the database.
        /// </summary>
        public string DbName = null;

        /// <summary>
        /// The query to execute against the database.
        /// </summary>
        public string DbQuery = null;

        #endregion

        #region Private-Members

        private DatabaseClient DbClient = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the Crawler.
        /// </summary>
        /// <param name="sourceUrl">The source URL for the content.</param>
        /// <param name="contentType">The content type, one of: json, xml, html, text.</param>
        public Crawler(string sourceUrl, string contentType)
        {
            if (String.IsNullOrEmpty(sourceUrl)) throw new ArgumentNullException(nameof(sourceUrl));
            if (String.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
            
            ContentType = contentType;
            SourceFile = sourceUrl;

            if (!ContentType.Equals("json")
                && !ContentType.Equals("xml")
                && !ContentType.Equals("html")
                && !ContentType.Equals("text"))
            {
                throw new ArgumentOutOfRangeException(nameof(contentType));
            }

            if (SourceFile.StartsWith("http://")
                || SourceFile.StartsWith("https://"))
            {
                IsUrl = true;
            }
            else
            {
                IsFile = true;
            }
        }

        /// <summary>
        /// Instantiates the Crawler.
        /// </summary>
        /// <param name="sourceUrl">The source URL for the content.</param>
        /// <param name="contentType">The DocType of the content.</param>
        public Crawler(string sourceUrl, DocType docType)
        {
            if (String.IsNullOrEmpty(sourceUrl)) throw new ArgumentNullException(nameof(sourceUrl));

            switch (docType)
            {
                case DocType.Html:
                    ContentType = "html";
                    break;
                case DocType.Json:
                    ContentType = "json";
                    break;
                case DocType.Text:
                    ContentType = "text";
                    break;
                case DocType.Xml:
                    ContentType = "xml";
                    break;
                default:
                    throw new ArgumentException("Unknown DocType");
            }
             
            SourceFile = sourceUrl;
             
            if (SourceFile.StartsWith("http://")
                || SourceFile.StartsWith("https://"))
            {
                IsUrl = true;
            }
            else
            {
                IsFile = true;
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
        public Crawler(string dbType, string serverHostname, int serverPort, string user, string pass, string instance, string databaseName, string query)
        {
            if (String.IsNullOrEmpty(dbType)) throw new ArgumentNullException(nameof(dbType));
            if (String.IsNullOrEmpty(serverHostname)) throw new ArgumentNullException(nameof(serverHostname));
            if (serverPort < 0) throw new ArgumentOutOfRangeException(nameof(serverPort));
            if (String.IsNullOrEmpty(databaseName)) throw new ArgumentNullException(nameof(databaseName));
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            IsDb = true;
            ContentType = "sql";

            DbType = dbType;
            DbServerHostname = serverHostname;
            DbServerPort = serverPort;
            DbUser = user;
            DbPass = pass;
            DbInstance = instance;
            DbName = databaseName;
            DbQuery = query;

            if (!DbType.Equals("mssql")
                && !DbType.Equals("mysql"))
            {
                throw new ArgumentOutOfRangeException(nameof(dbType));
            }

            DbClient = new DatabaseClient(DbType, DbServerHostname, DbServerPort, DbUser, DbPass, DbInstance, DbName);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve data from the specified source.
        /// </summary>
        /// <returns>Byte array of data from the specified source.</returns>
        public byte[] RetrieveBytes()
        {
            if (IsDb) throw new InvalidOperationException("Crawler initialized with database parameters, use RetrieveDataTable instead");

            byte[] data = null;

            if (IsFile)
            {
                #region File

                data = File.ReadAllBytes(SourceFile);

                #endregion
            }
            else if (IsUrl)
            {
                #region Url

                RestRequest req = new RestRequest(
                    SourceFile,
                    HttpMethod.GET,
                    null,
                    null,
                    true);

                RestResponse resp = req.Send(); 
                if (resp == null)
                {
                    throw new IOException("Unable to read data from specified URL");
                }

                if (resp.StatusCode < 200 || resp.StatusCode > 200)
                {
                    throw new IOException("Non-200 status code returned from server");
                }

                if (resp.Data != null && resp.Data.Length > 0)
                {
                    data = new byte[resp.Data.Length];
                    Buffer.BlockCopy(resp.Data, 0, data, 0, resp.Data.Length);
                }

                #endregion
            }
            else
            {
                throw new NotImplementedException("Unknown source media");
            }

            return data;
        }

        /// <summary>
        /// Retrieve data from the database.
        /// </summary>
        /// <returns>DataTable containing rows from the database.</returns>
        public DataTable RerieveDataTable()
        {
            if (!IsDb) throw new InvalidOperationException("Crawler initialized with file or web parameters, use RetrieveString instead");

            DataTable result = DbClient.Query(DbQuery);
            return result;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
