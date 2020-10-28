using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Komodo.Classes;
using Newtonsoft.Json;

namespace Komodo.Crawler
{
    /// <summary>
    /// Database crawler result.
    /// </summary>
    public class SqliteCrawlResult
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
        /// DataTable containing the crawled data.
        /// </summary>
        public DataTable DataTable = null;
        
        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public SqliteCrawlResult()
        {

        }
    }
}
