using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Komodo.Classes;
using Komodo.Crawler;

namespace Komodo.Parser
{
    /// <summary>
    /// Parser for JSON objects.
    /// </summary>
    public class JsonParser
    {
        #region Public-Members

        /// <summary>
        /// Text parser to use when evaluating text.
        /// </summary>
        public TextParser TextParser
        {
            get
            {
                return _TextParser;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(TextParser));
                _TextParser = value;
            }
        }

        /// <summary>
        /// Minimum length of a token to include in the result.
        /// </summary>
        public int MinimumTokenLength
        {
            get
            {
                return _MinimumTokenLength;
            }
            set
            {
                if (value < 1) throw new ArgumentException("Minimum token length must be one or greater.");
                _MinimumTokenLength = value;
            }
        }

        #endregion

        #region Private-Members 

        private int _MinimumTokenLength = 3;
        private TextParser _TextParser = new TextParser();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public JsonParser()
        { 
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Crawl and parse data from a URL.
        /// </summary>
        /// <param name="url">Source URL.</param>
        /// <returns>Parse result.</returns>
        public JsonParseResult ParseFromUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));  
            HttpCrawler crawler = new HttpCrawler(url); 
            JsonParseResult result = new JsonParseResult();
            HttpCrawlResult crawlResult = crawler.Get();
            if (!crawlResult.Success) return result;
            byte[] sourceData = crawlResult.Data;
            string sourceContent = Encoding.UTF8.GetString(sourceData);
            return ProcessSourceContent(sourceContent);
        }

        /// <summary>
        /// Crawl and parse data from a file.
        /// </summary>
        /// <param name="filename">Path and filename.</param>
        /// <returns>Parse result.</returns>
        public JsonParseResult ParseFromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            FileCrawler crawler = new FileCrawler(filename);
            JsonParseResult result = new JsonParseResult();
            FileCrawlResult crawlResult = crawler.Get();
            if (!crawlResult.Success) return result;
            byte[] sourceData = crawlResult.Data;
            string sourceContent = Encoding.UTF8.GetString(sourceData);
            return ProcessSourceContent(sourceContent);
        }

        /// <summary>
        /// Parse data from a string.
        /// </summary>
        /// <param name="data">JSON string.</param>
        /// <returns>Parse result.</returns>
        public JsonParseResult ParseString(string data)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            return ProcessSourceContent(data);
        }

        /// <summary>
        /// Parse data from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data containing JSON.</param>
        /// <returns>Parse result.</returns>
        public JsonParseResult ParseBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            string sourceContent = Encoding.UTF8.GetString(bytes);
            return ProcessSourceContent(sourceContent);
        }

        #endregion

        #region Private-Methods

        private JsonParseResult ProcessSourceContent(string content)
        {
            int maxDepth;
            int arrayCount;
            int nodeCount;

            JToken jtoken = JToken.Parse(content);

            JsonParseResult ret = new JsonParseResult();
            ret.Flattened = Flatten(jtoken, out maxDepth, out arrayCount, out nodeCount);
            ret.MaxDepth = maxDepth;
            ret.ArrayCount = arrayCount;
            ret.NodeCount = nodeCount; 
            ret.Schema = BuildSchema(ret.Flattened);
            ret.Tokens = GetTokens(ret.Flattened); 
            ret.NodeCount = ret.Flattened.Count;
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        private List<DataNode> Flatten(JToken jtoken, out int maxDepth, out int arrayCount, out int nodeCount)
        {
            maxDepth = 1;
            arrayCount = 0;
            nodeCount = 0;

            List<DataNode> nodes = new List<DataNode>();
            ExtractTokensAndValues(jtoken, "", 0, out nodes, out maxDepth, out arrayCount, out nodeCount);
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

        private Dictionary<string, DataType> BuildSchema(List<DataNode> nodes)
        {
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

            foreach (DataNode curr in nodes)
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

        private List<Token> GetTokens(List<DataNode> nodes)
        {
            List<Token> ret = new List<Token>();
            _TextParser.MinimumTokenLength = MinimumTokenLength;

            foreach (DataNode curr in nodes)
            {
                if (curr.Data == null) continue;
                if (String.IsNullOrEmpty(curr.Data.ToString())) continue;

                TextParseResult tpr = _TextParser.ParseString(curr.Data.ToString()); 

                foreach (Token currToken in tpr.Tokens)
                {
                    ret = ParserCommon.AddToken(currToken, ret);
                }
            }

            if (ret != null && ret.Count > 0)
            {
                ret = ret.OrderByDescending(u => u.Count).ToList();
            }

            return ret;
        }
         
        #endregion
    }
}
