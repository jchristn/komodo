using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Statistics for all indices.
    /// </summary>
    public class IndicesStats
    {
        #region Public-Members

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

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public IndicesStats()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        #endregion
    }
}
