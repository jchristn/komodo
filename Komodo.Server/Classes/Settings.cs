﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Komodo.Classes;

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
        /// Database settings.
        /// </summary>
        public DbSettings Database { get; set; }

        /// <summary>
        /// Temporary storage settings.
        /// </summary>
        public StorageSettings TempStorage { get; set; }

        /// <summary>
        /// Source document storage settings.
        /// </summary>
        public StorageSettings SourceDocuments { get; set; }

        /// <summary>
        /// Parsed document storage settings.
        /// </summary>
        public StorageSettings ParsedDocuments { get; set; }

        /// <summary>
        /// Postings document storage settings.
        /// </summary>
        public StorageSettings Postings { get; set; }

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LoggingSettings Logging { get; set; }
         
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
            /// API key to use for admin operations (please keep secret).
            /// </summary>
            public string AdminApiKey { get; set; }
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
          
        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
