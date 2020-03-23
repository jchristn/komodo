using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Komodo.Classes;

namespace Komodo.Crawler
{
    /// <summary>
    /// Database crawler result.
    /// </summary>
    public class SqlCrawlResult
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
        /// DataTable containing the crawled data.
        /// </summary>
        public DataTable DataTable = null;
        
        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public SqlCrawlResult()
        {

        }
    }
}
