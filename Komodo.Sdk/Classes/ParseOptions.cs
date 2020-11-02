using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Sdk.Classes
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
                if (value == null) throw new ArgumentNullException(nameof(Text));
                _Text = value;
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
                if (value == null) throw new ArgumentNullException(nameof(Html));
                _Html = value;
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
                if (value == null) throw new ArgumentNullException(nameof(Json));
                _Json = value;
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
                if (value == null) throw new ArgumentNullException(nameof(Xml));
                _Xml = value;
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
                if (value == null) throw new ArgumentNullException(nameof(Sql));
                _Sql = value;
            }
        }

        #endregion

        #region Private-Members

        private TextParseOptions _Text = new TextParseOptions();
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

        #endregion

        #region Private-Methods

        #endregion

        #region Public-Classes

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
                    if (value == null) throw new ArgumentNullException(nameof(value));
                    _ExcludeFromHead = value;
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
                    if (value == null) throw new ArgumentNullException(nameof(value));
                    _ExcludeFromBody = value;
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

        #endregion
    }
}
