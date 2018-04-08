using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomodoCore
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
        /// Database settings for the index.
        /// </summary>
        public DatabaseSettings Database { get; set; }

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
             
            /// <summary>
            /// Retrieve default database settings.
            /// </summary>
            /// <returns>DatabaseSettings.</returns>
            public static DatabaseSettings Default(string indexName, string rootDirectory)
            {
                DatabaseSettings ret = new DatabaseSettings();
                ret.Type = DatabaseType.Sqlite;
                ret.Filename = rootDirectory + "/" + indexName + ".db";
                ret.Debug = false;
                return ret;
            }
        }

        /// <summary>
        /// Storage settings for the index.
        /// </summary>
        public class StorageSettings
        {
            /// <summary>
            /// Disk storage settings.
            /// </summary>
            public DiskSettings Disk { get; set; }

            /// <summary>
            /// Microsoft Azure storage settings.
            /// </summary>
            public AzureSettings Azure { get; set; }

            /// <summary>
            /// AWS S3 storage settings.
            /// </summary>
            public AwsSettings Aws { get; set; }

            /// <summary>
            /// Kvpbase storage settings.
            /// </summary>
            public KvpbaseSettings Kvpbase { get; set; }

            /// <summary>
            /// Retrieve default storage settings for source documents.
            /// </summary>
            /// <returns>StorageSettings.</returns>
            public static StorageSettings DefaultSource()
            {
                StorageSettings ret = new StorageSettings();
                ret.Disk = DiskSettings.DefaultSource();
                ret.Azure = null;
                ret.Aws = null;
                ret.Kvpbase = null;
                return ret;
            }

            /// <summary>
            /// Retrieve default storage settings for parsed documents.
            /// </summary>
            /// <returns>StorageSettings.</returns>
            public static StorageSettings DefaultParsed()
            {
                StorageSettings ret = new StorageSettings();
                ret.Disk = DiskSettings.DefaultParsed();
                ret.Azure = null;
                ret.Aws = null;
                ret.Kvpbase = null;
                return ret;
            }

            /// <summary>
            /// Settings when using local filesystem for storage.
            /// </summary>
            public class DiskSettings
            {
                /// <summary>
                /// The filesystem directory to use.
                /// </summary>
                public string Directory { get; set; }

                /// <summary>
                /// Retrieve default settings for source documents.
                /// </summary>
                /// <returns>DiskSettings.</returns>
                public static DiskSettings DefaultSource()
                {
                    DiskSettings ret = new DiskSettings();
                    ret.Directory = "SourceDocuments";
                    return ret;
                }

                /// <summary>
                /// Retrieve default settings for parsed documents.
                /// </summary>
                /// <returns>DiskSettings.</returns>
                public static DiskSettings DefaultParsed()
                {
                    DiskSettings ret = new DiskSettings();
                    ret.Directory = "ParsedDocuments";
                    return ret;
                }
            }

            /// <summary>
            /// Settings when using Microsoft Azure BLOB Storage for storage.
            /// </summary>
            public class AzureSettings
            {
                /// <summary>
                /// Microsoft Azure BLOB storage account name.
                /// </summary>
                public string AccountName { get; set; }

                /// <summary>
                /// Microsoft Azure BLOB storage access key.
                /// </summary>
                public string AccessKey { get; set; }

                /// <summary>
                /// Microsoft Azure BLOB storage endpoint.
                /// </summary>
                public string Endpoint { get; set; }

                /// <summary>
                /// Microsoft Azure BLOB storage container.
                /// </summary>
                public string Container { get; set; }

                /// <summary>
                /// Retrieve default settings.
                /// </summary>
                /// <returns>AzureSettings.</returns>
                public static AzureSettings Default()
                {
                    AzureSettings ret = new AzureSettings();
                    ret.AccountName = "";
                    ret.AccessKey = "";
                    ret.Endpoint = "";
                    ret.Container = "";
                    return ret;
                }
            }

            /// <summary>
            /// Settings when using AWS S3 for storage.
            /// </summary>
            public class AwsSettings
            {
                /// <summary>
                /// AWS S3 access key.
                /// </summary>
                public string AccessKey { get; set; }

                /// <summary>
                /// AWS S3 secret key.
                /// </summary>
                public string SecretKey { get; set; }

                /// <summary>
                /// AWS S3 region.
                /// </summary>
                public string Region { get; set; }

                /// <summary>
                /// AWS S3 bucket.
                /// </summary>
                public string Bucket { get; set; }

                /// <summary>
                /// Retrieve default configuration.
                /// </summary>
                /// <returns>AwsSettings.</returns>
                public static AwsSettings Default()
                {
                    AwsSettings ret = new AwsSettings();
                    ret.AccessKey = "";
                    ret.SecretKey = "";
                    ret.Region = "";
                    ret.Bucket = "";
                    return ret;
                }
            }

            /// <summary>
            /// Settings when using kvpbase for storage.
            /// </summary>
            public class KvpbaseSettings
            {
                /// <summary>
                /// Kvpbase endpoint URL.
                /// </summary>
                public string Endpoint { get; set; }

                /// <summary>
                /// Kvpbase user GUID.
                /// </summary>
                public string UserGuid { get; set; }

                /// <summary>
                /// Kvpbase container.
                /// </summary>
                public string Container { get; set; }

                /// <summary>
                /// Kvpbase API key.
                /// </summary>
                public string ApiKey { get; set; }

                /// <summary>
                /// Retrieve default settings.
                /// </summary>
                /// <returns></returns>
                public static KvpbaseSettings Default()
                {
                    KvpbaseSettings ret = new KvpbaseSettings();
                    ret.Endpoint = "http://localhost:8080/";
                    ret.UserGuid = "default";
                    ret.Container = "Blobs";
                    ret.ApiKey = "default";
                    return ret;
                }
            }
        }
         
        #endregion
    }
}
