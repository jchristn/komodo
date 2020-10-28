using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

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
        [JsonProperty(Order = -3)]
        public bool Success = false;

        /// <summary>
        /// The name of the index.
        /// </summary>
        [JsonProperty(Order = -2)]
        public string Name;

        /// <summary>
        /// The globally-unique identifier for the index.
        /// </summary>
        [JsonProperty(Order = -1)]
        public string GUID;

        /// <summary>
        /// The number of terms in the index.
        /// </summary>
        public long Terms = 0;

        /// <summary>
        /// The number of postings in the index.
        /// </summary>
        public long Postings = 0;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        [JsonProperty(Order = 990)]
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// Source document statistics.
        /// </summary>
        [JsonProperty(Order = 991)]
        public DocumentsStats SourceDocuments = new DocumentsStats();

        /// <summary>
        /// Parsed document statistics.
        /// </summary>
        [JsonProperty(Order = 992)]
        public DocumentsStats ParsedDocuments = new DocumentsStats();

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
