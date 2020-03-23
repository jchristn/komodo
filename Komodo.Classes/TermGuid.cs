using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Classes
{
    /// <summary>
    /// Associates a term with a specific GUID.
    /// </summary>
    public class TermGuid
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the index.
        /// </summary>
        public string IndexGUID { get; set; }

        /// <summary>
        /// The term.
        /// </summary>
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

        /// <summary>
        /// Create a database insertable dictionary from the object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("guid", GUID);
            ret.Add("indexguid", IndexGUID);
            ret.Add("term", Term);
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static TermGuid FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            TermGuid ret = new TermGuid();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("indexguid") && row["indexguid"] != null && row["indexguid"] != DBNull.Value)
                ret.IndexGUID = row["indexguid"].ToString();

            if (row.Table.Columns.Contains("term") && row["term"] != null && row["term"] != DBNull.Value)
                ret.Term = row["term"].ToString(); 

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<TermGuid> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<TermGuid> ret = new List<TermGuid>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(TermGuid.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
