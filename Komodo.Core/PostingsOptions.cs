using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Configuration and options for postings processing and terms extraction.
    /// </summary>
    public class PostingsOptions
    {
        #region Public-Members

        /// <summary>
        /// Token length options.
        /// </summary>
        [JsonProperty(Order = -2)]
        public TokenLengthOptions TokenLength
        {
            get
            {
                return _TokenLength;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(TokenLength));
                _TokenLength = value;
            }
        }

        /// <summary>
        /// Token manipulation options.
        /// </summary>
        [JsonProperty(Order = -1)]
        public TokenManipulationOptions TokenManipulation
        {
            get
            {
                return _TokenManipulation;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(TokenManipulation));
                _TokenManipulation = value;
            }
        }

        /// <summary>
        /// Characters on which to split tokens.
        /// </summary>
        [JsonProperty(Order = 990)]
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
        /// Punctuation characters to remove.
        /// </summary>
        [JsonProperty(Order = 991)]
        public char[] PunctuationCharacters
        {
            get
            {
                return _PunctuationCharacters;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(PunctuationCharacters));
                _PunctuationCharacters = value;
            }
        }

        /// <summary>
        /// List of stop words to remove when enabled.
        /// </summary>
        [JsonProperty(Order = 992)]
        public List<string> StopWords
        {
            get
            {
                return _StopWords;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(StopWords));
                _StopWords = value;
            }
        }

        #endregion

        #region Private-Members

        private TokenLengthOptions _TokenLength = new TokenLengthOptions();
        private TokenManipulationOptions _TokenManipulation = new TokenManipulationOptions();

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

        private List<string> _StopWords = new List<string>
        {
            "a",
            "about",
            "above",
            "after",
            "again",
            "against",
            "aint",
            "ain't",
            "all",
            "also",
            "am",
            "an",
            "and",
            "any",
            "are",
            "arent",
            "aren't",
            "as",
            "at",
            "be",
            "because",
            "been",
            "before",
            "being",
            "below",
            "between",
            "both",
            "but",
            "by",
            "can",
            "cant",
            "can't",
            "cannot",
            "could",
            "couldnt",
            "couldn't",
            "did",
            "didnt",
            "didn't",
            "do",
            "does",
            "doesnt",
            "doesn't",
            "doing",
            "dont",
            "don't",
            "down",
            "during",
            "each",
            "few",
            "for",
            "from",
            "further",
            "had",
            "hadnt",
            "hadn't",
            "has",
            "hasnt",
            "hasn't",
            "have",
            "havent",
            "haven't",
            "having",
            "he",
            "hed",
            "he'd",
            "he'll",
            "hes",
            "he's",
            "her",
            "here",
            "heres",
            "here's",
            "hers",
            "herself",
            "him",
            "himself",
            "his",
            "how",
            "hows",
            "how's",
            "i",
            "id",
            "i'd",
            "i'll",
            "im",
            "i'm",
            "ive",
            "i've",
            "if",
            "in",
            "into",
            "is",
            "isnt",
            "isn't",
            "it",
            "its",
            "it's",
            "its",
            "itself",
            "lets",
            "let's",
            "me",
            "more",
            "most",
            "mustnt",
            "mustn't",
            "my",
            "myself",
            "no",
            "nor",
            "not",
            "of",
            "off",
            "on",
            "once",
            "only",
            "or",
            "other",
            "ought",
            "our",
            "ours",
            "ourselves",
            "out",
            "over",
            "own",
            "same",
            "shall",
            "shant",
            "shan't",
            "she",
            "she'd",
            "she'll",
            "shes",
            "she's",
            "should",
            "shouldnt",
            "shouldn't",
            "so",
            "some",
            "such",
            "than",
            "that",
            "thats",
            "that's",
            "the",
            "their",
            "theirs",
            "them",
            "themselves",
            "then",
            "there",
            "theres",
            "there's",
            "these",
            "they",
            "theyd",
            "they'd",
            "theyll",
            "they'll",
            "theyre",
            "they're",
            "theyve",
            "they've",
            "this",
            "those",
            "thou",
            "though",
            "through",
            "to",
            "too",
            "under",
            "until",
            "unto",
            "up",
            "very",
            "was",
            "wasnt",
            "wasn't",
            "we",
            "we'd",
            "we'll",
            "were",
            "we're",
            "weve",
            "we've",
            "werent",
            "weren't",
            "what",
            "whats",
            "what's",
            "when",
            "whens",
            "when's",
            "where",
            "wheres",
            "where's",
            "which",
            "while",
            "who",
            "whos",
            "who's",
            "whose",
            "whom",
            "why",
            "whys",
            "why's",
            "with",
            "wont",
            "won't",
            "would",
            "wouldnt",
            "wouldn't",
            "you",
            "youd",
            "you'd",
            "youll",
            "you'll",
            "youre",
            "you're",
            "youve",
            "you've",
            "your",
            "yours",
            "yourself",
            "yourselves"
        };

        private char[] _PunctuationCharacters = new char[]
        {
            '!',
            '\"',
            '#',
            '$',
            '%',
            '&',
            '\'',
            '(',
            ')',
            '*',
            '+',
            ',',
            '-',
            '.',
            '/',
            ':',
            ';',
            '<',
            '=',
            '>',
            '?',
            '@',
            '[',
            '\\',
            ']',
            '^',
            '_',
            '`',
            '{',
            '|',
            '}',
            '~',
            ' ',
            '\'',
            '\"',
            '\u001a',
            '\r',
            '\n',
            '\t'
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public PostingsOptions()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        #endregion

        #region Private-Methods

        #endregion

        #region Public-Classes

        /// <summary>
        /// Token length requirements.
        /// </summary>
        public class TokenLengthOptions
        {
            /// <summary>
            /// Minimum token length.
            /// </summary>
            public int Min
            {
                get
                {
                    return _Min;
                }
                set
                {
                    if (value < 1) throw new ArgumentException("Minimum token length must be greater than zero.");
                    _Min = value;
                }
            }

            /// <summary>
            /// Maximum token length.
            /// </summary>
            public int Max
            {
                get
                {
                    return _Max;
                }
                set
                {
                    if (value <= _Min) throw new ArgumentException("Maximum token length must be greater than the minimum token length.");
                    _Max = value;
                }
            }

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public TokenLengthOptions()
            {

            }

            private int _Min = 3;
            private int _Max = 32;
        }

        /// <summary>
        /// Token manipulation options.
        /// </summary>
        public class TokenManipulationOptions
        {
            /// <summary>
            /// True to set text to lowercase.
            /// </summary>
            public bool SetLowerCase = true;

            /// <summary>
            /// True to reduce whitespace.
            /// </summary>
            public bool ReduceWhitespace = true;

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
            /// Instantiate the object.
            /// </summary>
            public TokenManipulationOptions()
            {

            }

        }

        #endregion
    }
}
