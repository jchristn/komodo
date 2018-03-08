using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using HtmlAgilityPack;
using NUglify;
using RestWrapper;
using XmlToPox;

namespace KomodoCore
{
    /// <summary>
    /// Parsed XML document.
    /// </summary>
    public class ParsedXml
    {
        #region Public-Members

        /// <summary>
        /// Schema for the object.
        /// </summary>
        public Dictionary<string, DataType> Schema { get; set; }

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
        public List<DataNode> Flattened { get; set; }  // key, value, type

        /// <summary>
        /// List of tokens found within the object.
        /// </summary>
        public List<string> Tokens { get; set; }

        #endregion

        #region Private-Members
        
        private string SourceContent { get; set; }
        private string SourceContentPlain { get; set; }
        private string SourceUrl { get; set; }
        private XElement CurrentXElement{ get; set; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the ParsedXml object.
        /// </summary>
        public ParsedXml()
        {
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
            SourceContent = Encoding.UTF8.GetString(File.ReadAllBytes(filename));
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
            RestResponse resp = RestRequest.SendRequestSafe(url, "text/plain", "GET", null, null, false, false, null, null);
            if (resp == null) return false;
            if (resp.StatusCode != 200) return false;
            if (resp.Data == null || resp.Data.Length < 1) return false;
            SourceContent = Encoding.UTF8.GetString(resp.Data);
            SourceUrl = url;
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
            if (String.IsNullOrEmpty(sourceUrl)) throw new ArgumentNullException(nameof(sourceUrl));
            SourceUrl = sourceUrl;
            SourceContent = Encoding.UTF8.GetString(data);
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
            if (String.IsNullOrEmpty(sourceUrl)) throw new ArgumentNullException(nameof(sourceUrl));
            SourceUrl = sourceUrl;
            SourceContent = data;
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
            int nodeCount;
            int containerCount;

            SourceContentPlain = XmlTools.Convert(SourceContent);
            CurrentXElement = XElement.Parse(SourceContentPlain);
            
            Flattened = Flatten(out maxDepth, out nodeCount, out containerCount);
            MaxDepth = maxDepth;
            NodeCount = nodeCount;
            ContainerCount = containerCount;

            Schema = BuildSchema();
            Tokens = GetTokens();

            NodeCount = Flattened.Count;
            return true;
        }

        private List<DataNode> Flatten(out int maxDepth, out int nodeCount, out int containerCount)
        {
            maxDepth = 1;
            nodeCount = 0;
            containerCount = 0;

            List<DataNode> nodes = new List<DataNode>();
            ExtractTokensAndValues(CurrentXElement, "", 0, out nodes, out maxDepth, out nodeCount, out containerCount);
            return nodes;
        }

        private void ExtractTokensAndValues(
            XElement currElement, 
            string keyPrepend, 
            int currDepth, 
            out List<DataNode> nodes, 
            out int maxDepth, 
            out int nodeCount, 
            out int containerCount)
        {
            // see http://www.java2s.com/Tutorial/CSharp/0540__XML/LoopThroughXmlDocumentRecursively.htm

            nodes = new List<DataNode>();
            maxDepth = currDepth;
            nodeCount = 0;
            containerCount = 0;

            int childCount = currElement.Elements().Count();
            if (childCount > 0)
            {
                #region Container

                containerCount++;
                if (!String.IsNullOrEmpty(keyPrepend))
                {
                    nodes.Add(new DataNode(keyPrepend + "." + currElement.Name.ToString(), null, DataType.Object));
                }
                else
                {
                    nodes.Add(new DataNode(currElement.Name.ToString(), null, DataType.Object));
                }

                foreach (XElement childElement in currElement.Elements())
                {
                    List<DataNode> childNodes = new List<DataNode>();
                    int childMaxDepth;
                    int childNodeCount;
                    int childContainerCount;

                    if (String.IsNullOrEmpty(keyPrepend))
                    {
                        ExtractTokensAndValues(childElement, currElement.Name.ToString(), currDepth + 1, out childNodes, out childMaxDepth, out childNodeCount, out childContainerCount);
                    }
                    else
                    {
                        ExtractTokensAndValues(childElement, keyPrepend + "." + currElement.Name.ToString(), currDepth + 1, out childNodes, out childMaxDepth, out childNodeCount, out childContainerCount);
                    }

                    foreach (DataNode currNode in childNodes)
                    {
                        nodes.Add(currNode);
                    }

                    if (childMaxDepth > maxDepth) maxDepth = childMaxDepth;
                    containerCount += childContainerCount;
                    nodeCount += childNodeCount;
                    nodeCount++;
                }

                #endregion
            }
            else
            {
                #region Leaf

                nodeCount++;

                if (!String.IsNullOrEmpty(keyPrepend))
                {
                    nodes.Add(new DataNode(keyPrepend + "." + currElement.Name.ToString(), currElement.Value, DataNode.TypeFromValue(currElement.Value)));
                }
                else
                {
                    nodes.Add(new DataNode(currElement.Name.ToString(), currElement.Value, DataNode.TypeFromValue(currElement.Value)));
                }

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

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
