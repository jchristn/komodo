using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Komodo;
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
        /// Parse options.
        /// </summary>
        public ParseOptions ParseOptions
        {
            get
            {
                return _ParseOptions;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(ParseOptions));
                _ParseOptions = value;
            }
        }

        #endregion

        #region Private-Members 
         
        private TextParser _TextParser = new TextParser();
        private ParseOptions _ParseOptions = new ParseOptions();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public JsonParser()
        { 
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="options">Parse options.</param>
        public JsonParser(ParseOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _ParseOptions = options;
            _TextParser = new TextParser(_ParseOptions);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        /// <summary>
        /// Crawl and parse data from a URL.
        /// </summary>
        /// <param name="url">Source URL.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseFromUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            ParseResult ret = new ParseResult();
            ret.Json = new ParseResult.JsonParseResult();

            HttpCrawler crawler = new HttpCrawler(url);
            CrawlResult cr = crawler.Get();
            if (!cr.Success)
            {
                ret.Time.End = DateTime.Now.ToUniversalTime();
                return ret;
            }

            byte[] sourceData = cr.Data;
            string sourceContent = Encoding.UTF8.GetString(sourceData);
            return ProcessSourceContent(sourceContent);
        }

        /// <summary>
        /// Crawl and parse data from a file.
        /// </summary>
        /// <param name="filename">Path and filename.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseFromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            ParseResult ret = new ParseResult();
            ret.Json = new ParseResult.JsonParseResult();

            FileCrawler crawler = new FileCrawler(filename);
            CrawlResult cr = crawler.Get();
            if (!cr.Success)
            {
                ret.Time.End = DateTime.Now.ToUniversalTime();
                return ret;
            }

            byte[] sourceData = cr.Data;
            string sourceContent = Encoding.UTF8.GetString(sourceData);
            return ProcessSourceContent(sourceContent);
        }

        /// <summary>
        /// Parse data from a string.
        /// </summary>
        /// <param name="data">JSON string.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseString(string data)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            return ProcessSourceContent(data);
        }

        /// <summary>
        /// Parse data from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data containing JSON.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            string sourceContent = Encoding.UTF8.GetString(bytes);
            return ProcessSourceContent(sourceContent);
        }

        #endregion

        #region Private-Methods

        private ParseResult ProcessSourceContent(string content)
        {
            int maxDepth;
            int arrayCount;
            int nodeCount;

            ParseResult ret = new ParseResult();
            ret.Json = new ParseResult.JsonParseResult();

            JToken jtoken = JToken.Parse(content);

            ret.Flattened = Flatten(jtoken, out maxDepth, out arrayCount, out nodeCount);
            ret.Json.MaxDepth = maxDepth;
            ret.Json.Arrays = arrayCount;
            ret.Json.Nodes = nodeCount; 
            ret.Schema = ParserCommon.BuildSchema(ret.Flattened);
            ret.Tokens = ParserCommon.GetTokens(ret.Flattened, _TextParser); 

            ret.Success = true;
            ret.Time.End = DateTime.Now.ToUniversalTime();
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
          
        #endregion
    }
}
