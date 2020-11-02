using System;
using System.Collections.Generic;
using System.Linq;
using Komodo.Classes;
using Komodo.Parser;

namespace Komodo.Postings
{
    /// <summary>
    /// Komodo postings generator.
    /// </summary>
    public class PostingsGenerator
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private PostingsOptions _Options = new PostingsOptions();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public PostingsGenerator()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="options">Postings options.</param>
        public PostingsGenerator(PostingsOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _Options = options;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Process a parsed object.
        /// </summary>
        /// <param name="pr">Parse result.</param>
        /// <returns>Postings result.</returns>
        public PostingsResult Process(ParseResult pr)
        {
            if (pr == null) throw new ArgumentNullException(nameof(pr));
            return Process(new PostingsOptions(), pr);
        }

        /// <summary>
        /// Process a parsed object.
        /// </summary>
        /// <param name="options">Postings options.</param>
        /// <param name="pr">Parse result.</param>
        /// <returns>Postings result.</returns>
        public PostingsResult Process(PostingsOptions options, ParseResult pr)
        {
            if (pr == null) throw new ArgumentNullException(nameof(pr));

            PostingsResult ret = new PostingsResult(); 
            ret.Normalized = Normalizer.Normalize(options, pr); 

            Dictionary<string, Posting> postings = new Dictionary<string, Posting>();

            if (ret.Normalized.Tokens != null)
            {
                foreach (Token token in ret.Normalized.Tokens)
                {
                    if (String.IsNullOrEmpty(token.Value)) continue;
                    ret.Terms.Add(token.Value);
                    postings = AddOrUpdatePosting(postings, token);
                }
            }

            if (postings != null && postings.Count > 0) ret.Postings = postings.Values.ToList();
            if (ret.Postings != null && ret.Postings.Count > 0) ret.Postings = ret.Postings.OrderByDescending(p => p.Positions.Count).ToList();
            if (ret.Terms != null && ret.Terms.Count > 0) ret.Terms = ret.Terms.Distinct().ToList(); 

            ret.Success = true;
            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret; 
        }

        #endregion

        #region Private-Methods
         
        private Dictionary<string, Posting> AddOrUpdatePosting(Dictionary<string, Posting> postings, Token token)
        {
            if (token == null) return postings;
            if (String.IsNullOrEmpty(token.Value)) return postings;
            if (postings == null) postings = new Dictionary<string, Posting>();

            if (postings.ContainsKey(token.Value))
            {
                Posting match = postings[token.Value];
                Posting replace = new Posting();
                replace.Term = match.Term;
                replace.Frequency += token.Count;
                replace.Positions = new List<long>();
                if (match.Positions != null && match.Positions.Count > 0) replace.Positions.AddRange(match.Positions);
                if (token.Positions != null && token.Positions.Count > 0) replace.Positions.AddRange(token.Positions);
                if (replace.Positions != null && replace.Positions.Count > 0) replace.Positions = replace.Positions.Distinct().ToList();
                postings.Remove(token.Value);
                postings.Add(token.Value, replace);
            }
            else
            {
                Posting posting = new Posting();
                posting.Term = token.Value;
                posting.Frequency = token.Count;
                posting.Positions = new List<long>();
                posting.Positions.AddRange(token.Positions);
                postings.Add(posting.Term, posting);
            }

            return postings; 
        }

        #endregion
    }
}
