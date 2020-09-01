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

        private PostingsOptions _Options = null;

        #endregion

        #region Constructors-and-Factories

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
        /// <param name="parseResult">Parse result.</param>
        /// <returns>Postings result.</returns>
        public PostingsResult ProcessParseResult(object parseResult)
        {
            if (parseResult == null) throw new ArgumentNullException(nameof(parseResult));

            Type nodeType = parseResult.GetType();
            if (nodeType == typeof(HtmlParseResult))
            {
                return ProcessHtml((HtmlParseResult)parseResult);
            }
            else if (nodeType == typeof(JsonParseResult))
            {
                return ProcessJson((JsonParseResult)parseResult);
            }
            else if (nodeType == typeof(SqlParseResult))
            {
                return ProcessSql((SqlParseResult)parseResult);
            }
            else if (nodeType == typeof(TextParseResult))
            {
                return ProcessText((TextParseResult)parseResult);
            }
            else if (nodeType == typeof(XmlParseResult))
            {
                return ProcessXml((XmlParseResult)parseResult);
            }
            else
            {
                throw new ArgumentException("Unsupported parse result type: " + nodeType.ToString());
            }
        }

        #endregion

        #region Private-Methods

        private PostingsResult ProcessHtml(HtmlParseResult parsed)
        {
            PostingsResult ret = new PostingsResult();
             
            if (parsed != null)
            {
                #region Title

                if (!String.IsNullOrEmpty(parsed.PageTitle))
                {
                    List<string> terms = parsed.PageTitle.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region MetaDesc

                if (!String.IsNullOrEmpty(parsed.MetaDescription))
                {
                    List<string> terms = parsed.MetaDescription.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region MetaDescOpengraph

                if (!String.IsNullOrEmpty(parsed.MetaDescriptionOpengraph))
                {
                    List<string> terms = parsed.MetaDescriptionOpengraph.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region MetaKeywords

                if (!String.IsNullOrEmpty(parsed.MetaKeywords))
                {
                    List<string> terms = parsed.MetaKeywords.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region MetaVideoTagsOpengraph

                if (parsed.MetaVideoTagsOpengraph != null && parsed.MetaVideoTagsOpengraph.Count > 0)
                {
                    foreach (string curr in parsed.MetaVideoTagsOpengraph)
                    {
                        List<string> terms = curr.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                        int termCount = 0;
                        foreach (string tempTerm in terms)
                        {
                            if (String.IsNullOrEmpty(tempTerm)) continue;

                            string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                            ret.Terms.Add(currTerm);
                            AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                            termCount++;
                        }
                    }
                }

                #endregion

                #region ImageUrls

                if (parsed.ImageUrls != null && parsed.ImageUrls.Count > 0)
                {
                    int termCount = 0;
                    foreach (string tempTerm in parsed.ImageUrls)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region Links

                if (parsed.Links != null && parsed.Links.Count > 0)
                {
                    int termCount = 0;
                    foreach (string tempTerm in parsed.Links)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region Head

                if (!String.IsNullOrEmpty(parsed.Head))
                {
                    List<string> terms = parsed.Head.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region Body

                if (!String.IsNullOrEmpty(parsed.Body))
                {
                    List<string> terms = parsed.Body.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion
            }

            if (ret.Postings != null && ret.Postings.Count > 0) ret.Postings = ret.Postings.OrderByDescending(p => p.Positions.Count).ToList();
            ret.Terms = ret.Terms.Distinct().ToList(); 
            ret.Success = true;
            ret.Time.End = DateTime.Now; 
            return ret;
        }

        private PostingsResult ProcessJson(JsonParseResult parsed)
        {
            PostingsResult ret = new PostingsResult();

            if (parsed != null && parsed.Flattened != null && parsed.Flattened.Count > 0)
            {
                foreach (DataNode curr in parsed.Flattened)
                {
                    if (curr.Data == null) continue;

                    List<string> terms = curr.Data.ToString().Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }
            }

            if (ret.Postings != null && ret.Postings.Count > 0) ret.Postings = ret.Postings.OrderByDescending(p => p.Positions.Count).ToList();
            ret.Terms = ret.Terms.Distinct().ToList();
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        private PostingsResult ProcessSql(SqlParseResult parsed)
        {
            PostingsResult ret = new PostingsResult();

            if (parsed != null && parsed.Flattened != null && parsed.Flattened.Count > 0)
            {
                foreach (DataNode curr in parsed.Flattened)
                {
                    if (curr.Data == null) continue;

                    List<string> terms = curr.Data.ToString().Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }
            }

            if (ret.Postings != null && ret.Postings.Count > 0) ret.Postings = ret.Postings.OrderByDescending(p => p.Positions.Count).ToList();
            ret.Terms = ret.Terms.Distinct().ToList();
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        private PostingsResult ProcessText(TextParseResult parsed)
        {
            PostingsResult ret = new PostingsResult();

            if (parsed != null && parsed.Tokens != null && parsed.Tokens.Count > 0)
            {
                int termCount = 0;
                foreach (KeyValuePair<string, int> curr in parsed.Tokens)
                {
                    if (String.IsNullOrEmpty(curr.Key)) continue;

                    string currTerm = curr.Key.Trim().Trim(Environment.NewLine.ToCharArray());
                    if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                    ret.Terms.Add(currTerm);
                    AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                    termCount++;
                }
            }

            if (ret.Postings != null && ret.Postings.Count > 0) ret.Postings = ret.Postings.OrderByDescending(p => p.Positions.Count).ToList();
            ret.Terms = ret.Terms.Distinct().ToList();
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        private PostingsResult ProcessXml(XmlParseResult parsed)
        {
            PostingsResult ret = new PostingsResult();

            if (parsed != null && parsed.Flattened != null && parsed.Flattened.Count > 0)
            {
                foreach (DataNode curr in parsed.Flattened)
                {
                    if (curr.Data == null) continue;

                    List<string> terms = curr.Data.ToString().Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        ret.Terms.Add(currTerm);
                        AddOrUpdatePosting(ret.Postings, currTerm, termCount);
                        termCount++;
                    }
                }
            }

            if (ret.Postings != null && ret.Postings.Count > 0) ret.Postings = ret.Postings.OrderByDescending(p => p.Positions.Count).ToList();
            ret.Terms = ret.Terms.Distinct().ToList();
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        private void AddOrUpdatePosting(List<Posting> postings, string term, int termCount)
        {
            if (String.IsNullOrEmpty(term)) return;
            term = term.Trim().Trim(Environment.NewLine.ToCharArray());

            Posting match = postings.Where(s => s.Term.Equals(term)).FirstOrDefault();
            if (match == null || match == default(Posting))
            {
                #region First-Entry

                match = new Posting();
                match.Term = term; 
                match.Frequency = 1;
                match.Positions = new List<long>();
                match.Positions.Add(termCount);
                postings.Add(match);

                #endregion
            }
            else
            {
                #region Update

                postings.Remove(match);
                match.Frequency = match.Frequency + 1;
                match.Positions.Add(termCount);
                postings.Add(match);

                #endregion
            }
        }

        #endregion
    }
}
