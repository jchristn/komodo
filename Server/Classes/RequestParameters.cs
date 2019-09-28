using System;
using System.Net;
using System.Collections.Generic;

using Komodo.Core;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Request parameters extracted from the querystring and headers.
    /// </summary>
    public class RequestParameters
    {
        #region Public-Members

        /// <summary>
        /// Bypass indexing, i.e. store a document without indexing.
        /// </summary>
        public bool Bypass = false;

        /// <summary>
        /// Querystring 'cleanup' key, indicating that a cleanup should be performed.
        /// </summary>
        public bool Cleanup = false;

        /// <summary>
        /// Querystring 'dbtype', indicating the type of database.
        /// </summary>
        public string DbType = null;

        /// <summary>
        /// Querystring 'dbserver', indicating the database server IP address or hostname.
        /// </summary>
        public string DbServer = null;

        /// <summary>
        /// Querystring 'dbport', indicating the port number on which the database is accessible.
        /// </summary>
        public int DbPort = 0;

        /// <summary>
        /// Querystring 'dbuser', indicating the database username.
        /// </summary>
        public string DbUser = null;

        /// <summary>
        /// Querystring 'dbpass', indicating the password for the database user.
        /// </summary>
        public string DbPass = null;

        /// <summary>
        /// Querystring 'dbinstance', indicating the MSSQL instance.
        /// </summary>
        public string DbInstance { get; set; }

        /// <summary>
        /// Querystring 'dbname', indicating the database name.
        /// </summary>
        public string DbName = null;

        /// <summary>
        /// Querystring 'filename' key, indicating a source filename.
        /// </summary>
        public string Filename = null;

        /// <summary>
        /// Querystring 'name' key, typically used to indicate a document name.
        /// </summary>
        public string Name = null;

        /// <summary>
        /// Querystring 'parsed' key, indicating whether or not the parsed document should be used.
        /// </summary>
        public bool Parsed = false;

        /// <summary>
        /// Querystring 'pretty' key, indicating whether or not pretty formatting should be used.
        /// </summary>
        public bool Pretty = true;

        /// <summary>
        /// Querystring 'tags' key, typically used to indicate the tag data to attach to a document being indexed.
        /// </summary>
        public string Tags = null;

        /// <summary>
        /// Querystring 'title' key, typically used to indicate the title of a document for indexing.
        /// </summary>
        public string Title = null;

        /// <summary>
        /// Querystring 'type' key, indicating the type of document. 
        /// </summary>
        public string Type = null;

        /// <summary>
        /// Querystring 'url' key, indicating the source URL for a document.
        /// </summary>
        public string Url = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public RequestParameters()
        { 
        }

        #endregion

        #region Internal-Methods

        internal static RequestParameters FromDictionary(Dictionary<string, string> qs)
        {
            RequestParameters ret = new RequestParameters();

            if (qs == null) return ret;

            /*
            if (qs.ContainsKey("bypass")) ret.Bypass = Convert.ToBoolean(qs["bypass"]);
            if (qs.ContainsKey("cleanup")) ret.Cleanup = Convert.ToBoolean(qs["cleanup"]);

            if (qs.ContainsKey("dbtype")) ret.DbType = qs["dbtype"];
            if (qs.ContainsKey("dbserver")) ret.DbServer = qs["dbserver"];
            if (qs.ContainsKey("dbport")) ret.DbPort = Convert.ToInt32(qs["dbport"]);
            if (qs.ContainsKey("dbuser")) ret.DbUser = qs["dbuser"];
            if (qs.ContainsKey("dbpass")) ret.DbPass = qs["dbpass"];
            if (qs.ContainsKey("dbinstance")) ret.DbInstance = qs["dbinstance"];
            if (qs.ContainsKey("dbname")) ret.DbName = qs["dbname"];

            if (qs.ContainsKey("filename")) ret.Filename = qs["filename"];
            if (qs.ContainsKey("name")) ret.Name = qs["name"];
            if (qs.ContainsKey("parsed")) ret.Parsed = Convert.ToBoolean(qs["parsed"]);
            if (qs.ContainsKey("pretty")) ret.Pretty = Convert.ToBoolean(qs["pretty"]);
            if (qs.ContainsKey("tags")) ret.Tags = qs["tags"];
            if (qs.ContainsKey("title")) ret.Title = qs["title"];
            if (qs.ContainsKey("type")) ret.Type = qs["type"];
            if (qs.ContainsKey("url")) ret.Url = qs["url"];
            */

            if (qs.ContainsKey("bypass")) ret.Bypass = Convert.ToBoolean(qs["bypass"]);
            if (qs.ContainsKey("cleanup")) ret.Cleanup = Convert.ToBoolean(qs["cleanup"]);

            if (qs.ContainsKey("dbtype")) ret.DbType = WebUtility.UrlDecode(qs["dbtype"]);
            if (qs.ContainsKey("dbserver")) ret.DbServer = WebUtility.UrlDecode(qs["dbserver"]);
            if (qs.ContainsKey("dbport")) ret.DbPort = Convert.ToInt32(qs["dbport"]);
            if (qs.ContainsKey("dbuser")) ret.DbUser = WebUtility.UrlDecode(qs["dbuser"]);
            if (qs.ContainsKey("dbpass")) ret.DbPass = WebUtility.UrlDecode(qs["dbpass"]);
            if (qs.ContainsKey("dbinstance")) ret.DbInstance = WebUtility.UrlDecode(qs["dbinstance"]);
            if (qs.ContainsKey("dbname")) ret.DbName = WebUtility.UrlDecode(qs["dbname"]);

            if (qs.ContainsKey("filename")) ret.Filename = WebUtility.UrlDecode(qs["filename"]);
            if (qs.ContainsKey("name")) ret.Name = WebUtility.UrlDecode(qs["name"]);
            if (qs.ContainsKey("parsed")) ret.Parsed = Convert.ToBoolean(qs["parsed"]);
            if (qs.ContainsKey("pretty")) ret.Pretty = Convert.ToBoolean(qs["pretty"]);
            if (qs.ContainsKey("tags")) ret.Tags = WebUtility.UrlDecode(qs["tags"]);
            if (qs.ContainsKey("title")) ret.Title = WebUtility.UrlDecode(qs["title"]);
            if (qs.ContainsKey("type")) ret.Type = WebUtility.UrlDecode(qs["type"]);
            if (qs.ContainsKey("url")) ret.Url = WebUtility.UrlDecode(qs["url"]);
             
            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
