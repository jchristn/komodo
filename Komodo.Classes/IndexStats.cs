using System;
using System.Collections.Generic;
using System.Text; 

namespace Komodo.Classes
{
    /// <summary>
    /// Statistics for an index.
    /// </summary>
    public class IndexStats
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
        /// The name of the index.
        /// </summary>
        public string Name;

        /// <summary>
        /// The globally-unique identifier for the index.
        /// </summary>
        public string GUID;

        /// <summary>
        /// Source document statistics.
        /// </summary>
        public DocumentsStats SourceDocuments = new DocumentsStats();

        /// <summary>
        /// Parsed document statistics.
        /// </summary>
        public DocumentsStats ParsedDocuments = new DocumentsStats();

        /// <summary>
        /// The number of terms in the index.
        /// </summary>
        public long Terms = 0;

        /// <summary>
        /// The number of postings in the index.
        /// </summary>
        public long Postings = 0;

        /// <summary>
        /// Document statistics.
        /// </summary>
        public class DocumentsStats
        {
            /// <summary>
            /// The number of documents.
            /// </summary>
            public long Count = 0;

            /// <summary>
            /// The total size of the documents.
            /// </summary>
            public long Bytes = 0;
        }
    }
}
