using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// A postings entry for a document, i.e. a term, its frequency, and its positions.
    /// </summary>
    public class Posting
    {
        #region Public-Members

        /// <summary>
        /// Term.
        /// </summary>
        [JsonProperty(Order = -1)]
        public string Term { get; set; } = null;

        /// <summary>
        /// The frequency with which the term was found.
        /// </summary>
        public long Frequency { get; set; } = 0;

        /// <summary>
        /// The character positions where the term was found.
        /// </summary>
        [JsonProperty(Order = 990)]
        public List<long> Positions { get; set; } = new List<long>();
         
        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the Posting object.
        /// </summary>
        public Posting()
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

        #region Private-Methods

        #endregion 
    }
}
