using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;

namespace Komodo.Core
{ 
    /// <summary>
    /// Provides details about how many documents matched the supplied search.
    /// </summary>
    public class MatchCounts
    {
        #region Public-Members

        /// <summary>
        /// The number of documents that matched the specified terms.
        /// </summary>
        public int TermsMatch { get; set; }

        /// <summary>
        /// The number of documents that matchd the specified terms and the specified filters.
        /// </summary>
        public int FilterMatch { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories
        
        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public MatchCounts()
        {
            TermsMatch = 0;
            FilterMatch = 0;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
