using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomodoCore
{
    /// <summary>
    /// Static class for normalizing content.
    /// </summary>
    public static class Normalizer
    {
        #region Normalize-Parsed-Objects

        /// <summary>
        /// Normalize parsed HTML.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="data">Parsed HTML.</param>
        /// <returns>Normalized parsed HTML.</returns>
        public static ParsedHtml NormalizeHtml(IndexOptions options, ParsedHtml data)
        {
            if (options == null) options = new IndexOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));
            
            ParsedHtml ret = Common.CopyObject<ParsedHtml>(data);
            if (options.NormalizeCase)
            {
                ret.PageTitle = NormalizeStringCase(ret.PageTitle);
                ret.MetaDescription = NormalizeStringCase(ret.MetaDescription);
                ret.MetaDescriptionOpengraph = NormalizeStringCase(ret.MetaDescriptionOpengraph);
                ret.MetaKeywords = NormalizeStringCase(ret.MetaKeywords);
                ret.MetaImageOpengraph = NormalizeStringCase(ret.MetaImageOpengraph);
                ret.MetaVideoTagsOpengraph = NormalizeStringListCase(ret.MetaVideoTagsOpengraph);
                ret.ImageUrls = NormalizeStringListCase(ret.ImageUrls);
                ret.Links = NormalizeStringListCase(ret.Links);
                ret.Head = NormalizeStringCase(ret.Head);
                ret.Body = NormalizeStringCase(ret.Body);
                ret.BodyStripped = NormalizeStringCase(ret.BodyStripped);
                ret.Tokens = NormalizeStringListCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.PageTitle = RemoveStringPunctuation(ret.PageTitle);
                ret.MetaDescription = RemoveStringPunctuation(ret.MetaDescription);
                ret.MetaDescriptionOpengraph = RemoveStringPunctuation(ret.MetaDescriptionOpengraph);
                ret.MetaKeywords = RemoveStringPunctuation(ret.MetaKeywords);
                // ret.MetaImageOpengraph = RemoveStringPunctuation(ret.MetaImageOpengraph);
                ret.MetaVideoTagsOpengraph = RemoveStringListPunctuation(ret.MetaVideoTagsOpengraph);
                // ret.ImageUrls = RemovePunctuation(ret.ImageUrls);
                // ret.Links = RemovePunctuation(ret.Links);
                ret.Head = RemoveStringPunctuation(ret.Head);
                ret.Body = RemoveStringPunctuation(ret.Body);
                ret.BodyStripped = RemoveStringPunctuation(ret.BodyStripped);
                ret.Tokens = RemoveStringListPunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.PageTitle = RemoveStringStopWords(options, ret.PageTitle);
                ret.MetaDescription = RemoveStringStopWords(options, ret.MetaDescription);
                ret.MetaDescriptionOpengraph = RemoveStringStopWords(options, ret.MetaDescriptionOpengraph);
                ret.MetaKeywords = RemoveStringStopWords(options, ret.MetaKeywords);
                // ret.MetaImageOpengraph = RemoveStringStopWords(options, ret.MetaImageOpengraph);
                ret.MetaVideoTagsOpengraph = RemoveStringListStopWords(options, ret.MetaVideoTagsOpengraph);
                // ret.ImageUrls = RemoveStopWords(options, ret.ImageUrls);
                // ret.Links = RemoveStopWords(options, ret.Links);
                ret.Head = RemoveStringStopWords(options, ret.Head);
                ret.Body = RemoveStringStopWords(options, ret.Body);
                ret.BodyStripped = RemoveStringStopWords(options, ret.BodyStripped);
                ret.Tokens = RemoveStringListStopWords(options, ret.Tokens);
            }

            return ret;
        }

        /// <summary>
        /// Normalize parsed JSON.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="data">Parsed JSON.</param>
        /// <returns>Normalized parsed JSON.</returns>
        public static ParsedJson NormalizeJson(IndexOptions options, ParsedJson data)
        {
            if (options == null) options = new IndexOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            ParsedJson ret = Common.CopyObject<ParsedJson>(data);
            if (options.NormalizeCase)
            {
                ret.Schema = NormalizeSchemaCase(ret.Schema);
                ret.Flattened = NormalizeDataNodeCase(ret.Flattened);
                ret.Tokens = NormalizeStringListCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.Tokens = RemoveStringListPunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.Tokens = RemoveStringListStopWords(options, ret.Tokens);
            }

            return ret;
        }

        /// <summary>
        /// Normalize parsed SQL.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="data">Parsed SQL.</param>
        /// <returns>Normalized parsed SQL.</returns>
        public static ParsedSql NormalizeSql(IndexOptions options, ParsedSql data)
        {
            if (options == null) options = new IndexOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            ParsedSql ret = Common.CopyObject<ParsedSql>(data);
            if (options.NormalizeCase)
            {
                ret.Schema = NormalizeSchemaCase(ret.Schema);
                ret.Flattened = NormalizeDataNodeCase(ret.Flattened);
                ret.Tokens = NormalizeStringListCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.Tokens = RemoveStringListPunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.Tokens = RemoveStringListStopWords(options, ret.Tokens);
            }

            return ret;
        }

        /// <summary>
        /// Normalize parsed XML.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="data">Parsed XML.</param>
        /// <returns>Normalized parsed XML.</returns>
        public static ParsedXml NormalizeXml(IndexOptions options, ParsedXml data)
        {
            if (options == null) options = new IndexOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            ParsedXml ret = Common.CopyObject<ParsedXml>(data);
            if (options.NormalizeCase)
            {
                ret.Schema = NormalizeSchemaCase(ret.Schema);
                ret.Flattened = NormalizeDataNodeCase(ret.Flattened);
                ret.Tokens = NormalizeStringListCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.Tokens = RemoveStringListPunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.Tokens = RemoveStringListStopWords(options, ret.Tokens);
            }

            return ret;
        }

        /// <summary>
        /// Normalize parsed text.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="data">Parsed text.</param>
        /// <returns>Normalized parsed text.</returns>
        public static ParsedText NormalizeText(IndexOptions options, ParsedText data)
        {
            if (options == null) options = new IndexOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            ParsedText ret = Common.CopyObject<ParsedText>(data);
            if (options.NormalizeCase)
            {
                ret.Tokens = NormalizeStringListCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.Tokens = RemoveStringListPunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.Tokens = RemoveStringListStopWords(options, ret.Tokens);
            }

            return ret;
        }

        #endregion

        #region Normalize-Case

        /// <summary>
        /// Normalize string case.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <returns>Token with normalized case.</returns>
        public static string NormalizeStringCase(string token)
        {
            if (String.IsNullOrEmpty(token)) return token;
            return token.ToLower();
        }

        /// <summary>
        /// Normalize string case for a list of strings.
        /// </summary>
        /// <param name="tokenList">List of tokens.</param>
        /// <returns>List of tokens with normalized case.</returns>
        public static List<string> NormalizeStringListCase(List<string> tokenList)
        {
            if (tokenList == null) return null;
            if (tokenList.Count < 1) return tokenList;
            List<string> ret = new List<string>();
            foreach (string curr in tokenList)
            {
                ret.Add(NormalizeStringCase(curr));
            }
            return ret;
        }

        /// <summary>
        /// Normalize case for an object.
        /// </summary>
        /// <param name="val">Object.</param>
        /// <param name="valType">The data type of the object.</param>
        /// <returns>Object with normalized case.</returns>
        public static object NormalizeObjectCase(object val, DataType valType)
        {
            if (val == null)
            {
                return null;
            }

            if (valType == DataType.String)
            {
                return NormalizeStringCase(val.ToString());
            }
            else
            {
                return val;
            }
        }

        /// <summary>
        /// Normalize the case for a dictionary.
        /// </summary>
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary with normalized case.</returns>
        public static Dictionary<string, DataType> NormalizeSchemaCase(Dictionary<string, DataType> dict)
        {
            if (dict == null) return null;
            if (dict.Count < 1) return dict;
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();
            foreach (KeyValuePair<string, DataType> curr in dict)
            {
                string key = null;
                if (!String.IsNullOrEmpty(curr.Key)) key = NormalizeStringCase(curr.Key);
                if (ret.ContainsKey(key)) throw new Exception("Normalizing case would create duplicate keys in supplied dictionary");
                ret.Add(key, curr.Value);
            }
            return ret;
        }

        /// <summary>
        /// Normalize the case for a dictionary.
        /// </summary>
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary with normalized case.</returns>
        public static Dictionary<string, string> NormalizeDictionaryCase(Dictionary<string, string> dict)
        {
            if (dict == null) return null;
            if (dict.Count < 1) return dict;
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> curr in dict)
            {
                string key = null;
                string val = null;
                if (!String.IsNullOrEmpty(curr.Key)) key = NormalizeStringCase(curr.Key);
                if (!String.IsNullOrEmpty(curr.Value)) val = NormalizeStringCase(curr.Value);
                if (ret.ContainsKey(key)) throw new Exception("Normalizing case would create duplicate keys in supplied dictionary");
                ret.Add(key, val);
            }
            return ret;
        }

        /// <summary>
        /// Normalize case for a list of data nodes.
        /// </summary>
        /// <param name="nodes">List of data nodes.</param>
        /// <returns>List of data nodes with normalized case.</returns>
        public static List<DataNode> NormalizeDataNodeCase(List<DataNode> nodes)
        {
            if (nodes == null) return null;
            if (nodes.Count < 1) return nodes;
            List<DataNode> ret = new List<DataNode>();
            foreach (DataNode curr in nodes)
            {
                string key = NormalizeStringCase(curr.Key);
                object data = NormalizeObjectCase(curr.Data, curr.Type);
                ret.Add(new DataNode(key, data, curr.Type));
            }
            return ret;
        }

        #endregion

        #region Remove-Punctuation

        /// <summary>
        /// Remove punctuation from a string token.
        /// </summary>
        /// <param name="token">String.</param>
        /// <returns>String without punctuation.</returns>
        public static string RemoveStringPunctuation(string token)
        {
            if (String.IsNullOrEmpty(token)) return token;

            string punctuation = "!\"#$%&\'()*+,-./:;<=>?@[\\]^_`{|}~";
            string ret = "";
            foreach (char c in token)
            {
                if (punctuation.Contains(c))
                {
                    ret += " ";
                }
                else
                {
                    ret += c;
                }
            }
             
            return ret;
        }

        /// <summary>
        /// Remove punctuation from a list of string tokens.
        /// </summary>
        /// <param name="tokenList">List of string tokens.</param>
        /// <returns>List of string tokens without punctuation.</returns>
        public static List<string> RemoveStringListPunctuation(List<string> tokenList)
        {
            if (tokenList == null) return null;
            if (tokenList.Count < 1) return tokenList;
            List<string> ret = new List<string>();
            foreach (string curr in tokenList)
            {
                ret.Add(RemoveStringPunctuation(curr));
            }
            return ret;
        }

        /// <summary>
        /// Remove punctuation from an object.
        /// </summary>
        /// <param name="val">Object.</param>
        /// <param name="valType">The data type of the object.</param>
        /// <returns>Object without punctuation.</returns>
        public static object RemoveObjectPunctuation(object val, DataType valType)
        {
            if (val == null) return null;

            if (valType == DataType.String)
            {
                return RemoveStringPunctuation(val.ToString());
            }
            else
            {
                return val;
            }
        }

        /// <summary>
        /// Remove punctuation from a dictionary.
        /// </summary>
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary without punctuation.</returns>
        public static Dictionary<string, string> RemoveDictionaryPunctuation(Dictionary<string, string> dict)
        {
            if (dict == null) return null;
            if (dict.Count < 1) return dict;
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> curr in dict)
            {
                string key = null;
                string val = null;
                if (!String.IsNullOrEmpty(curr.Key)) key = RemoveStringPunctuation(curr.Key);
                if (!String.IsNullOrEmpty(curr.Value)) val = RemoveStringPunctuation(curr.Value);
                if (ret.ContainsKey(key)) throw new Exception("Removing punctuation would create duplicate keys in supplied dictionary");
                ret.Add(key, val);
            }
            return ret;
        }

        /// <summary>
        /// Remove punctuation from a list of data nodes.
        /// </summary>
        /// <param name="nodes">List of data nodes.</param>
        /// <returns>List of data nodes without punctuation.</returns>
        public static List<DataNode> RemoveDataNodePunctuation(List<DataNode> nodes)
        {
            if (nodes == null) return null;
            if (nodes.Count < 1) return nodes;
            List<DataNode> ret = new List<DataNode>();
            foreach (DataNode curr in nodes)
            {
                string key = null;
                object data = null;

                if (!String.IsNullOrEmpty(curr.Key)) key = RemoveStringPunctuation(curr.Key);
                if (curr.Data != null) data = RemoveObjectPunctuation(curr.Data, curr.Type);
                ret.Add(new DataNode(key, data, curr.Type));
            }
            return ret;
        }

        #endregion

        #region Remove-Stop-Words

        /// <summary>
        /// Remove stop words from a string.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="token">String.</param>
        /// <returns>String without stop words.</returns>
        public static string RemoveStringStopWords(IndexOptions options, string token)
        {
            if (options == null) return token;
            if (options.StopWords == null || options.StopWords.Count < 1) return token;
            if (String.IsNullOrEmpty(token)) return token;

            string[] tokens = token.Split(options.SplitCharacters, StringSplitOptions.None);
            List<string> filteredTokens = new List<string>();
            foreach (string curr in tokens)
            {
                if (options.StopWords.Contains(curr)) continue;
                filteredTokens.Add(curr);
            }

            string ret = "";
            int added = 0;
            foreach (string curr in filteredTokens)
            {
                if (added == 0) ret += curr;
                else ret += " " + curr;
                added++;
            }

            return ret;
        }

        /// <summary>
        /// Remove stop words from a list of strings.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="tokenList">List of strings.</param>
        /// <returns>List of strings without stop words.</returns>
        public static List<string> RemoveStringListStopWords(IndexOptions options, List<string> tokenList)
        {
            if (tokenList == null) return null;
            if (tokenList.Count < 1) return tokenList;
            List<string> ret = new List<string>();
            foreach (string curr in tokenList)
            {
                ret.Add(RemoveStringStopWords(options, curr));
            }
            return ret;
        }

        /// <summary>
        /// Remove stop words from an object.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="token">Object.</param>
        /// <param name="tokenType">Data type of the object.</param>
        /// <returns>Object without stop words.</returns>
        public static object RemoveObjectStopWords(IndexOptions options, object token, DataType tokenType)
        {
            if (token == null) return null;

            if (tokenType == DataType.String)
            {
                return RemoveStringStopWords(options, token.ToString());
            }
            else
            {
                return token;
            }
        }

        /// <summary>
        /// Remove stop words from a dictionary.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="data">Dictionary.</param>
        /// <returns>Dictionary without stop words.</returns>
        public static Dictionary<string, string> RemoveDictionaryStopWords(IndexOptions options, Dictionary<string, string> data)
        {
            if (options == null) return data;
            if (options.StopWords == null || options.StopWords.Count < 1) return data;
            if (data == null) return null;
            if (data.Count < 1) return data;

            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> curr in data)
            {
                ret.Add(curr.Key, RemoveStringStopWords(options, curr.Value));
            }

            return ret;
        }

        /// <summary>
        /// Remove stop words from a list of data nodes.
        /// </summary>
        /// <param name="options">Index options.</param>
        /// <param name="data">List of data nodes.</param>
        /// <returns>List of data nodes without stop words.</returns>
        public static List<DataNode> RemoveDataNodeStopWords(IndexOptions options, List<DataNode> data)
        {
            if (options == null) return data;
            if (options.StopWords == null || options.StopWords.Count < 1) return data;
            if (data == null || data.Count < 1) return data;

            List<DataNode> ret = new List<DataNode>();
            foreach (DataNode curr in data)
            {
                if (curr.Data != null)
                {
                    ret.Add(new DataNode(curr.Key, RemoveObjectStopWords(options, curr.Data, curr.Type), curr.Type));
                }
                else
                {
                    ret.Add(new DataNode(curr.Key, null, curr.Type));
                }
            }

            return ret;
        }

        #endregion 
    }
}
