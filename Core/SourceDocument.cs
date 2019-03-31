using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string MasterDocId { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary>
        public DocType DocType { get; set; }

        /// <summary>
        /// The source URL of the document.
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// The content length of the source document.
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
            if (row["MasterDocId"] != DBNull.Value) ret.MasterDocId = row["MasterDocId"].ToString();

            DocType dt = DocType.Unknown;
            if (row["DocType"] != DBNull.Value) Enum.TryParse<DocType>(row["DocType"].ToString(), out dt);
            ret.DocType = dt;

            if (row["SourceUrl"] != DBNull.Value) ret.SourceUrl = row["SourceUrl"].ToString();
            if (row["ContentLength"] != DBNull.Value) ret.ContentLength = Convert.ToInt64(row["ContentLength"]);
            if (row["Created"] != DBNull.Value) ret.Created = Convert.ToDateTime(row["Created"].ToString());
            if (row["Indexed"] != DBNull.Value) ret.Indexed = Convert.ToDateTime(row["Indexed"].ToString());

            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
