using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komodo.Core
{
    /// <summary>
    /// Statistics object for a given index.
    /// </summary>
    public class IndexStats
    {
        #region Public-Members

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Statistics for source documents.
        /// </summary>
        public SourceDocumentStats SourceDocuments { get; set; }

        /// <summary>
        /// Statistics for parsed documents.
        /// </summary>
        public ParsedDocumentStats ParsedDocuments { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public IndexStats()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion

        #region Subordinate-Classes

        /// <summary>
        /// Statistics for source documents.
        /// </summary>
        public class SourceDocumentStats
        {
            /// <summary>
            /// The number of source documents.
            /// </summary>
            public long Count { get; set; }

            /// <summary>
            /// The total size of all source documents in bytes.
            /// </summary>
            public long SizeBytes { get; set; }

            /// <summary>
            /// Instantiates the object.
            /// </summary>
            public SourceDocumentStats()
            {

            }
        }

        /// <summary>
        /// Statistics for parsed documents.
        /// </summary>
        public class ParsedDocumentStats
        {
            /// <summary>
            /// The number of parsed documents.
            /// </summary>
            public long Count { get; set; }

            /// <summary>
            /// The total size of all parsed documents in bytes.
            /// </summary>
            public long SizeBytesParsed { get; set; }

            /// <summary>
            /// The total size of all source documents associated with parsed documents in bytes.
            /// </summary>
            public long SizeBytesSource { get; set; }

            /// <summary>
            /// Instantiates the object.
            /// </summary>
            public ParsedDocumentStats()
            {

            }
        }

        #endregion
    }
}
