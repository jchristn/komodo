using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Classes
{
    /// <summary>
    /// Repository where source and parsed documents are stored.
    /// </summary>
    public class Index
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
        /// Globally-unique identifier of the user that owns this index.
        /// </summary>
        public string OwnerGUID { get; set; }

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Index()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns this index.</param>
        /// <param name="name">The name of the index.</param> 
        public Index(string ownerGuid, string name)
        {
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));

            GUID = Guid.NewGuid().ToString();
            OwnerGUID = ownerGuid;
            Name = name;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns this index.</param>
        /// <param name="name">The name of the index.</param> 
        public Index(string guid, string ownerGuid, string name)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));

            GUID = guid;
            OwnerGUID = ownerGuid;
            Name = name;
        }

        /// <summary>
        /// Create a database insertable dictionary from the object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("guid", GUID);
            ret.Add("ownerguid", OwnerGUID);
            ret.Add("name", Name);
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static Index FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            Index ret = new Index();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("ownerguid") && row["ownerguid"] != null && row["ownerguid"] != DBNull.Value)
                ret.OwnerGUID = row["ownerguid"].ToString();

            if (row.Table.Columns.Contains("name") && row["name"] != null && row["name"] != DBNull.Value)
                ret.Name = row["name"].ToString(); 

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<Index> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<Index> ret = new List<Index>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(Index.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
