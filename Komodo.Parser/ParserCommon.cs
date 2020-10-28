using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Komodo.Classes;

namespace Komodo.Parser
{
    internal static class ParserCommon
    {
        internal static List<Token> AddToken(Token token, List<Token> tokens)
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
                    replace.Positions = replace.Positions.Distinct().ToList();
                }

                tokens.Remove(orig);
                tokens.Add(replace);
            }
            else
            {
                tokens.Add(token);
            }

            return tokens;
        } 
    }
}
