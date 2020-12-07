using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komodo;
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
        /// Normalize parse result.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="pr">Parse result.</param>
        /// <returns>Normalized parse result.</returns>
        public static ParseResult Normalize(PostingsOptions options, ParseResult pr)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (pr == null) throw new ArgumentNullException(nameof(pr));

            ParseResult ret = Common.CopyObject<ParseResult>(pr); 
            ret.Time = new Timestamps();
            ret.Time.Start = DateTime.Now.ToUniversalTime();

            if (options.TokenManipulation != null)
            {
                #region Token-Manipulation

                if (ret.Html != null)
                {
                    #region HTML

                    if (options.TokenManipulation.RemovePunctuation)
                    {
                        ret.Html.Head.Title = RemovePunctuation(options, ret.Html.Head.Title);
                        ret.Html.Head.MetaDescription = RemovePunctuation(options, ret.Html.Head.MetaDescription);
                        ret.Html.Head.MetaDescriptionOpengraph = RemovePunctuation(options, ret.Html.Head.MetaDescriptionOpengraph);
                        ret.Html.Head.MetaKeywords = RemovePunctuation(options, ret.Html.Head.MetaKeywords);
                        ret.Html.Head.MetaImageOpengraph = RemovePunctuation(options, ret.Html.Head.MetaImageOpengraph);
                        ret.Html.Head.MetaVideoTagsOpengraph = RemovePunctuation(options, ret.Html.Head.MetaVideoTagsOpengraph);
                        ret.Html.Head.Content = RemovePunctuation(options, ret.Html.Head.Content);
                        ret.Html.Head.Tokens = RemovePunctuation(options, ret.Html.Head.Tokens);

                        ret.Html.Body.Content = RemovePunctuation(options, ret.Html.Body.Content);
                        ret.Html.Body.Tokens = RemovePunctuation(options, ret.Html.Body.Tokens); 
                    }

                    if (options.TokenManipulation.SetLowerCase)
                    {
                        ret.Html.Head.Title = NormalizeCase(options, ret.Html.Head.Title);
                        ret.Html.Head.MetaDescription = NormalizeCase(options, ret.Html.Head.MetaDescription);
                        ret.Html.Head.MetaDescriptionOpengraph = NormalizeCase(options, ret.Html.Head.MetaDescriptionOpengraph);
                        ret.Html.Head.MetaKeywords = NormalizeCase(options, ret.Html.Head.MetaKeywords);
                        ret.Html.Head.MetaImageOpengraph = NormalizeCase(options, ret.Html.Head.MetaImageOpengraph);
                        ret.Html.Head.MetaVideoTagsOpengraph = NormalizeCase(options, ret.Html.Head.MetaVideoTagsOpengraph);
                        ret.Html.Head.Content = NormalizeCase(options, ret.Html.Head.Content);
                        ret.Html.Head.Tokens = NormalizeCase(options, ret.Html.Head.Tokens);

                        ret.Html.Body.ImageUrls = NormalizeCase(options, ret.Html.Body.ImageUrls);
                        ret.Html.Body.Links = NormalizeCase(options, ret.Html.Body.Links);
                        ret.Html.Body.Content = NormalizeCase(options, ret.Html.Body.Content);
                        ret.Html.Body.Tokens = NormalizeCase(options, ret.Html.Body.Tokens);
                    }

                    if (options.TokenManipulation.RemoveStopWords)
                    {
                        ret.Html.Head.Title = RemoveStopWords(options, ret.Html.Head.Title);
                        ret.Html.Head.MetaDescription = RemoveStopWords(options, ret.Html.Head.MetaDescription);
                        ret.Html.Head.MetaDescriptionOpengraph = RemoveStopWords(options, ret.Html.Head.MetaDescriptionOpengraph);
                        ret.Html.Head.MetaKeywords = RemoveStopWords(options, ret.Html.Head.MetaKeywords);
                        ret.Html.Head.MetaImageOpengraph = RemoveStopWords(options, ret.Html.Head.MetaImageOpengraph);
                        ret.Html.Head.MetaVideoTagsOpengraph = RemoveStopWords(options, ret.Html.Head.MetaVideoTagsOpengraph);
                        ret.Html.Head.Content = RemoveStopWords(options, ret.Html.Head.Content);
                        ret.Html.Head.Tokens = RemoveStopWords(options, ret.Html.Head.Tokens);

                        ret.Html.Body.Content = RemoveStopWords(options, ret.Html.Body.Content);
                        ret.Html.Body.Tokens = RemoveStopWords(options, ret.Html.Body.Tokens); 
                    }

                    if (options.TokenManipulation.ReduceWhitespace)
                    {
                        ret.Html.Head.Title = ReduceWhitespace(options, ret.Html.Head.Title);
                        ret.Html.Head.MetaDescription = ReduceWhitespace(options, ret.Html.Head.MetaDescription);
                        ret.Html.Head.MetaDescriptionOpengraph = ReduceWhitespace(options, ret.Html.Head.MetaDescriptionOpengraph);
                        ret.Html.Head.MetaKeywords = ReduceWhitespace(options, ret.Html.Head.MetaKeywords);
                        ret.Html.Head.MetaImageOpengraph = ReduceWhitespace(options, ret.Html.Head.MetaImageOpengraph);
                        ret.Html.Head.MetaVideoTagsOpengraph = ReduceWhitespace(options, ret.Html.Head.MetaVideoTagsOpengraph);
                        ret.Html.Head.Content = ReduceWhitespace(options, ret.Html.Head.Content);
                        ret.Html.Head.Tokens = ReduceWhitespace(options, ret.Html.Head.Tokens);

                        ret.Html.Body.ImageUrls = ReduceWhitespace(options, ret.Html.Body.ImageUrls);
                        ret.Html.Body.Links = ReduceWhitespace(options, ret.Html.Body.Links);
                        ret.Html.Body.Content = ReduceWhitespace(options, ret.Html.Body.Content);
                        ret.Html.Body.Tokens = ReduceWhitespace(options, ret.Html.Body.Tokens);
                    }

                    #endregion
                }

                if (options.TokenManipulation.RemovePunctuation)
                {
                    // do not change schema keys
                    // if (ret.Schema != null) ret.Schema = RemovePunctuation(options, ret.Schema);
                    if (ret.Flattened != null) ret.Flattened = RemovePunctuation(options, ret.Flattened);
                    if (ret.Tokens != null) ret.Tokens = RemovePunctuation(options, ret.Tokens);
                }

                if (options.TokenManipulation.SetLowerCase)
                {
                    if (ret.Schema != null) ret.Schema = NormalizeCase(options, ret.Schema);
                    if (ret.Flattened != null) ret.Flattened = NormalizeCase(options, ret.Flattened);
                    if (ret.Tokens != null) ret.Tokens = NormalizeCase(options, ret.Tokens);
                }

                if (options.TokenManipulation.RemoveStopWords)
                {
                    // do not change schema keys
                    // if (ret.Schema != null) ret.Schema = RemoveStopWords(options, ret.Schema);
                    if (ret.Flattened != null) ret.Flattened = RemoveStopWords(options, ret.Flattened);
                    if (ret.Tokens != null) ret.Tokens = RemoveStopWords(options, ret.Tokens);
                }

                if (options.TokenManipulation.ReduceWhitespace)
                {
                    // do not change schema keys
                    // if (ret.Schema != null) ret.Schema = ReduceWhitespace(options, ret.Schema);
                    if (ret.Flattened != null) ret.Flattened = ReduceWhitespace(options, ret.Flattened);
                    if (ret.Tokens != null) ret.Tokens = ReduceWhitespace(options, ret.Tokens);
                }

                #endregion
            }

            if (options.TokenLength != null)
            {
                #region Token-Length

                if (ret.Html != null && ret.Html.Head != null && ret.Html.Head.Tokens != null)
                {
                    List<Token> updated = new List<Token>();

                    foreach (Token token in ret.Html.Head.Tokens)
                    {
                        if (IsWithinLength(token.Value, options.TokenLength.Min, options.TokenLength.Max))
                            updated.Add(token);
                    }

                    ret.Html.Head.Tokens = updated;
                }

                if (ret.Html != null && ret.Html.Body != null && ret.Html.Body.Tokens != null)
                {
                    List<Token> updated = new List<Token>();

                    foreach (Token token in ret.Html.Body.Tokens)
                    {
                        if (IsWithinLength(token.Value, options.TokenLength.Min, options.TokenLength.Max))
                            updated.Add(token);
                    }

                    ret.Html.Body.Tokens = updated;
                }

                if (ret.Tokens != null)
                {
                    List<Token> updated = new List<Token>();

                    foreach (Token token in ret.Tokens)
                    {
                        if (IsWithinLength(token.Value, options.TokenLength.Min, options.TokenLength.Max))
                            updated.Add(token);
                    }

                    ret.Tokens = updated;
                } 

                #endregion
            }

            ret.Time.End = DateTime.Now.ToUniversalTime();
            ret.Success = true;
            return ret;
        }

        #endregion

        #region Normalize-Case

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="token">Token.</param>
        /// <returns>Token with normalized case.</returns>
        public static string NormalizeCase(PostingsOptions options, string token)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.SetLowerCase) return token;
            if (String.IsNullOrEmpty(token)) return null;

            return token.ToLower().Trim();
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="tokens">Token.</param>
        /// <returns>Token with normalized case.</returns>
        public static List<Token> NormalizeCase(PostingsOptions options, List<Token> tokens)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.SetLowerCase) return tokens;
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
        /// <param name="options">Postings options.</param>
        /// <param name="tokens">List of tokens.</param>
        /// <returns>List of tokens with normalized case.</returns>
        public static List<string> NormalizeCase(PostingsOptions options, List<string> tokens)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.SetLowerCase) return tokens;
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;

            List<string> ret = new List<string>();

            foreach (string curr in tokens)
            {
                ret.Add(NormalizeCase(options, curr));
            }

            return ret;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="val">Object.</param>
        /// <param name="valType">The data type of the object.</param>
        /// <returns>Object with normalized case.</returns>
        public static object NormalizeCase(PostingsOptions options, object val, DataType valType)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.SetLowerCase) return val;
            if (val == null) return null;

            if (valType == DataType.String)
            {
                return NormalizeCase(options, val.ToString());
            }

            return val;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary with normalized case.</returns>
        public static Dictionary<string, DataType> NormalizeCase(PostingsOptions options, Dictionary<string, DataType> dict)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.SetLowerCase) return dict;
            if (dict == null) return null;
            if (dict.Count < 1) return dict;

            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

            foreach (KeyValuePair<string, DataType> curr in dict)
            {
                string key = null;
                if (!String.IsNullOrEmpty(curr.Key)) key = NormalizeCase(options, curr.Key);
                if (ret.ContainsKey(key)) throw new Exception("Normalizing case would create duplicate keys in supplied dictionary");
                ret.Add(key, curr.Value);
            }

            return ret;
        }

        /// <summary>
        /// Normalize case.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="nodes">List of data nodes.</param>
        /// <returns>List of data nodes with normalized case.</returns>
        public static List<DataNode> NormalizeCase(PostingsOptions options, List<DataNode> nodes)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.SetLowerCase) return nodes;
            if (nodes == null) return null;
            if (nodes.Count < 1) return nodes;

            List<DataNode> ret = new List<DataNode>();

            foreach (DataNode curr in nodes)
            {
                string key = NormalizeCase(options, curr.Key);
                object data = NormalizeCase(options, curr.Data, curr.Type);
                ret.Add(new DataNode(key, data, curr.Type));
            }

            return ret;
        }

        #endregion

        #region Remove-Punctuation

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="token">String.</param>
        /// <returns>String without punctuation.</returns>
        public static string RemovePunctuation(PostingsOptions options, string token)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemovePunctuation) return token;
            if (options.PunctuationCharacters == null || options.PunctuationCharacters.Length < 1) return token;
            if (String.IsNullOrEmpty(token)) return token;

            return ReplaceChars(token, options.PunctuationCharacters, ' '); 
        }

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="tokens">List of string tokens.</param>
        /// <returns>List of string tokens without punctuation.</returns>
        public static List<string> RemovePunctuation(PostingsOptions options, List<string> tokens)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemovePunctuation) return tokens;
            if (options.PunctuationCharacters == null || options.PunctuationCharacters.Length < 1) return tokens;
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;

            List<string> ret = new List<string>();
            foreach (string token in tokens)
            {
                ret.Add(RemovePunctuation(options, token));
            }

            return ret;
        }

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="token">Token.</param>
        /// <returns>Token without punctuation.</returns>
        public static Token RemovePunctuation(PostingsOptions options, Token token)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemovePunctuation) return token;
            if (options.PunctuationCharacters == null || options.PunctuationCharacters.Length < 1) return token;
            if (token == null) return null;
            if (String.IsNullOrEmpty(token.Value)) return token;

            Token updated = new Token();
            updated.Value = RemovePunctuation(options, token.Value);
            updated.Count = token.Count;
            updated.Positions = token.Positions;
            return updated;
        }

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="tokens">Tokens.</param>
        /// <returns>Tokens without punctuation.</returns>
        public static List<Token> RemovePunctuation(PostingsOptions options, List<Token> tokens)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemovePunctuation) return tokens;
            if (options.PunctuationCharacters == null || options.PunctuationCharacters.Length < 1) return tokens;
            if (tokens == null) return null;
            if (tokens.Count < 1) return tokens;

            List<Token> ret = new List<Token>();

            foreach (Token token in tokens)
            {
                ret.Add(RemovePunctuation(options, token));
            }

            return ret;
        }

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="val">Object.</param>
        /// <param name="valType">The data type of the object.</param>
        /// <returns>Object without punctuation.</returns>
        public static object RemovePunctuation(PostingsOptions options, object val, DataType valType)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemovePunctuation) return val;
            if (options.PunctuationCharacters == null || options.PunctuationCharacters.Length < 1) return val;
            if (val == null) return null;

            if (valType == DataType.String)
            {
                return RemovePunctuation(options, val.ToString());
            }
            else
            {
                return val;
            }
        }

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary without punctuation.</returns>
        public static Dictionary<string, DataType> RemovePunctuation(PostingsOptions options, Dictionary<string, DataType> dict)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemovePunctuation) return dict;
            if (dict == null) return null;
            if (dict.Count < 1) return dict;

            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

            foreach (KeyValuePair<string, DataType> curr in dict)
            {
                string key = null;
                if (!String.IsNullOrEmpty(curr.Key)) key = RemovePunctuation(options, curr.Key);
                if (ret.ContainsKey(key)) throw new Exception("Removing punctuation would create duplicate keys in supplied dictionary");
                ret.Add(key, curr.Value);
            }

            return ret;
        }

        /// <summary>
        /// Remove punctuation.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="nodes">List of data nodes.</param>
        /// <returns>List of data nodes without punctuation.</returns>
        public static List<DataNode> RemovePunctuation(PostingsOptions options, List<DataNode> nodes)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemovePunctuation) return nodes;
            if (options.PunctuationCharacters == null || options.PunctuationCharacters.Length < 1) return nodes;
            if (nodes == null) return null;
            if (nodes.Count < 1) return nodes;

            List<DataNode> ret = new List<DataNode>();
            foreach (DataNode curr in nodes)
            {
                string key = null;
                object data = null;

                if (!String.IsNullOrEmpty(curr.Key)) key = RemovePunctuation(options, curr.Key);
                if (curr.Data != null) data = RemovePunctuation(options, curr.Data, curr.Type);
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
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemoveStopWords) return token;
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
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemoveStopWords) return tokens;
            if (options.StopWords == null || options.StopWords.Count < 1) return tokens;
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
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemoveStopWords) return tokens;
            if (options.StopWords == null || options.StopWords.Count < 1) return tokens;
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
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemoveStopWords) return token;
            if (options.StopWords == null || options.StopWords.Count < 1) return token;
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
        /// <param name="dict">Dictionary.</param>
        /// <returns>Dictionary without stop words.</returns>
        public static Dictionary<string, DataType> RemoveStopWords(PostingsOptions options, Dictionary<string, DataType> dict)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemovePunctuation) return dict;
            if (dict == null) return null;
            if (dict.Count < 1) return dict;

            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

            foreach (KeyValuePair<string, DataType> curr in dict)
            {
                string key = null;
                if (!String.IsNullOrEmpty(curr.Key)) key = RemoveStopWords(options, curr.Key);
                if (String.IsNullOrEmpty(key)) continue;
                if (ret.ContainsKey(key)) throw new Exception("Removing stop words would create duplicate keys in supplied dictionary");
                ret.Add(key, curr.Value);
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
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemoveStopWords) return data;
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

        #region Reduce-Whitespace

        /// <summary>
        /// Reduce whitespace.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="token">Token.</param>
        /// <returns>Token with reduced whitespace.</returns>
        public static string ReduceWhitespace(PostingsOptions options, string token)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.ReduceWhitespace) return token; 
            if (String.IsNullOrEmpty(token)) return token;

            token = token
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\f", " ")
                .Replace("\0", " ")
                .Replace("\t", " ");

            while (token.Contains("  ")) token = token.Replace("  ", " ");
            return token;
        }

        /// <summary>
        /// Reduce whitespace.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="tokens">Tokens.</param>
        /// <returns>Tokens with reduced whitespace.</returns>
        public static List<string> ReduceWhitespace(PostingsOptions options, List<string> tokens)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.ReduceWhitespace) return tokens;
            if (tokens == null || tokens.Count < 1) return tokens;

            List<string> ret = new List<string>();

            foreach (string token in tokens)
            {
                ret.Add(ReduceWhitespace(options, token));
            }

            if (ret != null && ret.Count > 0) ret = ret.Distinct().ToList();
            return ret;
        }

        /// <summary>
        /// Reduce whitespace.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="tokens">Tokens.</param>
        /// <returns>Tokens with reduced whitespace.</returns>
        public static List<Token> ReduceWhitespace(PostingsOptions options, List<Token> tokens)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.ReduceWhitespace) return tokens;
            if (tokens == null || tokens.Count < 1) return tokens;

            Dictionary<string, Token> dict = new Dictionary<string, Token>();
            List<Token> ret = new List<Token>();

            foreach (Token token in tokens)
            {
                if (String.IsNullOrEmpty(token.Value)) continue;
                token.Value = ReduceWhitespace(options, token.Value);
                dict = AddToken(dict, token);
            }

            if (dict != null && dict.Count > 0) ret = dict.Values.ToList();
            return ret;
        }

        /// <summary>
        /// Reduce whitespace.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="token">Token.</param>
        /// <param name="tokenType">Data type of the token.</param>
        /// <returns>Token with reduced whitespace.</returns>
        public static object ReduceWhitespace(PostingsOptions options, object token, DataType tokenType)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.ReduceWhitespace) return token;
            if (token == null) return null;

            if (tokenType == DataType.String)
            {
                return ReduceWhitespace(options, token.ToString());
            }
            else
            {
                return token;
            }
        }

        /// <summary>
        /// Reduce whitespace.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="data">Data nodes.</param>
        /// <returns>Data nodes with reduced whitespace.</returns>
        public static List<DataNode> ReduceWhitespace(PostingsOptions options, List<DataNode> data)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!options.TokenManipulation.RemoveStopWords) return data;
            if (options.StopWords == null || options.StopWords.Count < 1) return data;
            if (data == null || data.Count < 1) return data;

            List<DataNode> ret = new List<DataNode>();

            foreach (DataNode curr in data)
            {
                if (curr.Data != null)
                {
                    ret.Add(new DataNode(curr.Key, ReduceWhitespace(options, curr.Data, curr.Type), curr.Type));
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

        private static string ReplaceChars(string str, char[] separators, char newVal)
        {
            if (String.IsNullOrEmpty(str)) return str;
            if (separators == null || separators.Length < 1) return str;

            StringBuilder sb = new StringBuilder(str);
            foreach (var c in separators) { sb.Replace(c, newVal); }
            return sb.ToString();
        }

        private static bool IsWithinLength(string str, int minLen, int maxLen)
        {
            if (String.IsNullOrEmpty(str)) return false;
            if (str.Length < minLen || str.Length > maxLen) return false;
            return true;
        }

        private static List<Token> aAddToken(List<Token> tokens, Token token)
        {
            if (token == null) return tokens;
            if (String.IsNullOrEmpty(token.Value)) return tokens;
            if (tokens == null) tokens = new List<Token>();

            if (tokens.Any(t => t.Value.Equals(token.Value)))
            {
                Token orig = tokens.First(t => t.Value.Equals(token.Value));

                Token replace = new Token();
                replace.Value = orig.Value;
                replace.Count = orig.Count + token.Count;
                replace.Positions = new List<long>();

                if (token.Positions != null && token.Positions.Count > 0)
                {
                    replace.Positions.AddRange(token.Positions);
                    replace.Positions.AddRange(orig.Positions);
                }

                tokens.Remove(orig);
                tokens.Add(replace);
            }
            else
            {
                tokens.Add(token);
            }

            if (tokens != null && tokens.Count > 0)
            {
                tokens = tokens.OrderByDescending(t => t.Count).ToList();
            }

            return tokens;
        }

        private static Dictionary<string, Token> AddToken(Dictionary<string, Token> tokens, Token token)
        {
            if (token == null) return tokens;
            if (String.IsNullOrEmpty(token.Value)) return tokens;
            if (tokens == null) tokens = new Dictionary<string, Token>();

            if (tokens.ContainsKey(token.Value))
            {
                Token orig = tokens[token.Value];

                Token replace = new Token();
                replace.Value = orig.Value;
                replace.Count = orig.Count + token.Count;
                replace.Positions = new List<long>();

                if (token.Positions != null && token.Positions.Count > 0)
                {
                    replace.Positions.AddRange(token.Positions);
                    replace.Positions.AddRange(orig.Positions);
                }

                tokens.Remove(orig.Value);
                tokens.Add(replace.Value, replace);
            }
            else
            { 
                tokens.Add(token.Value, token);
            }  

            return tokens;
        }

        #endregion
    }
}
