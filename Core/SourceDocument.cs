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
    /// Metadata about documents added to an index.
    /// </summary>
    public class SourceDocument
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
        /// The master document ID, assigned through indexing.
        /// </summary>
        public string DocumentId { get; set; }

        /// <summary>
        /// The name of the document (may not be unique).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Document tags.
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary>
        public DocType DocumentType { get; set; }

        /// <summary>
        /// The source URL of the document.
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// The title of the document.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Content type of the document.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The content length of the source document.
        /// </summary>
        public long? ContentLength { get; set; }

        /// <summary>
        /// The MD5 hash of the document's data.
        /// </summary>
        public string Md5 { get; set; }

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
        public SourceDocument()
        {

        }

        /// <summary>
        /// Instantiate the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>SourceDocument.</returns>
        public static SourceDocument FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            SourceDocument ret = new SourceDocument();

            if (row["Id"] != DBNull.Value) ret.Id = Convert.ToInt32(row["Id"]);
            if (row["IndexName"] != DBNull.Value) ret.IndexName = row["IndexName"].ToString();
            if (row["DocumentId"] != DBNull.Value) ret.DocumentId = row["DocumentId"].ToString();
            if (row["Name"] != DBNull.Value) ret.Name = row["Name"].ToString();
            if (row["Tags"] != DBNull.Value) ret.Tags = row["Tags"].ToString();

            DocType dt = DocType.Unknown;
            if (row["DocumentType"] != DBNull.Value) Enum.TryParse<DocType>(row["DocumentType"].ToString(), out dt);
            ret.DocumentType = dt;

            if (row["SourceUrl"] != DBNull.Value) ret.SourceUrl = row["SourceUrl"].ToString();
            if (row["Title"] != DBNull.Value) ret.Title = row["Title"].ToString();
            if (row["ContentType"] != DBNull.Value) ret.ContentType = row["ContentType"].ToString();
            if (row["Md5"] != DBNull.Value) ret.Md5 = row["Md5"].ToString();
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
            ret.Add("Name", Name);
            ret.Add("Tags", Tags);
            ret.Add("DocumentType", DocumentType.ToString());
            ret.Add("SourceUrl", SourceUrl);
            ret.Add("Title", Title);
            ret.Add("ContentType", ContentType);
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
