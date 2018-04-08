using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomodoCore
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
        public Dictionary<string, string> NodeDocIds { get; set; }  // i.e. xml.child.data -> a19a83e...

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
            ret.NodeDocIds = new Dictionary<string, string>();
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
            ret.NodeDocIds = new Dictionary<string, string>();
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
            ret.NodeDocIds = new Dictionary<string, string>();
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
            ret.NodeDocIds = new Dictionary<string, string>();
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
            ret.NodeDocIds = new Dictionary<string, string>();
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

            if (NodeDocIds != null)
            {
                ret += "  Node Document IDs : " + NodeDocIds.Count() + Environment.NewLine;
                foreach (KeyValuePair<string, string> curr in NodeDocIds)
                {
                    ret += "    " + curr.Key + ": " + curr.Value + Environment.NewLine;
                }
            }

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
            GenerateHtmlNodeDocumentIds();
            GenerateHtmlPostingsAndTokens();
            Schema = Html.Schema;
            return;
        }

        private void ProcessJson()
        {
            Json = Normalizer.NormalizeJson(_Options, Json);
            GenerateJsonNodeDocumentIds();
            GenerateJsonPostingsAndTokens();
            Schema = Json.Schema;
            return;
        }

        private void ProcessSql()
        {
            Sql = Normalizer.NormalizeSql(_Options, Sql);
            GenerateSqlNodeDocumentIds();
            GenerateSqlPostingsAndTokens();
            Schema = Sql.Schema;
            return;
        }

        private void ProcessXml()
        {
            Xml = Normalizer.NormalizeXml(_Options, Xml);
            GenerateXmlNodeDocumentIds();
            GenerateXmlPostingsAndTokens();
            Schema = Xml.Schema;
            return;
        }

        private void ProcessText()
        {
            Text = Normalizer.NormalizeText(_Options, Text);
            GenerateTextNodeDocumentIds();
            GenerateTextPostingsAndTokens();
            Schema = null;
            return;
        }

        #endregion

        #region Node-ID-Generators-from-Schema

        private void GenerateHtmlNodeDocumentIds()
        {
            NodeDocIds = new Dictionary<string, string>();
            NodeDocIds.Add("Title", Guid.NewGuid().ToString());
            NodeDocIds.Add("MetaDesc", Guid.NewGuid().ToString());
            NodeDocIds.Add("MetaDescOpengraph", Guid.NewGuid().ToString());
            NodeDocIds.Add("MetaKeywords", Guid.NewGuid().ToString());
            NodeDocIds.Add("MetaImgOpengraph", Guid.NewGuid().ToString());
            NodeDocIds.Add("MetaVideoTagsOpengraph", Guid.NewGuid().ToString());
            NodeDocIds.Add("ImageUrls", Guid.NewGuid().ToString());
            NodeDocIds.Add("Links", Guid.NewGuid().ToString());
            NodeDocIds.Add("Head", Guid.NewGuid().ToString());
            NodeDocIds.Add("Body", Guid.NewGuid().ToString());
        }

        private void GenerateJsonNodeDocumentIds()
        {
            NodeDocIds = new Dictionary<string, string>();
            if (Json == null) return;
            if (Json.Schema == null) return;
            if (Json.Schema.Count < 1) return;

            foreach (KeyValuePair<string, DataType> curr in Json.Schema)
            {
                NodeDocIds.Add(curr.Key, MasterDocId + "." + Guid.NewGuid().ToString());
            }
        }

        private void GenerateSqlNodeDocumentIds()
        {
            NodeDocIds = new Dictionary<string, string>();
            if (Sql == null) return;
            if (Sql.Schema == null) return;
            if (Sql.Schema.Count < 1) return;

            foreach (KeyValuePair<string, DataType> curr in Sql.Schema)
            {
                NodeDocIds.Add(curr.Key, MasterDocId + "." + Guid.NewGuid().ToString());
            }
        }

        private void GenerateXmlNodeDocumentIds()
        {
            NodeDocIds = new Dictionary<string, string>();
            if (Xml == null) return;
            if (Xml.Schema == null) return;
            if (Xml.Schema.Count < 1) return;

            foreach (KeyValuePair<string, DataType> curr in Xml.Schema)
            {
                NodeDocIds.Add(curr.Key, MasterDocId + "." + Guid.NewGuid().ToString());
            }
        }

        private void GenerateTextNodeDocumentIds()
        {
            NodeDocIds = new Dictionary<string, string>();
            return;
        }

        #endregion

        #region Postings-Generators

        private string RetrieveDocumentId(string key)
        {
            if (NodeDocIds == null || NodeDocIds.Count < 1) return null;
            if (NodeDocIds.ContainsKey(key)) return NodeDocIds[key];
            return null;
        }

        private Posting RetrievePosting(string key, object val)
        {
            if (Postings == null || Postings.Count < 1) return null;
            if (String.IsNullOrEmpty(key)) return null;
            if (val == null) return null;
        
            foreach (Posting p in Postings)
            {
                if (String.Compare(p.DocumentId, key) == 0)
                {
                    if (p.Term == null) continue;
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

                        Posting currPosting = RetrievePosting("Title", currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "Title";
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "Title";
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

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

                        Posting currPosting = RetrievePosting("MetaDesc", currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "MetaDesc";
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "MetaDesc";
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

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

                        Posting currPosting = RetrievePosting("MetaDescOpengraph", currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "MetaDescOpengraph";
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "MetaDescOpengraph";
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

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

                        Posting currPosting = RetrievePosting("MetaKeywords", currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "MetaKeywords";
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "MetaKeywords";
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

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

                            Posting currPosting = RetrievePosting("MetaVideoTagsOpengraph", currTerm);
                            if (currPosting != null)
                            {
                                // remove and update
                                Postings.Remove(currPosting);
                                currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                                currPosting.DocumentId = "MetaVideoTagsOpengraph";
                                currPosting.Frequency = currPosting.Frequency + 1;
                                currPosting.Positions.Add(termCount);
                                Postings.Add(currPosting);
                            }
                            else
                            {
                                // create new entry
                                currPosting = new Posting();
                                currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                                currPosting.DocumentId = "MetaVideoTagsOpengraph";
                                currPosting.Frequency = 1;
                                currPosting.Positions = new List<long>();
                                currPosting.Positions.Add(termCount);
                                Postings.Add(currPosting);
                            }

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

                        Posting currPosting = RetrievePosting("ImageUrls", currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "ImageUrls";
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "ImageUrls";
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

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

                        Posting currPosting = RetrievePosting("Links", currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "Links";
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "Links";
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

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

                        Posting currPosting = RetrievePosting("Head", currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "Head";
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "Head";
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

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

                        Posting currPosting = RetrievePosting("Body", currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "Body";
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = "Body";
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

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
                    string documentId = RetrieveDocumentId(curr.Key);
                    if (String.IsNullOrEmpty(documentId)) continue;
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

                        Posting currPosting = RetrievePosting(documentId, currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = documentId;
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting); 
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = documentId;
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting); 
                        }

                        termCount++;
                    }

                    Terms = Terms.Distinct().ToList();
                }
            }
        }

        private void GenerateSqlPostingsAndTokens()
        {
            Postings = new List<Posting>();
            Terms = new List<string>();

            if (Sql != null && Sql.Flattened != null && Sql.Flattened.Count > 0)
            {
                foreach (DataNode curr in Sql.Flattened)
                {
                    string documentId = RetrieveDocumentId(curr.Key);
                    if (String.IsNullOrEmpty(documentId)) continue;
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

                        Posting currPosting = RetrievePosting(documentId, currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = documentId;
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = documentId;
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

                        termCount++;
                    }

                    Terms = Terms.Distinct().ToList();
                }
            } 
        }

        private void GenerateXmlPostingsAndTokens()
        {
            Postings = new List<Posting>();
            Terms = new List<string>();

            if (Xml != null && Xml.Flattened != null && Xml.Flattened.Count > 0)
            {
                foreach (DataNode curr in Xml.Flattened)
                {
                    string documentId = RetrieveDocumentId(curr.Key);
                    if (String.IsNullOrEmpty(documentId)) continue;
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

                        Posting currPosting = RetrievePosting(documentId, currTerm);
                        if (currPosting != null)
                        {
                            // remove and update
                            Postings.Remove(currPosting);
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = documentId;
                            currPosting.Frequency = currPosting.Frequency + 1;
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }
                        else
                        {
                            // create new entry
                            currPosting = new Posting();
                            currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                            currPosting.DocumentId = documentId;
                            currPosting.Frequency = 1;
                            currPosting.Positions = new List<long>();
                            currPosting.Positions.Add(termCount);
                            Postings.Add(currPosting);
                        }

                        termCount++;
                    }

                    Terms = Terms.Distinct().ToList();
                }
            } 
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

                    Posting currPosting = RetrievePosting(MasterDocId, currTerm);
                    if (currPosting != null)
                    {
                        // remove and update
                        Postings.Remove(currPosting);
                        currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        currPosting.DocumentId = MasterDocId;
                        currPosting.Frequency = currPosting.Frequency + 1;
                        currPosting.Positions.Add(termCount);
                        Postings.Add(currPosting);
                    }
                    else
                    {
                        // create new entry
                        currPosting = new Posting();
                        currPosting.Term = currTerm.Trim().Trim(Environment.NewLine.ToCharArray());
                        currPosting.DocumentId = MasterDocId;
                        currPosting.Frequency = 1;
                        currPosting.Positions = new List<long>();
                        currPosting.Positions.Add(termCount);
                        Postings.Add(currPosting);
                    }

                    termCount++;
                }

            }
            Terms = Terms.Distinct().ToList(); 
        }

        #endregion

        #endregion

        #region Public-Static-Methods

        #endregion

        #region Private-Static-Methods

        #endregion
    }
}
