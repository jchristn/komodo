using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using DatabaseWrapper;
using Komodo.Classes;
using Komodo.Crawler;

namespace Komodo.Parser
{
    /// <summary>
    /// Parser for data in a SQL database (SQL Server, MySQL, PostgreSQL). 
    /// </summary>
    public class SqlParser
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

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SqlParser()
        {
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="options">Parse options.</param>
        public SqlParser(ParseOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _ParseOptions = options;
            _TextParser = new TextParser(_ParseOptions);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Parse data from a DataTable.
        /// </summary>
        /// <param name="dt">DataTable.</param>
        /// <returns>Parse result.</returns>
        public ParseResult Parse(DataTable dt)
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt)); 
            return ProcessSourceContent(dt);
        }

        /// <summary>
        /// Crawl, query, and parse data from a SQL database.
        /// </summary>
        /// <param name="dbSettings">Database settings.</param>
        /// <param name="query">Query to execute.</param>
        /// <returns>Parse result.</returns>
        public ParseResult ParseFromQuery(DbSettings dbSettings, string query)
        {
            if (dbSettings == null) throw new ArgumentNullException(nameof(dbSettings));
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            ParseResult ret = new ParseResult();
            ret.Sql = new ParseResult.SqlParseResult();

            SqlCrawler crawler = new SqlCrawler(dbSettings, query);
            CrawlResult cr = crawler.Get();

            if (!cr.Success)
            {
                ret.Time.End = DateTime.Now.ToUniversalTime();
                return ret;
            }

            return ProcessSourceContent(cr.DataTable);
        }
         
        #endregion

        #region Private-Methods
         
        private ParseResult ProcessSourceContent(DataTable dataTable)
        {
            ParseResult ret = new ParseResult();
            ret.Sql = new ParseResult.SqlParseResult();

            if (dataTable == null || dataTable.Rows.Count < 1)
            { 
                ret.Time.End = DateTime.Now.ToUniversalTime();
                return ret;
            }
             
            List<Dictionary<string, object>> dicts = Common.DataTableToListDictionary(dataTable);
            foreach (Dictionary<string, object> dict in dicts)
            {
                foreach (KeyValuePair<string, object> kvp in dict)
                {
                    ret.Flattened.Add(new DataNode(kvp.Key, kvp.Value, DataNode.TypeFromValue(kvp.Value)));
                }
            }

            ret.Schema = ParserCommon.BuildSchema(ret.Flattened);
            ret.Tokens = ParserCommon.GetTokens(ret.Flattened, _TextParser);
            ret.Sql.Rows = dataTable.Rows.Count;
            ret.Sql.Columns = dataTable.Columns.Count;

            ret.Success = true;
            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret;
        }
          
        #endregion
    }
}
