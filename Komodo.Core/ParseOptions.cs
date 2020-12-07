using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo
{
    /// <summary>
    /// Options to indicate how the parser should behave.
    /// </summary>
    public class ParseOptions
    {
        #region Public-Members
         
        /// <summary>
        /// HTML-specific parse options.
        /// </summary>
        public TextParseOptions Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value ?? throw new ArgumentNullException(nameof(Text));
            }
        }

        /// <summary>
        /// CSV-specific parse options.
        /// </summary>
        public CsvParseOptions Csv
        {
            get
            {
                return _Csv;
            }
            set
            {
                _Csv = value ?? throw new ArgumentNullException(nameof(Csv));
            }
        }

        /// <summary>
        /// HTML-specific parse options.
        /// </summary>
        public HtmlParseOptions Html
        {
            get
            {
                return _Html;
            }
            set
            {
                _Html = value ?? throw new ArgumentNullException(nameof(Html));
            }
        }

        /// <summary>
        /// JSON-specific parse options.
        /// </summary>
        public JsonParseOptions Json
        {
            get
            {
                return _Json;
            }
            set
            {
                _Json = value ?? throw new ArgumentNullException(nameof(Json));
            }
        }

        /// <summary>
        /// XML-specific parse options.
        /// </summary>
        public XmlParseOptions Xml
        {
            get
            {
                return _Xml;
            }
            set
            {
                _Xml = value ?? throw new ArgumentNullException(nameof(Xml));
            }
        }

        /// <summary>
        /// SQL and Sqlite specific parse options.
        /// </summary>
        public SqlParseOptions Sql
        {
            get
            {
                return _Sql;
            }
            set
            {
                _Sql = value ?? throw new ArgumentNullException(nameof(Sql));
            }
        }
         
        #endregion

        #region Private-Members

        private TextParseOptions _Text = new TextParseOptions();
        private CsvParseOptions _Csv = new CsvParseOptions();
        private HtmlParseOptions _Html = new HtmlParseOptions();
        private JsonParseOptions _Json = new JsonParseOptions();
        private XmlParseOptions _Xml = new XmlParseOptions();
        private SqlParseOptions _Sql = new SqlParseOptions();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ParseOptions()
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
        /// CSV-specific parse options.
        /// </summary>
        public class CsvParseOptions
        { 
            /// <summary>
            /// Delimiter found between rows.
            /// </summary>
            public string RowDelimiter
            {
                get
                {
                    return _RowDelimiter;
                }
                set
                {
                    if (value == null || value.Length < 1) throw new ArgumentNullException(nameof(RowDelimiter));
                    _RowDelimiter = value;
                }
            }

            /// <summary>
            /// Delimiter found between records within a row.
            /// </summary>
            public char ColumnDelimiter
            {
                get
                {
                    return _ColumnDelimiter;
                }
                set
                {
                    _ColumnDelimiter = value;
                }
            }

            /// <summary>
            /// Characters to strip if found at the beginning or end of a value.
            /// </summary>
            public char[] QuoteCharacters { get; set; } = new char[] { '\"', '\'' };

            /// <summary>
            /// Enable or disable trimming whitespace found at the beginning and end of values.
            /// </summary>
            public bool TrimWhitespace = true;

            /// <summary>
            /// The prefix of the column name to use when rows are identified containing more columns than found in the header row.
            /// </summary>
            public string UnknownColumnPrefix
            {
                get
                {
                    return _UnknownColumnPrefix;
                }
                set
                {
                    if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(UnknownColumnPrefix));
                    _UnknownColumnPrefix = value;
                }
            }

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public CsvParseOptions()
            {

            }

            private string _RowDelimiter = Environment.NewLine;
            private char _ColumnDelimiter = ',';
            private string _UnknownColumnPrefix = "_column_";
        }

        /// <summary>
        /// HTML-specific parse options.
        /// </summary>
        public class HtmlParseOptions
        { 
            /// <summary>
            /// List of nodes to exclude from the HTML head.
            /// </summary>
            public List<string> ExcludeFromHead
            {
                get
                {
                    return _ExcludeFromHead;
                }
                set
                {
                    _ExcludeFromHead = value ?? throw new ArgumentNullException(nameof(value));
                }
            }

            /// <summary>
            /// List of nodes to exclude from the HTML body.
            /// </summary>
            public List<string> ExcludeFromBody
            {
                get
                {
                    return _ExcludeFromBody;
                }
                set
                {
                    _ExcludeFromBody = value ?? throw new ArgumentNullException(nameof(value));
                }
            }

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public HtmlParseOptions()
            {

            }

            private List<string> _ExcludeFromHead = new List<string>
            {
                "style",
                "script",
                "#comment"
            };

            private List<string> _ExcludeFromBody = new List<string>
            {
                "style",
                "script",
                "#comment"
            };
        }

        /// <summary>
        /// JSON-specific parse options.
        /// </summary>
        public class JsonParseOptions
        {
            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public JsonParseOptions()
            {

            }
        }

        /// <summary>
        /// SQL and Sqlite parse options.
        /// </summary>
        public class SqlParseOptions
        {
            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public SqlParseOptions()
            {

            }
        }

        /// <summary>
        /// Text-specific parse options.
        /// </summary>
        public class TextParseOptions
        {
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

            /// <summary>
            /// List of stop words to remove when enabled.
            /// </summary> 
            public List<string> StopWords
            {
                get
                {
                    return _StopWords;
                }
                set
                {
                    _StopWords = value ?? throw new ArgumentNullException(nameof(StopWords));
                }
            }

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public TextParseOptions()
            {

            }

            private int _MinimumTokenLength = 3;
            private char[] _SplitCharacters = new char[]
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
        }

        /// <summary>
        /// XML-specific parse options.
        /// </summary>
        public class XmlParseOptions
        { 
            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public XmlParseOptions()
            {

            }
        }

        #endregion
    }
}
