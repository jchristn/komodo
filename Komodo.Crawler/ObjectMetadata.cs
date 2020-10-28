using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BlobHelper;
using Komodo.Classes;
using Newtonsoft.Json;

namespace Komodo.Crawler
{
    /// <summary>
    /// Metadata for a crawled object.
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
}
