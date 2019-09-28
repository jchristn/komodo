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
    internal class ConnectionManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private List<Connection> _Connections;
        private readonly object _Lock;

        #endregion

        #region Constructors-and-Factories

        internal ConnectionManager()
        {
            _Connections = new List<Connection>();
            _Lock = new object();
        }

        #endregion

        #region Internal-Methods
         
        internal void Add(int threadId, HttpContext ctx)
        {
            if (threadId <= 0) return;
            if (ctx == null) return;
            if (ctx.Request == null) return;

            Connection conn = new Connection();
            conn.ThreadId = threadId;
            conn.SourceIp = ctx.Request.SourceIp;
            conn.SourcePort = ctx.Request.SourcePort;
            conn.Method = ctx.Request.Method;
            conn.RawUrl = ctx.Request.RawUrlWithoutQuery;
            conn.StartTime = DateTime.Now.ToUniversalTime();
            conn.EndTime = DateTime.Now.ToUniversalTime();

            lock (_Lock)
            {
                _Connections.Add(conn);
            }
        }
         
        internal void Close(int threadId)
        {
            if (threadId <= 0) return;

            lock (_Lock)
            {
                _Connections = _Connections.Where(x => x.ThreadId != threadId).ToList();
            }
        }
         
        internal List<Connection> GetActiveConnections()
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
