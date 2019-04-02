using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseWrapper;

namespace Komodo.Core
{
    /// <summary>
    /// A document matching a search query.
    /// </summary>
    public class Document
    {
        #region Public-Members

        /// <summary>
        /// Master document ID.
        /// </summary>
        public string MasterDocId { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary>
        public DocType? DocumentType { get; set; }

        /// <summary>
        /// The score of the document, between 0 and 1.  Only relevant when optional filters are supplied in the search.
        /// </summary>
        public decimal? Score { get; set; }

        /// <summary>
        /// Error description strings, if any.
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// The document's data, if requested in the search query.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// The parsed document, if requested in the search query.
        /// </summary>
        public object Parsed { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public Document()
        {
            MasterDocId = null;
            DocumentType = DocType.Unknown;
            Score = 0m;
            Errors = new List<string>();
            Data = null;
            Parsed = null;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
