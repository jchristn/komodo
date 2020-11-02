using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Komodo.Classes;

namespace Komodo.Parser
{
    /// <summary>
    /// Commonly-used parser methods.
    /// </summary>
    public static class ParserCommon
    {
        /// <summary>
        /// Adds a token to a list of tokens.  
        /// If the token already exists in the list, the existing entry is updated and merged with the supplied token.
        /// If the token entry does not exist in the list, it is added.
        /// </summary>
        /// <param name="tokens">List of Tokens.</param>
        /// <param name="token">Token to include in the list.</param>
        /// <returns>Updated list of Tokens.</returns>
        public static List<Token> AddToken(List<Token> tokens, Token token)
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

        /// <summary>
        /// Adds a token to a token dictionary.  
        /// If the token already exists in the dictionary, the existing entry is updated and merged with the supplied token.
        /// If the token entry does not exist in the dictionary, it is added.
        /// </summary>
        /// <param name="tokens">Dictionary of Tokens.</param>
        /// <param name="token">Token to include in the list.</param>
        /// <returns>Updated dictionary of Tokens.</returns>
        public static Dictionary<string, Token> AddToken(Dictionary<string, Token> tokens, Token token)
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

                tokens.Remove(token.Value);
                tokens.Add(token.Value, replace);
            }
            else
            {
                tokens.Add(token.Value, token);
            } 

            return tokens;
        }

        /// <summary>
        /// Get tokens from a string.
        /// </summary>
        /// <param name="data">String.</param>
        /// <param name="options">Parse options.</param>
        /// <returns>List of tokens.</returns>
        public static List<Token> GetTokens(string data, ParseOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            Dictionary<string, Token> dict = new Dictionary<string, Token>();
            List<Token> ret = new List<Token>();
            List<string> temp = new List<string>();

            temp = new List<string>(data.Split(options.Text.SplitCharacters, StringSplitOptions.RemoveEmptyEntries));

            if (temp != null && temp.Count > 0)
            {
                for (int i = 0; i < temp.Count; i++)
                {
                    string tempStr = new string(temp[i].ToCharArray());
                    if (!String.IsNullOrEmpty(tempStr))
                    {
                        tempStr = tempStr.Trim();
                        if (!String.IsNullOrEmpty(tempStr))
                        {
                            if (tempStr.Length < options.Text.MinimumTokenLength) continue;

                            Token token = new Token();
                            token.Value = tempStr;
                            token.Count = 1;
                            token.Positions.Add(i);
                            
                            if (dict.ContainsKey(token.Value))
                            {
                                Token orig = dict[token.Value];
                                Token replace = new Token();
                                replace.Value = orig.Value; 
                                replace.Count = orig.Count + 1;
                                replace.Positions = new List<long>();

                                if (token.Positions != null && token.Positions.Count > 0)
                                {
                                    replace.Positions.Add(i);
                                    replace.Positions.AddRange(orig.Positions);
                                }

                                dict.Remove(token.Value);
                                dict.Add(replace.Value, replace);
                            }
                            else
                            {
                                dict.Add(token.Value, token);
                            }
                        }
                    }
                }
            }

            if (dict != null && dict.Count > 0)
            {
                ret = dict.Values.ToList().OrderByDescending(u => u.Count).ToList();
            }

            return ret;
        }

        /// <summary>
        /// Get tokens from a list of data nodes.
        /// </summary>
        /// <param name="nodes">List of data nodes.</param>
        /// <param name="parser">Text parser.</param>
        /// <returns>List of tokens.</returns>
        public static List<Token> GetTokens(List<DataNode> nodes, TextParser parser)
        {
            List<Token> ret = new List<Token>();

            foreach (DataNode curr in nodes)
            {
                if (curr.Data == null) continue;
                if (String.IsNullOrEmpty(curr.Data.ToString())) continue;

                ParseResult tpr = parser.ParseString(curr.Data.ToString());

                foreach (Token currToken in tpr.Tokens)
                {
                    ret = AddToken(ret, currToken);
                }
            }

            if (ret != null && ret.Count > 0)
            {
                ret = ret.OrderByDescending(u => u.Count).ToList();
            }

            return ret;
        }

        /// <summary>
        /// Build schema from a list of data nodes.
        /// </summary>
        /// <param name="nodes">List of data nodes.</param>
        /// <returns>Dictionary containing schema.</returns>
        public static Dictionary<string, DataType> BuildSchema(List<DataNode> nodes)
        {
            Dictionary<string, DataType> ret = new Dictionary<string, DataType>();

            foreach (DataNode curr in nodes)
            {
                if (ret.ContainsKey(curr.Key))
                {
                    if (ret[curr.Key].Equals("null") && !curr.Type.Equals(DataType.Null))
                    { 
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
    }
}
