using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks; 
using HtmlAgilityPack;
using NUglify;
using RestWrapper;

namespace Komodo.Core
{
    /// <summary>
    /// Parsed HTML document.
    /// </summary>
    public class ParsedHtml
    {
        #region Public-Members
         
        /// <summary>
        /// The title of the HTML page.
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// The meta description of the HTML page.
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// The opengraph meta description of the HTML page.
        /// </summary>
        public string MetaDescriptionOpengraph { get; set; }

        /// <summary>
        /// The meta keywords of the HTML page.
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// The opengraph meta image of the HTML page.
        /// </summary>
        public string MetaImageOpengraph { get; set; }

        /// <summary>
        /// List of opengraph meta video tags of the HTML page.
        /// </summary>
        public List<string> MetaVideoTagsOpengraph { get; set; }

        /// <summary>
        /// Image URLs from the HTML page.
        /// </summary>
        public List<string> ImageUrls { get; set; }

        /// <summary>
        /// Links from the HTML page.
        /// </summary>
        public List<string> Links { get; set; }
         
        /// <summary>
        /// Full HTML head content.
        /// </summary>
        public string Head { get; set; }

        /// <summary>
        /// Full HTML body content.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Full HTML body content with tags stripped.
        /// </summary>
        public string BodyStripped { get; set; }

        /// <summary>
        /// List of tokens found in the HTML body.
        /// </summary>
        public List<string> Tokens { get; set; }
        
        /// <summary>
        /// Schema of the HTML document.
        /// </summary>
        public Dictionary<string, DataType> Schema { get; set; }

        #endregion

        #region Private-Members

        private string _SourceUrl { get; set; }
        private string _SourceContent { get; set; }
        private HtmlDocument _HtmlDoc { get; set; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the ParsedHtml object.
        /// </summary>
        public ParsedHtml()
        {
            _HtmlDoc = new HtmlDocument();
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
            _HtmlDoc.LoadHtml(_SourceContent);
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
            _SourceContent = Encoding.UTF8.GetString(resp.Data);
            _SourceUrl = url;
            _HtmlDoc.LoadHtml(_SourceContent);
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
            _HtmlDoc.LoadHtml(_SourceContent);
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
            _HtmlDoc.LoadHtml(_SourceContent);
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
            ret += "  PageTitle          : " + PageTitle + Environment.NewLine;
            ret += "  MetaDescription    : " + MetaDescription + Environment.NewLine;
            ret += "  MetaDescription OG : " + MetaDescriptionOpengraph + Environment.NewLine;
            ret += "  MetaKeywords       : " + MetaKeywords + Environment.NewLine;
            ret += "  MetaImage OG       : " + MetaImageOpengraph + Environment.NewLine;

            if (MetaVideoTagsOpengraph != null && MetaVideoTagsOpengraph.Count > 0)
            {
                ret += "  MetaVideoTags OG   : " + MetaVideoTagsOpengraph.Count + Environment.NewLine;
                foreach (string curr in MetaVideoTagsOpengraph) ret += "    " + curr + Environment.NewLine;
            }

            if (ImageUrls != null && ImageUrls.Count > 0)
            {
                ret += "  ImageUrls          : " + ImageUrls.Count + Environment.NewLine;
                foreach (string curr in ImageUrls) ret += "    " + curr + Environment.NewLine;
            }

            if (Links != null && Links.Count > 0)
            {
                ret += "  Links              : " + Links.Count + Environment.NewLine;
                foreach (string curr in Links) ret += "    " + curr + Environment.NewLine;
            }

            ret += "  Head               : " + Head.Length + " characters" + Environment.NewLine;
            ret += "  Body               : " + Body.Length + " characters" + Environment.NewLine;
            ret += "  BodyStripped       : " + BodyStripped.Length + " characters" + Environment.NewLine;

            if (Tokens != null && Tokens.Count > 0)
            {
                ret += "  Tokens             : " + Tokens.Count + Environment.NewLine;
                foreach (string curr in Tokens) ret += "    " + curr + Environment.NewLine;
            }

            ret += "---";
            return ret;
        }

        #endregion

        #region Private-Methods

        private bool ProcessSourceContent()
        {
            // 
            // Load HTML Document
            //
            _HtmlDoc = new HtmlDocument();
            _HtmlDoc.LoadHtml(_SourceContent);

            //
            // Metadata
            //
            PageTitle = GetPageTitle();
            MetaDescription = GetMetaDescription();
            MetaKeywords = GetMetaKeywords();
            MetaImageOpengraph = GetMetaImageOpengraph();
            MetaDescriptionOpengraph = GetMetaDescriptionOpengraph();
            MetaVideoTagsOpengraph = GetMetaVideoTagsOpengraph();
            ImageUrls = GetImageUrls();
            Links = GetLinks();

            //
            // Data
            //
            Head = GetHtmlHead();
            Body = GetHtmlBody();
            BodyStripped = GetHtmlBodyStripped();
            Tokens = GetTokens();

            return true;
        }

        private void BuildSchema()
        {
            Schema = new Dictionary<string, DataType>();

            // Metadata
            Schema.Add("PageTitle", DataType.String);
            Schema.Add("MetaDescription", DataType.String);
            Schema.Add("MetaDescriptionOpengraph", DataType.String);
            Schema.Add("MetaKeywords", DataType.String);
            Schema.Add("MetaImageOpengraph", DataType.String);
            Schema.Add("MetaVideoTagsOpengraph", DataType.Array);
            Schema.Add("ImageUrls", DataType.Array);
            Schema.Add("Links", DataType.Array);

            // Data
            Schema.Add("Head", DataType.String);
            Schema.Add("Body", DataType.String);
            Schema.Add("BodyStripped", DataType.String);
            Schema.Add("Tokens", DataType.Array); 
        }

        private string GetPageTitle()
        {
            Match m = Regex.Match(_SourceContent, @"<title>\s*(.+?)\s*</title>");
            if (m.Success)
                return WebUtility.UrlDecode(m.Groups[1].Value);
            else return "";
        }

        private string GetMetaDescription()
        {
            HtmlNode mdnode = _HtmlDoc.DocumentNode.SelectSingleNode("//meta[@name='description']");
            if (mdnode != null)
            {
                HtmlAttribute desc = mdnode.Attributes["content"];
                return WebUtility.UrlDecode(desc.Value);
            }

            return null;
        }

        private string GetMetaKeywords()
        {
            try
            {
                return WebUtility.UrlDecode(_HtmlDoc.DocumentNode.SelectSingleNode("//meta[@name='keywords']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetMetaImageOpengraph()
        {
            try
            {
                return WebUtility.UrlDecode(_HtmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetMetaDescriptionOpengraph()
        {
            try
            {
                return WebUtility.UrlDecode(_HtmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:description']").Attributes["content"].Value);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<string> GetMetaVideoTagsOpengraph()
        {
            List<string> ret = new List<string>();

            try
            {
                var metas = _HtmlDoc.DocumentNode.SelectNodes("//meta[@property='og:video:tag']");
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

        private List<string> GetImageUrls()
        {
            List<string> links = new List<string>();
             
            // 
            // using regex
            //
            string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
            MatchCollection matchesImgSrc = Regex.Matches(_SourceContent, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in matchesImgSrc)
            {
                string href = m.Groups[1].Value;
                href = FormatUrl(href.Trim());
                
                string formatted = FormatUrl(WebUtility.UrlDecode(href));
                if (!String.IsNullOrEmpty(formatted)) links.Add(formatted);
            }

            //
            // using opengraph
            //
            string openGraphImage = GetMetaImageOpengraph();
            if (!String.IsNullOrEmpty(openGraphImage))
            {
                string formatted = FormatUrl(openGraphImage);
                if (!String.IsNullOrEmpty(formatted)) links.Add(formatted);
            }

            //
            // deduplicate
            //
            links = links.Distinct().ToList();
            return links;
        }

        private List<string> GetLinks()
        {
            List<string> links = new List<string>();

            //
            // using HtmlAgilityPack
            //
            foreach (HtmlNode link in _HtmlDoc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string formatted = FormatUrl(link.GetAttributeValue("href", ""));
                if (!String.IsNullOrEmpty(formatted)) links.Add(formatted);
            }

            //
            // Deduplicate
            //
            links = links.Distinct().ToList();
            return links;
        }

        private string GetHtmlHead()
        {
            return _HtmlDoc.DocumentNode.SelectSingleNode("//head").InnerText;
        }

        private string GetHtmlBody()
        {
            return _HtmlDoc.DocumentNode.SelectSingleNode("//body").InnerText;
        }

        private string GetHtmlBodyStripped()
        {
            HtmlDocument removed = new HtmlDocument();
            removed.LoadHtml(_SourceContent);

            var nodes = removed.DocumentNode.SelectNodes("//script|//style");

            foreach (var node in nodes)
                node.ParentNode.RemoveChild(node);

            return removed.DocumentNode.OuterHtml;
        }

        private List<string> GetTokens()
        {
            List<string> ret = new List<string>();
            var result = Uglify.HtmlToText(_SourceContent);
            List<string> unfiltered = result.Code.Split(' ').ToList();

            if (unfiltered != null && unfiltered.Count > 0)
            {
                foreach (string curr in unfiltered)
                {
                    if (String.IsNullOrEmpty(curr)) continue;
                    if (String.IsNullOrWhiteSpace(curr)) continue;

                    string token = "";
                    foreach (char c in curr)
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
            }

            return ret;
        }

        private string FormatUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) return null;

            string formatted = "";

            if (url.StartsWith("http"))
            {
                formatted = url;
            }
            else if (url.StartsWith("//"))
            {
                if (_SourceUrl.EndsWith("/"))
                {
                    formatted = _SourceUrl + url.Substring(2);
                }
                else
                {
                    formatted = _SourceUrl + url.Substring(1);
                }
            }
            else if (url.StartsWith("/"))
            {
                if (_SourceUrl.EndsWith("/"))
                {
                    formatted = _SourceUrl + url.Substring(1);
                }
                else
                {
                    formatted = _SourceUrl + url;
                }
            }
            else if (url.StartsWith("./"))
            {
                if (_SourceUrl.EndsWith("/"))
                {
                    formatted = _SourceUrl + url.Substring(2);
                }
                else
                {
                    formatted = _SourceUrl + "/" + url.Substring(2);
                }
            }
            else
            {
                formatted = null;
            }

            return formatted;
        }

        #endregion 
    }
}
