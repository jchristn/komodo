using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Configuration and options for postings processing and terms extraction.
    /// </summary>
    public class PostingsOptions
    {
        #region Public-Members

        /// <summary>
        /// True to set text to lowercase.
        /// </summary>
        public bool NormalizeCase = true;

        /// <summary>
        /// True to remove punctuation characters.
        /// </summary>
        public bool RemovePunctuation = true;

        /// <summary>
        /// True to remove stopwords.
        /// </summary>
        public bool RemoveStopWords = true;

        /// <summary>
        /// True to perform stemming on tokens.
        /// </summary>
        public bool PerformStemming = false;

        /// <summary>
        /// Minimum length of a token.
        /// </summary>
        public int MinTokenLength = 3;

        /// <summary>
        /// Maximum length of a token.
        /// </summary>
        public int MaxTokenLength = 32;

        /// <summary>
        /// Characters on which to split terms.
        /// </summary>
        [JsonProperty(Order = 990)]
        public char[] SplitCharacters = new char[] 
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

        /// <summary>
        /// List of stop words.
        /// </summary>
        [JsonProperty(Order = 991)]
        public List<string> StopWords = new List<string>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public PostingsOptions()
        {
            SetStopWordsFromFile();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a human readable string of the object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "---" + Environment.NewLine;
            ret += "Postings Options" + Environment.NewLine;
            ret += "  Normalize Case     : " + NormalizeCase + Environment.NewLine;
            ret += "  Remove Punctuation : " + RemovePunctuation + Environment.NewLine;
            ret += "  Remove Stop Words  : " + RemoveStopWords + Environment.NewLine;
            ret += "  Perform Stemming   : " + PerformStemming + Environment.NewLine;
            ret += "  Min Token Length   : " + MinTokenLength + Environment.NewLine;
            ret += "  Max Token Length   : " + MaxTokenLength + Environment.NewLine;

            if (StopWords != null)
            {
                ret += "  Stop Words         : " + StopWords.Count + Environment.NewLine;
            }

            if (StopWords != null)
            {
                ret += "  Split Characters   : " + SplitCharacters.Length + " characters" + Environment.NewLine;
            }
            ret += Environment.NewLine;
            return ret;
        }

        /// <summary>
        /// Set the stop words using StopWords.txt in the working directory or in the Docs directory, if either exists.
        /// </summary>
        public void SetStopWordsFromFile()
        {
            string[] lines;

            if (File.Exists("StopWords.txt"))
            {
                try
                {
                    lines = File.ReadAllLines("StopWords.txt");
                    StopWords = lines.ToList();
                }
                catch (Exception)
                {
                }
            }
            else if (Directory.Exists("Docs"))
            {
                if (File.Exists("Docs/StopWords.txt"))
                {
                    try
                    {
                        lines = File.ReadAllLines("StopWords.txt");
                        StopWords = lines.ToList();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
