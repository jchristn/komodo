using System;
using SyslogLogging;
using WatsonWebserver;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Connection with a client.
    /// </summary>
    public class Connection
    {
        #region Public-Members

        /// <summary>
        /// Thread ID.
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// Client IP address.
        /// </summary>
        public string SourceIp { get; set; }

        /// <summary>
        /// Client port number.
        /// </summary>
        public int SourcePort { get; set; }

        /// <summary>
        /// HTTP method being used.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Raw URL.
        /// </summary>
        public string RawUrl { get; set; }

        /// <summary>
        /// Start time of the connection.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End time of the connection.
        /// </summary>
        public DateTime EndTime { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Connection()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
