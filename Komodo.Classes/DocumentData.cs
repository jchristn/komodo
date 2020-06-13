using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Komodo.Classes
{
    /// <summary>
    /// Data from a document stored in Komodo.
    /// </summary>
    public class DocumentData
    {
        /// <summary>
        /// The content type of the document.
        /// </summary>
        public string ContentType = null;

        /// <summary>
        /// The content length of the source document.
        /// </summary>
        public long ContentLength = 0;

        /// <summary>
        /// The stream containing the source document's data.  Important: accessing the 'Data' parameter will fully read 'DataStream'.
        /// </summary>
        [JsonIgnore]
        public Stream DataStream = null;

        /// <summary>
        /// Byte array containing the data from the source DataStream.  Important: accessing the 'Data' will fully read 'DataStream'.
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (_Data != null) return _Data;
                if (ContentLength <= 0) return null;
                if (DataStream == null) return null;
                _Data = Common.StreamToBytes(DataStream);
                return _Data;
            }
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="contentType">Content type.</param>
        /// <param name="contentLength">Content length.</param>
        /// <param name="stream">Stream containing the data.</param>
        public DocumentData(string contentType, long contentLength, Stream stream)
        {
            ContentType = contentType;
            ContentLength = contentLength;
            DataStream = stream;
        }

        private byte[] _Data;
    }
}
