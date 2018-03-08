using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SyslogLogging;
using WatsonWebserver;

namespace KomodoServer
{
    public class ConnectionManager
    {
        #region Private-Members

        private List<Connection> ActiveConnections;
        private readonly object ConnectionLock;

        #endregion

        #region Constructors-and-Factories

        public ConnectionManager()
        {
            ActiveConnections = new List<Connection>();
            ConnectionLock = new object();
        }

        #endregion

        #region Public-Methods

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

            lock (ConnectionLock)
            {
                ActiveConnections.Add(conn);
            }
        }

        public void Close(int threadId)
        {
            if (threadId <= 0) return;

            lock (ConnectionLock)
            {
                ActiveConnections = ActiveConnections.Where(x => x.ThreadId != threadId).ToList();
            }
        }
        
        public List<Connection> GetActiveConnections()
        {
            List<Connection> curr = new List<Connection>();

            lock (ConnectionLock)
            {
                curr = new List<Connection>(ActiveConnections);
            }

            return curr;
        }

        #endregion
    }
}
