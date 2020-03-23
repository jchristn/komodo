using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RestWrapper;
using Komodo.Classes;
using Komodo.Crawler;

namespace Komodo.Parser
{
    /// <summary>
    /// Parser for HTML objects.
    /// </summary>
    public class HtmlParser
    {
        #region Public-Members

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
        public HtmlParser()
        { 
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Crawl and parse data from a URL.
        /// </summary>
        /// <param name="url">Source URL.</param>
        /// <returns>Parse result.</returns>
        public HtmlParseResult ParseFromUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            HttpCrawler crawler = new HttpCrawler(url);
            HtmlParseResult result = new HtmlParseResult();
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
        public HtmlParseResult ParseFromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            FileCrawler crawler = new FileCrawler(filename);
            HtmlParseResult result = new HtmlParseResult();
            FileCrawlResult crawlResult = crawler.Get();
            if (!crawlResult.Success) return result;
            byte[] sourceData = crawlResult.Data;
            string sourceContent = Encoding.UTF8.GetString(sourceData);
            return ProcessSourceContent(sourceContent);
        }

        /// <summary>
        /// Parse data from a string.
        /// </summary>
        /// <param name="data">HTML string.</param>
        /// <returns>Parse result.</returns>
        public HtmlParseResult ParseString(string data)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            return ProcessSourceContent(data);
        }

        /// <summary>
        /// Parse data from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data containing HTML.</param>
        /// <returns>Parse result.</returns>
        public HtmlParseResult ParseBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            string sourceContent = Encoding.UTF8.GetString(bytes);
            return ProcessSourceContent(sourceContent);
        }
         
        #endregion

        #region Private-Methods

        private HtmlParseResult ProcessSourceContent(string data)
        {
            // 
            // Load HTML Document
            //
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);
            HtmlParseResult ret = new HtmlParseResult();

            //
            // Metadata
            //
            ret.PageTitle = GetPageTitle(data);
            ret.MetaDescription = GetMetaDescription(doc);
            ret.MetaKeywords = GetMetaKeywords(doc);
            ret.MetaImageOpengraph = GetMetaImageOpengraph(doc);
            ret.MetaDescriptionOpengraph = GetMetaDescriptionOpengraph(doc);
            ret.MetaVideoTagsOpengraph = GetMetaVideoTagsOpengraph(doc);
            ret.ImageUrls = GetImageUrls(doc, data);
            ret.Links = GetLinks(doc);

            //
            // Data
            //
            ret.Head = GetHtmlHead(doc);
            ret.Body = GetHtmlBody(doc);
            ret.BodyStripped = GetHtmlBodyStripped(data);
            ret.Tokens = GetTokens(ret.BodyStripped);
            ret.Schema = BuildSchema();

            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }
         
        private Dictionary<string, DataType> BuildSchema()
        {
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

            // Metadata
            ret.Add("PageTitle", DataType.String);
            ret.Add("MetaDescription", DataType.String);
            ret.Add("MetaDescriptionOpengraph", DataType.String);
            ret.Add("MetaKeywords", DataType.String);
            ret.Add("MetaImageOpengraph", DataType.String);
            ret.Add("MetaVideoTagsOpengraph", DataType.Array);
            ret.Add("ImageUrls", DataType.Array);
            ret.Add("Links", DataType.Array);

            // Data
            ret.Add("Head", DataType.String);
            ret.Add("Body", DataType.String);
            ret.Add("BodyStripped", DataType.String);
            ret.Add("Tokens", DataType.Array);

            return ret;
        } 

        private string GetPageTitle(string data)
        {
            Match m = Regex.Match(data, @"<title>\s*(.+?)\s*</title>");
            if (m.Success)
                return WebUtility.UrlDecode(m.Groups[1].Value);
            else return "";
        }

        private string GetMetaDescription(HtmlDocument doc)
        {
            HtmlNode mdnode = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
            if (mdnode != null)
            {
                HtmlAttribute desc = mdnode.Attributes["content"];
                return WebUtility.UrlDecode(desc.Value);
            }

            return null;
        }

        private string GetMetaKeywords(HtmlDocument doc)
        {
            try
            {
                return WebUtility.UrlDecode(doc.DocumentNode.SelectSingleNode("//meta[@name='keywords']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetMetaImageOpengraph(HtmlDocument doc)
        {
            try
            {
                return WebUtility.UrlDecode(doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetMetaDescriptionOpengraph(HtmlDocument doc)
        {
            try
            {
                return WebUtility.UrlDecode(doc.DocumentNode.SelectSingleNode("//meta[@property='og:description']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetMetaVideoTagsOpengraph(HtmlDocument doc)
        {
            List<string> ret = new List<string>();

            try
            {
                var metas = doc.DocumentNode.SelectNodes("//meta[@property='og:video:tag']");
                foreach (HtmlNode curr in metas)
                {
                    ret.Add(WebUtility.UrlDecode(curr.Attributes["content"].Value));
                }

                return ret;
            }
            catch (Exception)
            {
                return ret;
            }
        }

        private List<string> GetImageUrls(HtmlDocument doc, string data)
        {
            List<string> links = new List<string>();

            // 
            // using regex
            //
            string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
            MatchCollection matchesImgSrc = Regex.Matches(data, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in matchesImgSrc)
            {
                string link = m.Groups[1].Value;
                if (!String.IsNullOrEmpty(link)) link = link.Trim();
                // link = FormatUrl(link.Trim());

                string formatted = WebUtility.UrlDecode(link);
                if (!String.IsNullOrEmpty(formatted)) links.Add(formatted);
            }

            //
            // using opengraph
            //
            string openGraphImage = GetMetaImageOpengraph(doc);
            if (!String.IsNullOrEmpty(openGraphImage))
            {
                // string formatted = FormatUrl(openGraphImage);
                if (!String.IsNullOrEmpty(openGraphImage)) links.Add(openGraphImage);
            }

            //
            // deduplicate
            //
            links = links.Distinct().ToList();
            return links;
        }

        private List<string> GetLinks(HtmlDocument doc)
        {
            List<string> links = new List<string>();

            //
            // using HtmlAgilityPack
            //
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string curr = link.GetAttributeValue("href", "");
                // string formatted = FormatUrl(link.GetAttributeValue("href", ""));
                if (!String.IsNullOrEmpty(curr)) links.Add(curr);
            }

            //
            // Deduplicate
            //
            links = links.Distinct().ToList();
            return links;
        }

        private string GetHtmlHead(HtmlDocument doc)
        {
            string head = doc.DocumentNode.SelectSingleNode("//head").InnerText;
            if (!String.IsNullOrEmpty(head)) head = head.Trim();
            return head;
        }

        private string GetHtmlBody(HtmlDocument doc)
        {
            string body = doc.DocumentNode.SelectSingleNode("//body").InnerText;
            if (!String.IsNullOrEmpty(body)) body = body.Trim();
            return body;
        }

        private string GetHtmlBodyStripped(string data)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            var nodes = doc.DocumentNode.SelectNodes("//script|//style");

            foreach (var node in nodes)
                node.ParentNode.RemoveChild(node);

            return doc.DocumentNode.OuterHtml;
        }

        private List<string> GetTokens(string data)
        {
            List<string> lines = new List<string>();

            // Using HtmlAgilityPack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            HtmlNode root = doc.DocumentNode;
            foreach (HtmlNode node in root.DescendantsAndSelf())
            {
                if (!node.HasChildNodes)
                {
                    string text = node.InnerText;
                    if (!String.IsNullOrEmpty(text)) text = text.Trim();
                    if (!String.IsNullOrEmpty(text)) lines.Add(text);
                }
            }

            // Using nuglify...
            /*
            UglifyResult result = Uglify.HtmlToText(data);
            string unfiltered = result.Code;

            TextParser.MinimumTokenLength = MinimumTokenLength;
            TextParseResult tpr = TextParser.ParseString(unfiltered);

            foreach (string currToken in tpr.Tokens)
            {
                ret.Add(currToken.ToLower());
            } 
            */

            List<string> ret = new List<string>();
            TextParser tp = new TextParser();
            foreach (string line in lines)
            {
                TextParseResult tpr = tp.ParseString(line);
                if (tpr != null && tpr.Tokens != null && tpr.Tokens.Count > 0)
                {
                    foreach (string curr in tpr.Tokens)
                    {
                        ret.Add(curr);
                    }
                }
            }

            ret = ret.Distinct().ToList();
            return ret; 
        }
         
        #endregion 
    }
}
