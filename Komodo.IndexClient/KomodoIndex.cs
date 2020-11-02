using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlobHelper;
using Watson.ORM;
using Watson.ORM.Core; 
using RestWrapper;
using Komodo.Classes; 
using Komodo.Parser;
using Komodo.Postings;
using Common = Komodo.Classes.Common;
using DbType = Komodo.Classes.DbType;
using EnumerationResult = Komodo.Classes.EnumerationResult;
using Index = Komodo.Classes.Index;

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

        /// <summary>
        /// Direct access to the underlying ORM.
        /// </summary>
        public WatsonORM ORM
        {
            get
            {
                return _ORM;
            }
        }

        #endregion

        #region Private-Members

        private string _Name = null;
        private string _GUID = null; 

        private DbSettings _DbSettings = null;
        private WatsonORM _ORM = null;

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
        public KomodoIndex(DbSettings dbSettings, StorageSettings sourceDocs, StorageSettings parsedDocs, StorageSettings postings, Index idx)
        {
            if (dbSettings == null) throw new ArgumentNullException(nameof(dbSettings));
            if (idx == null) throw new ArgumentNullException(nameof(idx));
            if (String.IsNullOrEmpty(idx.Name)) throw new ArgumentNullException(nameof(idx.Name));
            if (String.IsNullOrEmpty(idx.GUID)) throw new ArgumentNullException(nameof(idx.GUID));
            if (sourceDocs == null) throw new ArgumentNullException(nameof(sourceDocs));
            if (parsedDocs == null) throw new ArgumentNullException(nameof(parsedDocs));
            if (postings == null) throw new ArgumentNullException(nameof(postings)); 

            _DbSettings = dbSettings;
            _ORM = new WatsonORM(_DbSettings.ToDatabaseSettings());

            _ORM.InitializeDatabase();
            _ORM.InitializeTable(typeof(ApiKey));
            _ORM.InitializeTable(typeof(Index));
            _ORM.InitializeTable(typeof(Metadata));
            _ORM.InitializeTable(typeof(MetadataDocument));
            _ORM.InitializeTable(typeof(Node));
            _ORM.InitializeTable(typeof(ParsedDocument));
            _ORM.InitializeTable(typeof(Permission));
            _ORM.InitializeTable(typeof(SourceDocument));
            _ORM.InitializeTable(typeof(TermDoc));
            _ORM.InitializeTable(typeof(TermGuid));
            _ORM.InitializeTable(typeof(User));

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

            DbExpression eIndex = new DbExpression(
                _ORM.GetColumnName<SourceDocument>(nameof(SourceDocument.IndexGUID)),
                DbOperators.Equals,
                _GUID);

            while (true)
            {
                List<SourceDocument> sourceDocs = _ORM.SelectMany<SourceDocument>(0, 100, eIndex);
                if (sourceDocs == null || sourceDocs.Count < 1) break;

                foreach (SourceDocument sourceDoc in sourceDocs)
                {
                    DbExpression eSourceDoc = new DbExpression(
                        _ORM.GetColumnName<SourceDocument>(nameof(SourceDocument.GUID)),
                        DbOperators.Equals,
                        sourceDoc.GUID);

                    _ORM.DeleteMany<SourceDocument>(eSourceDoc);
                    await _SourceDocsStorage.Delete(sourceDoc.GUID);
                }
            }

            #endregion

            #region Parsed-Documents
             
            while (true)
            {
                List<ParsedDocument> parsedDocs = _ORM.SelectMany<ParsedDocument>(0, 100, eIndex);
                if (parsedDocs == null || parsedDocs.Count < 1) break;

                foreach (ParsedDocument parsedDoc in parsedDocs)
                { 
                    DbExpression eParsedDoc = new DbExpression(
                        _ORM.GetColumnName<ParsedDocument>(nameof(ParsedDocument.GUID)),
                        DbOperators.Equals,
                        parsedDoc.GUID);

                    _ORM.DeleteMany<ParsedDocument>(eParsedDoc);
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

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<SourceDocument>(nameof(SourceDocument.GUID)),
                DbOperators.Equals,
                sourceGuid);

            return _ORM.SelectFirst<SourceDocument>(e);
        }

        /// <summary>
        /// Retrieve a parsed document by the source document GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>ParsedDocument.</returns>
        public ParsedDocument GetParsedDocument(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<ParsedDocument>(nameof(ParsedDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);

            return _ORM.SelectFirst<ParsedDocument>(e); 
        }

        /// <summary>
        /// Retrieve parse result by source document GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>Parse result.</returns>
        public object GetParseResult(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<ParsedDocument>(nameof(ParsedDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);

            ParsedDocument parsed = _ORM.SelectFirst<ParsedDocument>(e);
            if (parsed == null) return null;

            return Common.DeserializeJson<ParseResult>(_ParsedDocsStorage.Get(parsed.GUID).Result); 
        }

        /// <summary>
        /// Retrieve postings by source document GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>Postings.</returns>
        public PostingsResult GetPostings(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid)); 
            ParsedDocument parsed = GetParsedDocument(sourceGuid);
            if (parsed == null) return null; 
            byte[] data = _PostingsStorage.Get(parsed.GUID).Result; 
            if (data == null) return null; 
            return Common.DeserializeJson<PostingsResult>(data);
        }

        /// <summary>
        /// Retrieve metadata documents attached to a source document GUID.
        /// Metadata documents serve as pointers to derived metadata files which have to be retrieved separately by their GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>List of metadata documents.</returns>
        public List<MetadataDocument> GetMetadataDocuments(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<MetadataDocument>(nameof(MetadataDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);

            return _ORM.SelectMany<MetadataDocument>(e);
        }

        /// <summary>
        /// Check if a source document exists by GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsSource(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<SourceDocument>(nameof(SourceDocument.GUID)),
                DbOperators.Equals,
                sourceGuid);

            SourceDocument ret = _ORM.SelectFirst<SourceDocument>(e);
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

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<ParsedDocument>(nameof(ParsedDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);

            ParsedDocument ret = _ORM.SelectFirst<ParsedDocument>(e);
            if (ret != null && ret != default(ParsedDocument)) return true;
            return false;
        }

        /// <summary>
        /// Check if metadata documents exist by the source document GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <returns>True if exists.</returns>
        public bool ExistsMetadata(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<MetadataDocument>(nameof(MetadataDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);

            MetadataDocument ret = _ORM.SelectFirst<MetadataDocument>(e);
            if (ret != null && ret != default(MetadataDocument)) return true;
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

            ret.Source = _ORM.Insert<SourceDocument>(sourceDoc);

            ret.Time.PersistSourceDocument.End = DateTime.Now.ToUniversalTime();

            #endregion

            #region Parse

            if (parse)
            {
                #region Parse

                ret.Time.Parse.Start = DateTime.Now.ToUniversalTime();
                ParseResult parseResult = null;

                if (sourceDoc.Type == DocType.Html)
                {
                    HtmlParser htmlParser = new HtmlParser();
                    parseResult = htmlParser.ParseBytes(data);
                }
                else if (sourceDoc.Type == DocType.Json)
                {
                    JsonParser jsonParser = new JsonParser();
                    parseResult = jsonParser.ParseBytes(data); 
                }
                else if (sourceDoc.Type == DocType.Text)
                {
                    TextParser textParser = new TextParser();
                    parseResult = textParser.ParseBytes(data); 
                }
                else if (sourceDoc.Type == DocType.Xml)
                {
                    XmlParser xmlParser = new XmlParser();
                    parseResult = xmlParser.ParseBytes(data); 
                } 

                ret.ParseResult = parseResult;
                ret.Time.Parse.End = DateTime.Now.ToUniversalTime();

                #endregion

                #region Generate-Postings

                if (ret.ParseResult != null)
                {
                    ret.Time.Postings.Start = DateTime.Now.ToUniversalTime();
                    PostingsGenerator postings = new PostingsGenerator(options);
                    ret.Postings = postings.Process(ret.ParseResult);
                    ret.Time.Postings.End = DateTime.Now.ToUniversalTime();
                }
                else
                {
                    ret.Time.Postings = null;
                }

                #endregion

                #region Persist-Parsed-Doc

                if (ret.ParseResult != null)
                {
                    ret.Time.PersistParsedDocument.Start = DateTime.Now.ToUniversalTime();
                    ret.Parsed = new ParsedDocument(
                        sourceDoc.GUID,
                        sourceDoc.OwnerGUID,
                        _GUID,
                        sourceDoc.Type,
                        data.Length,
                        ret.Postings.Terms.Count,
                        ret.Postings.Postings.Count);

                    ret.Parsed = _ORM.Insert<ParsedDocument>(ret.Parsed);
                    await _ParsedDocsStorage.Write(ret.Parsed.GUID, "application/json", Common.SerializeJson(ret.ParseResult, true));
                    ret.Time.PersistParsedDocument.End = DateTime.Now.ToUniversalTime();
                }
                else
                {
                    ret.Time.PersistParsedDocument = null;
                }

                #endregion

                #region Persist-Postings 

                if (ret.ParseResult != null)
                {
                    ret.Time.PersistPostingsDocument.Start = DateTime.Now.ToUniversalTime();
                    await _PostingsStorage.Write(ret.Parsed.GUID, "application/json", Common.SerializeJson(ret.Postings, true));
                    ret.Time.PersistPostingsDocument.End = DateTime.Now.ToUniversalTime();
                }
                else
                {
                    ret.Time.PersistPostingsDocument = null;
                }

                #endregion

                #region Persist-Terms

                if (ret.ParseResult != null)
                {
                    ret.Time.Terms.Start = DateTime.Now.ToUniversalTime();

                    // term, guid
                    Dictionary<string, string> termGuids = new Dictionary<string, string>();
                     
                    foreach (Token token in ret.ParseResult.Tokens)
                    {
                        DbExpression e = new DbExpression(
                            _ORM.GetColumnName<TermGuid>(nameof(TermGuid.Term)),
                            DbOperators.Equals,
                            token.Value);

                        TermGuid tg = _ORM.SelectFirst<TermGuid>(e);
                        if (tg == null || tg == default(TermGuid))
                        {
                            tg = new TermGuid();
                            tg.GUID = Guid.NewGuid().ToString();
                            tg.IndexGUID = _GUID;
                            tg.Term = token.Value;
                            tg = _ORM.Insert<TermGuid>(tg);
                        }

                        termGuids.Add(token.Value, tg.GUID);
                    }

                    // Term Docs
                    foreach (KeyValuePair<string, string> termGuid in termGuids)
                    {
                        TermDoc td = new TermDoc(_GUID, termGuid.Value, sourceDoc.GUID, ret.Parsed.GUID);
                        td = _ORM.Insert<TermDoc>(td);
                    }

                    ret.Time.Terms.End = DateTime.Now.ToUniversalTime();
                }
                else
                {
                    ret.Time.Terms = null;
                }

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
        /// Add metadata document to a source document.
        /// </summary>
        /// <param name="sourceDoc">Source document.</param>
        /// <param name="metadataDoc">Metadata document.</param>
        /// <returns>Metadata document.</returns>
        public MetadataDocument AddMetadata(SourceDocument sourceDoc, MetadataDocument metadataDoc)
        {
            if (sourceDoc == null) throw new ArgumentNullException(nameof(sourceDoc));
            if (metadataDoc == null) throw new ArgumentNullException(nameof(metadataDoc)); 

            metadataDoc.IndexGUID = _GUID;
            metadataDoc.OwnerGUID = sourceDoc.OwnerGUID;

            return _ORM.Insert<MetadataDocument>(metadataDoc);
        }
         
        /// <summary>
        /// Remove a document.  This will also delete parsed documents and metadata documents attached to the source document.
        /// Derived metadata documents will not be deleted; they will need to be deleted individually, each by their own GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        public void Remove(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));

            DbExpression eSourceDoc = new DbExpression(
                _ORM.GetColumnName<SourceDocument>(nameof(SourceDocument.GUID)),
                DbOperators.Equals,
                sourceGuid);

            SourceDocument sourceDoc = _ORM.SelectFirst<SourceDocument>(eSourceDoc);
            if (sourceDoc != null && sourceDoc != default(SourceDocument))
            {
                _ORM.Delete<SourceDocument>(sourceDoc);
                _SourceDocsStorage.Delete(sourceDoc.GUID);
            }

            DbExpression eParsedDoc = new DbExpression(
                _ORM.GetColumnName<ParsedDocument>(nameof(ParsedDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);
             
            List<ParsedDocument> parsedDocs = _ORM.SelectMany<ParsedDocument>(eParsedDoc);
            if (parsedDocs != null && parsedDocs.Count > 0)
            {
                foreach (ParsedDocument parsedDoc in parsedDocs)
                {
                    _ORM.Delete<ParsedDocument>(parsedDoc);
                    _ParsedDocsStorage.Delete(parsedDoc.GUID);
                }
            }

            DbExpression eMetadataDoc = new DbExpression(
                _ORM.GetColumnName<MetadataDocument>(nameof(ParsedDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);

            List<MetadataDocument> metadataDocs = _ORM.SelectMany<MetadataDocument>(eMetadataDoc);
            if (metadataDocs != null && metadataDocs.Count > 0)
            {
                foreach (MetadataDocument metadataDoc in metadataDocs)
                {
                    _ORM.Delete<MetadataDocument>(metadataDoc); 
                }
            }
        }

        /// <summary>
        /// Remove metadata associated with a document.
        /// Derived metadata documents will not be deleted; they will need to be deleted individually, each by their own GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        public void RemoveMetadata(string sourceGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));

            DbExpression eMetadataDoc = new DbExpression(
                _ORM.GetColumnName<MetadataDocument>(nameof(ParsedDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);

            List<MetadataDocument> metadataDocs = _ORM.SelectMany<MetadataDocument>(eMetadataDoc);
            if (metadataDocs != null && metadataDocs.Count > 0)
            {
                foreach (MetadataDocument metadataDoc in metadataDocs)
                {
                    _ORM.Delete<MetadataDocument>(metadataDoc);
                }
            }
        }

        /// <summary>
        /// Remove a specific metadata document associated with a document.
        /// Derived metadata documents will not be deleted; they will need to be deleted individually, each by their own GUID.
        /// </summary>
        /// <param name="sourceGuid">Source document GUID.</param>
        /// <param name="metadataDocGuid">Metadata document GUID.</param>
        public void RemoveMetadata(string sourceGuid, string metadataDocGuid)
        {
            if (String.IsNullOrEmpty(sourceGuid)) throw new ArgumentNullException(nameof(sourceGuid));
            if (String.IsNullOrEmpty(metadataDocGuid)) throw new ArgumentNullException(nameof(metadataDocGuid));

            DbExpression eMetadataDoc = new DbExpression(
                _ORM.GetColumnName<MetadataDocument>(nameof(MetadataDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                sourceGuid);

            eMetadataDoc.PrependAnd(new DbExpression(
                _ORM.GetColumnName<MetadataDocument>(nameof(MetadataDocument.GUID)),
                DbOperators.Equals,
                metadataDocGuid));

            List<MetadataDocument> metadataDocs = _ORM.SelectMany<MetadataDocument>(eMetadataDoc);
            if (metadataDocs != null && metadataDocs.Count > 0)
            {
                foreach (MetadataDocument metadataDoc in metadataDocs)
                {
                    _ORM.Delete<MetadataDocument>(metadataDoc);
                }
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
        public EnumerationResult Enumerate(EnumerationQuery query)
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
                "WHERE indexguid = '" + Sanitize(_GUID) + "'";

            DataTable result = _ORM.Query(query);
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
                "WHERE indexguid = '" + Sanitize(_GUID) + "'";

            result = _ORM.Query(query);
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
                "WHERE indexguid = '" + Sanitize(_GUID) + "'";

            result = _ORM.Query(query);
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
                _ORM.Dispose();
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

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<ParsedDocument>(nameof(ParsedDocument.SourceDocumentGUID)),
                DbOperators.Equals,
                guid);

            ParsedDocument ret = _ORM.SelectFirst<ParsedDocument>(e);
            if (ret != null) return ret.GUID;
            return null;
        }
        
        private string Sanitize(string val)
        {
            string ret = "";

            //
            // null, below ASCII range, above ASCII range
            //
            for (int i = 0; i < val.Length; i++)
            {
                if (((int)(val[i]) == 10) ||      // Preserve carriage return
                    ((int)(val[i]) == 13))        // and line feed
                {
                    ret += val[i];
                }
                else if ((int)(val[i]) < 32)
                {
                    continue;
                }
                else
                {
                    ret += val[i];
                }
            }

            //
            // double dash
            //
            int doubleDash = 0;
            while (true)
            {
                doubleDash = ret.IndexOf("--");
                if (doubleDash < 0)
                {
                    break;
                }
                else
                {
                    ret = ret.Remove(doubleDash, 2);
                }
            }

            //
            // open comment
            // 
            int openComment = 0;
            while (true)
            {
                openComment = ret.IndexOf("/*");
                if (openComment < 0) break;
                else
                {
                    ret = ret.Remove(openComment, 2);
                }
            }

            //
            // close comment
            //
            int closeComment = 0;
            while (true)
            {
                closeComment = ret.IndexOf("*/");
                if (closeComment < 0) break;
                else
                {
                    ret = ret.Remove(closeComment, 2);
                }
            }

            //
            // in-string replacement
            //
            ret = ret.Replace("'", "''");
            return ret;
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

            DbExpression eTerm = new DbExpression(
                _ORM.GetColumnName<TermGuid>(nameof(TermGuid.Term)),
                DbOperators.In, 
                masterTerms);

            List<TermGuid> masterTermsGuids = _ORM.SelectMany<TermGuid>(null, null, eTerm);

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
                        ParseResult parseResult = GetParseResultFromParsedDocumentGuid(currDoc.Type, currDoc.GUID);
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

                        #region Retrieve-Source-Document

                        SourceDocument sourceDoc = null; 

                        if (query.IncludeMetadata)
                        {
                            DbExpression eSource = new DbExpression(
                                _ORM.GetColumnName<SourceDocument>(nameof(SourceDocument.GUID)),
                                DbOperators.Equals,
                                currDoc.SourceDocumentGUID);

                            sourceDoc = _ORM.SelectFirst<SourceDocument>(eSource);
                        }

                        #endregion

                        #region Add-to-List

                        MatchedDocument matchDoc = new MatchedDocument();
                        matchDoc.DocumentType = currDoc.Type;
                        matchDoc.FiltersScore = filtersScore;
                        matchDoc.GUID = currDoc.SourceDocumentGUID;
                        matchDoc.Score = aggregateScore;
                        matchDoc.TermsScore = termsScore;
                        matchDoc.Metadata = sourceDoc;
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
                "WHERE indexguid = '" + Sanitize(_GUID) + "'";

            result = _ORM.Query(query);
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
                    if (requiredAdded == 0) required += "'" + Sanitize(curr.Value) + "'";
                    else required += ",'" + Sanitize(curr.Value) + "'";
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
                    if (excludedAdded == 0) excluded += "'" + Sanitize(curr.Value) + "'";
                    else excluded += ",'" + Sanitize(curr.Value) + "'";
                    excludedAdded++;
                }
            }

            #endregion

            #region Get-Matches

            query = DocumentsMatchingTermsQuery(indexStart, maxResults, required, excluded);
            result = _ORM.Query(query);

            List<ParsedDocument> ret = new List<ParsedDocument>();
            if (result != null && result.Rows != null && result.Rows.Count > 0)
            {
                foreach (DataRow curr in result.Rows)
                {
                    string guid = curr["parseddocguid"].ToString();

                    DbExpression eParsed = new DbExpression(
                        _ORM.GetColumnName<ParsedDocument>(nameof(ParsedDocument.GUID)),
                        DbOperators.Equals,
                        guid);

                    ParsedDocument parsedDoc = _ORM.SelectFirst<ParsedDocument>(eParsed);
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
                "  indexguid = '" + Sanitize(_GUID) + "' " +
                "  AND termguid in (" + required + ") ";

            if (!String.IsNullOrEmpty(excluded)) query +=
                "  AND NOT EXISTS ( " +
                "    SELECT 1 " +
                "      FROM termdocs b " +
                "      WHERE " +
                "        a.parseddocsguid = b.parseddocsguid " +
                "        AND b.termguid in (" + excluded + ") " +
                "    ) ";

            switch (_DbSettings.Type)
            {
                case DbType.SqlServer:
                    query += "OFFSET " + indexStart + " ROWS FETCH NEXT " + maxResults + " ROWS ONLY ";
                    break;
                case DbType.Mysql:
                    query += "LIMIT " + indexStart + ", " + maxResults + " ";
                    break;
                case DbType.Postgresql:
                    query += "OFFSET " + indexStart + " LIMIT " + maxResults + " ";
                    break;
                case DbType.Sqlite:
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
                "  COUNT(*) AS entrycount, " +
                "  COUNT(DISTINCT(parseddocguid)) AS docscount " +
                "FROM termdocs " +
                "WHERE indexguid = '" + Sanitize(_GUID) + "'";

            result = _ORM.Query(query);
            entryCount = Convert.ToInt32(result.Rows[0]["entrycount"]);
            docsCount = Convert.ToInt32(result.Rows[0]["docscount"]);
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
                    if (added == 0) termsStr += "'" + Sanitize(curr.Value) + "'";
                    else termsStr += ",'" + Sanitize(curr.Value) + "'";
                    added++;
                }
            }

            #endregion

            #region Get-Matches

            query =
                "SELECT DISTINCT a.parseddocguid " +
                "FROM termdocs a " +
                "WHERE " +
                "  indexguid = '" + Sanitize(_GUID) + " " +
                "  AND termguid in (" + termsStr + ") ";

            result = _ORM.Query(query);

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

            DbExpression eParsed = new DbExpression(
                _ORM.GetColumnName<ParsedDocument>(nameof(ParsedDocument.GUID)),
                DbOperators.In,
                parsedDocGuids);

            List<ParsedDocument> ret = _ORM.SelectMany<ParsedDocument>(null, null, eParsed);

            #endregion

            return ret;
        }

        private void DocumentMatchesOptionalTerms(ParsedDocument doc, ParseResult parseResult, List<string> queryTerms, Dictionary<string, string> terms, out decimal? score, out Dictionary<string, string> matched)
        {
            score = null;
            matched = new Dictionary<string, string>();
            if (terms == null || terms.Count < 1) return;
            if (queryTerms == null || queryTerms.Count < 1) return;

            int termsTotal = queryTerms.Count;
            int matchCount = 0;
             
            if (parseResult == null || parseResult.Tokens == null || parseResult.Tokens.Count < 1) return;

            foreach (KeyValuePair<string, string> currTerm in terms)
            {
                if (parseResult.Tokens.Any(t => t.Value.Equals(currTerm.Key)))
                {
                    matchCount++;
                    matched.Add(currTerm.Key, currTerm.Value);
                }
            }

            score = Convert.ToDecimal(matchCount) / Convert.ToDecimal(termsTotal); 
        }

        private ParseResult GetParseResultFromParsedDocumentGuid(DocType type, string guid)
        {
            byte[] data = _ParsedDocsStorage.Get(guid).Result;
            if (data == null) return null;
            return Common.DeserializeJson<ParseResult>(data); 
        }
         
        private bool DocumentMatchesFilters(ParsedDocument doc, ParseResult parseResult, List<SearchFilter> requiredFilters, List<SearchFilter> excludeFilters)
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

        private void DocumentMatchesOptionalFilters(ParsedDocument doc, ParseResult parseResult, List<SearchFilter> optionalFilters, out decimal? score, out List<SearchFilter> matched)
        {
            score = null;
            matched = new List<SearchFilter>();
            if (optionalFilters == null || optionalFilters.Count < 1) return;

            int filtersTotal = optionalFilters.Count;
            int matchCount = 0;

            if (parseResult == null || parseResult.Tokens == null || parseResult.Tokens.Count < 1) return;

            foreach (SearchFilter filter in optionalFilters)
            { 
                if (DocumentMatchesFilter(doc, parseResult, filter))
                { 
                    matchCount++;
                    matched.Add(filter);
                } 
            }

            score = Convert.ToDecimal(matchCount) / Convert.ToDecimal(filtersTotal); 
        }

        private bool DocumentMatchesFilter(ParsedDocument doc, ParseResult parseResult, SearchFilter filter)
        {
            if (filter == null) return true;

            if (doc.Type == DocType.Html || doc.Type == DocType.Text) return false; 
             
            if (parseResult.Flattened.Exists(n => n.Key.Equals(filter.Field)))
            {
                List<DataNode> filteredNodes = parseResult.Flattened.Where(n => n.Key.Equals(filter.Field)).ToList();
                if (filteredNodes == null || filteredNodes.Count < 1)
                {
                    return false;
                }
                else
                {
                    foreach (DataNode node in parseResult.Flattened)
                    {
                        if (filter.EvaluateValue(node.Data)) return true;
                    } 
                }
            }

            return false;
        }
         
        #endregion

        #region Private-Enumeration-Methods

        private EnumerationResult EnumerateInternal(EnumerationQuery query)
        {
            EnumerationResult result = new EnumerationResult(query);

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<SourceDocument>(nameof(SourceDocument.Id)),
                DbOperators.GreaterThan,
                0);

            e.PrependAnd(new DbExpression(
                _ORM.GetColumnName<SourceDocument>(nameof(SourceDocument.IndexGUID)),
                DbOperators.Equals,
                _GUID));

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

            List<SourceDocument> matches = _ORM.SelectMany<SourceDocument>(query.StartIndex, query.MaxResults, e);

            result.Matches = matches;
            result.Success = true;
            result.Time.End = DateTime.Now.ToUniversalTime();
            return result;
        }

        private DbExpression FilterToExpression(SearchFilter filter)
        {
            switch (filter.Condition)
            {
                case SearchCondition.Contains:
                    return new DbExpression(filter.Field, DbOperators.Contains, filter.Value);
                case SearchCondition.ContainsNot:
                    return new DbExpression(filter.Field, DbOperators.ContainsNot, filter.Value);
                case SearchCondition.EndsWith:
                    return new DbExpression(filter.Field, DbOperators.EndsWith, filter.Value);
                case SearchCondition.Equals:
                    return new DbExpression(filter.Field, DbOperators.Equals, filter.Value);
                case SearchCondition.GreaterThan:
                    return new DbExpression(filter.Field, DbOperators.GreaterThan, filter.Value);
                case SearchCondition.GreaterThanOrEqualTo:
                    return new DbExpression(filter.Field, DbOperators.GreaterThanOrEqualTo, filter.Value);
                case SearchCondition.IsNotNull:
                    return new DbExpression(filter.Field, DbOperators.IsNotNull, null);
                case SearchCondition.IsNull:
                    return new DbExpression(filter.Field, DbOperators.IsNull, null);
                case SearchCondition.LessThan:
                    return new DbExpression(filter.Field, DbOperators.LessThan, filter.Value);
                case SearchCondition.LessThanOrEqualTo:
                    return new DbExpression(filter.Field, DbOperators.LessThanOrEqualTo, filter.Value);
                case SearchCondition.NotEquals:
                    return new DbExpression(filter.Field, DbOperators.NotEquals, filter.Value);
                case SearchCondition.StartsWith:
                    return new DbExpression(filter.Field, DbOperators.StartsWith, filter.Value);
                default:
                    throw new ArgumentException("Unknown filter condition: " + filter.Condition.ToString());
            }
        }

        #endregion
    }
}
