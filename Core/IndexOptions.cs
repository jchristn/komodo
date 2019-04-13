using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komodo.Core
{
    /// <summary>
    /// Configuration and options for an index.
    /// </summary>
    public class IndexOptions
    {
        #region Public-Members

        /// <summary>
        /// True to set text to lowercase.
        /// </summary>
        public bool NormalizeCase { get; set; }

        /// <summary>
        /// True to remove punctuation characters.
        /// </summary>
        public bool RemovePunctuation { get; set; }

        /// <summary>
        /// True to remove stopwords.
        /// </summary>
        public bool RemoveStopWords { get; set; }

        /// <summary>
        /// True to perform stemming on tokens.
        /// </summary>
        public bool PerformStemming { get; set; }

        /// <summary>
        /// Minimum length of a token.
        /// </summary>
        public int MinTokenLength { get; set; }

        /// <summary>
        /// Maximum length of a token.
        /// </summary>
        public int MaxTokenLength { get; set; }

        /// <summary>
        /// List of stop words.
        /// </summary>
        public List<string> StopWords { get; set; }

        /// <summary>
        /// Characters on which to split terms.
        /// </summary>
        public string[] SplitCharacters = new string[] { "]", "[", ",", ".", " ", "'", "\"", ";", "<", ">", ".", "/", "\\", "|", "{", "}", "(", ")" };
         
        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the IndexOptions
        /// </summary>
        public IndexOptions()
        {
            NormalizeCase = true;
            RemovePunctuation = true;
            RemoveStopWords = true;
            PerformStemming = true;
            MinTokenLength = 3;
            MaxTokenLength = 64; 

            StopWords = SetDefaultStopWords();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a human readable string of the index options.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "---" + Environment.NewLine;
            ret += "Index Options" + Environment.NewLine;
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
                ret += "  Split Characters   : " + SplitCharacters.Length + Environment.NewLine;
            }
            ret += Environment.NewLine;
            return ret;
        }

        #endregion

        #region Private-Methods

        private List<string> SetDefaultStopWords()
        {
            string[] lines;

            if (File.Exists("StopWords.txt"))
            {    
                try
                {
                    lines = File.ReadAllLines("StopWords.txt");
                    return lines.ToList();
                }
                catch (Exception)
                {
                    return new List<string>();
                }
            }
            else if (Directory.Exists("Docs"))
            {
                if (File.Exists("Docs\\StopWords.txt"))
                {
                    try
                    {
                        lines = File.ReadAllLines("StopWords.txt");
                        return lines.ToList();
                    }
                    catch (Exception)
                    {
                        return new List<string>();
                    }
                }
            }
            else
            {
                return new List<string>();
            }

            return new List<string>();
        }

        #endregion 
    }
}
