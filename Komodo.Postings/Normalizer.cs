using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komodo.Classes;
using Komodo.Parser;

namespace Komodo.Postings
{
    /// <summary>
    /// Static class for normalizing content.
    /// </summary>
    public static class Normalizer
    {
        #region Public-Methods

        #region Normalize-Parsed-Objects

        /// <summary>
        /// Normalize parsed HTML.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="data">Parsed HTML.</param>
        /// <returns>Normalized parsed HTML.</returns>
        public static HtmlParseResult NormalizeHtml(PostingsOptions options, HtmlParseResult data)
        {
            if (options == null) options = new PostingsOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            HtmlParseResult ret = Common.CopyObject<HtmlParseResult>(data);
            if (options.NormalizeCase)
            {
                ret.PageTitle = NormalizeCase(ret.PageTitle);
                ret.MetaDescription = NormalizeCase(ret.MetaDescription);
                ret.MetaDescriptionOpengraph = NormalizeCase(ret.MetaDescriptionOpengraph);
                ret.MetaKeywords = NormalizeCase(ret.MetaKeywords);
                ret.MetaImageOpengraph = NormalizeCase(ret.MetaImageOpengraph);
                ret.MetaVideoTagsOpengraph = NormalizeCase(ret.MetaVideoTagsOpengraph);
                ret.ImageUrls = NormalizeCase(ret.ImageUrls);
                ret.Links = NormalizeCase(ret.Links);
                ret.Head = NormalizeCase(ret.Head);
                ret.Body = NormalizeCase(ret.Body);
                ret.BodyStripped = NormalizeCase(ret.BodyStripped);
                ret.Tokens = NormalizeCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.PageTitle = RemoveStringPunctuation(ret.PageTitle);
                ret.MetaDescription = RemoveStringPunctuation(ret.MetaDescription);
                ret.MetaDescriptionOpengraph = RemoveStringPunctuation(ret.MetaDescriptionOpengraph);
                ret.MetaKeywords = RemoveStringPunctuation(ret.MetaKeywords);
                // ret.MetaImageOpengraph = RemoveStringPunctuation(ret.MetaImageOpengraph);
                ret.MetaVideoTagsOpengraph = RemoveStringPunctuation(ret.MetaVideoTagsOpengraph);
                // ret.ImageUrls = RemovePunctuation(ret.ImageUrls);
                // ret.Links = RemovePunctuation(ret.Links);
                ret.Head = RemoveStringPunctuation(ret.Head);
                ret.Body = RemoveStringPunctuation(ret.Body);
                ret.BodyStripped = RemoveStringPunctuation(ret.BodyStripped);
                ret.Tokens = RemovePunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.PageTitle = RemoveStopWords(options, ret.PageTitle);
                ret.MetaDescription = RemoveStopWords(options, ret.MetaDescription);
                ret.MetaDescriptionOpengraph = RemoveStopWords(options, ret.MetaDescriptionOpengraph);
                ret.MetaKeywords = RemoveStopWords(options, ret.MetaKeywords);
                // ret.MetaImageOpengraph = RemoveStringStopWords(options, ret.MetaImageOpengraph);
                ret.MetaVideoTagsOpengraph = RemoveStopWords(options, ret.MetaVideoTagsOpengraph);
                // ret.ImageUrls = RemoveStopWords(options, ret.ImageUrls);
                // ret.Links = RemoveStopWords(options, ret.Links);
                ret.Head = RemoveStopWords(options, ret.Head);
                ret.Body = RemoveStopWords(options, ret.Body);
                ret.BodyStripped = RemoveStopWords(options, ret.BodyStripped);
                ret.Tokens = RemoveStopWords(options, ret.Tokens);
            }

            return ret;
        }

        /// <summary>
        /// Normalize parsed JSON.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="data">Parsed JSON.</param>
        /// <returns>Normalized parsed JSON.</returns>
        public static JsonParseResult NormalizeJson(PostingsOptions options, JsonParseResult data)
        {
            if (options == null) options = new PostingsOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            JsonParseResult ret = Common.CopyObject<JsonParseResult>(data);
            if (options.NormalizeCase)
            {
                ret.Schema = NormalizeCase(ret.Schema);
                ret.Flattened = NormalizeCase(ret.Flattened);
                ret.Tokens = NormalizeCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.Tokens = RemovePunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.Tokens = RemoveStopWords(options, ret.Tokens);
            }

            return ret;
        }

        /// <summary>
        /// Normalize parsed SQL.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="data">Parsed SQL.</param>
        /// <returns>Normalized parsed SQL.</returns>
        public static SqlParseResult NormalizeSql(PostingsOptions options, SqlParseResult data)
        {
            if (options == null) options = new PostingsOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            SqlParseResult ret = Common.CopyObject<SqlParseResult>(data);
            if (options.NormalizeCase)
            {
                ret.Schema = NormalizeCase(ret.Schema);
                ret.Flattened = NormalizeCase(ret.Flattened);
                ret.Tokens = NormalizeCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.Tokens = RemovePunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.Tokens = RemoveStopWords(options, ret.Tokens);
            }

            return ret;
        }

        /// <summary>
        /// Normalize parsed XML.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="data">Parsed XML.</param>
        /// <returns>Normalized parsed XML.</returns>
        public static XmlParseResult NormalizeXml(PostingsOptions options, XmlParseResult data)
        {
            if (options == null) options = new PostingsOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            XmlParseResult ret = Common.CopyObject<XmlParseResult>(data);
            if (options.NormalizeCase)
            {
                ret.Schema = NormalizeCase(ret.Schema);
                ret.Flattened = NormalizeCase(ret.Flattened);
                ret.Tokens = NormalizeCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.Tokens = RemovePunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.Tokens = RemoveStopWords(options, ret.Tokens);
            }

            return ret;
        }

        /// <summary>
        /// Normalize parsed text.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="data">Parsed text.</param>
        /// <returns>Normalized parsed text.</returns>
        public static TextParseResult NormalizeText(PostingsOptions options, TextParseResult data)
        {
            if (options == null) options = new PostingsOptions();
            if (data == null) throw new ArgumentNullException(nameof(data));

            TextParseResult ret = Common.CopyObject<TextParseResult>(data);
            if (options.NormalizeCase)
            {
                ret.Tokens = NormalizeCase(ret.Tokens);
            }

            if (options.RemovePunctuation)
            {
                ret.Tokens = RemovePunctuation(ret.Tokens);
            }

            if (options.RemoveStopWords)
            {
                ret.Tokens = RemoveStopWords(options, ret.Tokens);
            }

            return ret;
        }

        #endregion

        #region Normalize-Case

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <returns>Token with normalized case.</returns>
        public static string NormalizeCase(string token)
        {
            if (String.IsNullOrEmpty(token)) return null;
            return token.ToLower().Trim();
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="tokens">Token.</param>
        /// <returns>Token with normalized case.</returns>
        public static List<Token> NormalizeCase(List<Token> tokens)
        {
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;

            List<Token> ret = new List<Token>();

            foreach (Token token in tokens)
            {
                if (String.IsNullOrEmpty(token.Value)) continue;

                Token updated = new Token();
                updated.Value = token.Value.ToLower().Trim();
                updated.Count = token.Count;
                updated.Positions = token.Positions;
                ret.Add(updated);
            }

            return ret;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="tokens">List of tokens.</param>
        /// <returns>List of tokens with normalized case.</returns>
        public static List<string> NormalizeCase(List<string> tokens)
        {
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;
            List<string> ret = new List<string>();
            foreach (string curr in tokens)
            {
                ret.Add(NormalizeCase(curr));
            }
            return ret;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="tokens">Dictionary where the keys contain tokens.</param>
        /// <returns>Dictionary of tokens with normalized case.</returns>
        public static Dictionary<string, int> NormalizeCase(Dictionary<string, int> tokens)
        {
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;
            Dictionary<string, int> ret = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> curr in tokens)
            {
                AddToDictionary(curr.Key, curr.Value, ret);
            }
            return ret;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="val">Object.</param>
        /// <param name="valType">The data type of the object.</param>
        /// <returns>Object with normalized case.</returns>
        public static object NormalizeCase(object val, DataType valType)
        {
            if (val == null) return null;
            if (valType == DataType.String) return NormalizeCase(val.ToString());
            return val;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary with normalized case.</returns>
        public static Dictionary<string, DataType> NormalizeCase(Dictionary<string, DataType> dict)
        {
            if (dict == null) return null;
            if (dict.Count < 1) return dict;
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();
            foreach (KeyValuePair<string, DataType> curr in dict)
            {
                string key = null;
                if (!String.IsNullOrEmpty(curr.Key)) key = NormalizeCase(curr.Key);
                if (ret.ContainsKey(key)) throw new Exception("Normalizing case would create duplicate keys in supplied dictionary");
                ret.Add(key, curr.Value);
            }
            return ret;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary with normalized case.</returns>
        public static Dictionary<string, string> NormalizeCase(Dictionary<string, string> dict)
        {
            if (dict == null) return null;
            if (dict.Count < 1) return dict;
            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> curr in dict)
            {
                string key = null;
                string val = null;
                if (!String.IsNullOrEmpty(curr.Key)) key = NormalizeCase(curr.Key);
                if (!String.IsNullOrEmpty(curr.Value)) val = NormalizeCase(curr.Value);
                if (ret.ContainsKey(key)) throw new Exception("Normalizing case would create duplicate keys in supplied dictionary");
                ret.Add(key, val);
            }
            return ret;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="nodes">List of data nodes.</param>
        /// <returns>List of data nodes with normalized case.</returns>
        public static List<DataNode> NormalizeCase(List<DataNode> nodes)
        {
            if (nodes == null) return null;
            if (nodes.Count < 1) return nodes;
            List<DataNode> ret = new List<DataNode>();
            foreach (DataNode curr in nodes)
            {
                string key = NormalizeCase(curr.Key);
                object data = NormalizeCase(curr.Data, curr.Type);
                ret.Add(new DataNode(key, data, curr.Type));
            }
            return ret;
        }

        #endregion

        #region Remove-Punctuation

        /// <summary>
        /// Remove punctuation.
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
        /// Remove punctuation.
        /// </summary>
        /// <param name="tokens">List of string tokens.</param>
        /// <returns>List of string tokens without punctuation.</returns>
        public static List<string> RemoveStringPunctuation(List<string> tokens)
        {
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;
            List<string> ret = new List<string>();
            foreach (string curr in tokens)
            {
                ret.Add(RemoveStringPunctuation(curr));
            }
            return ret;
        }

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="tokens">Dictionary containing tokens as keys.</param>
        /// <returns>Dictionary of tokens without punctuation.</returns>
        public static List<Token> RemovePunctuation(List<Token> tokens)
        {
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;
            List<Token> ret = new List<Token>();
            
            foreach (Token token in tokens)
            {
                ret = AddToken(token, ret);
            }

            return ret;
        }

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="val">Object.</param>
        /// <param name="valType">The data type of the object.</param>
        /// <returns>Object without punctuation.</returns>
        public static object RemovePunctuation(object val, DataType valType)
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
        /// Remove punctuation.
        /// </summary>
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary without punctuation.</returns>
        public static Dictionary<string, string> RemovePunctuation(Dictionary<string, string> dict)
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
        /// Remove punctuation.
        /// </summary>
        /// <param name="nodes">List of data nodes.</param>
        /// <returns>List of data nodes without punctuation.</returns>
        public static List<DataNode> RemovePunctuation(List<DataNode> nodes)
        {
            if (nodes == null) return null;
            if (nodes.Count < 1) return nodes;
            List<DataNode> ret = new List<DataNode>();
            foreach (DataNode curr in nodes)
            {
                string key = null;
                object data = null;

                if (!String.IsNullOrEmpty(curr.Key)) key = RemoveStringPunctuation(curr.Key);
                if (curr.Data != null) data = RemovePunctuation(curr.Data, curr.Type);
                ret.Add(new DataNode(key, data, curr.Type));
            }
            return ret;
        }

        #endregion

        #region Remove-Stop-Words

        /// <summary>
        /// Remove stop words.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="token">String.</param>
        /// <returns>String without stop words.</returns>
        public static string RemoveStopWords(PostingsOptions options, string token)
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
        /// Remove stop words.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="tokens">List of strings.</param>
        /// <returns>List of strings without stop words.</returns>
        public static List<string> RemoveStopWords(PostingsOptions options, List<string> tokens)
        {
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;
            List<string> ret = new List<string>();
            foreach (string curr in tokens)
            {
                ret.Add(RemoveStopWords(options, curr));
            }
            return ret;
        }

        /// <summary>
        /// Remove stop words.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="tokens">Dictionary containing tokens as keys.</param>
        /// <returns>Dictionary without stop words.</returns>
        public static List<Token> RemoveStopWords(PostingsOptions options, List<Token> tokens)
        {
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;
            List<Token> ret = new List<Token>();

            foreach (Token token in tokens)
            {
                string updated = RemoveStopWords(options, token.Value);
                if (!String.IsNullOrEmpty(updated))
                {
                    token.Value = updated;
                    ret.Add(token);
                }
            }

            return ret;
        }

        /// <summary>
        /// Remove stop words.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="token">Object.</param>
        /// <param name="tokenType">Data type of the object.</param>
        /// <returns>Object without stop words.</returns>
        public static object RemoveStopWords(PostingsOptions options, object token, DataType tokenType)
        {
            if (token == null) return null;

            if (tokenType == DataType.String)
            {
                return RemoveStopWords(options, token.ToString());
            }
            else
            {
                return token;
            }
        }

        /// <summary>
        /// Remove stop words.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="data">Dictionary.</param>
        /// <returns>Dictionary without stop words.</returns>
        public static Dictionary<string, string> RemoveStopWords(PostingsOptions options, Dictionary<string, string> data)
        {
            if (options == null) return data;
            if (options.StopWords == null || options.StopWords.Count < 1) return data;
            if (data == null) return null;
            if (data.Count < 1) return data;

            Dictionary<string, string> ret = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> curr in data)
            {
                ret.Add(curr.Key, RemoveStopWords(options, curr.Value));
            }

            return ret;
        }

        /// <summary>
        /// Remove stop words.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="data">List of data nodes.</param>
        /// <returns>List of data nodes without stop words.</returns>
        public static List<DataNode> RemoveStopWords(PostingsOptions options, List<DataNode> data)
        {
            if (options == null) return data;
            if (options.StopWords == null || options.StopWords.Count < 1) return data;
            if (data == null || data.Count < 1) return data;

            List<DataNode> ret = new List<DataNode>();
            foreach (DataNode curr in data)
            {
                if (curr.Data != null)
                {
                    ret.Add(new DataNode(curr.Key, RemoveStopWords(options, curr.Data, curr.Type), curr.Type));
                }
                else
                {
                    ret.Add(new DataNode(curr.Key, null, curr.Type));
                }
            }

            return ret;
        }

        #endregion

        #endregion

        #region Private-Methods

        private static void AddToDictionary(string key, int val, Dictionary<string, int> dict)
        {
            if (String.IsNullOrEmpty(key)) return;
            if (dict == null) return;

            if (dict.ContainsKey(key))
            {
                int orig = dict[key];
                val = val + orig;
                dict.Remove(key);
                dict.Add(key, val);
            }
            else
            {
                dict.Add(key, val);
            }
        }

        private static List<Token> AddToken(Token token, List<Token> tokens)
        {
            if (token == null) return tokens;
            if (String.IsNullOrEmpty(token.Value)) return tokens;
            if (tokens == null) tokens = new List<Token>();

            if (tokens.Any(t => t.Value.Equals(token.Value)))
            {
                Token original = tokens.First(t => t.Value.Equals(token.Value));
                Token updated = new Token();
                updated.Value = original.Value;
                updated.Count = original.Count + token.Count;
                updated.Positions = new List<long>();
                if (original.Positions != null && original.Positions.Count > 0) updated.Positions.AddRange(original.Positions);
                if (token.Positions != null && token.Positions.Count > 0) updated.Positions.AddRange(token.Positions);
                if (updated.Positions.Count > 0) updated.Positions = updated.Positions.Distinct().ToList();

                tokens.Remove(original);
                tokens.Add(updated);
            }
            else
            {
                tokens.Add(token);
            }

            return tokens;
        }

        #endregion
    }
}
