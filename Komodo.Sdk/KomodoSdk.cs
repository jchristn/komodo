using System;
using System.Collections.Generic; 
using System.IO; 
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 
using RestWrapper;
using Newtonsoft.Json; 
using Komodo.Sdk.Classes;  
using Index = Komodo.Sdk.Classes.Index;

namespace Komodo.Sdk
{
    /// <summary>
    /// SDK for Komodo information storage, search, and retrieval platform.
    /// </summary>
    public class KomodoSdk
    {
        #region Public-Members

        /// <summary>
        /// Accept or decline server certificates that cannot be verified or are invalid.
        /// </summary>
        public bool AcceptInvalidCertificates { get; set; }

        #endregion

        #region Private-Members

        private string _Endpoint;
        private string _ApiKey;
        private Dictionary<string, string> _AuthHeaders = new Dictionary<string, string>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initialize the SDK.
        /// </summary>
        /// <param name="endpoint">Endpoint URL of the form http://[hostname]:[port]/.</param>
        /// <param name="apiKey">API key to use when accessing Komodo.</param> 
        public KomodoSdk(string endpoint, string apiKey)
        {
            if (String.IsNullOrEmpty(endpoint)) throw new ArgumentNullException(nameof(endpoint));
            if (String.IsNullOrEmpty(apiKey)) throw new ArgumentNullException(nameof(apiKey));

            if (!endpoint.EndsWith("/")) endpoint += "/";

            _Endpoint = endpoint;
            _ApiKey = apiKey;

            _AuthHeaders = new Dictionary<string, string>();
            _AuthHeaders.Add("x-api-key", _ApiKey);

            AcceptInvalidCertificates = true;
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
            return KomodoCommon.SerializeJson(this, pretty);
        }

        /// <summary>
        /// Test authenticated connectivity to Komodo.
        /// </summary>
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>True if successful.</returns>
        public async Task<bool> Loopback(CancellationToken token = default)
        {
            try
            {
                RestRequest req = new RestRequest(
                    _Endpoint + "loopback",
                    HttpMethod.GET,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(token).ConfigureAwait(false);

                if (resp != null && resp.StatusCode == 200) return true;
                else return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// Return the list of indices available on the server.
        /// </summary> 
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>List of index names.</returns>
        public async Task<List<string>> GetIndices(CancellationToken token = default)
        {
            try
            {
                RestRequest req = new RestRequest(
                    _Endpoint + "indices",
                    HttpMethod.GET,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<List<string>>(respData);
                }

                return new List<string>();
            }
            catch (TaskCanceledException)
            {
                return new List<string>();
            }
            catch (OperationCanceledException)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Retrieve index details.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>Index.</returns>
        public async Task<Index> GetIndex(string indexName, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            try
            {
                string url = indexName;

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.GET,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<Index>(respData);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve statistics related to an index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param> 
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>Index statistics.</returns>
        public async Task<IndexStats> GetIndexStats(string indexName, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            try
            {
                string url = indexName + "/stats";

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.GET,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<IndexStats>(respData);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Creates an index.
        /// </summary> 
        /// <param name="index">Index.</param> 
        /// <param name="token">Cancellation token used to cancel the request.</param>
        public async Task CreateIndex(Index index, CancellationToken token = default)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (String.IsNullOrEmpty(index.Name)) throw new ArgumentNullException(nameof(index.Name));

            try
            {
                string url = "indices";

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.POST,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(KomodoCommon.SerializeJson(index, true), token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// Deletes an index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="cleanup">True to delete files associated with the index.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param> 
        public async Task DeleteIndex(string indexName, bool cleanup, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            try
            {
                string url = indexName;
                if (cleanup) url += "?cleanup";

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.DELETE,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// Add a document to the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docName">Name of the document.</param>
        /// <param name="sourceUrl">Source URL for the data (overrides 'data' parameter).</param>
        /// <param name="title">Title for the document.</param>
        /// <param name="tags">Document tags.</param>
        /// <param name="docType">Type of document.</param>
        /// <param name="data">Data from the document.</param> 
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>Index result.</returns>
        public Task<IndexResult> AddDocument(string indexName, string docName, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data, CancellationToken token = default)
        {
            return AddDocument(indexName, docName, null, sourceUrl, title, tags, docType, data, token);
        }

        /// <summary>
        /// Add a document to the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docName">Name of the document.</param>
        /// <param name="docGuid">Document GUID.</param>
        /// <param name="sourceUrl">Source URL for the data (overrides 'data' parameter).</param>
        /// <param name="title">Title for the document.</param>
        /// <param name="tags">Document tags.</param>
        /// <param name="docType">Type of document.</param>
        /// <param name="data">Data from the document.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param> 
        /// <returns>Index result.</returns>
        public async Task<IndexResult> AddDocument(string indexName, string docName, string docGuid, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docName)) throw new ArgumentNullException(nameof(docName));
            if (String.IsNullOrEmpty(sourceUrl)
                && (data == null || data.Length < 1)) throw new ArgumentException("Either sourceUrl or data must be populated.");

            try
            {
                string docTypeStr = DocTypeString(docType);

                string url = indexName;
                if (!String.IsNullOrEmpty(docGuid)) url += "/" + docGuid;
                url += "?type=" + docTypeStr;

                if (!String.IsNullOrEmpty(sourceUrl)) url += "&url=" + WebUtility.UrlEncode(sourceUrl);
                if (!String.IsNullOrEmpty(title)) url += "&title=" + WebUtility.UrlEncode(title);
                if (!String.IsNullOrEmpty(docName)) url += "&name=" + WebUtility.UrlEncode(docName);

                if (tags != null && tags.Count > 0)
                {
                    url += "&tags=" + WebUtility.UrlEncode(KomodoCommon.StringListToCsv(tags));
                }

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.POST,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(data).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<IndexResult>(respData);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Add a document to the specified index asynchronously and expect a postback with the results.
        /// </summary>
        /// <param name="indexName">Name of the index.</param> 
        /// <param name="sourceUrl">Source URL for the data (overrides 'data' parameter).</param>
        /// <param name="title">Title for the document.</param>
        /// <param name="tags">Document tags.</param>
        /// <param name="docType">Type of document.</param>
        /// <param name="data">Data from the document.</param>
        /// <param name="postbackUrl">URL to which results should be POSTed back.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>Index result.</returns>
        public Task<IndexResult> AddDocumentAsync(string indexName, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data, string postbackUrl, CancellationToken token = default)
        {
            return AddDocumentAsync(indexName, null, sourceUrl, title, tags, docType, data, postbackUrl, token);
        }

        /// <summary>
        /// Add a document to the specified index asynchronously and expect a postback with the results.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docGuid">Document GUID.</param>
        /// <param name="sourceUrl">Source URL for the data (overrides 'data' parameter).</param>
        /// <param name="title">Title for the document.</param>
        /// <param name="tags">Document tags.</param>
        /// <param name="docType">Type of document.</param>
        /// <param name="data">Data from the document.</param>
        /// <param name="postbackUrl">URL to which results should be POSTed back.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>Index result.</returns>
        public async Task<IndexResult> AddDocumentAsync(string indexName, string docGuid, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data, string postbackUrl, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceUrl)
                && (data == null || data.Length < 1)) throw new ArgumentException("Either sourceUrl or data must be populated.");

            try
            {
                string docTypeStr = DocTypeString(docType);

                string url = indexName;
                if (!String.IsNullOrEmpty(docGuid)) url += "/" + docGuid;
                url += "?type=" + docTypeStr;
                url += "&async";

                if (!String.IsNullOrEmpty(postbackUrl)) url += "&postback=" + WebUtility.UrlEncode(postbackUrl);
                if (!String.IsNullOrEmpty(sourceUrl)) url += "&url=" + WebUtility.UrlEncode(sourceUrl);
                if (!String.IsNullOrEmpty(title)) url += "&title=" + WebUtility.UrlEncode(title);

                if (tags != null && tags.Count > 0)
                {
                    url += "&tags=" + WebUtility.UrlEncode(KomodoCommon.StringListToCsv(tags));
                }

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.POST,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(data, token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<IndexResult>(respData);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Store a document in the specified index without parsing and indexing.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceUrl">Source URL for the data (overrides 'data' parameter).</param>
        /// <param name="title">Title for the document.</param>
        /// <param name="tags">Document tags.</param>
        /// <param name="docType">Type of document.</param>
        /// <param name="data">Data from the document.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param> 
        /// <returns>Index result.</returns>
        public Task<IndexResult> StoreDocument(string indexName, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data, CancellationToken token = default)
        {
            return StoreDocument(indexName, null, sourceUrl, title, tags, docType, data, token);
        }

        /// <summary>
        /// Store a document in the specified index without parsing and indexing.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docGuid">Document GUID.</param>
        /// <param name="sourceUrl">Source URL for the data (overrides 'data' parameter).</param>
        /// <param name="title">Title for the document.</param>
        /// <param name="tags">Document tags.</param>
        /// <param name="docType">Type of document.</param>
        /// <param name="data">Data from the document.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param> 
        /// <returns>Index result.</returns>
        public async Task<IndexResult> StoreDocument(string indexName, string docGuid, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceUrl)
                && (data == null || data.Length < 1)) throw new ArgumentException("Either sourceUrl or data must be populated.");

            try
            {
                string docTypeStr = DocTypeString(docType);

                string url = indexName + "?type=" + docTypeStr + "&bypass";
                if (!String.IsNullOrEmpty(docGuid)) url += "/" + docGuid;
                if (!String.IsNullOrEmpty(sourceUrl)) url += "&url=" + WebUtility.UrlEncode(sourceUrl);
                if (!String.IsNullOrEmpty(title)) url += "&title=" + WebUtility.UrlEncode(title);

                if (tags != null && tags.Count > 0)
                {
                    url += "&tags=" + WebUtility.UrlEncode(KomodoCommon.StringListToCsv(tags));
                }

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.POST,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(data, token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<IndexResult>(respData);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve a document source from the specified index.
        /// </summary>
        /// <param name="name">Name of the index.</param>
        /// <param name="docId">Document ID.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>Komodo object.</returns>
        public async Task<DocumentData> GetSourceDocument(string name, string docId, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            try
            {
                string url = name + "/" + docId;

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.GET,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    return new DocumentData(resp.ContentType, resp.ContentLength, resp.Data);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve metadata for a document from the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docId">Document ID.</param> 
        /// <param name="token">Cancellation token used to cancel the request.</param>
        /// <returns>DocumentMetadata.</returns>
        public async Task<DocumentMetadata> GetDocumentMetadata(string indexName, string docId, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            try
            {
                string url = indexName + "/" + docId + "?parsed&pretty";

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.GET,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<DocumentMetadata>(respData);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Deletes a document from the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docId">Document ID.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param> 
        public async Task DeleteDocument(string indexName, string docId, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            try
            {
                string url = indexName + "/" + docId;

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.DELETE,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        /// <summary>
        /// Search the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">Search query.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param> 
        /// <returns>Search result.</returns>
        public async Task<SearchResult> Search(string indexName, SearchQuery query, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (query == null) throw new ArgumentNullException(nameof(query));

            try
            {
                string url = indexName;

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.PUT,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(KomodoCommon.SerializeJson(query, true), token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<SearchResult>(respData);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Enumerate source documents in the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">Enumeration query.</param>
        /// <param name="token">Cancellation token used to cancel the request.</param> 
        /// <returns>Enumeration result.</returns>
        public async Task<EnumerationResult> Enumerate(string indexName, EnumerationQuery query, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (query == null) throw new ArgumentNullException(nameof(query));

            try
            {
                string url = indexName + "?enumerate";

                RestRequest req = new RestRequest(
                    _Endpoint + url,
                    HttpMethod.PUT,
                    _AuthHeaders,
                    "application/json");

                req.IgnoreCertificateErrors = AcceptInvalidCertificates;

                RestResponse resp = await req.SendAsync(KomodoCommon.SerializeJson(query, true), token).ConfigureAwait(false);

                KomodoException e = KomodoException.FromRestResponse(resp);
                if (e != null) throw e;

                if (resp.Data != null && resp.ContentLength > 0)
                {
                    byte[] respData = KomodoCommon.StreamToBytes(resp.Data);
                    return KomodoCommon.DeserializeJson<EnumerationResult>(respData);
                }

                return null;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        #endregion

        #region Private-Methods

        private string DocTypeString(DocType docType)
        {
            switch (docType)
            {
                case DocType.Html:
                    return "html";
                case DocType.Json:
                    return "json";
                case DocType.Sql:
                    return "sql";
                case DocType.Text:
                    return "text";
                case DocType.Xml:
                    return "xml";
                case DocType.Unknown:
                    return "unknown";
            }

            throw new ArgumentException("Unknown DocType: " + docType.ToString());
        }
         
        #endregion
    }
}
