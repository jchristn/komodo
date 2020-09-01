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
using Watson.ORM;
using Watson.ORM.Core;
using Komodo.Classes;

namespace Komodo.Parser
{
    /// <summary>
    /// Parsed Sqlite data.
    /// </summary>
    public class SqliteParseResult
    {
        #region Public-Members

        /// <summary>
        /// Indicates if the parser was successful.
        /// </summary>
        public bool Success = false;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        public Timestamps Time = new Timestamps();
         
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
        public List<DataNode> Flattened = new List<DataNode>();

        /// <summary>
        /// Tokens found including their count.
        /// </summary>
        public Dictionary<string, int> Tokens = new Dictionary<string, int>();

        /// <summary>
        /// Schema of the document.
        /// </summary>
        public Dictionary<string, DataType> Schema = new Dictionary<string, DataType>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SqliteParseResult()
        {
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Returns a human-readable string version of the object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += "---" + Environment.NewLine;
            ret += "  Success     : " + Success + Environment.NewLine;  
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
                ret += "  Tokens             : " + Tokens.Count + Environment.NewLine;
                foreach (KeyValuePair<string, int> curr in Tokens) ret += "    " + curr.Key + " [" + curr.Value + "]" + Environment.NewLine;
            }

            ret += "---";
            return ret;
        }

        #endregion

        #region Private-Methods
         
        #endregion
    }
}
