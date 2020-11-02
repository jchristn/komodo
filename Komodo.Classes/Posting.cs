using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Komodo.Classes
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
        public string Term { get; set; }
         
        /// <summary>
        /// The frequency with which the term was found.
        /// </summary>
        public long Frequency { get; set; }

        /// <summary>
        /// The character positions where the term was found.
        /// </summary>
        [JsonProperty(Order = 990)]
        public List<long> Positions { get; set; }
         
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
        /// Returns a human-readable string version of the object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "[" + Term + " frequency " + Frequency + "]: ";
            if (Positions != null)
            {
                int added = 0;
                foreach (long curr in Positions)
                {
                    if (added == 0) ret += curr;
                    else ret += "," + curr;
                    added++;
                }
            }
            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
