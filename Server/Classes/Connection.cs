using System;
using SyslogLogging;
using WatsonWebserver;

namespace KomodoServer
{
    public class Connection
    {
        #region Public-Members

        public int ThreadId { get; set; }
        public string SourceIp { get; set; }
        public int SourcePort { get; set; }
        public HttpMethod Method { get; set; }
        public string RawUrl { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        #endregion

        #region Constructors-and-Factories

        public Connection()
        {

        }

        #endregion
    }
}
