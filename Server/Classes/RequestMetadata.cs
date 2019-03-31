using System;
using System.Collections.Generic;
using System.IO;
using SyslogLogging;
using Komodo.Core;
using WatsonWebserver;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Request metadata.
    /// </summary>
    public class RequestMetadata
    {
        #region Public-Members

        /// <summary>
        /// The received HTTP request.
        /// </summary>
        public HttpRequest Http { get; set; }

        /// <summary>
        /// Request parameters extracted from the headers and querystring.
        /// </summary>
        public RequestParameters Params { get; set; }

        /// <summary>
        /// The user associated with the request.
        /// </summary>
        public UserMaster User { get; set; }

        /// <summary>
        /// The API key associated with the request.
        /// </summary>
        public ApiKey ApiKey { get; set; }

        /// <summary>
        /// The permissions associated with the user and API key.
        /// </summary>
        public ApiKeyPermission Permission { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public RequestMetadata()
        {
            Params = new RequestParameters();
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
