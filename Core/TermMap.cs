using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Core
{
    /// <summary>
    /// Maps a term to a set of identifiers.
    /// </summary>
    public class TermMap
    {
        #region Public-Members

        /// <summary>
        /// ID (database row).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Term.
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// GUID.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Timestamp from last update.
        /// </summary>
        public DateTime LastUpdate { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public TermMap()
        {

        }

        /// <summary>
        /// Instantiate the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>TermMap.</returns>
        public static TermMap FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            TermMap ret = new TermMap();
            ret.Id = Convert.ToInt32(row["Id"]);
            ret.Term = row["Term"].ToString();
            ret.GUID = row["GUID"].ToString();
            ret.Created = Convert.ToDateTime(row["Created"].ToString());
            ret.LastUpdate = Convert.ToDateTime(row["LastUpdate"].ToString());
            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
