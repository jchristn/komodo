using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Maps a term to a given document within a given index.
    /// </summary>
    [Table("termdocs")]
    public class TermDoc
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        [JsonIgnore]
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [JsonProperty(Order = -4)]
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the index.
        /// </summary>
        [JsonProperty(Order = -3)]
        [Column("indexguid", false, DataTypes.Nvarchar, 64, false)]
        public string IndexGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the term.
        /// </summary>
        [JsonProperty(Order = -2)]
        [Column("termguid", false, DataTypes.Nvarchar, 64, false)]
        public string TermGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the source document.
        /// </summary>
        [JsonProperty(Order = -1)]
        [Column("sourcedocguid", false, DataTypes.Nvarchar, 64, false)]
        public string SourceDocumentGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the parsed document.
        /// </summary>
        [Column("parseddocguid", false, DataTypes.Nvarchar, 64, false)]
        public string ParsedDocumentGUID { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public TermDoc()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="termGuid">Globally-unique identifier of the term.</param>
        /// <param name="sourceDocGuid">Globally-unique identifier of the source document.</param>
        /// <param name="parsedDocGuid">Globally-unique identifier of the parsed document.</param>
        public TermDoc(string indexGuid, string termGuid, string sourceDocGuid, string parsedDocGuid)
        {
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(termGuid)) throw new ArgumentNullException(nameof(termGuid));
            if (String.IsNullOrEmpty(sourceDocGuid)) throw new ArgumentNullException(nameof(sourceDocGuid));
            if (String.IsNullOrEmpty(parsedDocGuid)) throw new ArgumentNullException(nameof(parsedDocGuid));

            GUID = Guid.NewGuid().ToString();
            IndexGUID = indexGuid;
            TermGUID = termGuid;
            SourceDocumentGUID = sourceDocGuid;
            ParsedDocumentGUID = parsedDocGuid;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="termGuid">Globally-unique identifier of the term.</param>
        /// <param name="sourceDocGuid">Globally-unique identifier of the source document.</param>
        /// <param name="parsedDocGuid">Globally-unique identifier of the parsed document.</param>
        public TermDoc(string guid, string indexGuid, string termGuid, string sourceDocGuid, string parsedDocGuid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(termGuid)) throw new ArgumentNullException(nameof(termGuid));
            if (String.IsNullOrEmpty(sourceDocGuid)) throw new ArgumentNullException(nameof(sourceDocGuid));
            if (String.IsNullOrEmpty(parsedDocGuid)) throw new ArgumentNullException(nameof(parsedDocGuid));

            GUID = guid;
            IndexGUID = indexGuid;
            TermGUID = termGuid;
            SourceDocumentGUID = sourceDocGuid;
            ParsedDocumentGUID = parsedDocGuid;
        } 
    }
}
