using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using DatabaseWrapper;

namespace KomodoCore
{ 
    /// <summary>
    /// Parsed SQL data.
    /// </summary>
    public class ParsedSql
    {
        #region Public-Members

        /// <summary>
        /// Schema for the object.
        /// </summary>
        public Dictionary<string, DataType> Schema { get; set; }

        /// <summary>
        /// Number of rows.
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Number of columns.
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// Flattened representation of the object.
        /// </summary>
        public List<DataNode> Flattened { get; set; }  // key, value, type

        /// <summary>
        /// List of tokens found within the object.
        /// </summary>
        public List<string> Tokens { get; set; }

        #endregion

        #region Private-Members
        
        private DatabaseClient Db { get; set; }
        private string DbType { get; set; }
        private string ServerHostname { get; set; }
        private int ServerPort { get; set; }
        private string User { get; set; }
        private string Password { get; set; }
        private string Instance { get; set; }
        private string Database { get; set; }
        private string Query { get; set; }
        private DataTable SourceContent { get; set; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the ParsedJson object.
        /// </summary>
        public ParsedSql()
        {
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Load data from a SQL query.
        /// </summary>
        /// <param name="dbType">The database type, one of: mssql, mysql, pgsql.</param>
        /// <param name="serverHostname">The server hostname.</param>
        /// <param name="serverPort">The TCP port on which to connect.</param>
        /// <param name="user">The database username.</param>
        /// <param name="pass">The database password.</param>
        /// <param name="instance">The database instance, only relevant to mssql databases.</param>
        /// <param name="databaseName">The database name.</param>
        /// <param name="query">The query to execute.</param>
        /// <returns>True if successful.</returns>
        public bool LoadDatabase(string dbType, string serverHostname, int serverPort, string user, string pass, string instance, string databaseName, string query)
        {
            if (String.IsNullOrEmpty(dbType)) throw new ArgumentNullException(nameof(dbType));
            if (!dbType.Equals("mssql") && !dbType.Equals("mysql")) throw new ArgumentException("dbType must be either mssql or mysql");
            if (String.IsNullOrEmpty(serverHostname)) throw new ArgumentNullException(nameof(serverHostname));
            if (serverPort < 1) throw new ArgumentOutOfRangeException(nameof(serverPort));
            if (String.IsNullOrEmpty(databaseName)) throw new ArgumentNullException(nameof(databaseName));
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            DbType = dbType;
            ServerHostname = serverHostname;
            ServerPort = serverPort;
            User = user;
            Password = pass;
            Instance = instance;
            Database = databaseName;
            Query = query;

            switch (dbType)
            {
                case "mssql":
                    Db = new DatabaseClient(DbTypes.MsSql, serverHostname, serverPort, user, pass, instance, databaseName);
                    break;

                case "mysql":
                    Db = new DatabaseClient(DbTypes.MySql, serverHostname, serverPort, user, pass, instance, databaseName);
                    break;

                default:
                    throw new ArgumentException("dbType must be either mssql or mysql");
            }

            return ProcessSourceContent();
        }

        /// <summary>
        /// Returns a human-readable string version of the object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += "---" + Environment.NewLine;
            ret += "  Database    : " + ServerHostname + ":" + ServerPort + " db " + Database + (String.IsNullOrEmpty(Instance) ? "" : " instance " + Instance) + Environment.NewLine;
            ret += "  Query       : " + Query + Environment.NewLine;
            ret += "  Rows        : " + Rows + Environment.NewLine;
            ret += "  Columns     : " + Columns + Environment.NewLine;
            ret += "  Nodes       : " + (Rows * Columns) + Environment.NewLine;

            if (Schema != null && Schema.Count > 0)
            {
                ret += "  Schema      : " + Schema.Count + " entries" + Environment.NewLine;
                foreach (KeyValuePair<string, DataType> curr in Schema)
                {
                    ret += "    " + curr.Key + ": " + curr.Value.ToString() + Environment.NewLine;
                }
            }

            if (Flattened != null && Flattened.Count > 0)
            {
                ret += "  Tokens in Flattened SQL : " + Flattened.Count + Environment.NewLine;
                foreach (DataNode currNode in Flattened)
                {
                    ret += "    " + currNode.Key + " (" + currNode.Type.ToString() + "): " + (currNode.Data != null ? currNode.Data.ToString() : "null") + Environment.NewLine;
                }
            }

            if (Tokens != null && Tokens.Count > 0)
            {
                ret += "  Tokens : " + Tokens.Count + " entries" + Environment.NewLine;
                foreach (string curr in Tokens)
                {
                    ret += "    " + curr + Environment.NewLine;
                }
            }
          
            ret += "---";
            return ret;
        }

        #endregion

        #region Private-Methods

        private bool ProcessSourceContent()
        {
            SourceContent = Db.RawQuery(Query);
            if (SourceContent == null || SourceContent.Rows.Count < 1)
            {
                Rows = 0;
                Columns = 0;
                Schema = new Dictionary<string, DataType>();
                return true;
            }

            Flattened = new List<DataNode>();
            List<Dictionary<string, object>> dicts = Common.DataTableToListDictionary(SourceContent);
            foreach (Dictionary<string, object> dict in dicts)
            {
                foreach (KeyValuePair<string, object> kvp in dict)
                {
                    Flattened.Add(new DataNode(kvp.Key, kvp.Value, DataNode.TypeFromValue(kvp.Value)));
                }
            }

            Schema = BuildSchema();
            Tokens = GetTokens();
            Rows = SourceContent.Rows.Count;
            Columns = SourceContent.Columns.Count;

            return true;
        }
        
        private Dictionary<string, DataType> BuildSchema()
        {
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

            foreach (DataNode curr in Flattened)
            {
                if (ret.ContainsKey(curr.Key))
                {
                    if (ret[curr.Key].Equals("null") && !curr.Type.Equals(DataType.Null))
                    {
                        // replace null with more specific type
                        ret.Remove(curr.Key);
                        ret.Add(curr.Key, curr.Type);
                    }
                    continue;
                }
                else
                {
                    ret.Add(curr.Key, curr.Type);
                }
            }

            return ret;
        }

        private List<string> GetTokens()
        {
            List<string> ret = new List<string>();

            foreach (DataNode curr in Flattened)
            {
                if (curr.Data == null) continue;
                if (String.IsNullOrEmpty(curr.Data.ToString())) continue;

                string token = "";
                foreach (char c in curr.Data.ToString())
                {
                    if ((int)c < 32) continue;
                    if ((int)c > 57 && (int)c < 64) continue;
                    if ((int)c > 90 && (int)c < 97) continue;
                    if ((int)c > 122) continue;
                    token += c;
                }

                if (String.IsNullOrEmpty(token)) continue;
                ret.Add(token.ToLower());
            }

            return ret;
        }

        #endregion

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
