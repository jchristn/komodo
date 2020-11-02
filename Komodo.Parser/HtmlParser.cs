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
        public HtmlParser()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="options">Parse options.</param>
        public HtmlParser(ParseOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _ParseOptions = options;
            _TextParser = new TextParser(_ParseOptions);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Crawl and parse data from a URL.
        /// </summary>
        /// <param name="url">Source URL.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseFromUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            ParseResult ret = new ParseResult();
            ret.Html = new ParseResult.HtmlParseResult();

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
            ret.Html = new ParseResult.HtmlParseResult();

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
        /// <param name="data">HTML string.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseString(string data)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            return ProcessSourceContent(data);
        }

        /// <summary>
        /// Parse data from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data containing HTML.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            string sourceContent = Encoding.UTF8.GetString(bytes);
            return ProcessSourceContent(sourceContent);
        }
         
        #endregion

        #region Private-Methods

        private ParseResult ProcessSourceContent(string data)
        {
            #region Load-Document

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            ParseResult ret = new ParseResult();
            ret.Html = new ParseResult.HtmlParseResult();

            #endregion

            #region Head

            ret.Html.Head.Title = GetTitle(data);
            ret.Html.Head.MetaDescription = GetMetaDescription(doc);
            ret.Html.Head.MetaKeywords = GetMetaKeywords(doc);
            ret.Html.Head.MetaImageOpengraph = GetMetaImageOpengraph(doc);
            ret.Html.Head.MetaDescriptionOpengraph = GetMetaDescriptionOpengraph(doc);
            ret.Html.Head.MetaVideoTagsOpengraph = GetMetaVideoTagsOpengraph(doc);

            StringBuilder head = new StringBuilder(" ");

            if (!String.IsNullOrEmpty(ret.Html.Head.Title))
                head.Append(" " + ret.Html.Head.Title);

            if (!String.IsNullOrEmpty(ret.Html.Head.MetaDescription))
                head.Append(" " + ret.Html.Head.MetaDescription);

            if (ret.Html.Head.MetaKeywords != null && ret.Html.Head.MetaKeywords.Count > 0)
                head.Append(" " + String.Join(" ", ret.Html.Head.MetaKeywords));

            if (!String.IsNullOrEmpty(ret.Html.Head.MetaDescriptionOpengraph))
                head.Append(" " + ret.Html.Head.MetaDescriptionOpengraph);

            if (ret.Html.Head.MetaVideoTagsOpengraph != null && ret.Html.Head.MetaVideoTagsOpengraph.Count > 0)
                head.Append(" " + String.Join(" ", ret.Html.Head.MetaVideoTagsOpengraph));
            
            ret.Html.Head.Content = head.ToString();
            ret.Html.Head.Tokens = ParserCommon.GetTokens(ret.Html.Head.Content, _ParseOptions);
             
            #endregion

            #region Body

            ret.Html.Body.ImageUrls = GetImageUrls(doc, data);
            ret.Html.Body.Links = GetLinks(doc);
            ret.Html.Body.Content = GetHtmlBody(doc);
            ret.Html.Body.Tokens = ParserCommon.GetTokens(ret.Html.Body.Content, _ParseOptions);

            #endregion

            #region Data
             
            ret.Tokens = new List<Token>();

            long bodyStartingPosition = 0; 
            if (ret.Html.Head.Tokens != null && ret.Html.Head.Tokens.Count > 0)
            {
                ret.Tokens.AddRange(ret.Html.Head.Tokens);

                foreach (Token token in ret.Html.Head.Tokens)
                { 
                    if (token.Positions != null && token.Positions.Count > 0)
                    {
                        long maxPos = token.Positions.Max(); 

                        if (maxPos >= bodyStartingPosition)
                        { 
                            bodyStartingPosition = (maxPos + 1);
                        }
                    }
                }
            }

            // bodyStartingPosition + [body token position] will yield the correct position across the entire set of tokens

            if (ret.Html.Body.Tokens != null && ret.Html.Body.Tokens.Count > 0)
            {
                List<Token> updatedTokens = new List<Token>();

                foreach (Token token in ret.Html.Body.Tokens)
                {
                    Token updated = new Token();
                    updated.Value = token.Value;
                    updated.Count = token.Count;
                    updated.Positions = new List<long>();

                    if (token.Positions != null  && token.Positions.Count > 0)
                    {
                        foreach (long tokenPos in token.Positions)
                        {
                            long updatedPosition = bodyStartingPosition + tokenPos;
                            updated.Positions.Add(updatedPosition);
                        }
                    }

                    updatedTokens.Add(updated);

                    ret.Html.Body.Tokens = updatedTokens;
                }

                foreach (Token token in ret.Html.Body.Tokens)
                {
                    ret.Tokens = ParserCommon.AddToken(ret.Tokens, token);
                }
            }
             
            ret.Schema = BuildSchema();

            #endregion

            ret.Success = true;
            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret;
        }
         
        private Dictionary<string, DataType> BuildSchema()
        {
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();
             
            // Head
            ret.Add("Html.Head.Title", DataType.String);
            ret.Add("Html.Head.MetaDescription", DataType.String);
            ret.Add("Html.Head.MetaKeywords", DataType.String);
            ret.Add("Html.Head.MetaImageOpengraph", DataType.String);
            ret.Add("Html.Head.MetaDescriptionOpengraph", DataType.String);
            ret.Add("Html.Head.MetaVideoTagsOpengraph", DataType.Array);
            ret.Add("Html.Head.Tokens", DataType.Array);
            ret.Add("Html.Head.Content", DataType.String);

            // Body
            ret.Add("Html.Body.ImageUrls", DataType.Array);
            ret.Add("Html.Body.Links", DataType.Array);
            ret.Add("Html.Body.Tokens", DataType.Array);
            ret.Add("Html.Body.Content", DataType.String);
             
            return ret;
        } 

        private string GetTitle(string data)
        {
            Match m = Regex.Match(data, @"<title>\s*(.+?)\s*</title>");
            if (m != null && m.Success) return WebUtility.UrlDecode(m.Groups[1].Value);
            else return null;
        }

        private string GetMetaValue(HtmlDocument doc, string prop)
        {
            if (doc == null || doc.DocumentNode == null) return null;
            if (String.IsNullOrEmpty(prop)) return null;

            try
            {
                if (doc != null && doc.DocumentNode != null)
                {
                    HtmlNode node = doc.DocumentNode.SelectSingleNode(prop);
                    if (node != null)
                    {
                        HtmlAttribute attr = node.Attributes["content"];
                        if (attr != null) return WebUtility.UrlDecode(attr.Value);
                    }
                }
            }
            catch (Exception)
            {

            }

            return null;
        }

        private string GetMetaDescription(HtmlDocument doc)
        {
            return GetMetaValue(doc, "//meta[@name='description']"); 
        }

        private List<string> GetMetaKeywords(HtmlDocument doc)
        {
            List<string> ret = new List<string>();
            string keywords = GetMetaValue(doc, "//meta[@name='keywords']");
            if (!String.IsNullOrEmpty(keywords))
            {
                keywords = keywords
                    .Replace(",", " ")
                    .Replace(";", " ")
                    .Replace("-", " ")
                    .Replace("_", " ")
                    .Replace("/", " ");

                while (keywords.Contains("  ")) keywords = keywords.Replace("  ", " ");
                string[] parts = keywords.Split(' ');
                if (parts != null && parts.Length > 0) ret = parts.ToList();
            }

            return ret;
        }

        private string GetMetaImageOpengraph(HtmlDocument doc)
        {
            return GetMetaValue(doc, "//meta[@property='og:image']"); 
        }

        private string GetMetaDescriptionOpengraph(HtmlDocument doc)
        {
            return GetMetaValue(doc, "//meta[@property='og:description']"); 
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
            }
            catch (Exception)
            {

            }

            if (ret != null && ret.Count > 0) ret = ret.Distinct().ToList();
            return ret;
        }

        private List<string> GetImageUrls(HtmlDocument doc, string data)
        {
            List<string> links = new List<string>();

            // 
            // using regex
            //
            string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
            MatchCollection matches = Regex.Matches(data, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            
            if (matches != null && matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    string link = m.Groups[1].Value;
                    if (!String.IsNullOrEmpty(link)) link = link.Trim();
                    // link = FormatUrl(link.Trim());

                    string formatted = WebUtility.UrlDecode(link);
                    if (!String.IsNullOrEmpty(formatted)) links.Add(formatted);
                }
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
            if (links != null && links.Count > 0) links = links.Distinct().ToList();
            return links;
        }

        private List<string> GetLinks(HtmlDocument doc)
        {
            List<string> links = new List<string>();

            //
            // using HtmlAgilityPack
            //
            if (doc != null && doc.DocumentNode != null)
            {
                HtmlNodeCollection col = doc.DocumentNode.SelectNodes("//a[@href]");

                if (col != null && col.Count > 0)
                {
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                    {
                        string curr = link.GetAttributeValue("href", "");
                        // string formatted = FormatUrl(link.GetAttributeValue("href", ""));
                        if (!String.IsNullOrEmpty(curr)) links.Add(curr);
                    }
                }
            }

            //
            // Deduplicate
            //
            if (links != null && links.Count > 0) links = links.Distinct().ToList();
            return links;
        }

        private string GetHtmlHead(HtmlDocument doc)
        {
            string head = null;

            if (doc != null && doc.DocumentNode != null)
            {
                foreach (string node in _ParseOptions.Html.ExcludeFromHead)
                { 
                    IEnumerable<HtmlNode> removeList = doc.DocumentNode.Descendants().Where(n => n.Name == node);
                    if (removeList != null && removeList.Count() > 0)
                    {
                        foreach (HtmlNode removeNode in removeList.ToList())
                        {
                            removeNode.Remove();
                        }
                    }
                }

                head = doc.DocumentNode.SelectSingleNode("//head").InnerText; 
            }

            if (!String.IsNullOrEmpty(head))
            {
                head = WebUtility.HtmlDecode(
                    head.Trim()
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Replace("\t", " ")
                    );

                while (head.Contains("  ")) head = head.Replace("  ", " ");
            }

            return head;
        }

        private string GetHtmlBody(HtmlDocument doc)
        {
            string body = null;

            if (doc != null && doc.DocumentNode != null)
            {
                foreach (string node in _ParseOptions.Html.ExcludeFromBody)
                { 
                    IEnumerable<HtmlNode> removeList = doc.DocumentNode.Descendants().Where(n => n.Name == node);
                    if (removeList != null && removeList.Count() > 0)
                    {
                        foreach (HtmlNode removeNode in removeList.ToList())
                        {
                            removeNode.Remove();
                        }
                    }
                }

                body = doc.DocumentNode.SelectSingleNode("//body").InnerText;
            }

            if (!String.IsNullOrEmpty(body))
            {
                body = WebUtility.HtmlDecode(
                    body.Trim()
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Replace("\t", " ")
                    );

                while (body.Contains("  ")) body = body.Replace("  ", " ");
            }

            return body;
        }
         
        private List<Token> GetTokens(List<string> data)
        {
            if (data == null || data.Count < 1) return new List<Token>(); 
            string token = String.Join(" ", data);
            return GetTokens(token);
        }

        private List<Token> GetTokens(string data)
        {
            if (String.IsNullOrEmpty(data)) return new List<Token>();

            List<string> lines = new List<string>();

            // Using HtmlAgilityPack
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(data);

            HtmlNode root = doc.DocumentNode;
            if (root != null)
            {
                IEnumerable<HtmlNode> nodes = root.DescendantsAndSelf();
                if (nodes != null && nodes.Count() > 0)
                {
                    foreach (HtmlNode node in nodes.ToList())
                    {
                        if (!node.HasChildNodes)
                        {
                            string text = node.InnerText;
                            if (!String.IsNullOrEmpty(text)) text = text.Trim();
                            if (!String.IsNullOrEmpty(text)) lines.Add(text);
                        }
                    }
                }
            }

            List<Token> ret = new List<Token>();
             
            if (lines != null && lines.Count > 0)
            {
                foreach (string line in lines)
                {
                    ParseResult pr = _TextParser.ParseString(line);
                    if (pr != null && pr.Tokens != null && pr.Tokens.Count > 0)
                    {
                        foreach (Token currToken in pr.Tokens)
                        {
                            ret = ParserCommon.AddToken(ret, currToken);
                        }
                    }
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
