using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;
using Komodo.Core.Enums;

namespace Komodo.Core
{
    /// <summary>
    /// A document matching a search query.
    /// </summary>
    public class MatchingDocument
    {
        #region Public-Members

        /// <summary>
        /// Document ID.
        /// </summary>
        public string DocumentId = null;

        /// <summary>
        /// The type of document.
        /// </summary>
        public DocType DocumentType = DocType.Unknown;

        /// <summary>
        /// The score of the document, between 0 and 1.  Only relevant when optional filters are supplied in the search.
        /// </summary>
        public decimal Score = 1m;
         
        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public MatchingDocument()
        { 
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
