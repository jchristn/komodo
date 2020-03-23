using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Classes
{
    /// <summary>
    /// System participating in Komodo.
    /// </summary>
    public class Node
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
        /// The hostname of the node.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// The port on which the node is listening for incoming HTTP or HTTPS requests.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Specifies whether or not SSL is required.
        /// </summary>
        public bool Ssl { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Node()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="hostname">The hostname of the node.</param>
        /// <param name="port">The port on which the node is listening for incoming HTTP or HTTPS requests.</param>
        /// <param name="ssl">Specifies whether or not SSL is required.</param>
        public Node(string hostname, int port, bool ssl)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0) throw new ArgumentException("Port must be zero or greater.");

            GUID = Guid.NewGuid().ToString();
            Hostname = hostname;
            Port = port;
            Ssl = ssl;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="hostname">The hostname of the node.</param>
        /// <param name="port">The port on which the node is listening for incoming HTTP or HTTPS requests.</param>
        /// <param name="ssl">Specifies whether or not SSL is required.</param>
        public Node(string guid, string hostname, int port, bool ssl)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0) throw new ArgumentException("Port must be zero or greater.");

            GUID = guid;
            Hostname = hostname;
            Port = port;
            Ssl = ssl;
        }

        /// <summary>
        /// Create a database insertable dictionary from the object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("guid", GUID);
            ret.Add("hostname", Hostname);
            ret.Add("port", Port);
            ret.Add("ssl", Convert.ToInt32(Ssl));
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static Node FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            Node ret = new Node();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("hostname") && row["hostname"] != null && row["hostname"] != DBNull.Value)
                ret.Hostname = row["hostname"].ToString();

            if (row.Table.Columns.Contains("port") && row["port"] != null && row["port"] != DBNull.Value)
                ret.Port = Convert.ToInt32(row["port"]);

            if (row.Table.Columns.Contains("ssl") && row["ssl"] != null && row["ssl"] != DBNull.Value)
                ret.Ssl = Convert.ToBoolean(row["ssl"]);

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<Node> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<Node> ret = new List<Node>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(Node.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
