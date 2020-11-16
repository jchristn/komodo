using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private Dictionary<string, string> _AuthHeaders;

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
        /// Test authenticated connectivity to Komodo.
        /// </summary>
        /// <returns>True if successful.</returns>
        public async Task<bool> Loopback()
        {
            RestRequest req = new RestRequest(
                _Endpoint + "loopback",
                HttpMethod.GET,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync();

            if (resp != null && resp.StatusCode == 200) return true;
            else return false;
        }

        /// <summary>
        /// Return the list of indices available on the server.
        /// </summary> 
        /// <returns>List of index names.</returns>
        public async Task<List<string>> GetIndices()
        {
            RestRequest req = new RestRequest(
                _Endpoint + "indices",
                HttpMethod.GET,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync();

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<List<string>>(respData);
            }

            return new List<string>();
        }

        /// <summary>
        /// Retrieve index details.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <returns>Index.</returns>
        public async Task<Index> GetIndex(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            string url = indexName;

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.GET,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync();

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<Index>(respData);
            }

            return null;
        }

        /// <summary>
        /// Retrieve statistics related to an index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param> 
        /// <returns>Index statistics.</returns>
        public async Task<IndexStats> GetIndexStats(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            string url = indexName + "/stats";

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.GET,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync();

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<IndexStats>(respData);
            }

            return null;
        }

        /// <summary>
        /// Creates an index.
        /// </summary> 
        /// <param name="index">Index.</param> 
        public async Task CreateIndex(Index index)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            if (String.IsNullOrEmpty(index.Name)) throw new ArgumentNullException(nameof(index.Name));

            string url = "indices";

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.POST,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync(SerializeJson(index, true));

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;
        }

        /// <summary>
        /// Deletes an index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="cleanup">True to delete files associated with the index.</param> 
        public async Task DeleteIndex(string indexName, bool cleanup)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            string url = indexName;
            if (cleanup) url += "?cleanup";

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.DELETE,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync();

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;
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
        /// <returns>Index result.</returns>
        public async Task<IndexResult> AddDocument(string indexName, string docName, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data)
        {
            return await AddDocument(indexName, docName, null, sourceUrl, title, tags, docType, data);
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
        /// <returns>Index result.</returns>
        public async Task<IndexResult> AddDocument(string indexName, string docName, string docGuid, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docName)) throw new ArgumentNullException(nameof(docName));
            if (String.IsNullOrEmpty(sourceUrl)
                && (data == null || data.Length < 1)) throw new ArgumentException("Either sourceUrl or data must be populated.");

            string docTypeStr = DocTypeString(docType);

            string url = indexName;
            if (!String.IsNullOrEmpty(docGuid)) url += "/" + docGuid;
            url += "?type=" + docTypeStr;

            if (!String.IsNullOrEmpty(sourceUrl)) url += "&url=" + WebUtility.UrlEncode(sourceUrl);
            if (!String.IsNullOrEmpty(title)) url += "&title=" + WebUtility.UrlEncode(title);
            if (!String.IsNullOrEmpty(docName)) url += "&name=" + WebUtility.UrlEncode(docName);

            if (tags != null && tags.Count > 0)
            {
                url += "&tags=" + WebUtility.UrlEncode(StringListToCsv(tags));
            }

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.POST,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync(data);

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<IndexResult>(respData);
            }

            return null;
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
        /// <returns>Index result.</returns>
        public async Task<IndexResult> AddDocumentAsync(string indexName, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data, string postbackUrl)
        {
            return await AddDocumentAsync(indexName, null, sourceUrl, title, tags, docType, data, postbackUrl);
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
        /// <returns>Index result.</returns>
        public async Task<IndexResult> AddDocumentAsync(string indexName, string docGuid, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data, string postbackUrl)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceUrl)
                && (data == null || data.Length < 1)) throw new ArgumentException("Either sourceUrl or data must be populated.");

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
                url += "&tags=" + WebUtility.UrlEncode(StringListToCsv(tags));
            }

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.POST,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync(data);

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<IndexResult>(respData);
            }

            return null;
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
        /// <returns>Index result.</returns>
        public async Task<IndexResult> StoreDocument(string indexName, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data)
        {
            return await StoreDocument(indexName, null, sourceUrl, title, tags, docType, data);
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
        /// <returns>Index result.</returns>
        public async Task<IndexResult> StoreDocument(string indexName, string docGuid, string sourceUrl, string title, List<string> tags, DocType docType, byte[] data)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceUrl)
                && (data == null || data.Length < 1)) throw new ArgumentException("Either sourceUrl or data must be populated.");

            string docTypeStr = DocTypeString(docType);

            string url = indexName + "?type=" + docTypeStr + "&bypass";
            if (!String.IsNullOrEmpty(docGuid)) url += "/" + docGuid;
            if (!String.IsNullOrEmpty(sourceUrl)) url += "&url=" + WebUtility.UrlEncode(sourceUrl);
            if (!String.IsNullOrEmpty(title)) url += "&title=" + WebUtility.UrlEncode(title);

            if (tags != null && tags.Count > 0)
            {
                url += "&tags=" + WebUtility.UrlEncode(StringListToCsv(tags));
            }
             
            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.POST,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync(data);

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<IndexResult>(respData);
            }

            return null;
        }

        /// <summary>
        /// Retrieve a document source from the specified index.
        /// </summary>
        /// <param name="name">Name of the index.</param>
        /// <param name="docId">Document ID.</param>
        /// <returns>Komodo object.</returns>
        public async Task<DocumentData> GetSourceDocument(string name, string docId)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            string url = name + "/" + docId;

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.GET,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync();

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                return new DocumentData(resp.ContentType, resp.ContentLength, resp.Data);
            }

            return null;
        }

        /// <summary>
        /// Retrieve metadata for a document from the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docId">Document ID.</param> 
        /// <returns>DocumentMetadata.</returns>
        public async Task<DocumentMetadata> GetDocumentMetadata(string indexName, string docId)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            string url = indexName + "/" + docId + "?parsed&pretty";

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.GET,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync();

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<DocumentMetadata>(respData);
            }

            return null;
        }

        /// <summary>
        /// Deletes a document from the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docId">Document ID.</param> 
        public async Task DeleteDocument(string indexName, string docId)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            string url = indexName + "/" + docId;

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.DELETE,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync();

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;
        }

        /// <summary>
        /// Search the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">Search query.</param> 
        /// <returns>Search result.</returns>
        public async Task<SearchResult> Search(string indexName, SearchQuery query)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (query == null) throw new ArgumentNullException(nameof(query));

            string url = indexName;

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.PUT,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync(SerializeJson(query, true));

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<SearchResult>(respData);
            }

            return null;
        }

        /// <summary>
        /// Enumerate source documents in the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">Enumeration query.</param> 
        /// <returns>Enumeration result.</returns>
        public async Task<EnumerationResult> Enumerate(string indexName, EnumerationQuery query)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (query == null) throw new ArgumentNullException(nameof(query));

            string url = indexName + "?enumerate";

            RestRequest req = new RestRequest(
                _Endpoint + url,
                HttpMethod.PUT,
                _AuthHeaders,
                "application/json");

            req.IgnoreCertificateErrors = AcceptInvalidCertificates;

            RestResponse resp = await req.SendAsync(SerializeJson(query, true));

            KomodoException e = KomodoException.FromRestResponse(resp);
            if (e != null) throw e;

            if (resp.Data != null && resp.ContentLength > 0)
            {
                byte[] respData = StreamToBytes(resp.Data);
                return DeserializeJson<EnumerationResult>(respData);
            }

            return null;
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

        private static string SerializeJson(object obj, bool pretty)
        {
            if (obj == null) return null;
            string json;

            if (pretty)
            {
                json = JsonConvert.SerializeObject(
                  obj,
                  Newtonsoft.Json.Formatting.Indented,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                  });
            }
            else
            {
                json = JsonConvert.SerializeObject(obj,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc
                  });
            }

            return json;
        }

        private static T DeserializeJson<T>(string json)
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json)); 
            return JsonConvert.DeserializeObject<T>(json);
        }

        private static T DeserializeJson<T>(byte[] data)
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
            return DeserializeJson<T>(Encoding.UTF8.GetString(data));
        }

        private static byte[] StreamToBytes(Stream input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new InvalidOperationException("Input stream is not readable");

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
         
        private static List<string> CsvToStringList(string csv)
        {
            if (String.IsNullOrEmpty(csv))
            {
                return null;
            }

            List<string> ret = new List<string>();

            string[] array = csv.Split(',');

            if (array != null && array.Length > 0)
            {
                foreach (string curr in array)
                {
                    if (String.IsNullOrEmpty(curr)) continue;
                    ret.Add(curr.Trim());
                }
            }

            return ret;
        }

        private static string StringListToCsv(List<string> strings)
        {
            if (strings == null || strings.Count < 1) return null;

            int added = 0;
            string ret = "";

            foreach (string curr in strings)
            {
                if (added == 0)
                {
                    ret += curr;
                }
                else
                {
                    ret += "," + curr;
                }

                added++;
            }

            return ret;
        }

        #endregion
    }
}
