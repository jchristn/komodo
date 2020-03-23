using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using RestWrapper;
using Komodo.Classes;

namespace Komodo.Parser
{
    /// <summary>
    /// Parsed text document.
    /// </summary>
    public class HtmlParseResult
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
         
        #endregion

        #region Constructors-and-Factories
         
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public HtmlParseResult()
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
            ret += "  Success            : " + Success + Environment.NewLine; 
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
         
        #endregion 
    }
}
