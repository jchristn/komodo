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

namespace Komodo.Core
{
    /// <summary>
    /// Parsed text document.
    /// </summary>
    public class ParsedText
    {
        #region Public-Members

        /// <summary>
        /// List of tokens found within the object.
        /// </summary>
        public List<string> Tokens { get; set; }

        #endregion

        #region Private-Members
        
        private string _SourceContent { get; set; }
        private string _SourceUrl { get; set; }

        private char[] SplitChars = new char[] { ' ', ',', '.', '#', ':', ';', '\'', '\"', '-', '_', '\r', '\n', '(', ')', '&', '?', '/', '[', ']', '{', '}', '|', '*', '<', '>', '\u001a' };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the ParsedText object.
        /// </summary>
        public ParsedText()
        { 
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
                null);

            RestResponse resp = req.Send();
            if (resp == null || resp.StatusCode != 200 || resp.Data == null || resp.ContentLength < 1) return false;
            _SourceContent = Encoding.UTF8.GetString(Common.StreamToBytes(resp.Data));
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
            Tokens = GetTokens();  
            return true;
        }
         
        private List<string> GetTokens()
        {
            List<string> temp = new List<string>();

            temp = new List<string>(_SourceContent.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries));

            if (temp != null && temp.Count > 0)
            {
                List<string> ret = new List<string>();
                foreach (string s in temp)
                {
                    string tempStr = String.Copy(s);
                    if (!String.IsNullOrEmpty(tempStr))
                    {
                        tempStr = tempStr.Trim();
                        if (!String.IsNullOrEmpty(tempStr)) ret.Add(tempStr);
                    }
                }
                return ret;
            }

            return temp;
        }

        #endregion 
    }
}
