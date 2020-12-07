using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlobHelper;
using RestWrapper;
using Watson.ORM;
using Komodo;
using Komodo.Parser;
using Komodo.IndexClient;
using Komodo.IndexManager;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Metadata processor for Komodo.
    /// </summary>
    public class MetadataProcessor
    {
        #region Public-Members

        /// <summary>
        /// Metadata policy containing rules and actions.
        /// </summary>
        public MetadataPolicy Policy
        {
            get
            {
                return _Policy;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                else _Policy = value;
            }
        }

        #endregion

        #region Private-Members

        private MetadataPolicy _Policy = null; 
        private KomodoIndices _Indices = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataProcessor()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="policy">Metadata policy.</param>
        /// <param name="indices">Komodo index manager.</param>
        public MetadataProcessor(MetadataPolicy policy, KomodoIndices indices)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (indices == null) throw new ArgumentNullException(nameof(indices)); 
            
            _Policy = policy;
            _Indices = indices;    
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        /// <summary>
        /// Determine if the document matches a rule.
        /// </summary>
        /// <param name="source">Source document.</param>
        /// <param name="parsed">Parsed document.</param>
        /// <param name="parseResult">Parse result.</param>
        /// <returns>List of metadata rules.</returns>
        public List<MetadataRule> GetMatchingRules(SourceDocument source, ParsedDocument parsed, ParseResult parseResult)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (parsed == null) throw new ArgumentNullException(nameof(parsed));
            if (parseResult == null) throw new ArgumentNullException(nameof(parseResult));

            List<MetadataRule> ret = new List<MetadataRule>();

            if (_Policy != null && _Policy.Rules != null && _Policy.Rules.Count > 0)
            {
                foreach (MetadataRule rule in _Policy.Rules)
                {
                    if (!rule.Enabled) continue;
                    if (rule.Required == null) continue;

                    if (rule.Required.Terms != null && rule.Required.Terms.Count > 0)
                    {
                        bool requiredTermsMatch = true;

                        foreach (string term in rule.Required.Terms)
                        {
                            if (!ParseResultMatchesTerm(parseResult, source.Type, term))
                            {
                                requiredTermsMatch = false;
                                break;
                            }
                        }

                        if (!requiredTermsMatch) continue;
                    }

                    if (rule.Required.Filter != null && rule.Required.Filter.Count > 0)
                    {
                        bool requiredFilterMatch = true;

                        foreach (SearchFilter filter in rule.Required.Filter)
                        {
                            if (!ParseResultMatchesFilter(parseResult, source.Type, filter))
                            {
                                requiredFilterMatch = false;
                                break;
                            }
                        }

                        if (!requiredFilterMatch) continue;
                    }

                    if (rule.Exclude.Terms != null && rule.Exclude.Terms.Count > 0)
                    {
                        bool excludeTermsMatch = false;

                        foreach (string term in rule.Exclude.Terms)
                        {
                            if (ParseResultMatchesTerm(parseResult, source.Type, term))
                            {
                                excludeTermsMatch = true;
                                break;
                            }
                        }

                        if (excludeTermsMatch) continue;
                    }

                    if (rule.Exclude.Filter != null && rule.Exclude.Filter.Count > 0)
                    {
                        bool excludeFilterMatch = false;

                        foreach (SearchFilter filter in rule.Exclude.Filter)
                        {
                            if (ParseResultMatchesFilter(parseResult, source.Type, filter))
                            {
                                excludeFilterMatch = true;
                                break;
                            }
                        }

                        if (excludeFilterMatch) continue;
                    }

                    ret.Add(rule);
                }
            }

            return ret;
        }

        /// <summary>
        /// Process a document using the configured rules.
        /// </summary>
        /// <param name="source">Source document.</param>
        /// <param name="parsed">Parsed document.</param>
        /// <param name="parseResult">Parse result.</param>
        /// <returns>Metadata result.</returns>
        public async Task<MetadataResult> ProcessDocument(SourceDocument source, ParsedDocument parsed, ParseResult parseResult)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (parsed == null) throw new ArgumentNullException(nameof(parsed));
            if (parseResult == null) throw new ArgumentNullException(nameof(parseResult));

            MetadataResult result = new MetadataResult();
            result.Source = source;
            result.Parsed = parsed;
            result.ParseResult = parseResult;

            List<MetadataRule> matchingRules = GetMatchingRules(source, parsed, parseResult);
            if (matchingRules == null || matchingRules.Count < 1) return result;

            foreach (MetadataRule rule in matchingRules)
            {
                result.MatchingRules.Add(rule);
                 
                if (rule.AddMetadataDocument != null && rule.AddMetadataDocument.Count > 0)
                {
                    foreach (AddMetadataDocumentAction addDocAction in rule.AddMetadataDocument)
                    {
                        if (addDocAction.Properties == null || addDocAction.Properties.Count < 1) continue;

                        #region Retrieve-Index-Clients

                        KomodoIndex src = _Indices.GetIndexClient(source.IndexGUID);
                        if (src == null) throw new InvalidOperationException("Unable to find source index " + source.IndexGUID); 

                        KomodoIndex dst = _Indices.GetIndexClient(addDocAction.IndexGUID);
                        if (dst == null) throw new InvalidOperationException("Unable to find destination index " + addDocAction.IndexGUID); 

                        #endregion

                        #region Generate-Derived-Metadata-Document

                        Dictionary<string, object> derivedDocument = new Dictionary<string, object>();

                        foreach (MetadataDocumentProperty prop in addDocAction.Properties)
                        {
                            if (prop.ValueAction == PropertyValueAction.CopyFromDocument)
                            { 
                                string val = GetValueFromParseResult(parseResult, prop.SourceProperty);
                                derivedDocument.Add(prop.Key, val);
                            }
                            else if (prop.ValueAction == PropertyValueAction.Static)
                            { 
                                derivedDocument.Add(prop.Key, prop.Value);
                            }
                        }

                        byte[] derivedDocBytes = Encoding.UTF8.GetBytes(Common.SerializeJson(derivedDocument, true));

                        #endregion

                        #region Store-in-Database

                        MetadataDocument metadataDoc = new MetadataDocument();
                        metadataDoc.Created = DateTime.Now.ToUniversalTime();
                        metadataDoc.GUID = Guid.NewGuid().ToString();
                        metadataDoc.IndexGUID = source.IndexGUID;
                        metadataDoc.OwnerGUID = source.OwnerGUID;
                        metadataDoc.SourceDocumentGUID = source.GUID;
                        metadataDoc.TargetIndexGUID = addDocAction.IndexGUID;
                        metadataDoc.Type = DocType.Json;

                        metadataDoc = src.AddMetadata(source, metadataDoc);

                        #endregion

                        #region Index

                        SourceDocument derivedSourceDoc = new SourceDocument(
                            metadataDoc.GUID,
                            source.OwnerGUID,
                            addDocAction.IndexGUID,
                            addDocAction.Name,
                            addDocAction.Title,
                            addDocAction.Tags,
                            DocType.Json,
                            null,
                            "application/json",
                            derivedDocBytes.Length,
                            Common.Md5(derivedDocBytes));

                        IndexResult idxResult = await dst.Add(derivedSourceDoc, derivedDocBytes, addDocAction.Parse, new PostingsOptions());
                        
                        #endregion

                        #region Store-Results

                        result.MetadataDocuments.Add(metadataDoc);
                        result.DerivedDocuments.Add(idxResult.SourceDocument);
                        result.DerivedDocumentsData.Add(derivedDocument);
                        result.DerivedIndexResults.Add(idxResult);
                        
                        #endregion 
                    }
                }  
            } 

            foreach (MetadataRule rule in matchingRules)
            {
                if (rule.Postback != null
                    && rule.Postback.Urls != null
                    && rule.Postback.Urls.Count > 0)
                { 
                    MetadataResult postbackMetadata = Common.CopyObject<MetadataResult>(result);
                    if (!rule.Postback.IncludeSource) postbackMetadata.Source = null;
                    if (!rule.Postback.IncludeParsed) postbackMetadata.Parsed = null;
                    if (!rule.Postback.IncludeParseResult) postbackMetadata.ParseResult = null;
                    if (!rule.Postback.IncludeMetadata) postbackMetadata.MetadataDocuments = null;
                    if (!rule.Postback.IncludeRules) postbackMetadata.MatchingRules = null;
                    if (!rule.Postback.IncludeDerivedDocuments) postbackMetadata.DerivedDocuments = null;

                    rule.Postback.Urls = rule.Postback.Urls.Distinct().ToList();

                    foreach (string url in rule.Postback.Urls)
                    { 
                        if (String.IsNullOrEmpty(url)) continue;
                         
                        RestRequest req = new RestRequest(
                            url,
                            HttpMethod.POST,
                            null,
                            "application/json");
                        
                        RestResponse resp = req.Send(Common.SerializeJson(result, true));

                        result.PostbackStatusCodes.Add(url, resp.StatusCode);
                    } 
                }
            }

            return result;
        }

        #endregion

        #region Private-Methods

        private bool ParseResultMatchesTerm(ParseResult parseResult, DocType docType, string term)
        {
            if (parseResult == null || parseResult.Tokens == null || parseResult.Tokens.Count < 1) return false;
            return parseResult.Tokens.Any(t => t.Value.Equals(term)); 
        }

        private bool ParseResultMatchesFilter(ParseResult parseResult, DocType docType, SearchFilter filter)
        {
            if (parseResult == null || parseResult.Tokens == null || parseResult.Tokens.Count < 1) return false;
            return DataNodesMatchesFilter(parseResult.Flattened, filter); 
        }

        private bool DataNodesMatchesFilter(List<DataNode> nodes, SearchFilter filter)
        {
            if (nodes == null || nodes.Count < 1) return false;

            if (nodes.Exists(n => n.Key.Equals(filter.Field)))
            {
                List<DataNode> fieldMatchNodes = nodes.Where(n => n.Key.Equals(filter.Field)).ToList();

                foreach (DataNode node in fieldMatchNodes)
                {
                    if (filter.EvaluateValue(node.Data))
                    {
                        return true;
                    }
                } 
            }

            return false;
        }

        private string GetValueFromParseResult(ParseResult parseResult, string key)
        {
            if (parseResult == null) return null;
            if (parseResult.Flattened == null || parseResult.Flattened.Count < 1) return null;
            if (String.IsNullOrEmpty(key)) return null; 
             
            if (parseResult.Flattened.Any(n => n.Key.Equals(key)))
            {
                DataNode node = parseResult.Flattened.Where(n => n.Key.Equals(key)).First();
                if (node.Data == null) return null;
                return node.Data.ToString();
            }
             
            return null;
        }

        #endregion
    }
}
