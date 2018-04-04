using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomodoCore
{
    /// <summary>
    /// Index metadata.
    /// </summary>
    public class Index
    {
        #region Public-Members

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string IndexName { get; set; }  

        /// <summary>
        /// The root directory of the index.
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Index options for the index.
        /// </summary>
        public IndexOptions Options { get; set; }

        /// <summary>
        /// Enable or disable database debugging.
        /// </summary>
        public bool DatabaseDebug { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the Index.
        /// </summary>
        public Index()
        {

        }

        /// <summary>
        /// Instantiate the Index.
        /// </summary>
        /// <param name="filename">The file containing JSON from which the index should be instantiated.</param>
        /// <returns>Index.</returns>
        public static Index FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename)) throw new FileNotFoundException("File not found");
            string contents = Common.ReadTextFile(filename);
            Index ret = Common.DeserializeJson<Index>(contents);
            return ret;
        }

        /// <summary>
        /// Instantiate the Index.
        /// </summary>
        /// <param name="row">The DataRow from which the index should be instantiated.</param>
        /// <returns>Index.</returns>
        public static Index FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));
            Index ret = new Index();
            ret.IndexName = row["Name"].ToString();
            ret.RootDirectory = row["Directory"].ToString();
            ret.Options = Common.DeserializeJson<IndexOptions>(row["Options"].ToString());
            ret.DatabaseDebug = false;
            return ret;
        }

        /// <summary>
        /// Instantiate the Index.
        /// </summary>
        /// <param name="table">The DataTable from which the index should be instantiated.</param>
        /// <returns>Index.</returns>
        public static Index FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (table.Rows.Count != 1) throw new ArgumentException("Table has more than one row");
            foreach (DataRow row in table.Rows)
            {
                return Index.FromDataRow(row);
            }
            return null;
        }

        /// <summary>
        /// Instantiate a list of Index objects.
        /// </summary>
        /// <param name="table">Te DataTable from which the list should be instantiated.</param>
        /// <returns>List of Index.</returns>
        public static List<Index> ListFromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            List<Index> ret = new List<Index>();
            foreach (DataRow row in table.Rows)
            {
                ret.Add(Index.FromDataRow(row));
            }
            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
