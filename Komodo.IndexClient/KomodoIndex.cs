using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlobHelper;
using DatabaseWrapper;
using RestWrapper;
using Komodo.Classes;
using Komodo.Database; 
using Komodo.Parser;
using Komodo.Postings;

namespace Komodo.IndexClient
{
    /// <summary>
    /// Komodo index.
    /// </summary>
    public class KomodoIndex : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name
        {
            get
            {
                return _Name;
            }
        }

        /// <summary>
        /// The globally-unique identifier of the index.
        /// </summary>
        public string GUID
        {
            get
            {
                return _GUID;
            }
        }

        /// <summary>
        /// Method to invoke when sending a log message.
        /// </summary>
        public Action<string> Logger = null;

        #endregion

        #region Private-Members

        private string _Name = null;
        private string _GUID = null; 

        private DatabaseSettings _DatabaseSettings = null;
        private KomodoDatabase _Database = null;

        private StorageSettings _SourceDocsStorageSettings = null;
        private StorageSettings _ParsedDocsStorageSettings = null;
        private StorageSettings _PostingsStorageSettings = null;
        private Blobs _SourceDocsStorage = null;
        private Blobs _ParsedDocsStorage = null;
        private Blobs _PostingsStorage = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="dbSettings">Database settings.</param>
        /// <param name="sourceDocs">Storage settings for source documents.</param>
        /// <param name="parsedDocs">Storage settings for parsed documents.</param>
        /// <param name="postings">Storage settings fpr postings.</param>
        /// <param name="idx">Index configuration.</param>
        public KomodoIndex(DatabaseSettings dbSettings, StorageSettings sourceDocs, StorageSettings parsedDocs, StorageSettings postings, Index idx)
        {
            if (dbSettings == null) throw new ArgumentNullException(nameof(dbSettings));
            if (idx == null) throw new ArgumentNullException(nameof(idx));
            if (String.IsNullOrEmpty(idx.Name)) throw new ArgumentNullException(nameof(idx.Name));
            if (String.IsNullOrEmpty(idx.GUID)) throw new ArgumentNullException(nameof(idx.GUID));
            if (sourceDocs == null) throw new ArgumentNullException(nameof(sourceDocs));
            if (parsedDocs == null) throw new ArgumentNullException(nameof(parsedDocs));
            if (postings == null) throw new ArgumentNullException(nameof(postings));

            _DatabaseSettings = dbSettings;
            _Database = new KomodoDatabase(_DatabaseSettings);
            _Name = idx.Name;
            _GUID = idx.GUID; 

            _SourceDocsStorageSettings = sourceDocs;
            _ParsedDocsStorageSettings = parsedDocs;
            _PostingsStorageSettings = postings;

            InitializeStorage();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Delete all records and objects associated with the index.  
        /// This is a destructive operation.
        /// </summary> 
        public async Task Destroy()
        { 
            #region Source-Documents

            Expression e = new Expression("guid", Operators.Equals, _GUID);

            while (true)
            {
                List<SourceDocument> sourceDocs = _Database.SelectMany<SourceDocument>(0, 100, e, "ORDER BY id DESC");
                if (sourceDocs == null || sourceDocs.Count < 1) break;

                foreach (SourceDocument sourceDoc in sourceDocs)
                {
                    _Database.DeleteByGUID<SourceDocument>(sourceDoc.GUID);
                    await _SourceDocsStorage.Delete(sourceDoc.GUID);
                }
            }

            #endregion

            #region Parsed-Documents

            e = new Expression("sourcedocguid", Operators.Equals, _GUID);

            while (true)
            {
                List<ParsedDocument> parsedDocs = _Database.SelectMany<ParsedDocument>(0, 100, e, "ORDER BY id DESC");
                if (parsedDocs == null || parsedDocs.Count < 1) break;

                foreach (ParsedDocument parsedDoc in parsedDocs)
                {
                    _Database.DeleteByGUID<ParsedDocument>(parsedDoc.GUID);
                    await _ParsedDocsStorage.Delete(parsedDoc.GUID);
                }
            }

            #endregion 
        }

        /// <summary>
        /// Retrieve source document content.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>DocumentContent.</returns>
        public async Task<DocumentData> GetSourceDocumentContent(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));

            SourceDocument source = GetSourceDocumentMetadata(sourceGuid);
            if (source == null) return null;

            BlobData blobData = await _SourceDocsStorage.GetStream(sourceGuid);
            if (blobData != null)
            {
                DocumentData ret = new DocumentData(source.ContentType, blobData.ContentLength, blobData.Data);
                return ret;
            }

            return null;
        }

        /// <summary>
        /// Retrieve source document metadata.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>SourceDocument.</returns>
        public SourceDocument GetSourceDocumentMetadata(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));
            return _Database.SelectByGUID<SourceDocument>(sourceGuid);
        }

        /// <summary>
        /// Retrieve a parsed document by the source document GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>ParsedDocument.</returns>
        public ParsedDocument GetParsedDocument(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));
            string parsedGuid = ParsedDocGuidFromSourceDocGuid(sourceGuid);
            if (String.IsNullOrEmpty(parsedGuid)) return null;
            ParsedDocument ret = _Database.SelectByGUID<ParsedDocument>(parsedGuid);
            return ret;
        }

        /// <summary>
        /// Retrieve parse result by source document GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>Parse result.</returns>
        public object GetParseResult(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));
            string parsedGuid = ParsedDocGuidFromSourceDocGuid(sourceGuid);
            if (String.IsNullOrEmpty(parsedGuid)) return null;
            ParsedDocument parsed = _Database.SelectByGUID<ParsedDocument>(parsedGuid);
            if (parsed == null) return null;
            switch (parsed.Type)
            {
                case DocType.Html:
                    return Common.DeserializeJson<HtmlParseResult>(_ParsedDocsStorage.Get(parsedGuid).Result);
                case DocType.Json:
                    return Common.DeserializeJson<JsonParseResult>(_ParsedDocsStorage.Get(parsedGuid).Result);
                case DocType.Sql:
                    return Common.DeserializeJson<SqlParseResult>(_ParsedDocsStorage.Get(parsedGuid).Result);
                case DocType.Text:
                    return Common.DeserializeJson<TextParseResult>(_ParsedDocsStorage.Get(parsedGuid).Result);
                case DocType.Xml:
                    return Common.DeserializeJson<XmlParseResult>(_ParsedDocsStorage.Get(parsedGuid).Result);
                default:
                    throw new Exception("Unsupported document type: " + parsed.Type.ToString());
            }
        }

        /// <summary>
        /// Retrieve postings by source document GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>Postings.</returns>
        public PostingsResult GetPostings(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));
            string parsedGuid = ParsedDocGuidFromSourceDocGuid(sourceGuid);
            ParsedDocument parsed = GetParsedDocument(parsedGuid);
            if (parsed == null) return null;
            byte[] data = _ParsedDocsStorage.Get(parsedGuid).Result;
            if (data == null) return null;
            return Common.DeserializeJson<PostingsResult>(data);
        }

        /// <summary>
        /// Check if a source document exists by GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsSource(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));
            SourceDocument ret = _Database.SelectByGUID<SourceDocument>(sourceGuid);
            if (ret != null && ret != default(SourceDocument)) return true;
            return false;
        }

        /// <summary>
        /// Check if a parsed document exists by the source document GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsParsed(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));
            string parsedGuid = ParsedDocGuidFromSourceDocGuid(sourceGuid);
            if (String.IsNullOrEmpty(parsedGuid)) return false;
            ParsedDocument ret = _Database.SelectByGUID<ParsedDocument>(parsedGuid);
            if (ret != null && ret != default(ParsedDocument)) return true;
            return false;
        }

        /// <summary>
        /// Store or store-and-index a document.
        /// </summary>
        /// <param name="sourceDoc">Source document.</param>
        /// <param name="data">Byte data.</param>
        /// <param name="parse">True if the document should be parsed and indexed.</param>
        /// <param name="options">Postings options.</param> 
        /// <returns>Index result.</returns>
        public async Task<IndexResult> Add(SourceDocument sourceDoc, byte[] data, bool parse, PostingsOptions options)
        {
            return await Add(sourceDoc, data, parse, options, null);
        }

        /// <summary>
        /// Store or store-and-index a document.
        /// </summary>
        /// <param name="sourceDoc">Source document.</param>
        /// <param name="data">Byte data.</param>
        /// <param name="parse">True if the document should be parsed and indexed.</param>
        /// <param name="options">Postings options.</param>
        /// <param name="postbackUrl">URL to which results should be POSTed.</param>
        /// <returns>Index result.</returns>
        public async Task<IndexResult> Add(SourceDocument sourceDoc, byte[] data, bool parse, PostingsOptions options, string postbackUrl)
        {
            if (sourceDoc == null) throw new ArgumentNullException(nameof(sourceDoc));
            if (String.IsNullOrEmpty(sourceDoc.OwnerGUID)) throw new ArgumentNullException(nameof(sourceDoc.OwnerGUID));
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));

            #region Check-GUID

            IndexResult ret = new IndexResult();
            if (!String.IsNullOrEmpty(sourceDoc.GUID))
            {
                if (ExistsSource(sourceDoc.GUID)) return ret;
            }
            else
            {
                sourceDoc.GUID = Guid.NewGuid().ToString();
            }

            #endregion

            #region Persist-Source

            await _SourceDocsStorage.Write(sourceDoc.GUID, sourceDoc.ContentType, data);

            _Database.Insert<SourceDocument>(sourceDoc);

            ret.Time.PersistSourceDocument.End = DateTime.Now.ToUniversalTime();

            #endregion

            #region Parse

            if (parse)
            {
                #region Parse

                ret.Time.Parse.Start = DateTime.Now.ToUniversalTime();

                if (sourceDoc.Type == DocType.Html)
                {
                    HtmlParser htmlParser = new HtmlParser();
                    HtmlParseResult htmlResult = htmlParser.ParseBytes(data);
                    ret.ParseResult = htmlResult;
                }
                else if (sourceDoc.Type == DocType.Json)
                {
                    JsonParser jsonParser = new JsonParser();
                    JsonParseResult jsonResult = jsonParser.ParseBytes(data);
                    ret.ParseResult = jsonResult;
                }
                else if (sourceDoc.Type == DocType.Text)
                {
                    TextParser textParser = new TextParser();
                    TextParseResult textResult = textParser.ParseBytes(data);
                    ret.ParseResult = textResult;
                }
                else if (sourceDoc.Type == DocType.Xml)
                {
                    XmlParser xmlParser = new XmlParser();
                    XmlParseResult xmlResult = xmlParser.ParseBytes(data);
                    ret.ParseResult = xmlResult;
                }
                else
                {
                    throw new Exception("Unsupported document type: " + sourceDoc.Type.ToString());
                }

                ret.Time.Parse.End = DateTime.Now.ToUniversalTime();

                #endregion

                #region Generate-Postings

                ret.Time.Postings.Start = DateTime.Now.ToUniversalTime();
                PostingsGenerator postings = new PostingsGenerator(options);
                ret.Postings = postings.ProcessParseResult(ret.ParseResult);
                ret.Time.Postings.End = DateTime.Now.ToUniversalTime();

                #endregion

                #region Persist-Parsed-Doc

                ret.Time.PersistParsedDocument.Start = DateTime.Now.ToUniversalTime();
                ParsedDocument parsedDoc = new ParsedDocument(
                    sourceDoc.GUID,
                    sourceDoc.OwnerGUID,
                    _GUID,
                    sourceDoc.Type,
                    data.Length,
                    ret.Postings.Terms.Count,
                    ret.Postings.Postings.Count);

                parsedDoc = _Database.Insert<ParsedDocument>(parsedDoc);
                await _ParsedDocsStorage.Write(parsedDoc.GUID, "application/json", Common.SerializeJson(ret.ParseResult, true));
                ret.Time.PersistParsedDocument.End = DateTime.Now.ToUniversalTime();

                #endregion

                #region Persist-Postings 

                ret.Time.PersistPostingsDocument.Start = DateTime.Now.ToUniversalTime();
                await _PostingsStorage.Write(parsedDoc.GUID, "application/json", Common.SerializeJson(ret.Postings, true));
                ret.Time.PersistPostingsDocument.End = DateTime.Now.ToUniversalTime();

                #endregion

                #region Persist-Terms

                ret.Time.Terms.Start = DateTime.Now.ToUniversalTime();

                // term, guid
                Dictionary<string, string> termGuids = new Dictionary<string, string>();

                // Term GUIDs
                List<string> terms = GetParseResultTerms(sourceDoc.Type, ret.ParseResult);
                if (terms != null && terms.Count > 0) terms = terms.Distinct().ToList();

                foreach (string term in terms)
                {
                    Expression e = new Expression("term", Operators.Equals, term);
                    TermGuid tg = _Database.SelectByFilter<TermGuid>(e, "ORDER BY id DESC");
                    if (tg == null || tg == default(TermGuid))
                    {
                        tg = new TermGuid();
                        tg.GUID = Guid.NewGuid().ToString();
                        tg.IndexGUID = _GUID;
                        tg.Term = term;
                        tg = _Database.Insert<TermGuid>(tg);
                    }

                    termGuids.Add(term, tg.GUID);
                }

                // Term Docs
                foreach (string term in terms)
                {
                    TermDoc td = new TermDoc(_GUID, termGuids[term], sourceDoc.GUID, parsedDoc.GUID);
                    td = _Database.Insert<TermDoc>(td);
                }

                ret.Time.Terms.End = DateTime.Now.ToUniversalTime();

                #endregion 
            }

            #endregion

            ret.Success = true;
            ret.GUID = sourceDoc.GUID;
            ret.Time.Overall.End = DateTime.Now.ToUniversalTime();

            if (!String.IsNullOrEmpty(postbackUrl))
            {
                RestRequest restReq = new RestRequest(
                    postbackUrl,
                    HttpMethod.POST,
                    null,
                    "application/json");

                RestResponse restResp = await restReq.SendAsync(Common.SerializeJson(ret, true));
            }

            return ret;
        }

        /// <summary>
        /// Remove a document.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        public void Remove(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));

            SourceDocument sourceDoc = _Database.SelectByGUID<SourceDocument>(sourceGuid);
            if (sourceDoc != null && sourceDoc != default(SourceDocument))
            {
                _Database.Delete(sourceDoc);
                _SourceDocsStorage.Delete(sourceDoc.GUID);
            }

            Expression e = new Expression("sourcedocguid", Operators.Equals, sourceGuid);
            ParsedDocument parsedDoc = _Database.SelectByFilter<ParsedDocument>(e, "ORDER BY id DESC");
            if (parsedDoc != null && parsedDoc != default(ParsedDocument))
            {
                _Database.Delete(parsedDoc);
                _ParsedDocsStorage.Delete(parsedDoc.GUID);
            }
        }

        /// <summary>
        /// Search the index.
        /// </summary>
        /// <param name="query">Search query.</param>
        /// <returns>Search result.</returns>
        public SearchResult Search(SearchQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return SearchInternal(query);
        }

        /// <summary>
        /// Enumerate the index.
        /// </summary>
        /// <param name="query">Enumeration query.</param>
        /// <returns>Enumeraiton result.</returns>
        public Komodo.Classes.EnumerationResult Enumerate(EnumerationQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            return EnumerateInternal(query);
        }

        /// <summary>
        /// Retrieve index statistics.
        /// </summary>
        /// <returns>Index statistics.</returns>
        public IndexStats Stats()
        {
            #region Toplevel

            IndexStats ret = new IndexStats();
            ret.Name = _Name;
            ret.GUID = _GUID;

            string query =
                "SELECT " +
                "  SUM(terms) AS termscount, " +
                "  SUM(postings) AS postingscount " +
                "FROM parseddocs " +
                "WHERE indexguid = '" + _Database.Sanitize(_GUID) + "'";

            DataTable result = _Database.Query(query);
            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    if (row.Table.Columns.Contains("termscount") && row["termscount"] != null && row["termscount"] != DBNull.Value)
                    {
                        ret.Terms = Convert.ToInt64(row["termscount"]);
                    }

                    if (row.Table.Columns.Contains("postingscount") && row["postingscount"] != null && row["postingscount"] != DBNull.Value)
                    {
                        ret.Postings = Convert.ToInt64(row["postingscount"]);
                    }
                }
            }

            #endregion

            #region Source-Documents

            query =
                "SELECT " +
                "  COUNT(*) AS docscount, " +
                "  SUM(contentlength) AS bytestotal " +
                "FROM sourcedocs " +
                "WHERE indexguid = '" + _Database.Sanitize(_GUID) + "'";

            result = _Database.Query(query);
            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    if (row.Table.Columns.Contains("docscount") && row["docscount"] != null && row["docscount"] != DBNull.Value)
                    {
                        ret.SourceDocuments.Count = Convert.ToInt64(row["docscount"]);
                    }

                    if (row.Table.Columns.Contains("bytestotal") && row["bytestotal"] != null && row["bytestotal"] != DBNull.Value)
                    {
                        ret.SourceDocuments.Bytes = Convert.ToInt64(row["bytestotal"]);
                    }
                }
            }

            #endregion

            #region Parsed-Documents

            query =
                "SELECT " +
                "  COUNT(*) AS docscount, " +
                "  SUM(contentlength) AS bytestotal " +
                "FROM parseddocs " +
                "WHERE indexguid = '" + _Database.Sanitize(_GUID) + "'";

            result = _Database.Query(query);
            if (result != null && result.Rows.Count > 0)
            {
                foreach (DataRow row in result.Rows)
                {
                    if (row.Table.Columns.Contains("docscount") && row["docscount"] != null && row["docscount"] != DBNull.Value)
                    {
                        ret.ParsedDocuments.Count = Convert.ToInt64(row["docscount"]);
                    }

                    if (row.Table.Columns.Contains("bytestotal") && row["bytestotal"] != null && row["bytestotal"] != DBNull.Value)
                    {
                        ret.ParsedDocuments.Bytes = Convert.ToInt64(row["bytestotal"]);
                    }
                }
            }

            #endregion

            #region Return

            ret.Success = true;
            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret;

            #endregion
        }

        #endregion

        #region Private-General-Methods

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        /// <param name="disposing">Indicate if child resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _Database.Dispose();
            }
        }

        private void InitializeStorage()
        {
            #region Source-Documents

            if (_SourceDocsStorageSettings.Aws != null) _SourceDocsStorage = new Blobs(_SourceDocsStorageSettings.Aws);
            else if (_SourceDocsStorageSettings.Azure != null) _SourceDocsStorage = new Blobs(_SourceDocsStorageSettings.Azure);
            else if (_SourceDocsStorageSettings.Disk != null) _SourceDocsStorage = new Blobs(_SourceDocsStorageSettings.Disk);
            else if (_SourceDocsStorageSettings.Kvpbase != null) _SourceDocsStorage = new Blobs(_SourceDocsStorageSettings.Kvpbase);
            else throw new ArgumentException("No storage settings found in source document storage settings object.");

            #endregion

            #region Parsed-Documents

            if (_ParsedDocsStorageSettings.Aws != null) _ParsedDocsStorage = new Blobs(_ParsedDocsStorageSettings.Aws);
            else if (_ParsedDocsStorageSettings.Azure != null) _ParsedDocsStorage = new Blobs(_ParsedDocsStorageSettings.Azure);
            else if (_ParsedDocsStorageSettings.Disk != null) _ParsedDocsStorage = new Blobs(_ParsedDocsStorageSettings.Disk);
            else if (_ParsedDocsStorageSettings.Kvpbase != null) _ParsedDocsStorage = new Blobs(_ParsedDocsStorageSettings.Kvpbase);
            else throw new ArgumentException("No storage settings found in parsed document storage settings object.");

            #endregion

            #region Postings

            if (_PostingsStorageSettings.Aws != null) _PostingsStorage = new Blobs(_PostingsStorageSettings.Aws);
            else if (_PostingsStorageSettings.Azure != null) _PostingsStorage = new Blobs(_PostingsStorageSettings.Azure);
            else if (_PostingsStorageSettings.Disk != null) _PostingsStorage = new Blobs(_PostingsStorageSettings.Disk);
            else if (_PostingsStorageSettings.Kvpbase != null) _PostingsStorage = new Blobs(_PostingsStorageSettings.Kvpbase);
            else throw new ArgumentException("No storage settings found in postings storage settings object.");

            #endregion
        }

        private string ParsedDocGuidFromSourceDocGuid(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            Expression e = new Expression("sourcedocguid", Operators.Equals, guid);
            ParsedDocument ret = _Database.SelectByFilter<ParsedDocument>(e, "ORDER BY id DESC");
            if (ret != null) return ret.GUID;
            return null;
        }

        #endregion

        #region Private-Search-Methods

        private SearchResult SearchInternal(SearchQuery query)
        {
            SearchResult result = new SearchResult();
            result.IndexName = _Name;
            result.Query = query;

            #region Setup-Dictionaries

            DateTime dtPrep = DateTime.Now;
            List<string> masterTerms = new List<string>();

            if (query.Required != null && query.Required.Terms != null && query.Required.Terms.Count > 0)
            {
                masterTerms.AddRange(query.Required.Terms);
            }

            if (query.Optional != null && query.Optional.Terms != null && query.Optional.Terms.Count > 0)
            {
                masterTerms.AddRange(query.Optional.Terms);
            }

            if (query.Exclude != null && query.Exclude.Terms != null && query.Exclude.Terms.Count > 0)
            {
                masterTerms.AddRange(query.Exclude.Terms);
            }

            masterTerms = masterTerms.Distinct().ToList();

            Expression e = new Expression("term", Operators.In, masterTerms);
            List<TermGuid> masterTermsGuids = _Database.SelectMany<TermGuid>(null, null, e, "ORDER BY id DESC");

            Dictionary<string, string> masterTermsDict = new Dictionary<string, string>();
            foreach (TermGuid curr in masterTermsGuids)
            {
                masterTermsDict.Add(curr.Term, curr.GUID);
            }

            // key is term, val is guid
            Dictionary<string, string> requiredTermsDict = new Dictionary<string, string>();
            Dictionary<string, string> optionalTermsDict = new Dictionary<string, string>();
            Dictionary<string, string> excludeTermsDict = new Dictionary<string, string>();

            if (query.Required != null && query.Required.Terms != null && query.Required.Terms.Count > 0)
            {
                foreach (string currTerm in query.Required.Terms)
                {
                    if (masterTermsDict.ContainsKey(currTerm))
                    {
                        requiredTermsDict.Add(currTerm, masterTermsDict[currTerm]);
                    }
                    else
                    {
                        result.TermsNotFound.Add(currTerm);
                    }
                }
            }

            if (query.Optional != null && query.Optional.Terms != null && query.Optional.Terms.Count > 0)
            {
                foreach (string currTerm in query.Optional.Terms)
                {
                    if (masterTermsDict.ContainsKey(currTerm))
                    {
                        optionalTermsDict.Add(currTerm, masterTermsDict[currTerm]);
                    }
                    else
                    {
                        result.TermsNotFound.Add(currTerm);
                    }
                }
            }

            if (query.Exclude != null && query.Exclude.Terms != null && query.Exclude.Terms.Count > 0)
            {
                foreach (string currTerm in query.Exclude.Terms)
                {
                    if (masterTermsDict.ContainsKey(currTerm))
                    {
                        excludeTermsDict.Add(currTerm, masterTermsDict[currTerm]);
                    }
                    else
                    {
                        result.TermsNotFound.Add(currTerm);
                    }
                }
            }

            result.Time.PreparationMs += Common.TotalMsFrom(dtPrep);

            #endregion

            #region Outer-Loop

            List<MatchedDocument> matchDocuments = new List<MatchedDocument>();
            bool endReached = false;
            int indexStart = query.StartIndex;
            int maxResults = query.MaxResults;
            long entryCount = 0;
            long docsCount = 0;

            while (!endReached)
            {
                #region Required-and-Exclude-Terms

                DateTime dtReqExclTerms = DateTime.Now;
                List<ParsedDocument> currDocs = GetDocumentsMatchingTerms(indexStart, maxResults, requiredTermsDict, excludeTermsDict, out entryCount, out docsCount, out endReached);
                result.Time.RequiredExcludedTermsMs += Common.TotalMsFrom(dtReqExclTerms);

                #endregion

                #region Process-Each-Document

                if (currDocs != null && currDocs.Count > 0)
                {
                    foreach (ParsedDocument currDoc in currDocs)
                    {
                        #region Read-Parse-Result

                        DateTime dtParseResult = DateTime.Now;
                        object parseResult = GetParseResultFromParsedDocumentGuid(currDoc.Type, currDoc.GUID);
                        result.Time.ReadParseResultsMs += Common.TotalMsFrom(dtParseResult);

                        #endregion

                        #region Optional-Terms

                        DateTime dtOptTerms = DateTime.Now;
                        decimal? termsScore = null;
                        Dictionary<string, string> matchedTerms = new Dictionary<string, string>();
                        DocumentMatchesOptionalTerms(currDoc, parseResult, query.Optional.Terms, optionalTermsDict, out termsScore, out matchedTerms);
                        result.Time.OptionalTermsMs += Common.TotalMsFrom(dtOptTerms);

                        #endregion

                        #region Required-and-Exclude-Filters

                        DateTime dtReqExclFilters = DateTime.Now;
                        if (!DocumentMatchesFilters(currDoc, parseResult, query.Required.Filter, query.Exclude.Filter))
                        {
                            result.Time.RequiredExcludedFiltersMs += Common.TotalMsFrom(dtReqExclFilters);
                            continue;
                        }
                        else
                        {
                            result.Time.RequiredExcludedFiltersMs += Common.TotalMsFrom(dtReqExclFilters);
                        }

                        #endregion

                        #region Optional-Filters

                        DateTime dtOptFilters = DateTime.Now;
                        decimal? filtersScore = null;
                        List<SearchFilter> matchedFilters = new List<SearchFilter>();
                        DocumentMatchesOptionalFilters(currDoc, parseResult, query.Optional.Filter, out filtersScore, out matchedFilters);
                        result.Time.OptionalFiltersMs += Common.TotalMsFrom(dtOptFilters);

                        #endregion

                        #region Aggregate-Score

                        decimal aggregateScore = 1m;

                        if (termsScore != null && filtersScore != null)
                        {
                            aggregateScore = (Convert.ToDecimal(termsScore) + Convert.ToDecimal(filtersScore)) / 2;
                        }
                        else if (termsScore != null && filtersScore == null)
                        {
                            aggregateScore = Convert.ToDecimal(termsScore);
                        }
                        else if (termsScore == null && filtersScore != null)
                        {
                            aggregateScore = Convert.ToDecimal(filtersScore);
                        }

                        #endregion

                        #region Add-to-List

                        MatchedDocument matchDoc = new MatchedDocument();
                        matchDoc.DocumentType = currDoc.Type;
                        matchDoc.FiltersScore = filtersScore;
                        matchDoc.GUID = currDoc.SourceDocumentGUID;
                        matchDoc.Score = aggregateScore;
                        matchDoc.TermsScore = termsScore;
                        matchDocuments.Add(matchDoc);

                        if (matchDocuments.Count >= query.MaxResults) break; // exit foreach

                        #endregion
                    }
                }

                #endregion

                if (matchDocuments.Count >= query.MaxResults) break;    // exit while
                if (endReached) break;                                  // exit while
                else indexStart += query.MaxResults;
            }

            #endregion

            result.Time.Overall.End = DateTime.Now.ToUniversalTime();
            result.Success = true;
            result.Documents = matchDocuments;
            result.SortMatchesByScore();

            return result;
        }

        private List<ParsedDocument> GetDocumentsMatchingTerms(
            int indexStart,
            int maxResults,
            Dictionary<string, string> requiredTerms,
            Dictionary<string, string> excludeTerms,
            out long entryCount,
            out long docsCount,
            out bool endReached)
        {
            endReached = false;
            entryCount = 0;
            docsCount = 0;
            string query = null;
            DataTable result = null;

            #region Check-End 

            query =
                "SELECT " +
                "  COUNT(*) AS entrycount, " +
                "  COUNT(DISTINCT(parseddocguid)) AS docscount " +
                "FROM termdocs " +
                "WHERE indexguid = '" + _Database.Sanitize(_GUID) + "'";

            result = _Database.Query(query);
            entryCount = Convert.ToInt32(result.Rows[0]["entrycount"]);
            docsCount = Convert.ToInt32(result.Rows[0]["docscount"]);
            if (indexStart + maxResults > entryCount) endReached = true;

            #endregion

            #region Prepare-Strings

            int requiredAdded = 0;
            string required = "";
            if (requiredTerms != null && requiredTerms.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in requiredTerms)
                {
                    if (String.IsNullOrEmpty(curr.Key) || String.IsNullOrEmpty(curr.Value)) continue;
                    if (requiredAdded == 0) required += "'" + _Database.Sanitize(curr.Value) + "'";
                    else required += ",'" + _Database.Sanitize(curr.Value) + "'";
                    requiredAdded++;
                }
            }

            int excludedAdded = 0;
            string excluded = "";
            if (excludeTerms != null && excludeTerms.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in excludeTerms)
                {
                    if (String.IsNullOrEmpty(curr.Key) || String.IsNullOrEmpty(curr.Value)) continue;
                    if (excludedAdded == 0) excluded += "'" + _Database.Sanitize(curr.Value) + "'";
                    else excluded += ",'" + _Database.Sanitize(curr.Value) + "'";
                    excludedAdded++;
                }
            }

            #endregion

            #region Get-Matches

            query = DocumentsMatchingTermsQuery(indexStart, maxResults, required, excluded);
            result = _Database.Query(query);

            List<ParsedDocument> ret = new List<ParsedDocument>();
            if (result != null && result.Rows != null && result.Rows.Count > 0)
            {
                foreach (DataRow curr in result.Rows)
                {
                    string guid = curr["parseddocguid"].ToString();
                    ParsedDocument parsedDoc = _Database.SelectByGUID<ParsedDocument>(guid);
                    if (parsedDoc != null && parsedDoc != default(ParsedDocument)) ret.Add(parsedDoc);
                }
            }

            #endregion

            return ret;
        }

        private string DocumentsMatchingTermsQuery(int indexStart, int maxResults, string required, string excluded)
        {
            string query =
                "SELECT DISTINCT a.parseddocguid " +
                "FROM termdocs a " +
                "WHERE " +
                "  indexguid = '" + _Database.Sanitize(_GUID) + "' " +
                "  AND termguid in (" + required + ") ";

            if (!String.IsNullOrEmpty(excluded)) query +=
                "  AND NOT EXISTS ( " +
                "    SELECT 1 " +
                "      FROM termdocs b " +
                "      WHERE " +
                "        a.parseddocsguid = b.parseddocsguid " +
                "        AND b.termguid in (" + excluded + ") " +
                "    ) ";

            switch (_DatabaseSettings.Type)
            {
                case DbTypes.MsSql:
                    query += "OFFSET " + indexStart + " ROWS FETCH NEXT " + maxResults + " ROWS ONLY ";
                    break;
                case DbTypes.MySql:
                    query += "LIMIT " + indexStart + ", " + maxResults + " ";
                    break;
                case DbTypes.PgSql:
                    query += "OFFSET " + indexStart + " LIMIT " + maxResults + " ";
                    break;
                case DbTypes.Sqlite:
                    query += "LIMIT " + maxResults + " OFFSET " + indexStart + " ";
                    break;
            }

            return query;
        }

        private List<ParsedDocument> GetDocumentsForTerms(Dictionary<string, string> terms, int? indexStart, int? maxResults, out long entryCount, out long docsCount, out bool endReached)
        {
            #region Variables

            endReached = false;
            entryCount = 0;
            docsCount = 0;
            string query = null;
            DataTable result = null;

            #endregion

            #region Check-End 

            query =
                "SELECT " +
                "  COUNT(*) AS entryCount, " +
                "  COUNT(DISTINCT(parseddocguid)) AS docsCount " +
                "FROM termdocs " +
                "WHERE indexguid = '" + _Database.Sanitize(_GUID) + "'";

            result = _Database.Query(query);
            entryCount = Convert.ToInt32(result.Rows[0]["entryCount"]);
            docsCount = Convert.ToInt32(result.Rows[0]["docsCount"]);
            if (Convert.ToInt32(indexStart) + Convert.ToInt32(maxResults) > entryCount) endReached = true;

            #endregion

            #region Prepare-Strings

            int added = 0;
            string termsStr = "";
            if (terms != null && terms.Count > 0)
            {
                foreach (KeyValuePair<string, string> curr in terms)
                {
                    if (String.IsNullOrEmpty(curr.Key) || String.IsNullOrEmpty(curr.Value)) continue;
                    if (added == 0) termsStr += "'" + _Database.Sanitize(curr.Value) + "'";
                    else termsStr += ",'" + _Database.Sanitize(curr.Value) + "'";
                    added++;
                }
            }

            #endregion

            #region Get-Matches

            query =
                "SELECT DISTINCT a.parseddocguid " +
                "FROM termdocs a " +
                "WHERE " +
                "  indexguid = '" + _Database.Sanitize(_GUID) + " " +
                "  AND termguid in (" + termsStr + ") ";

            result = _Database.Query(query);

            #endregion

            #region Retrieve-Parsed-Documents

            List<string> parsedDocGuids = new List<string>();
            if (result != null && result.Rows != null && result.Rows.Count > 0)
            {
                foreach (DataRow curr in result.Rows)
                {
                    parsedDocGuids.Add(curr["parseddocguid"].ToString());
                }
            }

            Expression e = new Expression("guid", Operators.In, parsedDocGuids);
            List<ParsedDocument> ret = _Database.SelectMany<ParsedDocument>(null, null, e, "ORDER BY id DESC");

            #endregion

            return ret;
        }

        private void DocumentMatchesOptionalTerms(ParsedDocument doc, object parseResult, List<string> queryTerms, Dictionary<string, string> terms, out decimal? score, out Dictionary<string, string> matched)
        {
            score = null;
            matched = new Dictionary<string, string>();
            if (terms == null || terms.Count < 1) return;
            if (queryTerms == null || queryTerms.Count < 1) return;

            int termsTotal = queryTerms.Count;
            int matchCount = 0;

            List<string> docTerms = GetParseResultTerms(doc.Type, parseResult);
            if (docTerms == null || docTerms.Count < 1) return;

            foreach (KeyValuePair<string, string> currTerm in terms)
            {
                if (docTerms.Contains(currTerm.Key))
                {
                    matchCount++;
                    matched.Add(currTerm.Key, currTerm.Value);
                }
            }

            score = Convert.ToDecimal(matchCount) / Convert.ToDecimal(termsTotal);
            // Console.WriteLine("Match count: " + matchCount + " / Terms total: " + termsTotal + " = Score: " + score);
        }

        private object GetParseResultFromParsedDocumentGuid(DocType type, string guid)
        {
            byte[] data = _ParsedDocsStorage.Get(guid).Result;
            if (data == null) return null;

            if (type == DocType.Html)
            {
                return Common.DeserializeJson<HtmlParseResult>(data);
            }
            else if (type == DocType.Json)
            {
                return Common.DeserializeJson<JsonParseResult>(data);
            }
            else if (type == DocType.Sql)
            {
                return Common.DeserializeJson<SqlParseResult>(data);
            }
            else if (type == DocType.Text)
            {
                return Common.DeserializeJson<TextParseResult>(data);
            }
            else if (type == DocType.Xml)
            {
                return Common.DeserializeJson<XmlParseResult>(data);
            }
            else
            {
                throw new ArgumentException("Unknown doucment type: " + type.ToString());
            }
        }

        private List<string> GetParseResultTerms(DocType type, object parseResult)
        {
            if (type == DocType.Html)
            {
                return ((HtmlParseResult)parseResult).Tokens;
            }
            else if (type == DocType.Json)
            {
                return ((JsonParseResult)parseResult).Tokens;
            }
            else if (type == DocType.Sql)
            {
                return ((SqlParseResult)parseResult).Tokens;
            }
            else if (type == DocType.Text)
            {
                return ((TextParseResult)parseResult).Tokens;
            }
            else if (type == DocType.Xml)
            {
                return ((XmlParseResult)parseResult).Tokens;
            }
            else
            {
                throw new ArgumentException("Unknown doucment type: " + type.ToString());
            }
        }

        private bool DocumentMatchesFilters(ParsedDocument doc, object parseResult, List<SearchFilter> requiredFilters, List<SearchFilter> excludeFilters)
        {
            if (requiredFilters == null) requiredFilters = new List<SearchFilter>();
            if (excludeFilters == null) excludeFilters = new List<SearchFilter>();
            if (requiredFilters.Count < 1 && excludeFilters.Count < 1) return true;

            if (requiredFilters.Count > 0)
            {
                foreach (SearchFilter filter in requiredFilters)
                {
                    if (!DocumentMatchesFilter(doc, parseResult, filter)) return false;
                }
            }

            if (excludeFilters.Count > 0)
            {
                foreach (SearchFilter filter in excludeFilters)
                {
                    if (DocumentMatchesFilter(doc, parseResult, filter)) return false;
                }
            }

            return true;
        }

        private void DocumentMatchesOptionalFilters(ParsedDocument doc, object parseResult, List<SearchFilter> optionalFilters, out decimal? score, out List<SearchFilter> matched)
        {
            score = null;
            matched = new List<SearchFilter>();
            if (optionalFilters == null || optionalFilters.Count < 1) return;

            int filtersTotal = optionalFilters.Count;
            int matchCount = 0;

            List<string> docTerms = GetParseResultTerms(doc.Type, parseResult);
            if (docTerms == null || docTerms.Count < 1) return;

            foreach (SearchFilter filter in optionalFilters)
            {
                // Console.Write("Evaluating filter: " + filter.Field + " " + filter.Condition.ToString() + " " + filter.Value + ": ");
                if (DocumentMatchesFilter(doc, parseResult, filter))
                {
                    // Console.WriteLine("MATCH");
                    matchCount++;
                    matched.Add(filter);
                }
                else
                {
                    // Console.WriteLine("NO MATCH");
                }
            }

            score = Convert.ToDecimal(matchCount) / Convert.ToDecimal(filtersTotal);
            // Console.WriteLine("Match count: " + matchCount + " / Filters total: " + filtersTotal + " = Score: " + score);
        }

        private bool DocumentMatchesFilter(ParsedDocument doc, object parseResult, SearchFilter filter)
        {
            if (filter == null) return true;
            List<DataNode> nodes = null;

            if (doc.Type == DocType.Html) return false;
            else if (doc.Type == DocType.Json) nodes = ((JsonParseResult)parseResult).Flattened;
            else if (doc.Type == DocType.Sql) nodes = ((SqlParseResult)parseResult).Flattened;
            else if (doc.Type == DocType.Text) return false;
            else if (doc.Type == DocType.Xml) nodes = ((XmlParseResult)parseResult).Flattened;
            else throw new ArgumentException("Unknown doucment type: " + doc.Type.ToString());

            if (nodes != null && nodes.Count > 0) return DataNodeMatchExists(nodes, filter);
            else return false;
        }

        private bool DataNodeMatchExists(List<DataNode> nodes, SearchFilter filter)
        {
            decimal nodeDecimalVal = 0m;
            decimal filterDecimalVal = 0m;

            foreach (DataNode node in nodes)
            {
                if (node.Key.Equals(filter.Field))
                {
                    switch (filter.Condition)
                    {
                        case SearchCondition.Contains:
                            if (node.Data == null && String.IsNullOrEmpty(filter.Value)) return true;
                            if (node.Data == null) return false;
                            if (node.Data.ToString().Contains(filter.Value)) return true;
                            return false;
                        case SearchCondition.ContainsNot:
                            if (node.Data == null && String.IsNullOrEmpty(filter.Value)) return false;
                            if (node.Data == null) return true;
                            if (node.Data.ToString().Contains(filter.Value)) return false;
                            return true;
                        case SearchCondition.EndsWith:
                            if (node.Data == null && String.IsNullOrEmpty(filter.Value)) return true;
                            if (node.Data == null) return false;
                            if (node.Data.ToString().EndsWith(filter.Value)) return true;
                            return false;
                        case SearchCondition.Equals:
                            if (node.Data == null && String.IsNullOrEmpty(filter.Value)) return true;
                            if (node.Data == null) return false;
                            if (node.Data.ToString().Equals(filter.Value)) return true;
                            return false;
                        case SearchCondition.GreaterThan:
                            if (node.Data == null) return false;
                            if (String.IsNullOrEmpty(node.Data.ToString())) return false;
                            if (Decimal.TryParse(node.Data.ToString(), out nodeDecimalVal))
                            {
                                if (Decimal.TryParse(filter.Value, out filterDecimalVal))
                                {
                                    return (nodeDecimalVal > filterDecimalVal);
                                }
                            }
                            return false;
                        case SearchCondition.GreaterThanOrEqualTo:
                            if (node.Data == null) return false;
                            if (String.IsNullOrEmpty(node.Data.ToString())) return false;
                            if (Decimal.TryParse(node.Data.ToString(), out nodeDecimalVal))
                            {
                                if (Decimal.TryParse(filter.Value, out filterDecimalVal))
                                {
                                    return (nodeDecimalVal >= filterDecimalVal);
                                }
                            }
                            return false;
                        case SearchCondition.IsNotNull:
                            if (node.Data != null) return true;
                            return false;
                        case SearchCondition.IsNull:
                            if (node.Data == null) return true;
                            return false;
                        case SearchCondition.LessThan:
                            if (node.Data == null) return false;
                            if (String.IsNullOrEmpty(node.Data.ToString())) return false;
                            if (Decimal.TryParse(node.Data.ToString(), out nodeDecimalVal))
                            {
                                if (Decimal.TryParse(filter.Value, out filterDecimalVal))
                                {
                                    return (nodeDecimalVal < filterDecimalVal);
                                }
                            }
                            return false;
                        case SearchCondition.LessThanOrEqualTo:
                            if (node.Data == null) return false;
                            if (String.IsNullOrEmpty(node.Data.ToString())) return false;
                            if (Decimal.TryParse(node.Data.ToString(), out nodeDecimalVal))
                            {
                                if (Decimal.TryParse(filter.Value, out filterDecimalVal))
                                {
                                    return (nodeDecimalVal <= filterDecimalVal);
                                }
                            }
                            return false;
                        case SearchCondition.NotEquals:
                            if (node.Data == null && String.IsNullOrEmpty(filter.Value)) return false;
                            if (node.Data == null) return true;
                            if (node.Data.ToString().Equals(filter.Value)) return false;
                            return true;
                        case SearchCondition.StartsWith:
                            if (node.Data == null && String.IsNullOrEmpty(filter.Value)) return true;
                            if (node.Data == null) return false;
                            if (node.Data.ToString().StartsWith(filter.Value)) return true;
                            return false;
                        default:
                            throw new ArgumentException("Unknown search condition type: " + filter.Condition.ToString());
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private-Enumeration-Methods

        private Komodo.Classes.EnumerationResult EnumerateInternal(EnumerationQuery query)
        {
            Komodo.Classes.EnumerationResult result = new Komodo.Classes.EnumerationResult(query);

            Expression e = new Expression("id", Operators.GreaterThan, 0);
            e.PrependAnd("indexguid", Operators.Equals, _GUID);

            if (query.Filters != null)
            {
                foreach (SearchFilter filter in query.Filters)
                {
                    if (!String.IsNullOrEmpty(filter.Field))
                    {
                        e.PrependAnd(FilterToExpression(filter));
                    }
                }
            }

            List<SourceDocument> matches = _Database.SelectMany<SourceDocument>(query.StartIndex, query.MaxResults, e, "ORDER BY id DESC");

            result.Matches = matches;
            result.Success = true;
            result.Time.End = DateTime.Now.ToUniversalTime();
            return result;
        }

        private Expression FilterToExpression(SearchFilter filter)
        {
            switch (filter.Condition)
            {
                case SearchCondition.Contains:
                    return new Expression(filter.Field, Operators.Contains, filter.Value);
                case SearchCondition.ContainsNot:
                    return new Expression(filter.Field, Operators.ContainsNot, filter.Value);
                case SearchCondition.EndsWith:
                    return new Expression(filter.Field, Operators.EndsWith, filter.Value);
                case SearchCondition.Equals:
                    return new Expression(filter.Field, Operators.Equals, filter.Value);
                case SearchCondition.GreaterThan:
                    return new Expression(filter.Field, Operators.GreaterThan, filter.Value);
                case SearchCondition.GreaterThanOrEqualTo:
                    return new Expression(filter.Field, Operators.GreaterThanOrEqualTo, filter.Value);
                case SearchCondition.IsNotNull:
                    return new Expression(filter.Field, Operators.IsNotNull, null);
                case SearchCondition.IsNull:
                    return new Expression(filter.Field, Operators.IsNull, null);
                case SearchCondition.LessThan:
                    return new Expression(filter.Field, Operators.LessThan, filter.Value);
                case SearchCondition.LessThanOrEqualTo:
                    return new Expression(filter.Field, Operators.LessThanOrEqualTo, filter.Value);
                case SearchCondition.NotEquals:
                    return new Expression(filter.Field, Operators.NotEquals, filter.Value);
                case SearchCondition.StartsWith:
                    return new Expression(filter.Field, Operators.StartsWith, filter.Value);
                default:
                    throw new ArgumentException("Unknown filter condition: " + filter.Condition.ToString());
            }
        }

        #endregion
    }
}
