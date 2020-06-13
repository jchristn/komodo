using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Watson.ORM;
using Watson.ORM.Core;
using Komodo.Classes;
using Common = Komodo.Classes.Common;
using DiskSettings = BlobHelper.DiskSettings;

namespace Komodo.Daemon
{
    /// <summary>
    /// Komodo daemon settings.
    /// </summary>
    public class DaemonSettings
    {
        #region Public-Members
         
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
        public DaemonSettings()
        {

        }

        /// <summary>
        /// Load the configuration from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Configuration object.</returns>
        public static DaemonSettings FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename)) throw new FileNotFoundException("Unable to find " + filename);
            string contents = File.ReadAllText(filename);
            DaemonSettings ret = Common.DeserializeJson<DaemonSettings>(contents);
            return ret;
        }

        /// <summary>
        /// Default settings.
        /// </summary>
        /// <returns>Settings.</returns>
        public static DaemonSettings Default()
        {
            DaemonSettings ret = new DaemonSettings();

            ret.Database = new DbSettings("./Data/Komodo.db");
            ret.TempStorage = new StorageSettings(new DiskSettings("./Data/Temp/"));
            ret.SourceDocuments = new StorageSettings(new DiskSettings("./Data/Source/"));
            ret.ParsedDocuments = new StorageSettings(new DiskSettings("./Data/Parsed/"));
            ret.Postings = new StorageSettings(new DiskSettings("./Data/Postings/"));

            return ret;
        }

        #endregion

        #region Public-Embedded-Classes
         
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
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Prepare files and directories for use.
        /// </summary>
        public void PrepareFilesAndDirectories()
        {
            string dir = null;

            if (Database == null)
            {
                throw new ArgumentException("Settings.Database must be populated.");
            }
            else
            {
                if (!String.IsNullOrEmpty(Database.Filename))
                {
                    dir = Path.GetDirectoryName(Path.GetFullPath(Database.Filename));
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    if (!File.Exists(Path.GetFullPath(Database.Filename))) File.Create(Database.Filename).Dispose();
                }
            }

            if (TempStorage == null)
            {
                throw new ArgumentException("Settings.TempStorage must be populated.");
            }
            else
            {
                if (TempStorage.Disk != null)
                {
                    if (!String.IsNullOrEmpty(TempStorage.Disk.Directory))
                    {
                        dir = Path.GetDirectoryName(Path.GetFullPath(TempStorage.Disk.Directory));
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir); 
                    }
                }
            }

            if (SourceDocuments == null)
            {
                throw new ArgumentException("Settings.SourceDocuments must be populated.");
            }
            else
            {
                if (SourceDocuments.Disk != null)
                {
                    if (!String.IsNullOrEmpty(SourceDocuments.Disk.Directory))
                    {
                        dir = Path.GetDirectoryName(Path.GetFullPath(SourceDocuments.Disk.Directory));
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    }
                }
            }

            if (ParsedDocuments == null)
            {
                throw new ArgumentException("Settings.ParsedDocuments must be populated.");
            }
            else
            {
                if (ParsedDocuments.Disk != null)
                {
                    if (!String.IsNullOrEmpty(ParsedDocuments.Disk.Directory))
                    {
                        dir = Path.GetDirectoryName(Path.GetFullPath(ParsedDocuments.Disk.Directory));
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    }
                }
            }

            if (Postings == null)
            {
                throw new ArgumentException("Settings.Postings must be populated.");
            }
            else
            {
                if (Postings.Disk != null)
                {
                    if (!String.IsNullOrEmpty(Postings.Disk.Directory))
                    {
                        dir = Path.GetDirectoryName(Path.GetFullPath(Postings.Disk.Directory));
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    }
                }
            }
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
