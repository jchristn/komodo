using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text; 
using System.Xml.Linq;  
using XmlToPox;
using Komodo;
using Komodo.Crawler;

namespace Komodo.Parser
{
    /// <summary>
    /// Parser for XML data.
    /// </summary>
    public class XmlParser
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
        public XmlParser()
        {
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="options">Parse options.</param>
        public XmlParser(ParseOptions options)
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
            ret.Xml = new ParseResult.XmlParseResult();

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
            ret.Xml = new ParseResult.XmlParseResult();

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
        /// <param name="data">XML string.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseString(string data)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            return ProcessSourceContent(data);
        }

        /// <summary>
        /// Parse data from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data containing XML.</param>
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
            ParseResult ret = new ParseResult();
            ret.Xml = new ParseResult.XmlParseResult();

            int maxDepth = 0;
            int nodes = 0;
            int arrays = 0;

            string pox = XmlTools.Convert(content);
            XElement xe = XElement.Parse(pox);

            ret.Flattened = Flatten(xe, out maxDepth, out nodes, out arrays);
            ret.Xml.MaxDepth = maxDepth;
            ret.Xml.Nodes = nodes;
            ret.Xml.Arrays = arrays;

            ret.Schema = ParserCommon.BuildSchema(ret.Flattened);
            ret.Tokens = ParserCommon.GetTokens(ret.Flattened, _TextParser);
             
            ret.Success = true;
            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret;
        }

        private List<DataNode> Flatten(XElement xe, out int maxDepth, out int nodes, out int arrays)
        {
            maxDepth = 1;
            nodes = 0;
            arrays = 0;

            List<DataNode> dataNodes = new List<DataNode>();
            ExtractTokensAndValues(xe, "", 0, out dataNodes, out maxDepth, out nodes, out arrays);
            return dataNodes;
        }

        private void ExtractTokensAndValues(
            XElement currElement,
            string keyPrepend,
            int currDepth,
            out List<DataNode> dataNodes,
            out int maxDepth,
            out int nodes,
            out int arrays)
        {
            // see http://www.java2s.com/Tutorial/CSharp/0540__XML/LoopThroughXmlDocumentRecursively.htm

            dataNodes = new List<DataNode>();
            maxDepth = currDepth;
            nodes = 0;
            arrays = 0;

            int childCount = currElement.Elements().Count();
            if (childCount > 0)
            {
                #region Container

                arrays++;
                if (!String.IsNullOrEmpty(keyPrepend))
                {
                    dataNodes.Add(new DataNode(keyPrepend + "." + currElement.Name.ToString(), null, DataType.Object));
                }
                else
                {
                    dataNodes.Add(new DataNode(currElement.Name.ToString(), null, DataType.Object));
                }

                foreach (XElement childElement in currElement.Elements())
                {
                    List<DataNode> childDataNodes = new List<DataNode>();
                    int childMaxDepth;
                    int childNodes;
                    int childArrays;

                    if (String.IsNullOrEmpty(keyPrepend))
                    {
                        ExtractTokensAndValues(childElement, currElement.Name.ToString(), currDepth + 1, out childDataNodes, out childMaxDepth, out childNodes, out childArrays);
                    }
                    else
                    {
                        ExtractTokensAndValues(childElement, keyPrepend + "." + currElement.Name.ToString(), currDepth + 1, out childDataNodes, out childMaxDepth, out childNodes, out childArrays);
                    }

                    foreach (DataNode currNode in childDataNodes)
                    {
                        dataNodes.Add(currNode);
                    }

                    if (childMaxDepth > maxDepth) maxDepth = childMaxDepth;
                    arrays += childArrays;
                    nodes += childNodes;
                    nodes++;
                }

                #endregion
            }
            else
            {
                #region Leaf

                nodes++;

                if (!String.IsNullOrEmpty(keyPrepend))
                {
                    dataNodes.Add(new DataNode(keyPrepend + "." + currElement.Name.ToString(), currElement.Value, DataNode.TypeFromValue(currElement.Value)));
                }
                else
                {
                    dataNodes.Add(new DataNode(currElement.Name.ToString(), currElement.Value, DataNode.TypeFromValue(currElement.Value)));
                }

                #endregion
            }
        }
         
        #endregion 
    }
}
