using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using BlobHelper;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Crawl result.
    /// </summary>
    public class CrawlResult
    {        
        /// <summary>
        /// Indicates if the crawler was successful.
        /// </summary>
        [JsonProperty(Order = -2)]
        public bool Success = false;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        [JsonProperty(Order = -1)]
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// The filename to which the retrieved file was downloaded.
        /// </summary>
        public string Filename = null;

        /// <summary>
        /// Length of the file retrieved.
        /// </summary>
        public long ContentLength = 0;

        /// <summary>
        /// HTTP crawl result.
        /// </summary>
        [JsonProperty(Order = 990)]
        public HttpCrawlResult Http = null;

        /// <summary>
        /// Object metadata.
        /// </summary>
        [JsonProperty(Order = 994)]
        public ObjectMetadata Metadata = null;

        /// <summary>
        /// DataTable containing the crawled data.
        /// </summary>
        [JsonProperty(Order = 995)]
        public DataTable DataTable = null;

        /// <summary>
        /// File stream of the retrieved data.
        /// </summary>
        [JsonIgnore]
        public Stream DataStream = null;

        /// <summary>
        /// Read the stream fully into a byte array.
        /// </summary>
        [JsonProperty(Order = 996)]
        public byte[] Data
        {
            get
            {
                if (_Data != null) return _Data;
                if (DataStream == null) return null;
                if (!DataStream.CanRead) throw new IOException("Cannot read from file stream.");
                _Data = Common.StreamToBytes(DataStream);
                return _Data;
            }
        }

        /// <summary>
        /// Exception encountered during crawl.
        /// </summary>
        [JsonProperty(Order = 997)]
        public Exception Exception = null;

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public CrawlResult()
        {

        }

        private byte[] _Data;

        /// <summary>
        /// Object metadata.
        /// </summary>
        public class ObjectMetadata
        {
            /// <summary>
            /// Object key.
            /// </summary>
            [JsonProperty(Order = -1)]
            public string Key = null;

            /// <summary>
            /// Content type.
            /// </summary>
            public string ContentType = null;

            /// <summary>
            /// Content length.
            /// </summary>
            public long ContentLength = 0;

            /// <summary>
            /// Etag.
            /// </summary>
            public string ETag = null;

            /// <summary>
            /// Creation timestamp.
            /// </summary>
            [JsonProperty(Order = 990)]
            public DateTime? CreatedUtc = null;

            /// <summary>
            /// Last access timestamp.
            /// </summary>
            [JsonProperty(Order = 991)]
            public DateTime? LastAccessUtc = null;

            /// <summary>
            /// Last update timestamp.
            /// </summary>
            [JsonProperty(Order = 992)]
            public DateTime? LastUpdateUtc = null;

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public ObjectMetadata()
            {

            }

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            /// <param name="md">BLOB metadata.</param>
            /// <returns>Object metadata.</returns>
            public static ObjectMetadata FromBlobMetadata(BlobMetadata md)
            {
                if (md == null) throw new ArgumentNullException(nameof(md));

                ObjectMetadata ret = new ObjectMetadata();
                ret.Key = md.Key;
                ret.ContentType = md.ContentType;
                ret.ContentLength = md.ContentLength;
                ret.ETag = md.ETag;
                ret.CreatedUtc = md.CreatedUtc;
                ret.LastAccessUtc = md.LastAccessUtc;
                ret.LastUpdateUtc = md.LastUpdateUtc;
                return ret;
            }

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            /// <param name="fi">FileInfo.</param>
            /// <returns>Object metadata.</returns>
            public static ObjectMetadata FromFileInfo(FileInfo fi)
            {
                if (fi == null) throw new ArgumentNullException(nameof(fi));

                ObjectMetadata ret = new ObjectMetadata();
                ret.Key = fi.Name;
                ret.ContentLength = fi.Length;
                ret.ETag = Common.Md5File(fi.FullName);
                ret.CreatedUtc = fi.CreationTimeUtc;
                ret.LastAccessUtc = fi.LastAccessTimeUtc;
                ret.LastUpdateUtc = fi.LastWriteTimeUtc;
                return ret;
            }
        }

        /// <summary>
        /// HTTP crawl result.
        /// </summary>
        public class HttpCrawlResult
        {
            /// <summary>
            /// The HTTP status code.
            /// </summary>
            [JsonProperty(Order = -1)]
            public int StatusCode = 0;
             
            /// <summary>
            /// The HTTP headers in the response.
            /// </summary>
            [JsonProperty(Order = 990)]
            public Dictionary<string, string> Headers = new Dictionary<string, string>();

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public HttpCrawlResult()
            {

            }
        }
    }
}
