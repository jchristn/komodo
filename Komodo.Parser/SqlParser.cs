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
        public TextParser TextParser = new TextParser();

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

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SqlParser()
        {
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Parse data from a DataTable.
        /// </summary>
        /// <param name="dt">DataTable.</param>
        /// <returns>Parse result.</returns>
        public SqlParseResult Parse(DataTable dt)
        {
            if (dt == null) throw new ArgumentNullException(nameof(dt));
            SqlParseResult result = new SqlParseResult();
            return ProcessSourceContent(dt);
        }

        /// <summary>
        /// Crawl, query, and parse data from a SQL database.
        /// </summary>
        /// <param name="dbSettings">Database settings.</param>
        /// <param name="query">Query to execute.</param>
        /// <returns>Parse result.</returns>
        public SqlParseResult ParseFromQuery(DbSettings dbSettings, string query)
        {
            if (dbSettings == null) throw new ArgumentNullException(nameof(dbSettings));
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));
              
            SqlCrawler crawler = new SqlCrawler(dbSettings, query);
            SqlCrawlResult crawlResult = crawler.Get();
            SqlParseResult result = new SqlParseResult();
            if (!crawlResult.Success) return result;
            return ProcessSourceContent(crawlResult.DataTable);
        }
         
        #endregion

        #region Private-Methods
         
        private SqlParseResult ProcessSourceContent(DataTable dataTable)
        {
            SqlParseResult ret = new SqlParseResult(); 
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
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

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

        private List<string> GetTokens(List<DataNode> nodes)
        {
            List<string> ret = new List<string>();

            foreach (DataNode curr in nodes)
            {
                if (curr.Data == null) continue;
                if (String.IsNullOrEmpty(curr.Data.ToString())) continue;

                TextParser.MinimumTokenLength = MinimumTokenLength;
                TextParseResult tpr = TextParser.ParseString(curr.Data.ToString());

                foreach (string currToken in tpr.Tokens)
                {
                    ret.Add(currToken.ToLower());
                }
            }

            ret = ret.Distinct().ToList();
            return ret;
        }

        #endregion
    }
}
