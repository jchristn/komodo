using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KomodoCore;

namespace KomodoServer
{
    public class Config
    {
        #region Public-Members

        public string ProductVersion { get; set; }
        public string Environment { get; set; }
        public string EnvironmentSeparator { get; set; }
        public string DocumentationUrl { get; set; }
        public int EnableConsole { get; set; }

        public ServerSettings Server { get; set; }
        public FilesSettings Files { get; set; } 
        public LoggingSettings Logging { get; set; }
        public DebugSettings Debug { get; set; }
        public RestSettings Rest { get; set; }
        public IndexerSettings Indexer { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        public Config()
        {

        }

        public static Config FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename)) throw new FileNotFoundException("Unable to find " + filename);
            string contents = File.ReadAllText(filename);
            Config ret = Common.DeserializeJson<Config>(contents);
            return ret;
        }

        #endregion

        #region Public-Embedded-Classes

        public class ServerSettings
        {
            public string ListenerHostname { get; set; }
            public int ListenerPort { get; set; }
            public int Ssl { get; set; }

            public string HeaderApiKey { get; set; }
            public string HeaderEmail { get; set; }
            public string HeaderPassword { get; set; }
            public string HeaderVersion { get; set; }

            public string AdminApiKey { get; set; }
        }

        public class FilesSettings
        {
            public string UserMaster { get; set; }
            public string ApiKey { get; set; }
            public string ApiKeyPermission { get; set; }
            public string Indices { get; set; }
        }
         
        public class LoggingSettings
        {
            public string SyslogServerIp;
            public int SyslogServerPort;
            public string Header;
            public int MinimumLevel;
            public int LogHttpRequests;
            public int LogHttpResponses;
            public int ConsoleLogging;
        }

        public class DebugSettings
        {
            public int Database { get; set; }
        }

        public class RestSettings
        {
            public int UseWebProxy;
            public string WebProxyUrl;
            public int AcceptInvalidCerts;
        }

        public class IndexerSettings
        {
            public int IndexerIntervalMs { get; set; }
        }

        #endregion

        #region Private-Embedded-Classes

        #endregion 
    }
}
