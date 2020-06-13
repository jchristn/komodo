using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;

namespace Komodo.Classes
{
    /// <summary>
    /// Key-value pair configuration-related data.
    /// </summary>
    [Table("metadata")]
    public class Metadata
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        /// <summary>
        /// Key.
        /// </summary>
        [Column("configkey", false, DataTypes.Nvarchar, 64, false)]
        public string Key { get; set; }

        /// <summary>
        /// Value associated with the specified key.
        /// </summary>
        [Column("configval", false, DataTypes.Nvarchar, 2048, true)]
        public string Value { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Metadata()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="val">Value associated with the specified key.</param>
        public Metadata(string key, string val)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));

            Key = key;
            Value = val;
        }

        /// <summary>
        /// Create a database insertable dictionary from the object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("configkey", Key);
            ret.Add("configval", Value);
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static Metadata FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            Metadata ret = new Metadata();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("configkey") && row["configkey"] != null && row["configkey"] != DBNull.Value)
                ret.Key = row["configkey"].ToString();

            if (row.Table.Columns.Contains("configval") && row["configval"] != null && row["configval"] != DBNull.Value)
                ret.Value = row["configval"].ToString(); 

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<Metadata> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<Metadata> ret = new List<Metadata>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(Metadata.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
