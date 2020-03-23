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
    public class TextParseResult
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
        /// List of tokens found within the object.
        /// </summary>
        public List<string> Tokens = new List<string>();

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
            Tokens = new List<string>();
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
                ret += "  Tokens  : " + Tokens.Count + " entries" + Environment.NewLine;
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
         
        #endregion 
    }
}
