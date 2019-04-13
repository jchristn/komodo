using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komodo.Core
{
    /// <summary>
    /// Database record pertaining to a term and document.
    /// </summary>
    public class TermRecord
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
        /// Term.
        /// </summary>
        public string Term { get; set; }
         
        /// <summary>
        /// The time at which the record was created.
        /// </summary>
        public DateTime? Created { get; set; }
         
        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public TermRecord()
        {

        }

        /// <summary>
        /// Instantiate the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>TermRecord.</returns>
        public static TermRecord FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
             
            TermRecord ret = new TermRecord();
            if (row["Id"] != DBNull.Value) ret.Id = Convert.ToInt32(row["Id"]);
            if (row["IndexName"] != DBNull.Value) ret.IndexName = row["IndexName"].ToString();
            if (row["MasterDocId"] != DBNull.Value) ret.MasterDocId = row["MasterDocId"].ToString();
            if (row["Term"] != DBNull.Value) ret.Term = row["Term"].ToString();
            if (row["Created"] != DBNull.Value) ret.Created = Convert.ToDateTime(row["Created"].ToString()); 

            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
