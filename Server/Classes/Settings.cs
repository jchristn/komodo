﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komodo.Core;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Server configuration.
    /// </summary>
    public class Settings
    {
        #region Public-Members
          
        /// <summary>
        /// Enable or disable the console.
        /// </summary>
        public bool EnableConsole { get; set; }

        /// <summary>
        /// Server settings.
        /// </summary>
        public ServerSettings Server { get; set; }

        /// <summary>
        /// Files settings.
        /// </summary>
        public FilesSettings Files { get; set; } 

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LoggingSettings Logging { get; set; }

        /// <summary>
        /// Debug settings.
        /// </summary>
        public DebugSettings Debug { get; set; }

        /// <summary>
        /// REST settings.
        /// </summary>
        public RestSettings Rest { get; set; }
         
        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Settings()
        {

        }

        /// <summary>
        /// Load the configuration from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Configuration object.</returns>
        public static Settings FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename)) throw new FileNotFoundException("Unable to find " + filename);
            string contents = File.ReadAllText(filename);
            Settings ret = Common.DeserializeJson<Settings>(contents);
            return ret;
        }

        #endregion

        #region Public-Embedded-Classes

        /// <summary>
        /// Server settings.
        /// </summary>
        public class ServerSettings
        {
            /// <summary>
            /// Hostname on which to listen for incoming web requests.
            /// </summary>
            public string ListenerHostname { get; set; }

            /// <summary>
            /// TCP port on which to listen.
            /// </summary>
            public int ListenerPort { get; set; }

            /// <summary>
            /// Enable or disable SSL.
            /// </summary>
            public bool Ssl { get; set; }
             
            /// <summary>
            /// Custom header used to specify API key for authentication within the request.
            /// </summary>
            public string HeaderApiKey { get; set; }

            /// <summary>
            /// Custom header used to specify the email address for authentication within the request.
            /// </summary>
            public string HeaderEmail { get; set; }

            /// <summary>
            /// Custom header used to specify the password for authentication within the request.
            /// </summary>
            public string HeaderPassword { get; set; }
            
            /// <summary>
            /// API key to use for admin operations (please keep secret).
            /// </summary>
            public string AdminApiKey { get; set; }
        }

        /// <summary>
        /// Files settings.
        /// </summary>
        public class FilesSettings
        {
            /// <summary>
            /// Path and filename to the user master JSON file.
            /// </summary>
            public string UserMaster { get; set; }

            /// <summary>
            /// Path and filename to the API key file.
            /// </summary>
            public string ApiKey { get; set; }

            /// <summary>
            /// Path and filename to the API key permission file.
            /// </summary>
            public string ApiKeyPermission { get; set; }

            /// <summary>
            /// Path and filename to the indices file.
            /// </summary>
            public string Indices { get; set; }

            /// <summary>
            /// Path to directory where temp files are stored.
            /// </summary>
            public string TempFiles { get; set; }
        }
         
        /// <summary>
        /// Logging settings.
        /// </summary>
        public class LoggingSettings
        {
            /// <summary>
            /// IP address of the syslog server.
            /// </summary>
            public string SyslogServerIp;

            /// <summary>
            /// Syslog server port.
            /// </summary>
            public int SyslogServerPort;

            /// <summary>
            /// Header to include in each syslog message.
            /// </summary>
            public string Header;

            /// <summary>
            /// Minimum level required before sending a syslog message.
            /// </summary>
            public int MinimumLevel;
             
            /// <summary>
            /// Enable console logging.
            /// </summary>
            public bool ConsoleLogging;
        }

        /// <summary>
        /// Debug settings.
        /// </summary>
        public class DebugSettings
        {
            /// <summary>
            /// Enable database debugging.
            /// </summary>
            public bool Database { get; set; }
        }

        /// <summary>
        /// REST settings.
        /// </summary>
        public class RestSettings
        {
            /// <summary>
            /// Enable use of a web proxy.
            /// </summary>
            public bool UseWebProxy;

            /// <summary>
            /// URL to the web proxy.
            /// </summary>
            public string WebProxyUrl;

            /// <summary>
            /// Enable support and acceptance of invalid or unverifiable SSL certificates.
            /// </summary>
            public bool AcceptInvalidCerts;
        }
         
        #endregion
          
        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
