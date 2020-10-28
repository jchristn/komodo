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
using Komodo.Classes;
using Komodo.Crawler;
using DataType = Komodo.Classes.DataType;

namespace Komodo.Parser
{
    /// <summary>
    /// Parser for data in a Sqlite database file. 
    /// </summary>
    public class SqliteParser
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

        #endregion

        #region Private-Members
         
        private int _MinimumTokenLength = 3;
        private TextParser _TextParser = new TextParser();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SqliteParser()
        {
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Parse data from a DataTable.
        /// </summary>
        /// <param name="dt">DataTable.</param>
        /// <returns>Parse result.</returns>
        public SqliteParseResult Parse(DataTable dt)
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt));
            SqliteParseResult result = new SqliteParseResult();
            return ProcessSourceContent(dt);
        }

        /// <summary>
        /// Crawl, query, and parse data from a Sqlite database file.
        /// </summary>
        /// <param name="dbSettings">Database settings.</param>
        /// <param name="query">Query to execute.</param>
        /// <returns>Parse result.</returns>
        public SqliteParseResult ParseFromQuery(DbSettings dbSettings, string query)
        {
            if (dbSettings == null) throw new ArgumentNullException(nameof(dbSettings));
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));
              
            SqlCrawler crawler = new SqlCrawler(dbSettings, query);
            SqlCrawlResult crawlResult = crawler.Get();
            SqliteParseResult result = new SqliteParseResult();
            if (!crawlResult.Success) return result;
            return ProcessSourceContent(crawlResult.DataTable);
        }
         
        #endregion

        #region Private-Methods
         
        private SqliteParseResult ProcessSourceContent(DataTable dataTable)
        {
            SqliteParseResult ret = new SqliteParseResult(); 
            if (dataTable == null || dataTable.Rows.Count < 1)
            {
                ret.Rows = 0;
                ret.Columns = 0; 
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

            ret.Schema = BuildSchema(ret.Flattened);
            ret.Tokens = GetTokens(ret.Flattened);
            ret.Rows = dataTable.Rows.Count;
            ret.Columns = dataTable.Columns.Count;

            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        private Dictionary<string, DataType> BuildSchema(List<DataNode> nodes)
        {
            Dictionary<string, DataType> ret = new Dictionary<string, Komodo.Classes.DataType>();

            foreach (DataNode curr in nodes)
            {
                if (ret.ContainsKey(curr.Key))
                {
                    if (ret[curr.Key].Equals("null") && !curr.Type.Equals(DataType.Null))
                    {
                        // replace null with more specific type
                        ret.Remove(curr.Key);
                        ret.Add(curr.Key, curr.Type);
                    }
                    continue;
                }
                else
                {
                    ret.Add(curr.Key, curr.Type);
                }
            }

            return ret;
        }

        private List<Token> GetTokens(List<DataNode> nodes)
        {
            List<Token> ret = new List<Token>();

            foreach (DataNode curr in nodes)
            {
                if (curr.Data == null) continue;
                if (String.IsNullOrEmpty(curr.Data.ToString())) continue;

                _TextParser.MinimumTokenLength = MinimumTokenLength;
                TextParseResult tpr = _TextParser.ParseString(curr.Data.ToString());

                foreach (Token currToken in tpr.Tokens)
                {
                    ret = ParserCommon.AddToken(currToken, ret);
                }
            }

            if (ret != null && ret.Count > 0)
            {
                ret = ret.OrderByDescending(u => u.Count).ToList();
            }

            return ret;
        }
         
        #endregion
    }
}
