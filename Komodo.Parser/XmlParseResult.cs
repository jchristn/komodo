using System;
using System.Collections.Generic; 
using Komodo.Classes;

namespace Komodo.Parser
{
    /// <summary>
    /// Parsed XML document.
    /// </summary>
    public class XmlParseResult
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
        /// Maximum node depth.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Number of nodes within the object.
        /// </summary>
        public int NodeCount { get; set; }

        /// <summary>
        /// Number of containers within the object.
        /// </summary>
        public int ContainerCount { get; set; }

        /// <summary>
        /// Flattened representation of the object.
        /// </summary>
        public List<DataNode> Flattened { get; set; } = new List<DataNode>();

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
        public XmlParseResult()
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
            ret += "  Success         : " + Success + Environment.NewLine;
            ret += "  Max Depth       : " + MaxDepth + Environment.NewLine;
            ret += "  Node Count      : " + NodeCount + Environment.NewLine;
            ret += "  Container Count : " + ContainerCount + Environment.NewLine;

            if (Flattened != null && Flattened.Count > 0)
            {
                ret += "  Tokens in Flattened XML : " + Flattened.Count + " entries" + Environment.NewLine;
                foreach (DataNode currNode in Flattened)
                {
                    ret += "    " + currNode.Key + " (" + currNode.Type.ToString() + "): " + (currNode.Data != null ? currNode.Data.ToString() : "null") + Environment.NewLine;
                }
            }

            if (Schema != null && Schema.Count > 0)
            {
                ret += "  Schema : " + Schema.Count + " entries" + Environment.NewLine;
                foreach (KeyValuePair<string, DataType> currKvp in Schema)
                {
                    ret += "    " + currKvp.Key + ": " + currKvp.Value.ToString() + Environment.NewLine;
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
