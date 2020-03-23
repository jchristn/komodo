using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Classes
{
    /// <summary>
    /// Specifies API permissions associated with API keys. 
    /// </summary>
    public class Permission
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
        /// Globally-unique identifier of the user.
        /// </summary>
        public string UserGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the API key.
        /// </summary>
        public string ApiKeyGUID { get; set; }
         
        /// <summary>
        /// Allow or disallow search.
        /// </summary>
        public bool AllowSearch { get; set; }

        /// <summary>
        /// Allow or disallow document creation.
        /// </summary>
        public bool AllowCreateDocument { get; set; }

        /// <summary>
        /// Allow or disallow document deletion.
        /// </summary>
        public bool AllowDeleteDocument { get; set; }

        /// <summary>
        /// Allow or disallow index creation.
        /// </summary>
        public bool AllowCreateIndex { get; set; }

        /// <summary>
        /// Allow or disallow index deletion.
        /// </summary>
        public bool AllowDeleteIndex { get; set; }
         
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Permission()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="indexGuid">Index GUID.</param>
        /// <param name="userGuid">User GUID.</param>
        /// <param name="apiKeyGuid">API key GUID.</param>
        /// <param name="allowSearch">Allow search.</param>
        /// <param name="allowCreateDoc">Allow document creation.</param>
        /// <param name="allowDeleteDoc">Allow document deletion.</param>
        /// <param name="allowCreateIndex">Allow index creation.</param>
        /// <param name="allowDeleteIndex">Allow index deletion.</param>
        public Permission(string indexGuid, string userGuid, string apiKeyGuid, bool allowSearch, bool allowCreateDoc, bool allowDeleteDoc, bool allowCreateIndex, bool allowDeleteIndex)
        {
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(userGuid)) throw new ArgumentNullException(nameof(userGuid));
            if (String.IsNullOrEmpty(apiKeyGuid)) throw new ArgumentNullException(nameof(apiKeyGuid));

            GUID = Guid.NewGuid().ToString();
            IndexGUID = indexGuid;
            UserGUID = userGuid;
            ApiKeyGUID = apiKeyGuid;

            AllowSearch = allowSearch;
            AllowCreateDocument = allowCreateDoc;
            AllowDeleteDocument = allowDeleteDoc;
            AllowCreateIndex = allowCreateIndex;
            AllowDeleteIndex = allowDeleteIndex; 
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
            ret.Add("userguid", UserGUID);
            ret.Add("apikeyguid", ApiKeyGUID);
            ret.Add("allowsearch", Convert.ToInt32(AllowSearch));
            ret.Add("allowcreatedoc", Convert.ToInt32(AllowCreateDocument));
            ret.Add("allowdeletedoc", Convert.ToInt32(AllowDeleteDocument));
            ret.Add("allowcreateindex", Convert.ToInt32(AllowCreateIndex));
            ret.Add("allowdeleteindex", Convert.ToInt32(AllowDeleteIndex)); 
            return ret; 
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static Permission FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            Permission ret = new Permission();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("indexguid") && row["indexguid"] != null && row["indexguid"] != DBNull.Value)
                ret.IndexGUID = row["indexguid"].ToString();

            if (row.Table.Columns.Contains("userguid") && row["userguid"] != null && row["userguid"] != DBNull.Value)
                ret.UserGUID = row["userguid"].ToString();

            if (row.Table.Columns.Contains("apikeyguid") && row["apikeyguid"] != null && row["apikeyguid"] != DBNull.Value)
                ret.ApiKeyGUID = row["apikeyguid"].ToString();

            if (row.Table.Columns.Contains("allowsearch") && row["allowsearch"] != null && row["allowsearch"] != DBNull.Value)
                ret.AllowSearch = Convert.ToBoolean(row["allowsearch"]);

            if (row.Table.Columns.Contains("allowcreatedoc") && row["allowcreatedoc"] != null && row["allowcreatedoc"] != DBNull.Value)
                ret.AllowCreateDocument = Convert.ToBoolean(row["allowcreatedoc"]);

            if (row.Table.Columns.Contains("allowdeletedoc") && row["allowdeletedoc"] != null && row["allowdeletedoc"] != DBNull.Value)
                ret.AllowDeleteDocument = Convert.ToBoolean(row["allowdeletedoc"]);

            if (row.Table.Columns.Contains("allowcreateindex") && row["allowcreateindex"] != null && row["allowcreateindex"] != DBNull.Value)
                ret.AllowCreateIndex = Convert.ToBoolean(row["allowcreateindex"]);

            if (row.Table.Columns.Contains("allowdeleteindex") && row["allowdeleteindex"] != null && row["allowdeleteindex"] != DBNull.Value)
                ret.AllowDeleteIndex = Convert.ToBoolean(row["allowdeleteindex"]); 

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<Permission> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<Permission> ret = new List<Permission>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(Permission.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
