using System;
using System.Collections.Generic; 
using Komodo.Classes;
using Newtonsoft.Json;

namespace Komodo.Parser
{
    /// <summary>
    /// Parsed text document.
    /// </summary>
    public class TextParseResult
    {
        #region Public-Members

        /// <summary>
        /// Indicates if the parser was successful.
        /// </summary>
        [JsonProperty(Order = -2)]
        public bool Success = false;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        [JsonProperty(Order = -1)]
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// Tokens found including their count and positions.
        /// </summary>
        [JsonProperty(Order = 990)]
        public List<Token> Tokens = new List<Token>();

        #endregion

        #region Private-Members

        private string _SourceContent { get; set; }
        private string _SourceUrl { get; set; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the ParsedText object.
        /// </summary>
        public TextParseResult()
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
            ret += "  Success : " + Success + Environment.NewLine;
            if (Tokens != null && Tokens.Count > 0)
            {
                ret += "  Tokens             : " + Tokens.Count + Environment.NewLine;
                foreach (Token curr in Tokens)
                {
                    ret += "    " + curr.Value + " count " + curr.Count + Environment.NewLine;
                }
            }

            ret += "---";
            return ret;
        }

        #endregion

        #region Private-Methods
         
        #endregion 
    }
}
