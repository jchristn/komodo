using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Komodo.Classes;

namespace Komodo.Crawler
{
    /// <summary>
    /// Komodo crawler result.
    /// </summary>
    public class KomodoCrawlResult
    {
        /// <summary>
        /// Indicates if the crawler was successful.
        /// </summary>
        public bool Success = false;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// Object metadata.
        /// </summary>
        public ObjectMetadata Metadata = null;

        /// <summary>
        /// The filename to which the retrieved file was downloaded.
        /// </summary>
        public string Filename = null;
         
        /// <summary>
        /// Length of the file retrieved.
        /// </summary>
        public long ContentLength = 0;

        /// <summary>
        /// File stream of the retrieved data.
        /// </summary>
        [JsonIgnore]
        public Stream DataStream = null;

        /// <summary>
        /// Read the stream fully into a byte array.
        /// </summary>
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
        /// Instantiates the object.
        /// </summary>
        public KomodoCrawlResult()
        {

        }

        private byte[] _Data;
    }
}
