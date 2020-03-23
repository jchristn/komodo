using System;
using System.Collections.Generic;
using System.Text; 

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
        public bool Success = false;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// List of index statistics.
        /// </summary>
        public List<IndexStats> Stats = new List<IndexStats>();
    }
}
