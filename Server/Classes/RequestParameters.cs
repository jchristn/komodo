using System;
using System.Collections.Generic;
using System.IO;
using SyslogLogging;
using Komodo.Core;
using WatsonWebserver;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Request parameters extracted from the querystring and headers.
    /// </summary>
    public class RequestParameters
    {
        #region Public-Members
        
        /// <summary>
        /// Querystring 'cleanup' key, indicating that a cleanup should be performed.
        /// </summary>
        public bool Cleanup { get; set; }

        /// <summary>
        /// Querystring 'dbtype', indicating the type of database.
        /// </summary>
        public string DbType { get; set; }

        /// <summary>
        /// Querystring 'dbserver', indicating the database server IP address or hostname.
        /// </summary>
        public string DbServer { get; set; }

        /// <summary>
        /// Querystring 'dbport', indicating the port number on which the database is accessible.
        /// </summary>
        public int DbPort { get; set; }

        /// <summary>
        /// Querystring 'dbuser', indicating the database username.
        /// </summary>
        public string DbUser { get; set; }

        /// <summary>
        /// Querystring 'dbpass', indicating the password for the database user.
        /// </summary>
        public string DbPass { get; set; }

        /// <summary>
        /// Querystring 'dbinstance', indicating the MSSQL instance.
        /// </summary>
        public string DbInstance { get; set; }

        /// <summary>
        /// Querystring 'dbname', indicating the database name.
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// Querystring 'filename' key, indicating a source filename.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Querystring 'parsed' key, indicating whether or not the parsed document should be used.
        /// </summary>
        public bool Parsed { get; set; }

        /// <summary>
        /// Querystring 'pretty' key, indicating whether or not pretty formatting should be used.
        /// </summary>
        public bool Pretty { get; set; }

        /// <summary>
        /// Querystring 'type' key, indicating the type of document. 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Querystring 'url' key, indicating the source URL for a document.
        /// </summary>
        public string Url { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public RequestParameters()
        {
            Cleanup = false;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
