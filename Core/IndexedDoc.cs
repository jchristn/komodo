using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komodo.Core
{
    /// <summary>
    /// A document that has been parsed and prepared for inclusion in the index.
    /// </summary>
    public class IndexedDoc
    {
        #region Public-Members
         
        /// <summary>
        /// The document ID.
        /// </summary>
        public string MasterDocId { get; set; }

        /// <summary>
        /// The document type.
        /// </summary>
        public DocType DocumentType { get; set; }

        /// <summary>
        /// Document IDs for nodes.
        /// </summary>
        public Dictionary<string, string> aNodeDocIds { get; set; }  // i.e. xml.child.data -> a19a83e...

        /// <summary>
        /// Schema for the object.
        /// </summary>
        public Dictionary<string, DataType> Schema { get; set; }

        /// <summary>
        /// The postings for the document.
        /// </summary>
        public List<Posting> Postings { get; set; }       
        
        /// <summary>
        /// The list of terms found in the document.
        /// </summary>
        public List<string> Terms { get; set; }

        /// <summary>
        /// For HTML documents, the parsed HTML.
        /// </summary>
        public ParsedHtml Html { get; set; }

        /// <summary>
        /// For JSON documents, the parsed JSON.
        /// </summary>
        public ParsedJson Json { get; set; }

        /// <summary>
        /// For SQL databases, the parsed database results.
        /// </summary>
        public ParsedSql Sql { get; set; }

        /// <summary>
        /// For XML documents, the parsed XML.
        /// </summary>
        public ParsedXml Xml { get; set; }

        /// <summary>
        /// For text documents, the parsed text.
        /// </summary>
        public ParsedText Text { get; set; }

        #endregion

        #region Private-Members
         
        private IndexOptions _Options { get; set; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the IndexedDoc.
        /// </summary>
        public IndexedDoc()
        {

        }

        /// <summary>
        /// Create a JSON string from the indexed document.
        /// </summary>
        /// <param name="pretty">True for pretty formatting.</param>
        /// <returns>String containing JSON.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        /// <summary>
        /// Create an indexed document from parsed HTML.
        /// </summary>
        /// <param name="html">Parsed HTML object.</param>
        /// <param name="options">Options for the index.</param>
        /// <returns>IndexedDoc.</returns>
        public static IndexedDoc FromHtml(ParsedHtml html, IndexOptions options)
        {
            if (html == null) throw new ArgumentNullException(nameof(html));
            if (options == null) options = new IndexOptions();

            IndexedDoc ret = new IndexedDoc();
            ret.MasterDocId = Guid.NewGuid().ToString();
            ret.DocumentType = DocType.Html; 
            ret.Html = html;
            ret._Options = options;
            ret.Postings = new List<Posting>();
            ret.Terms = new List<string>();
            ret.ProcessHtml();
            return ret;
        }

        /// <summary>
        /// Create an indexed document from parsed JSON.
        /// </summary>
        /// <param name="json">Parsed JSON object.</param>
        /// <param name="options">Options for the index.</param>
        /// <returns>IndexedDoc.</returns>
        public static IndexedDoc FromJson(ParsedJson json, IndexOptions options)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            if (options == null) options = new IndexOptions();

            IndexedDoc ret = new IndexedDoc();
            ret.MasterDocId = Guid.NewGuid().ToString();
            ret.DocumentType = DocType.Json; 
            ret.Json = json;
            ret._Options = options;
            ret.Postings = new List<Posting>();
            ret.Terms = new List<string>();
            ret.ProcessJson();
            return ret;
        }

        /// <summary>
        /// Create an indexed document from parsed SQL.
        /// </summary>
        /// <param name="sql">Parsed SQL object.</param>
        /// <param name="options">Options for the index.</param>
        /// <returns>IndexedDoc.</returns>
        public static IndexedDoc FromSql(ParsedSql sql, IndexOptions options)
        {
            if (sql == null) throw new ArgumentNullException(nameof(sql));
            if (options == null) options = new IndexOptions();

            IndexedDoc ret = new IndexedDoc();
            ret.MasterDocId = Guid.NewGuid().ToString();
            ret.DocumentType = DocType.Sql; 
            ret.Sql = sql;
            ret._Options = options;
            ret.Postings = new List<Posting>();
            ret.Terms = new List<string>();
            ret.ProcessSql();
            return ret;
        }

        /// <summary>
        /// Create an indexed document from parsed XML.
        /// </summary>
        /// <param name="xml">Parsed XML object.</param>
        /// <param name="options">Options for the index.</param>
        /// <returns>IndexedDoc.</returns>
        public static IndexedDoc FromXml(ParsedXml xml, IndexOptions options)
        {
            if (xml == null) throw new ArgumentNullException(nameof(xml));
            if (options == null) options = new IndexOptions();

            IndexedDoc ret = new IndexedDoc();
            ret.MasterDocId = Guid.NewGuid().ToString();
            ret.DocumentType = DocType.Xml; 
            ret.Xml = xml;
            ret._Options = options;
            ret.Postings = new List<Posting>();
            ret.Terms = new List<string>();
            ret.ProcessXml();
            return ret;
        }

        /// <summary>
        /// Create an indexed document from parsed text.
        /// </summary>
        /// <param name="text">Parsed text object.</param>
        /// <param name="options">Options for the index.</param>
        /// <returns>IndexedDoc.</returns>
        public static IndexedDoc FromText(ParsedText text, IndexOptions options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (options == null) options = new IndexOptions();

            IndexedDoc ret = new IndexedDoc();
            ret.MasterDocId = Guid.NewGuid().ToString();
            ret.DocumentType = DocType.Text; 
            ret.Text = text;
            ret._Options = options;
            ret.Postings = new List<Posting>();
            ret.Terms = new List<string>();
            ret.ProcessText();
            return ret;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a human-readable string from the indexed document.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += "---" + Environment.NewLine;
            ret += "Master Document ID  : " + MasterDocId + Environment.NewLine;
             
            if (Postings != null)
            {
                ret += "  Postings          : " + Postings.Count() + Environment.NewLine;
                foreach (Posting curr in Postings)
                {
                    ret += "    " + curr.ToString() + Environment.NewLine;
                }
            }

            if (Terms != null)
            {
                ret += "  Terms             : " + Terms.Count() + Environment.NewLine;
                int added = 0;
                foreach (string curr in Terms)
                {
                    if (added == 0) ret += "    " + curr;
                    else ret += "," + curr;
                    added++;
                }
                ret += Environment.NewLine;
            }

            ret += _Options.ToString();

            /*
            if (InnerHtml != null) ret += "Inner HTML:" + Environment.NewLine + InnerHtml.ToString() + Environment.NewLine;
            if (InnerJson != null) ret += "Inner JSON:" + Environment.NewLine + InnerJson.ToString() + Environment.NewLine;
            if (InnerSql != null) ret += "Inner SQL:" + Environment.NewLine + InnerSql.ToString() + Environment.NewLine;
            if (InnerXml != null) ret += "Inner XML:" + Environment.NewLine + InnerXml.ToString() + Environment.NewLine;
            */

            return ret;
        }
         
        #endregion

        #region Private-Methods

        #region Processors

        private void ProcessHtml()
        {
            Html = Normalizer.NormalizeHtml(_Options, Html); 
            GenerateHtmlPostingsAndTokens();
            Schema = Html.Schema;
            return;
        }

        private void ProcessJson()
        {
            Json = Normalizer.NormalizeJson(_Options, Json); 
            GenerateJsonPostingsAndTokens();
            Schema = Json.Schema;
            return;
        }

        private void ProcessSql()
        {
            Sql = Normalizer.NormalizeSql(_Options, Sql); 
            GenerateSqlPostingsAndTokens();
            Schema = Sql.Schema;
            return;
        }

        private void ProcessXml()
        {
            Xml = Normalizer.NormalizeXml(_Options, Xml); 
            GenerateXmlPostingsAndTokens();
            Schema = Xml.Schema;
            return;
        }

        private void ProcessText()
        {
            Text = Normalizer.NormalizeText(_Options, Text); 
            GenerateTextPostingsAndTokens();
            Schema = null;
            return;
        }

        #endregion
         
        #region Postings-Generators
         
        private Posting RetrievePosting(string key, object val)
        {
            if (Postings == null || Postings.Count < 1) return null;
            if (String.IsNullOrEmpty(key)) return null;
            if (val == null) return null;
        
            foreach (Posting p in Postings)
            {
                if (p.MasterDocId.Equals(key)) 
                {
                    if (String.IsNullOrEmpty(p.Term)) continue;
                    if (String.Compare(p.Term.ToString(), val.ToString()) == 0) return p;
                }
            }
             
            return null;
        }

        private void GenerateHtmlPostingsAndTokens()
        {
            Postings = new List<Posting>();
            Terms = new List<string>();

            if (Html != null)
            {
                #region Title

                if (!String.IsNullOrEmpty(Html.PageTitle))
                {
                    List<string> terms = Html.PageTitle.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region MetaDesc

                if (!String.IsNullOrEmpty(Html.MetaDescription))
                {
                    List<string> terms = Html.MetaDescription.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region MetaDescOpengraph

                if (!String.IsNullOrEmpty(Html.MetaDescriptionOpengraph))
                {
                    List<string> terms = Html.MetaDescriptionOpengraph.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region MetaKeywords

                if (!String.IsNullOrEmpty(Html.MetaKeywords))
                {
                    List<string> terms = Html.MetaKeywords.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region MetaVideoTagsOpengraph

                if (Html.MetaVideoTagsOpengraph != null && Html.MetaVideoTagsOpengraph.Count > 0)
                {
                    foreach (string curr in Html.MetaVideoTagsOpengraph)
                    {
                        List<string> terms = curr.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                        int termCount = 0;
                        foreach (string tempTerm in terms)
                        {
                            if (String.IsNullOrEmpty(tempTerm)) continue;

                            string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                            Terms.Add(currTerm);
                            AddOrUpdatePosting(currTerm, termCount);
                            termCount++;
                        }
                    }
                }

                #endregion

                #region ImageUrls

                if (Html.ImageUrls != null && Html.ImageUrls.Count > 0)
                {
                    int termCount = 0;
                    foreach (string tempTerm in Html.ImageUrls)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region Links

                if (Html.Links != null && Html.Links.Count > 0)
                {
                    int termCount = 0;
                    foreach (string tempTerm in Html.Links)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region Head

                if (!String.IsNullOrEmpty(Html.Head))
                {
                    List<string> terms = Html.Head.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion

                #region Body

                if (!String.IsNullOrEmpty(Html.Body))
                {
                    List<string> terms = Html.Body.Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    }
                }

                #endregion
            }

            Terms = Terms.Distinct().ToList();
        }

        private void GenerateJsonPostingsAndTokens()
        {
            Postings = new List<Posting>();
            Terms = new List<string>();

            if (Json != null && Json.Flattened != null && Json.Flattened.Count > 0)
            {
                foreach (DataNode curr in Json.Flattened)
                {  
                    if (curr.Data == null) continue;
                         
                    List<string> terms = curr.Data.ToString().Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    } 
                }
            }

            Terms = Terms.Distinct().ToList();
        }

        private void GenerateSqlPostingsAndTokens()
        {
            Postings = new List<Posting>();
            Terms = new List<string>();

            if (Sql != null && Sql.Flattened != null && Sql.Flattened.Count > 0)
            {
                foreach (DataNode curr in Sql.Flattened)
                { 
                    if (curr.Data == null) continue;

                    List<string> terms = curr.Data.ToString().Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    } 
                }
            }

            Terms = Terms.Distinct().ToList();
        }

        private void GenerateXmlPostingsAndTokens()
        {
            Postings = new List<Posting>();
            Terms = new List<string>();

            if (Xml != null && Xml.Flattened != null && Xml.Flattened.Count > 0)
            {
                foreach (DataNode curr in Xml.Flattened)
                { 
                    if (curr.Data == null) continue;

                    List<string> terms = curr.Data.ToString().Split(_Options.SplitCharacters, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (_Options.RemovePunctuation) terms = Normalizer.RemoveStringListPunctuation(terms);

                    int termCount = 0;
                    foreach (string tempTerm in terms)
                    {
                        if (String.IsNullOrEmpty(tempTerm)) continue;

                        string currTerm = tempTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                        Terms.Add(currTerm);
                        AddOrUpdatePosting(currTerm, termCount);
                        termCount++;
                    } 
                }
            }

            Terms = Terms.Distinct().ToList();
        }

        private void GenerateTextPostingsAndTokens()
        {
            Postings = new List<Posting>();
            Terms = new List<string>();

            if (Text != null && Text.Tokens != null && Text.Tokens.Count > 0) 
            {
                int termCount = 0;
                foreach (string curr in Text.Tokens)
                { 
                    if (String.IsNullOrEmpty(curr)) continue;

                    string currTerm = curr.Trim().Trim(Environment.NewLine.ToCharArray());
                    if (currTerm.Length < _Options.MinTokenLength || currTerm.Length > _Options.MaxTokenLength) continue;

                    Terms.Add(currTerm);
                    AddOrUpdatePosting(currTerm, termCount);
                    termCount++;
                } 
            }

            Terms = Terms.Distinct().ToList(); 
        }

        private void AddOrUpdatePosting(string term, int termCount)
        {
            if (String.IsNullOrEmpty(term)) return;
            term = term.Trim().Trim(Environment.NewLine.ToCharArray());

            Posting match = Postings.Where(s => s.Term.Equals(term)).FirstOrDefault();
            if (match == null || match == default(Posting))
            {
                #region First-Entry

                match = new Posting();
                match.Term = term;
                match.MasterDocId = MasterDocId;
                match.Frequency = 1;
                match.Positions = new List<long>();
                match.Positions.Add(termCount);
                Postings.Add(match);

                #endregion
            }
            else
            {
                #region Update

                Postings.Remove(match);
                match.Frequency = match.Frequency + 1;
                match.Positions.Add(termCount);
                Postings.Add(match);

                #endregion
            }
        }

        #endregion

        #endregion 
    }
}
