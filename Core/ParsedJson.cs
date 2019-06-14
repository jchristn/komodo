using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using NUglify;
using RestWrapper; 
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Komodo.Core
{
    /// <summary>
    /// Parsed JSON document.
    /// </summary>
    public class ParsedJson
    {
        #region Public-Members

        /// <summary>
        /// Schema for the object.
        /// </summary> 
        public Dictionary<string, DataType> Schema { get; set; }  // key, type

        /// <summary>
        /// Maximum node depth.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Number of arrays within the object.
        /// </summary>
        public int ArrayCount { get; set; }

        /// <summary>
        /// Number of nodes within the object.
        /// </summary>
        public int NodeCount { get; set; }

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
        
        private string _SourceContent { get; set; }
        private string _SourceUrl { get; set; }
        private JToken _JToken { get; set; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the ParsedJson object.
        /// </summary>
        public ParsedJson()
        {
            Schema = new Dictionary<string, DataType>();
            MaxDepth = 0;
            ArrayCount = 0;
            NodeCount = 0;
            Flattened = new List<DataNode>();
            Tokens = new List<string>();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Load content from a file.
        /// </summary>
        /// <param name="filename">Path and filename.</param>
        /// <returns>True if successful.</returns>
        public bool LoadFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename)) throw new FileNotFoundException(nameof(filename));
            _SourceContent = Encoding.UTF8.GetString(File.ReadAllBytes(filename));
            return ProcessSourceContent();
        }

        /// <summary>
        /// Load content from a URL.
        /// </summary>
        /// <param name="url">URL to retrieve.</param>
        /// <returns>True if successful.</returns>
        public bool LoadUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            RestRequest req = new RestRequest(
                url,
                HttpMethod.GET,
                null,
                null,
                true);

            RestResponse resp = req.Send();
            if (resp == null) return false;
            if (resp.StatusCode != 200) return false;
            if (resp.Data == null || resp.Data.Length < 1) return false;
            _SourceContent = Encoding.UTF8.GetString(resp.Data);
            _SourceUrl = url;
            return ProcessSourceContent();
        }

        /// <summary>
        /// Load content from a byte array.
        /// </summary>
        /// <param name="data">Byte array.</param>
        /// <param name="sourceUrl">Source URL for the content.</param>
        /// <returns>True if successful.</returns>
        public bool LoadBytes(byte[] data, string sourceUrl)
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data)); 
            _SourceUrl = sourceUrl;
            _SourceContent = Encoding.UTF8.GetString(data);
            return ProcessSourceContent();
        }

        /// <summary>
        /// Load content from a string.
        /// </summary>
        /// <param name="data">String.</param>
        /// <param name="sourceUrl">Source URL for the content.</param>
        /// <returns>True if successful.</returns>
        public bool LoadString(string data, string sourceUrl)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data)); 
            _SourceUrl = sourceUrl;
            _SourceContent = data;
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
            ret += "  Max Depth   : " + MaxDepth + Environment.NewLine;
            ret += "  Array Count : " + ArrayCount + Environment.NewLine;
            ret += "  Node Count  : " + NodeCount + Environment.NewLine;

            if (Flattened != null && Flattened.Count > 0)
            {
                ret += "  Tokens in Flattened JSON : " + Flattened.Count + " entries" + Environment.NewLine;
                foreach (DataNode currNode in Flattened)
                {
                    ret += "    " + currNode.Key + " (" + currNode.Type + "): " + (currNode.Data != null ? currNode.Data.ToString() : "null") + Environment.NewLine;
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
            int maxDepth;
            int arrayCount;
            int nodeCount;

            _JToken = JToken.Parse(_SourceContent);

            Flattened = Flatten(out maxDepth, out arrayCount, out nodeCount);
            MaxDepth = maxDepth;
            ArrayCount = arrayCount;
            NodeCount = nodeCount;

            Schema = BuildSchema();
            Tokens = GetTokens(); 

            NodeCount = Flattened.Count;
            return true;
        }

        private List<DataNode> Flatten(out int maxDepth, out int arrayCount, out int nodeCount)
        {
            maxDepth = 1;
            arrayCount = 0;
            nodeCount = 0;

            List<DataNode> nodes = new List<DataNode>();
            ExtractTokensAndValues(_JToken, "", 0, out nodes, out maxDepth, out arrayCount, out nodeCount);
            return nodes;
        }

        private void ExtractTokensAndValues(
            JToken token, 
            string keyPrepend, 
            int currDepth, 
            out List<DataNode> nodes, 
            out int maxDepth, 
            out int arrayCount, 
            out int nodeCount)
        {
            // see http://stackoverflow.com/questions/39962368/deserializing-anything-using-json-net

            nodes = new List<DataNode>();
            maxDepth = currDepth;
            arrayCount = 0;
            nodeCount = 0;

            if (token.Type == JTokenType.Object)
            {
                #region Object

                nodeCount++;
                if (!String.IsNullOrEmpty(keyPrepend)) nodes.Add(new DataNode(keyPrepend, null, DataType.Object));

                foreach (JProperty prop in token.Children<JProperty>())
                {
                    List<DataNode> childNodes = new List<DataNode>();
                    int childMaxDepth;
                    int childArrayCount;
                    int childNodeCount;

                    if (String.IsNullOrEmpty(keyPrepend))
                    {
                        ExtractTokensAndValues(prop.Value, prop.Name, currDepth + 1, out childNodes, out childMaxDepth, out childArrayCount, out childNodeCount);
                    }
                    else
                    {
                        ExtractTokensAndValues(prop.Value, keyPrepend + "." + prop.Name, currDepth + 1, out childNodes, out childMaxDepth, out childArrayCount, out childNodeCount);
                    }
                    foreach (DataNode currNode in childNodes)
                    {
                        nodes.Add(currNode);
                    }

                    if (childMaxDepth > maxDepth) maxDepth = childMaxDepth;
                    arrayCount += childArrayCount;
                    nodeCount += childNodeCount;
                }
                
                #endregion
            }
            else if (token.Type == JTokenType.Array)
            {
                #region Array

                arrayCount++;
                if (!String.IsNullOrEmpty(keyPrepend)) nodes.Add(new DataNode(keyPrepend, null, DataType.Array));
                else nodes.Add(new DataNode("undefined", null, DataType.Array));

                foreach (JToken child in token.Children())
                {
                    List<DataNode> childNodes = new List<DataNode>();
                    int childMaxDepth;
                    int childArrayCount;
                    int childNodeCount;
                    
                    ExtractTokensAndValues(child, keyPrepend, currDepth, out childNodes, out childMaxDepth, out childArrayCount, out childNodeCount);
                    foreach (DataNode currNode in childNodes)
                    {
                        nodes.Add(currNode);
                    }

                    if (childMaxDepth > maxDepth) maxDepth = childMaxDepth;
                    arrayCount += childArrayCount;
                    nodeCount += childNodeCount;
                }
                
                #endregion
            }
            else
            {
                #region Key-Value-Pair

                nodes.Add(new DataNode(keyPrepend, token.ToString(), DataNode.TypeFromValue(token)));
                nodeCount++;

                #endregion
            }
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
    }
}
