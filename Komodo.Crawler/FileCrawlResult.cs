using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Komodo.Classes;

namespace Komodo.Crawler
{
    /// <summary>
    /// Filesystem crawler result.
    /// </summary>
    public class FileCrawlResult
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
        /// Length of the file retrieved.
        /// </summary>
        public long ContentLength = 0;

        /// <summary>
        /// File stream of the retrieved file.
        /// </summary>
        public FileStream FileStream = null;
        
        /// <summary>
        /// Read the FileStream fully into a byte array.
        /// </summary>
        public byte[] Data
        {
            get
            {
                if (_Data != null) return _Data;
                if (FileStream == null) return null;
                if (!FileStream.CanRead) throw new IOException("Cannot read from file stream.");
                _Data = Common.StreamToBytes(FileStream);
                return _Data;
            }
        }

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public FileCrawlResult()
        {

        }

        private byte[] _Data = null;
    }
}
