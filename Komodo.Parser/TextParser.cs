using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Komodo.Classes;
using Komodo.Crawler;

namespace Komodo.Parser
{
    /// <summary>
    /// Parser for text data.
    /// </summary>
    public class TextParser
    {
        #region Public-Members

        /// <summary>
        /// Characters upon which text should be split.
        /// </summary>
        public char[] SplitCharacters
        {
            get
            {
                return _SplitCharacters;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(SplitCharacters));
                _SplitCharacters = value;
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

        private char[] _SplitCharacters = new char[]
        {
            ']',
            '[',
            ',',
            '.',
            ' ',
            '\'',
            '\"',
            ';',
            ':',
            '<',
            '>',
            '.',
            '/',
            '\\',
            '|',
            '{',
            '}',
            '(',
            ')',
            '[',
            ']',
            '<',
            '>',
            '@',
            '&',
            '*',
            '#',
            '-',
            '_',
            '=',
            '\u001a',
            '\r',
            '\n',
            '\t'
        };

        private int _MinimumTokenLength = 3;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public TextParser()
        {
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Crawl and parse data from a URL.
        /// </summary>
        /// <param name="url">Source URL.</param>
        /// <returns>Parse result.</returns>
        public TextParseResult ParseFromUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            HttpCrawler crawler = new HttpCrawler(url);
            TextParseResult result = new TextParseResult();
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
        public TextParseResult ParseFromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            FileCrawler crawler = new FileCrawler(filename);
            TextParseResult result = new TextParseResult();
            FileCrawlResult crawlResult = crawler.Get();
            if (!crawlResult.Success) return result;
            byte[] sourceData = crawlResult.Data;
            string sourceContent = Encoding.UTF8.GetString(sourceData);
            return ProcessSourceContent(sourceContent);
        }

        /// <summary>
        /// Parse data from a string.
        /// </summary>
        /// <param name="data">Text string.</param>
        /// <returns>Parse result.</returns>
        public TextParseResult ParseString(string data)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            return ProcessSourceContent(data);
        }

        /// <summary>
        /// Parse data from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data containing text.</param>
        /// <returns>Parse result.</returns>
        public TextParseResult ParseBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes));
            string sourceContent = Encoding.UTF8.GetString(bytes);
            return ProcessSourceContent(sourceContent);
        }

        #endregion

        #region Private-Methods

        private TextParseResult ProcessSourceContent(string data)
        {
            TextParseResult ret = new TextParseResult();
            ret.Tokens = GetTokens(data);
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        private List<Token> GetTokens(string data)
        {
            List<Token> ret = new List<Token>();
            List<string> temp = new List<string>();

            temp = new List<string>(data.Split(_SplitCharacters, StringSplitOptions.RemoveEmptyEntries)); 

            if (temp != null && temp.Count > 0)
            {
                for (int i = 0; i < temp.Count; i++) 
                {
                    string tempStr = new string(temp[i].ToCharArray()); 
                    if (!String.IsNullOrEmpty(tempStr))
                    {
                        tempStr = tempStr.Trim();
                        if (!String.IsNullOrEmpty(tempStr))
                        {
                            if (tempStr.Length < _MinimumTokenLength) continue;

                            Token token = new Token();
                            token.Value = tempStr;
                            token.Count = 1;
                            token.Positions.Add(i);

                            ret = ParserCommon.AddToken(token, ret);
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
