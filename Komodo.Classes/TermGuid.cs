using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Associates a term with a specific GUID.
    /// </summary>
    [Table("termguids")]
    public class TermGuid
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
        [JsonProperty(Order = -2)]
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the index.
        /// </summary>
        [JsonProperty(Order = -1)]
        [Column("indexguid", false, DataTypes.Nvarchar, 64, false)]
        public string IndexGUID { get; set; }

        /// <summary>
        /// The term.
        /// </summary>
        [Column("term", false, DataTypes.Nvarchar, 64, false)]
        public string Term { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public TermGuid()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="term">The term.</param>
        public TermGuid(string indexGuid, string term)
        {
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(term)) throw new ArgumentNullException(nameof(term));

            GUID = Guid.NewGuid().ToString();
            IndexGUID = indexGuid;
            Term = term;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="term">The term.</param>
        public TermGuid(string guid, string indexGuid, string term)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(term)) throw new ArgumentNullException(nameof(term));

            GUID = guid;
            IndexGUID = indexGuid;
            Term = term;
        } 
    }
}
