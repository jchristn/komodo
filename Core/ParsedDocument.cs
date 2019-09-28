using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komodo.Core.Enums;

namespace Komodo.Core
{
    /// <summary>
    /// Metadata about documents parsed and processed by an index.
    /// </summary>
    public class ParsedDocument
    {
        #region Public-Members
         
        /// <summary>
        /// ID value for the database.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string IndexName { get; set; }

        /// <summary>
        /// Document ID, assigned through indexing.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary>
        public DocType DocumentType { get; set; }

        /// <summary>
        /// The content length of the source document.
        /// </summary>
        public long? SourceContentLength { get; set; }

        /// <summary>
        /// The content length of the parsed document.
        /// </summary>
        public long? ContentLength { get; set; }

        /// <summary>
        /// The time at which the record was created.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// The time at which the document was indexed.
        /// </summary>
        public DateTime? Indexed { get; set; }
         
        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ParsedDocument()
        {

        }

        /// <summary>
        /// Instantiate the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>ParsedDocument.</returns>
        public static ParsedDocument FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
             
            ParsedDocument ret = new ParsedDocument();
            if (row["Id"] != DBNull.Value) ret.Id = Convert.ToInt32(row["Id"]);
            if (row["IndexName"] != DBNull.Value) ret.IndexName = row["IndexName"].ToString();
            if (row["DocumentId"] != DBNull.Value) ret.DocumentId = row["DocumentId"].ToString();

            DocType dt = DocType.Unknown;
            if (row["DocumentType"] != DBNull.Value) Enum.TryParse<DocType>(row["DocumentType"].ToString(), out dt);
            ret.DocumentType = dt;

            if (row["SourceContentLength"] != DBNull.Value) ret.SourceContentLength = Convert.ToInt64(row["SourceContentLength"]);
            if (row["ContentLength"] != DBNull.Value) ret.ContentLength = Convert.ToInt64(row["ContentLength"]);
            if (row["Created"] != DBNull.Value) ret.Created = Convert.ToDateTime(row["Created"].ToString());
            if (row["Indexed"] != DBNull.Value) ret.Indexed = Convert.ToDateTime(row["Indexed"].ToString());

            return ret;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a dictionary from the object.
        /// </summary>
        /// <returns>Dictionary.</returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("IndexName", IndexName);
            ret.Add("DocumentId", DocumentId);
            ret.Add("DocumentType", DocumentType.ToString());
            ret.Add("SourceContentLength", SourceContentLength);
            ret.Add("ContentLength", ContentLength); 
            ret.Add("Created", Created);
            ret.Add("Indexed", Indexed);
            return ret; 
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
