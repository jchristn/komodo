using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlobHelper;
using DatabaseWrapper;
using Komodo.Core.Enums;

namespace Komodo.Core
{
    /// <summary>
    /// Index metadata.
    /// </summary>
    public class Index
    {
        #region Public-Members

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// The root directory of the index.
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Index options for the index.
        /// </summary>
        public IndexOptions Options { get; set; }

        /// <summary>
        /// Database settings for the documents database.
        /// </summary>
        public DatabaseSettings DocumentsDatabase { get; set; }

        /// <summary>
        /// Database settings for the postings database.
        /// </summary>
        public DatabaseSettings PostingsDatabase { get; set; }

        /// <summary>
        /// Storage settings for the index source documents.
        /// </summary>
        public StorageSettings StorageSource { get; set; }

        /// <summary>
        /// Storage settings for the index parsed documents.
        /// </summary>
        public StorageSettings StorageParsed { get; set; }
         
        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the Index.
        /// </summary>
        public Index()
        {

        }

        /// <summary>
        /// Instantiate the Index.
        /// </summary>
        /// <param name="filename">The file containing JSON from which the index should be instantiated.</param>
        /// <returns>Index.</returns>
        public static Index FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename)) throw new FileNotFoundException("File not found");
            string contents = Common.ReadTextFile(filename);
            Index ret = Common.DeserializeJson<Index>(contents);
            return ret;
        }
         
        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion

        #region Public-Subordinate-Classes

        /// <summary>
        /// Database settings for the index.  Do not use the same database across multiple indices!
        /// </summary>
        public class DatabaseSettings
        {
            /// <summary>
            /// The type of database.
            /// </summary>
            public DatabaseType Type { get; set; }

            /// <summary>
            /// Specify the filename when using the Sqlite database type.
            /// </summary>
            public string Filename { get; set; }

            /// <summary>
            /// Specify the hostname of the database server.
            /// </summary>
            public string Hostname { get; set; }

            /// <summary>
            /// Specify the port number for the database.
            /// </summary>
            public int Port { get; set; }

            /// <summary>
            /// Specify the name of the database.
            /// </summary>
            public string DatabaseName { get; set; }

            /// <summary>
            /// For Mssql SQL Express, specify the instance name.
            /// </summary>
            public string InstanceName { get; set; }

            /// <summary>
            /// The username to use when accessing the database.
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// The password to use when accessing the database.
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Enable or disable query logging.
            /// </summary>
            public bool Debug { get; set; } 
        }

        /// <summary>
        /// Storage settings for the index.
        /// </summary>
        public class StorageSettings
        {
            /// <summary>
            /// The type of external storage.
            /// </summary>
            public StorageType Type { get; set; }

            /// <summary>
            /// AWS S3 storage settings.
            /// </summary>
            public AwsSettings AwsS3 { get; set; }

            /// <summary>
            /// Microsoft Azure storage settings.
            /// </summary>
            public AzureSettings Azure { get; set; }

            /// <summary>
            /// Disk storage settings.
            /// </summary>
            public DiskSettings Disk { get; set; }

            /// <summary>
            /// Kvpbase storage settings.
            /// </summary>
            public KvpbaseSettings Kvpbase { get; set; } 
        }
         
        #endregion
    }
}
