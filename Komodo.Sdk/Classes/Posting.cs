using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Sdk.Classes
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
        public string Term { get; set; }

        /// <summary>
        /// The frequency with which the term was found.
        /// </summary>
        public long Frequency { get; set; }

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
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return KomodoCommon.SerializeJson(this, pretty);
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
