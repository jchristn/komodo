using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text; 
using System.Xml.Linq;  
using XmlToPox;
using Komodo.Classes;
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
        public TextParser TextParser = new TextParser();

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

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public XmlParser()
        {
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Crawl and parse data from a URL.
        /// </summary>
        /// <param name="url">Source URL.</param>
        /// <returns>Parse result.</returns>
        public XmlParseResult ParseFromUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            HttpCrawler crawler = new HttpCrawler(url);
            XmlParseResult result = new XmlParseResult();
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
        public XmlParseResult ParseFromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            FileCrawler crawler = new FileCrawler(filename);
            XmlParseResult result = new XmlParseResult();
            FileCrawlResult crawlResult = crawler.Get();
            if (!crawlResult.Success) return result;
            byte[] sourceData = crawlResult.Data;
            string sourceContent = Encoding.UTF8.GetString(sourceData);
            return ProcessSourceContent(sourceContent);
        }

        /// <summary>
        /// Parse data from a string.
        /// </summary>
        /// <param name="data">XML string.</param>
        /// <returns>Parse result.</returns>
        public XmlParseResult ParseString(string data)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            return ProcessSourceContent(data);
        }

        /// <summary>
        /// Parse data from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data containing XML.</param>
        /// <returns>Parse result.</returns>
        public XmlParseResult ParseBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            string sourceContent = Encoding.UTF8.GetString(bytes);
            return ProcessSourceContent(sourceContent);
        }

        #endregion

        #region Private-Methods

        private XmlParseResult ProcessSourceContent(string content)
        {
            int maxDepth = 0;
            int nodeCount = 0;
            int containerCount = 0;

            string pox = XmlTools.Convert(content);
            XElement xe = XElement.Parse(pox);

            XmlParseResult ret = new XmlParseResult();

            ret.Flattened = Flatten(xe, out maxDepth, out nodeCount, out containerCount);
            ret.MaxDepth = maxDepth;
            ret.NodeCount = nodeCount;
            ret.ContainerCount = containerCount;

            ret.Schema = BuildSchema(ret.Flattened);
            ret.Tokens = GetTokens(ret.Flattened);

            ret.NodeCount = ret.Flattened.Count;
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        private List<DataNode> Flatten(XElement xe, out int maxDepth, out int nodeCount, out int containerCount)
        {
            maxDepth = 1;
            nodeCount = 0;
            containerCount = 0;

            List<DataNode> nodes = new List<DataNode>();
            ExtractTokensAndValues(xe, "", 0, out nodes, out maxDepth, out nodeCount, out containerCount);
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

        private List<string> GetTokens(List<DataNode> nodes)
        {
            List<string> ret = new List<string>();

            foreach (DataNode curr in nodes)
            {
                if (curr.Data == null) continue;
                if (String.IsNullOrEmpty(curr.Data.ToString())) continue;

                TextParser.MinimumTokenLength = MinimumTokenLength;
                TextParseResult tpr = TextParser.ParseString(curr.Data.ToString());

                foreach (string currToken in tpr.Tokens)
                {
                    ret.Add(currToken.ToLower());
                }
            }

            ret = ret.Distinct().ToList();
            return ret;
        }

        #endregion 
    }
}
