using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using CsvHelper;
using CsvHelper.Configuration;
using Komodo;
using Komodo.Crawler;
using DataType = Komodo.DataType;

namespace Komodo.Parser
{
    /// <summary>
    /// Parser for data in a comma-separated value file.
    /// </summary>
    public class CsvParser
    {
        #region Public-Members

        /// <summary>
        /// Text parser to use when evaluating text.
        /// </summary>
        public TextParser TextParser
        {
            get
            {
                return _TextParser;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(TextParser));
                _TextParser = value;
            }
        }

        /// <summary>
        /// Parse options.
        /// </summary>
        public ParseOptions ParseOptions
        {
            get
            {
                return _ParseOptions;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(ParseOptions));
                _ParseOptions = value;
            }
        }

        #endregion

        #region Private-Members
         
        private TextParser _TextParser = new TextParser();
        private ParseOptions _ParseOptions = new ParseOptions();
        private CsvConfiguration _CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public CsvParser()
        {
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="options">Parse options.</param>
        public CsvParser(ParseOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _ParseOptions = options;
            _TextParser = new TextParser(_ParseOptions);
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

        /// <summary>
        /// Crawl and parse data from a URL.
        /// </summary>
        /// <param name="url">Source URL.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseFromUrl(string url)
        {
            if (String.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            ParseResult ret = new ParseResult();
            ret.Json = new ParseResult.JsonParseResult();

            HttpCrawler crawler = new HttpCrawler(url);
            CrawlResult cr = crawler.Get();
            if (!cr.Success)
            {
                ret.Time.End = DateTime.Now.ToUniversalTime();
                return ret;
            }

            byte[] sourceData = cr.Data; 
            return ProcessSourceContent(cr.Data);
        }

        /// <summary>
        /// Crawl and parse data from a file.
        /// </summary>
        /// <param name="filename">Path and filename.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseFromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            ParseResult ret = new ParseResult();
            ret.Json = new ParseResult.JsonParseResult();

            FileCrawler crawler = new FileCrawler(filename);
            CrawlResult cr = crawler.Get();
            if (!cr.Success)
            {
                ret.Time.End = DateTime.Now.ToUniversalTime();
                return ret;
            }

            byte[] sourceData = cr.Data; 
            return ProcessSourceContent(cr.Data);
        }

        /// <summary>
        /// Parse data from a string.
        /// </summary>
        /// <param name="data">JSON string.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseString(string data)
        {
            if (String.IsNullOrEmpty(data)) throw new ArgumentNullException(nameof(data));
            return ProcessSourceContent(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// Parse data from a byte array.
        /// </summary>
        /// <param name="bytes">Byte data containing JSON.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseBytes(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException(nameof(bytes)); 
            return ProcessSourceContent(bytes);
        }

        #endregion

        #region Private-Methods

        private ParseResult ProcessSourceContent(byte[] data)
        {
            ParseResult ret = new ParseResult();
            ret.Csv = new ParseResult.CsvParseResult();
            string[] headerNames = null;
            List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
            int rows = 0;
            int columns = 0;

            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(data, 0, data.Length);
                ms.Seek(0, SeekOrigin.Begin);

                using (TextReader tr = new StreamReader(ms))
                {
                    using (CsvHelper.CsvParser cp = new CsvHelper.CsvParser(tr, _CsvConfiguration))
                    { 
                        while (true)
                        {
                            string[] records = cp.Read();

                            if (records != null && records.Length > 0)
                            {
                                if (rows == 0)
                                {
                                    headerNames = records;

                                    List<string> headerNamesList = headerNames.Distinct().ToList();
                                    if (headerNamesList.Count != headerNames.Length)
                                    {
                                        throw new DuplicateNameException("Supplied CSV contains headers that would create duplicate columns.");
                                    }

                                    columns = headerNames.Length;
                                }
                                else
                                {
                                    Dictionary<string, object> dict = new Dictionary<string, object>();

                                    for (int i = 0; i < records.Length; i++)
                                    {
                                        if (headerNames.Length > i && !String.IsNullOrEmpty(headerNames[i]))
                                        {
                                            dict.Add(headerNames[i], records[i]);
                                        }
                                        else
                                        {
                                            dict.Add(_ParseOptions.Csv.UnknownColumnPrefix + i.ToString(), records[i]);
                                        }
                                    }

                                    if (records.Length != columns) ret.Csv.Irregular = true;
                                    if (records.Length > columns) columns = records.Length;
                                    dicts.Add(dict);
                                }

                                rows++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (dicts != null && dicts.Count > 0)
            {
                foreach (Dictionary<string, object> dict in dicts)
                {
                    foreach (KeyValuePair<string, object> kvp in dict)
                    {
                        ret.Flattened.Add(new DataNode(kvp.Key, kvp.Value, DataNode.TypeFromValue(kvp.Value)));
                    }
                }
            }

            ret.Schema = ParserCommon.BuildSchema(ret.Flattened);
            ret.Tokens = ParserCommon.GetTokens(ret.Flattened, _TextParser);
            ret.Csv.Rows = rows;
            ret.Csv.Columns = columns;

            ret.Success = true;
            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret; 
        }
         
        #endregion
    }
}
