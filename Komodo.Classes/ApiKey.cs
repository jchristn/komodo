using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Classes
{
    /// <summary>
    /// Identifies credentials that can be used by API requestors.
    /// </summary>
    public class ApiKey
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        public int Id { get; set;  }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user.
        /// </summary>
        public string UserGUID { get; set; }

        /// <summary>
        /// Indicates whether or not the API key is able to be used.
        /// </summary>
        public bool Active { get; set; }
        
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ApiKey()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="userGuid">Globally-unique identifier of the user.</param>
        /// <param name="active">Indicates whether or not the API key is able to be used.</param>
        public ApiKey(string userGuid, bool active)
        {
            if (String.IsNullOrEmpty(userGuid)) throw new ArgumentNullException(nameof(userGuid));

            GUID = Guid.NewGuid().ToString();
            UserGUID = userGuid;
            Active = active;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="userGuid">Globally-unique identifier of the user.</param>
        /// <param name="active">Indicates whether or not the API key is able to be used.</param>
        public ApiKey(string guid, string userGuid, bool active)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(userGuid)) throw new ArgumentNullException(nameof(userGuid));

            GUID = guid;
            UserGUID = userGuid;
            Active = active;
        }

        /// <summary>
        /// Create a database insertable dictionary from the object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("guid", GUID);
            ret.Add("userguid", UserGUID);
            ret.Add("active", Convert.ToInt32(Active));
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static ApiKey FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            ApiKey ret = new ApiKey();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("userguid") && row["userguid"] != null && row["userguid"] != DBNull.Value)
                ret.UserGUID = row["userguid"].ToString();
             
            if (row.Table.Columns.Contains("active") && row["active"] != null && row["active"] != DBNull.Value)
                ret.Active = Convert.ToBoolean(row["active"]);

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<ApiKey> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<ApiKey> ret = new List<ApiKey>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(ApiKey.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
