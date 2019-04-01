using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SyslogLogging;
using WatsonWebserver;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Connection manager.
    /// </summary>
    public class ConnectionManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private List<Connection> _Connections;
        private readonly object _Lock;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ConnectionManager()
        {
            _Connections = new List<Connection>();
            _Lock = new object();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add a connection.
        /// </summary>
        /// <param name="threadId">Thread ID.</param>
        /// <param name="req">HttpRequest object.</param>
        public void Add(int threadId, HttpRequest req)
        {
            if (threadId <= 0) return;
            if (req == null) return;

            Connection conn = new Connection();
            conn.ThreadId = threadId;
            conn.SourceIp = req.SourceIp;
            conn.SourcePort = req.SourcePort;
            conn.Method = req.Method;
            conn.RawUrl = req.RawUrlWithoutQuery;
            conn.StartTime = DateTime.Now;
            conn.EndTime = DateTime.Now;

            lock (_Lock)
            {
                _Connections.Add(conn);
            }
        }

        /// <summary>
        /// Close a connection.
        /// </summary>
        /// <param name="threadId">Thread ID.</param>
        public void Close(int threadId)
        {
            if (threadId <= 0) return;

            lock (_Lock)
            {
                _Connections = _Connections.Where(x => x.ThreadId != threadId).ToList();
            }
        }
        
        /// <summary>
        /// Retrieve a list of active connections.
        /// </summary>
        /// <returns>List of Connection objects.</returns>
        public List<Connection> GetActiveConnections()
        {
            List<Connection> curr = new List<Connection>();

            lock (_Lock)
            {
                curr = new List<Connection>(_Connections);
            }

            return curr;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
