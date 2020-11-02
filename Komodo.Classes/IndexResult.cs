using System;
using System.Collections.Generic;
using System.Text;
using Komodo.Classes;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Result from an indexing operation.
    /// </summary>
    public class IndexResult
    {
        /// <summary>
        /// Indicates if the indexing operation was successful.
        /// </summary>
        [JsonProperty(Order = -2)]
        public bool Success = false;

        /// <summary>
        /// The source document GUID.
        /// </summary>
        [JsonProperty(Order = -1)]
        public string GUID = null;
        
        /// <summary>
        /// The document type.
        /// </summary>
        public DocType Type = DocType.Unknown;

        /// <summary>
        /// Timestamps for the index operation.
        /// </summary>
        [JsonProperty(Order = 990)]
        public IndexResultTimestamps Time = new IndexResultTimestamps();

        /// <summary>
        /// Source document.
        /// </summary>
        [JsonProperty(Order = 991)]
        public SourceDocument Source = null;

        /// <summary>
        /// Parsed document, if any.
        /// </summary>
        [JsonProperty(Order = 992)]
        public ParsedDocument Parsed = null;

        /// <summary>
        /// The result of parsing, if enabled.
        /// </summary>
        [JsonProperty(Order = 993)]
        public ParseResult ParseResult = null;

        /// <summary>
        /// The result of generating postings, if enabled.
        /// </summary>
        [JsonProperty(Order = 994)]
        public PostingsResult Postings = null;

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public IndexResult()
        {

        }

        /// <summary>
        /// Timestamps for the index operation.
        /// </summary>
        public class IndexResultTimestamps
        {
            /// <summary>
            /// Start and end timestamps for the overall indexing operation.
            /// </summary>
            [JsonProperty(Order = -3)]
            public Timestamps Overall = new Timestamps();

            /// <summary>
            /// Start and end timestamps for persisting the source document to storage.
            /// </summary>
            [JsonProperty(Order = 990)]
            public Timestamps PersistSourceDocument = new Timestamps();

            /// <summary>
            /// Start and end timestamps for persisting the parsed document to storage.
            /// </summary>
            [JsonProperty(Order = 991)]
            public Timestamps PersistParsedDocument = new Timestamps();

            /// <summary>
            /// Start and end timestamps for persisting the postings object to storage.
            /// </summary>
            [JsonProperty(Order = 992)]
            public Timestamps PersistPostingsDocument = new Timestamps();

            /// <summary>
            /// Start and end timestamps for parsing the source document.
            /// </summary>
            [JsonProperty(Order = -2)]
            public Timestamps Parse = new Timestamps();

            /// <summary>
            /// Start and end timestamps for generating postings from the parsed document.
            /// </summary>
            [JsonProperty(Order = -1)]
            public Timestamps Postings = new Timestamps();

            /// <summary>
            /// Start and end timestamps for processing document terms and storing them in the database.
            /// </summary>
            public Timestamps Terms = new Timestamps(); 
        }
    }
}
