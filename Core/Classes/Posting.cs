using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomodoCore
{
    /// <summary>
    /// Inverted index entry.
    /// </summary>
    public class Posting
    {
        #region Public-Members

        /// <summary>
        /// Term.
        /// </summary>
        public object Term { get; set; }

        /// <summary>
        /// Document ID within which the term was found.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// The frequency with which the term was found.
        /// </summary>
        public int Frequency { get; set; }

        /// <summary>
        /// The character positions where the term was found.
        /// </summary>
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
            string ret = "";
            ret += Term.ToString() + " [" + DocumentId + ", " + Frequency + " freq]: ";
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

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
