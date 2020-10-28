using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Statistics for all indices.
    /// </summary>
    public class IndicesStats
    {
        /// <summary>
        /// Indicates if the statistics operation was successful.
        /// </summary>
        [JsonProperty(Order = -1)]
        public bool Success = false;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        [JsonProperty(Order = 990)]
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// List of index statistics.
        /// </summary>
        [JsonProperty(Order = 991)]
        public List<IndexStats> Stats = new List<IndexStats>();
    }
}
