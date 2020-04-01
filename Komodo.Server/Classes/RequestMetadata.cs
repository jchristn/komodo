using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WatsonWebserver;
using Komodo.Classes;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Request metadata.
    /// </summary>
    public class RequestMetadata
    {
        /// <summary>
        /// HTTP context.
        /// </summary>
        public HttpContext Http { get; }

        /// <summary>
        /// User.
        /// </summary>
        public User User { get; }

        /// <summary>
        /// API key.
        /// </summary>
        public ApiKey ApiKey { get; }

        /// <summary>
        /// Permissions associated with the API key.
        /// </summary>
        public Permission Permission { get; }

        /// <summary>
        /// Request parameters.
        /// </summary>
        public RequestParameters Params { get; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public RequestMetadata()
        {
            Params = new RequestParameters();
            if (Http != null && Http.Request.Querystring != null && Http.Request.QuerystringEntries.Count > 0)
                Params = RequestParameters.FromDictionary(Http.Request.QuerystringEntries);
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="http">HTTP context.</param>
        /// <param name="user">User.</param>
        /// <param name="apiKey">API key.</param>
        /// <param name="perm">Permissions associated with the API key.</param>
        public RequestMetadata(HttpContext http, User user, ApiKey apiKey, Permission perm)
        {
            Http = http;
            User = user;
            ApiKey = apiKey;
            Permission = perm;
            Params = new RequestParameters();
            if (Http != null && Http.Request.Querystring != null && Http.Request.QuerystringEntries.Count > 0)
                Params = RequestParameters.FromDictionary(Http.Request.QuerystringEntries);
        }

        /// <summary>
        /// Request parameters.
        /// </summary>
        public class RequestParameters
        {
            #region Public-Members

            /// <summary>
            /// Do not process the request, but rather, return the request metadata.  Useful for examining how the request was interpreted by the server.
            /// </summary>
            public bool Metadata = false;

            /// <summary>
            /// Querystring 'bypass', indicating that indexing should be bypassed, i.e. store a document without indexing.
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
            /// Querystring 'dbinstance', indicating the SQL Server Express instance.
            /// </summary>
            public string DbInstance = null;

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
            public bool Pretty = false;

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

            /// <summary>
            /// Indicates the URL to which results should be POSTed after an indexing operation.
            /// </summary>
            public string Postback = null;

            /// <summary>
            /// Indicates that an enumeration operation is desired, where appropriate.
            /// </summary>
            public bool Enumerate = false;

            /// <summary>
            /// Indicates that an indexing operation should be handled asynchronously.
            /// </summary>
            public bool Async = false;

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

                if (qs == null || qs.Count < 1) return ret;

                if (qs.ContainsKey("metadata")) ret.Metadata = true;
                if (qs.ContainsKey("bypass")) ret.Bypass = true;
                if (qs.ContainsKey("cleanup")) ret.Cleanup = true;
                if (qs.ContainsKey("async")) ret.Async = true;
                if (qs.ContainsKey("enumerate")) ret.Enumerate = true;

                if (qs.ContainsKey("dbtype")) ret.DbType = WebUtility.UrlDecode(qs["dbtype"]);
                if (qs.ContainsKey("dbserver")) ret.DbServer = WebUtility.UrlDecode(qs["dbserver"]);
                if (qs.ContainsKey("dbport")) ret.DbPort = Convert.ToInt32(qs["dbport"]);
                if (qs.ContainsKey("dbuser")) ret.DbUser = WebUtility.UrlDecode(qs["dbuser"]);
                if (qs.ContainsKey("dbpass")) ret.DbPass = WebUtility.UrlDecode(qs["dbpass"]);
                if (qs.ContainsKey("dbinstance")) ret.DbInstance = WebUtility.UrlDecode(qs["dbinstance"]);
                if (qs.ContainsKey("dbname")) ret.DbName = WebUtility.UrlDecode(qs["dbname"]);

                if (qs.ContainsKey("filename")) ret.Filename = WebUtility.UrlDecode(qs["filename"]);
                if (qs.ContainsKey("name")) ret.Name = WebUtility.UrlDecode(qs["name"]);
                if (qs.ContainsKey("parsed")) ret.Parsed = true;
                if (qs.ContainsKey("pretty")) ret.Pretty = true;
                if (qs.ContainsKey("tags")) ret.Tags = WebUtility.UrlDecode(qs["tags"]);
                if (qs.ContainsKey("title")) ret.Title = WebUtility.UrlDecode(qs["title"]);
                if (qs.ContainsKey("type")) ret.Type = WebUtility.UrlDecode(qs["type"]);
                if (qs.ContainsKey("url")) ret.Url = WebUtility.UrlDecode(qs["url"]);
                if (qs.ContainsKey("postback")) ret.Postback = WebUtility.UrlDecode(qs["postback"]);
                 
                return ret;
            }

            #endregion

            #region Private-Methods

            #endregion
        }
    }
}
