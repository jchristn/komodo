using System;
using System.Collections.Generic;
using System.Text;
using WatsonWebserver;
using Komodo.Database; 

namespace Komodo.Server.Classes
{
    internal class ConnManager
    {
        private readonly object _Lock = new object();
        private Dictionary<int, HttpContext> _Connections = new Dictionary<int, HttpContext>();

        internal ConnManager()
        {
        }
         
        internal void Add(int threadId, HttpContext ctx)
        {
            lock (_Lock)
            {
                if (_Connections.ContainsKey(threadId)) _Connections.Remove(threadId);
                _Connections.Add(threadId, ctx);
            }
        }

        internal void Remove(int threadId)
        {
            lock (_Lock)
            {
                if (_Connections.ContainsKey(threadId)) _Connections.Remove(threadId); 
            }
        }
        
        internal HttpContext Get(int threadId)
        {
            lock (_Lock)
            {
                if (_Connections.ContainsKey(threadId)) return _Connections[threadId];
                return null;
            }
        }

        internal Dictionary<string, HttpRequest> GetActiveConnections()
        {
            Dictionary<string, HttpRequest> ret = new Dictionary<string, HttpRequest>();

            lock (_Lock)
            {
                foreach (KeyValuePair<int, HttpContext> curr in _Connections)
                {
                    string key = curr.Value.Request.SourceIp + ":" + curr.Value.Request.SourcePort;
                    if (!ret.ContainsKey(key)) ret.Add(key, curr.Value.Request);
                }
            }

            return ret;
        }
    }
}
