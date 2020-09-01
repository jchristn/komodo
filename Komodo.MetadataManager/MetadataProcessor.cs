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
using Komodo.Classes;
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
         
        private MetadataPolicy _Policy = null; 
        private KomodoIndices _Indices = null;

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
        public MetadataProcessor(MetadataPolicy policy, KomodoIndices indices)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (indices == null) throw new ArgumentNullException(nameof(indices)); 
            
            _Policy = policy;
            _Indices = indices;    
        }

        /// <summary>
        /// Determine if the document matches a rule.
        /// </summary>
        /// <param name="source">Source document.</param>
        /// <param name="parsed">Parsed document.</param>
        /// <param name="rule">Matching metadata rules.</param>
        /// <returns></returns>
        public List<MetadataRule> GetMatchingRules(SourceDocument source, ParsedDocument parsed, object parseResult)
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
        /// <returns>True if successful.</returns>
        public async Task<MetadataResult> ProcessDocument(SourceDocument source, ParsedDocument parsed, object parseResult)
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
                        result.DerivedDocuments.Add(idxResult.Source);
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

        private bool ParseResultMatchesTerm(object parseResult, DocType docType, string term)
        {
            if (docType == DocType.Html)
            {
                if (((HtmlParseResult)parseResult).Tokens.ContainsKey(term)) return true;
            }
            else if (docType == DocType.Json)
            {
                if (((JsonParseResult)parseResult).Tokens.ContainsKey(term)) return true;
            }
            else if (docType == DocType.Sql)
            {
                if (((SqlParseResult)parseResult).Tokens.ContainsKey(term)) return true;
            }
            else if (docType == DocType.Text)
            {
                if (((TextParseResult)parseResult).Tokens.ContainsKey(term)) return true;
            }
            else if (docType == DocType.Xml)
            {
                if (((XmlParseResult)parseResult).Tokens.ContainsKey(term)) return true;
            }
            else
            {
                throw new ArgumentException("Unknown document type: " + docType.ToString() + ".");
            }

            return false;
        }

        private bool ParseResultMatchesFilter(object parseResult, DocType docType, SearchFilter filter)
        {
            if (docType == DocType.Html)
            {
                throw new ArgumentException("Filter matches not supported on documents of type Text.");
            }
            else if (docType == DocType.Json)
            {
                return DataNodesMatchesFilter(((JsonParseResult)parseResult).Flattened, filter);
            }
            else if (docType == DocType.Sql)
            {
                return DataNodesMatchesFilter(((SqlParseResult)parseResult).Flattened, filter);
            }
            else if (docType == DocType.Text)
            {
                throw new ArgumentException("Filter matches not supported on documents of type Text.");
            }
            else if (docType == DocType.Xml)
            {
                return DataNodesMatchesFilter(((XmlParseResult)parseResult).Flattened, filter);
            }
            else
            {
                throw new ArgumentException("Unknown document type: " + docType.ToString() + ".");
            } 
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

        private string GetValueFromParseResult(object parseResult, string key)
        {
            List<DataNode> nodes = new List<DataNode>();

            if (parseResult is JsonParseResult)
            {
                nodes = ((JsonParseResult)parseResult).Flattened;
            }
            else if (parseResult is SqlParseResult)
            {
                nodes = ((SqlParseResult)parseResult).Flattened;
            }
            else if (parseResult is XmlParseResult)
            {
                nodes = ((XmlParseResult)parseResult).Flattened;
            }
            else
            {
                throw new ArgumentException("Unsupported parse result type");
            }
             
            if (nodes.Any(n => n.Key.Equals(key)))
            {
                DataNode node = nodes.Where(n => n.Key.Equals(key)).First();
                if (node.Data == null)
                { 
                    return null;
                } 
                return node.Data.ToString();
            }
             
            return null;
        }
    }
}
