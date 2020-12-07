using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Parsed document.
    /// </summary>
    public class ParseResult
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
        /// CSV parse details.
        /// </summary>
        public CsvParseResult Csv = null;

        /// <summary>
        /// HTML parse details.
        /// </summary>
        public HtmlParseResult Html = null;

        /// <summary>
        /// XML parse details.
        /// </summary>
        public XmlParseResult Xml = null;

        /// <summary>
        /// JSON parse details.
        /// </summary>
        public JsonParseResult Json = null;

        /// <summary>
        /// SQL parse details.
        /// </summary>
        public SqlParseResult Sql = null;

        /// <summary>
        /// Tokens found including their count and positions.
        /// </summary>
        [JsonProperty(Order = 990)]
        public List<Token> Tokens = new List<Token>();

        /// <summary>
        /// Schema of the document.
        /// </summary>
        [JsonProperty(Order = 991)]
        public Dictionary<string, DataType> Schema = new Dictionary<string, DataType>();

        /// <summary>
        /// Flattened representation of the object.
        /// </summary>
        [JsonProperty(Order = 992)]
        public List<DataNode> Flattened = new List<DataNode>();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ParseResult()
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

        #region Public-Embedded-Classes

        /// <summary>
        /// CSV-specific parse details.
        /// </summary>
        public class CsvParseResult
        {
            /// <summary>
            /// Number of rows within the CSV data.
            /// </summary>
            public int Rows = 0;

            /// <summary>
            /// Number of columns within the CSV data.
            /// </summary>
            public int Columns = 0;

            /// <summary>
            /// Indicates if rows were encountered having an inconsistent number of columns.
            /// </summary>
            public bool Irregular = false;

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public CsvParseResult()
            {

            }
        }

        /// <summary>
        /// HTML-specific parse details.
        /// </summary>
        public class HtmlParseResult
        {
            /// <summary>
            /// HTML body contents and parse results.
            /// </summary>
            [JsonProperty(Order = -1)]
            public HtmlHead Head = new HtmlHead();

            /// <summary>
            /// HTML body contents and parse results.
            /// </summary>
            public HtmlBody Body = new HtmlBody();

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public HtmlParseResult()
            {
            }

            /// <summary>
            /// HTML body contents and parse results.
            /// </summary>
            public class HtmlHead
            {
                /// <summary>
                /// The title of the HTML page.
                /// </summary>
                [JsonProperty(Order = -5)]
                public string Title { get; set; }

                /// <summary>
                /// The meta description of the HTML page.
                /// </summary>
                [JsonProperty(Order = -4)]
                public string MetaDescription { get; set; }

                /// <summary>
                /// The opengraph meta description of the HTML page.
                /// </summary>
                [JsonProperty(Order = -3)]
                public string MetaDescriptionOpengraph { get; set; }

                /// <summary>
                /// The meta keywords of the HTML page.
                /// </summary>
                [JsonProperty(Order = -2)]
                public List<string> MetaKeywords = new List<string>();

                /// <summary>
                /// The opengraph meta image of the HTML page.
                /// </summary>
                [JsonProperty(Order = -1)]
                public string MetaImageOpengraph { get; set; }

                /// <summary>
                /// List of opengraph meta video tags of the HTML page.
                /// </summary>
                [JsonProperty(Order = 990)]
                public List<string> MetaVideoTagsOpengraph = new List<string>();

                /// <summary>
                /// Tokens found including their count and positions.
                /// </summary>
                [JsonProperty(Order = 991)]
                public List<Token> Tokens = new List<Token>();

                /// <summary>
                /// HTML head content.
                /// </summary>
                [JsonProperty(Order = 992)]
                public string Content { get; set; }

                /// <summary>
                /// Instantiate the object.
                /// </summary>
                public HtmlHead()
                {

                }
            } 

            /// <summary>
            /// HTML body contents and parse results.
            /// </summary>
            public class HtmlBody
            { 
                /// <summary>
                /// Image URLs from the HTML page.
                /// </summary>
                [JsonProperty(Order = 990)]
                public List<string> ImageUrls = new List<string>();

                /// <summary>
                /// Links from the HTML page.
                /// </summary>
                [JsonProperty(Order = 991)]
                public List<string> Links = new List<string>();

                /// <summary>
                /// Tokens found including their count and positions.
                /// </summary>
                [JsonProperty(Order = 992)]
                public List<Token> Tokens = new List<Token>();

                /// <summary>
                /// HTML body content.
                /// </summary>
                [JsonProperty(Order = 993)]
                public string Content { get; set; }

                /// <summary>
                /// Instantiate the object.
                /// </summary>
                public HtmlBody()
                {

                }
            }
        }

        /// <summary>
        /// JSON-specific parse details.
        /// </summary>
        public class JsonParseResult
        {
            /// <summary>
            /// Maximum node depth.
            /// </summary>
            [JsonProperty(Order = -2)]
            public int MaxDepth = 0;

            /// <summary>
            /// Number of arrays within the object.
            /// </summary>
            [JsonProperty(Order = -1)]
            public int Arrays = 0;

            /// <summary>
            /// Number of nodes within the object.
            /// </summary>
            public int Nodes = 0;

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public JsonParseResult()
            {

            }
        }

        /// <summary>
        /// SQL and Sqlite specific parse details.
        /// </summary>
        public class SqlParseResult
        { 
            /// <summary>
            /// Number of rows within the table.
            /// </summary>
            [JsonProperty(Order = -1)]
            public int Rows = 0;

            /// <summary>
            /// Number of columns within the table.
            /// </summary>
            public int Columns = 0;

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public SqlParseResult()
            {

            }
        }

        /// <summary>
        /// XML-specific parse details.
        /// </summary>
        public class XmlParseResult
        {
            /// <summary>
            /// Maximum node depth.
            /// </summary>
            [JsonProperty(Order = -2)]
            public int MaxDepth = 0;

            /// <summary>
            /// Number of arrays within the object.
            /// </summary>
            [JsonProperty(Order = -1)]
            public int Arrays = 0;

            /// <summary>
            /// Number of nodes within the object.
            /// </summary>
            public int Nodes = 0;

            /// <summary>
            /// Instantiate the object.
            /// </summary>
            public XmlParseResult()
            {

            }
        }

        #endregion
    }
}
