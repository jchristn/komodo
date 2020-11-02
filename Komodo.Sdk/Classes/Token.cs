using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Komodo.Sdk.Classes
{
    /// <summary>
    /// A token from a parsed document.
    /// </summary>
    public class Token
    {
        #region Public-Members

        /// <summary>
        /// The token.
        /// </summary>
        [JsonProperty(Order = -1)]
        public string Value { get; set; }

        /// <summary>
        /// The frequency with which the token occurs.
        /// </summary>
        public int Count = 0;

        /// <summary>
        /// The positions in the token list where the token appears.
        /// </summary>
        [JsonProperty(Order = 990)]
        public List<long> Positions
        {
            get
            {
                return _Positions;
            }
            set
            {
                if (value == null) _Positions = new List<long>();
                else _Positions = value;
            }
        }

        #endregion

        #region Private-Members

        private List<long> _Positions = new List<long>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the Token.
        /// </summary>
        public Token()
        {

        }

        /// <summary>
        /// Instantiates the Token.
        /// </summary>
        /// <param name="value">The token.</param>
        public Token(string value)
        {
            if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

            Value = value;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
