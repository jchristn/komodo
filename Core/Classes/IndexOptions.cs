using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomodoCore
{
    /// <summary>
    /// Configuration and options for an index.
    /// </summary>
    public class IndexOptions
    {
        #region Public-Members

        /// <summary>
        /// True to set text to lowercase.
        /// </summary>
        public bool NormalizeCase { get; set; }

        /// <summary>
        /// True to remove punctuation characters.
        /// </summary>
        public bool RemovePunctuation { get; set; }

        /// <summary>
        /// True to remove stopwords.
        /// </summary>
        public bool RemoveStopWords { get; set; }

        /// <summary>
        /// True to perform stemming on tokens.
        /// </summary>
        public bool PerformStemming { get; set; }

        /// <summary>
        /// Minimum length of a token.
        /// </summary>
        public int MinTokenLength { get; set; }

        /// <summary>
        /// Maximum length of a token.
        /// </summary>
        public int MaxTokenLength { get; set; }

        /// <summary>
        /// List of stop words.
        /// </summary>
        public List<string> StopWords { get; set; }

        /// <summary>
        /// Characters on which to split terms.
        /// </summary>
        public string[] SplitCharacters = new string[] { "]", "[", ",", ".", " ", "'", "\"", ";", "<", ">", ".", "/", "\\", "|", "{", "}", "(", ")" };

        /// <summary>
        /// Storage settings for the index.
        /// </summary>
        public StorageSettings Storage { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the IndexOptions
        /// </summary>
        public IndexOptions()
        {
            NormalizeCase = true;
            RemovePunctuation = true;
            RemoveStopWords = true;
            PerformStemming = true;
            MinTokenLength = 3;
            MaxTokenLength = 64;

            StopWords = SetDefaultStopWords();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a human readable string of the index options.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "---" + Environment.NewLine;
            ret += "Index Options" + Environment.NewLine;
            ret += "  Normalize Case     : " + NormalizeCase + Environment.NewLine;
            ret += "  Remove Punctuation : " + RemovePunctuation + Environment.NewLine;
            ret += "  Remove Stop Words  : " + RemoveStopWords + Environment.NewLine;
            ret += "  Perform Stemming   : " + PerformStemming + Environment.NewLine;
            ret += "  Min Token Length   : " + MinTokenLength + Environment.NewLine;
            ret += "  Max Token Length   : " + MaxTokenLength + Environment.NewLine;

            if (StopWords != null)
            {
                ret += "  Stop Words         : " + StopWords.Count + Environment.NewLine;
            }
            
            if (StopWords != null)
            {
                ret += "  Split Characters   : " + SplitCharacters.Length + Environment.NewLine;
            }
            ret += Environment.NewLine;
            return ret;
        }

        #endregion

        #region Private-Methods

        private List<string> SetDefaultStopWords()
        {
            string[] lines;

            if (File.Exists("StopWords.txt"))
            {    
                try
                {
                    lines = File.ReadAllLines("StopWords.txt");
                    return lines.ToList();
                }
                catch (Exception)
                {
                    return new List<string>();
                }
            }
            else if (Directory.Exists("Docs"))
            {
                if (File.Exists("Docs\\StopWords.txt"))
                {
                    try
                    {
                        lines = File.ReadAllLines("StopWords.txt");
                        return lines.ToList();
                    }
                    catch (Exception)
                    {
                        return new List<string>();
                    }
                }
            }
            else
            {
                return new List<string>();
            }

            return new List<string>();
        }

        #endregion

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion

        #region Public-Subordinate-Classes

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
            /// Retrieve default storage settings.
            /// </summary>
            /// <returns>StorageSettings.</returns>
            public static StorageSettings Default()
            {
                StorageSettings ret = new StorageSettings();
                ret.Disk = DiskSettings.Default();
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
                /// Retrieve default settings.
                /// </summary>
                /// <returns>DiskSettings.</returns>
                public static DiskSettings Default()
                {
                    DiskSettings ret = new DiskSettings();
                    ret.Directory = "SourceDocuments";
                    return ret;
                }
            }

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
