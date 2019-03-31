using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using RestWrapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Komodo.Core
{
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
        public bool Loopback()
        {
            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + "loopback",
                "application/json",
                "GET",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                null);

            if (resp != null && resp.StatusCode == 200)
            {
                Debug.WriteLine("KomodoSdk Loopback success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk Loopback failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Return the list of indices available on the server.
        /// </summary>
        /// <param name="indices">List of string index names.</param>
        /// <returns>True if successful.</returns>
        public bool GetIndices(out List<string> indices)
        {
            indices = new List<string>();
            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + "indices",
                "application/json",
                "GET",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                null);

            if (resp != null && resp.StatusCode == 200 && resp.Data != null && resp.Data.Length > 0)
            {
                indices = Common.DeserializeJson<List<string>>(resp.Data);
                Debug.WriteLine("KomodoSdk GetIndices returning " + indices.Count + " entries");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk GetIndices failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Retrieve index details.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="index">Index details.</param>
        /// <returns>True if successful.</returns>
        public bool GetIndex(string indexName, out Index index)
        {
            index = null;
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            string url = indexName;

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "GET",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                null);

            if (resp != null && resp.StatusCode >= 200 && resp.StatusCode <= 299)
            { 
                index = Common.DeserializeJson<Index>(resp.Data);
                Debug.WriteLine("KomodoSdk GetIndex returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk GetIndex failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Retrieve statistics related to an index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="stats">Index statistics.</param>
        /// <returns>True if successful.</returns>
        public bool GetIndexStats(string indexName, out IndexStats stats)
        {
            stats = null;
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            string url = indexName + "/stats";

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "GET",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                null);

            if (resp != null && resp.StatusCode >= 200 && resp.StatusCode <= 299)
            { 
                stats = Common.DeserializeJson<IndexStats>(resp.Data);
                Debug.WriteLine("KomodoSdk GetIndexStats returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk GetIndexStats failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Creates an index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="options">Index options.</param>
        /// <returns>True if successful.</returns>
        public bool CreateIndex(string indexName, Index index)
        { 
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (index == null) throw new ArgumentNullException(nameof(index));

            string url = "indices";

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "POST",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                Encoding.UTF8.GetBytes(Common.SerializeJson(index, true)));

            if (resp != null && resp.StatusCode >= 200 && resp.StatusCode <= 299)
            { 
                Debug.WriteLine("KomodoSdk CreateIndex returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk CreateIndex failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Deletes an index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="cleanup">True to delete files associated with the index.</param>
        /// <returns>True if successful.</returns>
        public bool DeleteIndex(string indexName, bool cleanup)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName)); 

            string url = indexName;
            if (cleanup) url += "?cleanup=true";

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "DELETE",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                null);

            if (resp != null && resp.StatusCode >= 200 && resp.StatusCode <= 299)
            {
                Debug.WriteLine("KomodoSdk DeleteIndex returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk DeleteIndex failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Add a document to the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceUrl">Source URL for the data (overrides 'data' parameter).</param>
        /// <param name="docType">Type of document.</param>
        /// <param name="data">Data from the document.</param>
        /// <returns>True if successful.</returns>
        public bool AddDocument(string indexName, string sourceUrl, DocType docType, byte[] data)
        {
            IndexResponse resp = null;
            return AddDocument(indexName, sourceUrl, docType, data, out resp);
        }

        /// <summary>
        /// Add a document to the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="sourceUrl">Source URL for the data (overrides 'data' parameter).</param>
        /// <param name="docType">Type of document.</param>
        /// <param name="data">Data from the document.</param>
        /// <param name="response">Response data from the server.</param>
        /// <returns>True if successful.</returns>
        public bool AddDocument(string indexName, string sourceUrl, DocType docType, byte[] data, out IndexResponse response)
        {
            response = null;
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(sourceUrl)
                && (data == null || data.Length < 1)) throw new ArgumentException("Either sourceUrl or data must be populated.");

            string docTypeStr = DocTypeString(docType);

            string url = indexName + "?type=" + docTypeStr;
            if (!String.IsNullOrEmpty(sourceUrl)) url += "&url=" + WebUtility.UrlEncode(sourceUrl);

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "POST",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                data);

            if (resp != null && resp.StatusCode == 200 && resp.Data != null && resp.Data.Length > 0)
            {
                response = Common.DeserializeJson<IndexResponse>(resp.Data);
                Debug.WriteLine("KomodoSdk AddDocument returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk AddDocument failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Retrieve a document source from the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docId">Document ID.</param>
        /// <param name="data">Data from the document.</param>
        /// <returns>True if successful.</returns>
        public bool GetSourceDocument(string indexName, string docId, out byte[] data)
        {
            data = null;
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            string url = indexName + "/" + docId;

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "GET",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                null);

            if (resp != null && resp.StatusCode == 200 && resp.Data != null && resp.Data.Length > 0)
            {
                data = new byte[resp.Data.Length];
                Buffer.BlockCopy(resp.Data, 0, data, 0, resp.Data.Length);
                Debug.WriteLine("KomodoSdk GetSourceDocument returning " + resp.Data.Length + " bytes");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk GetSourceDocument failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Retrieve a parsed, indexed document from the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docId">Document ID.</param>
        /// <param name="doc">Indexed and parsed document.</param>
        /// <returns>True if successful.</returns>
        public bool GetParsedDocument(string indexName, string docId, out IndexedDoc doc)
        {
            doc = null;
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            string url = indexName + "/" + docId + "?parsed=true&pretty=true";

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "GET",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                null);

            if (resp != null && resp.StatusCode == 200 && resp.Data != null && resp.Data.Length > 0)
            {
                doc = Common.DeserializeJson<IndexedDoc>(resp.Data);
                Debug.WriteLine("KomodoSdk GetParsedDocument returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk GetParsedDocument failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Deletes a document from the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="docId">Document ID.</param>
        /// <returns>True if successful.</returns>
        public bool DeleteDocument(string indexName, string docId)
        { 
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(docId)) throw new ArgumentNullException(nameof(docId));

            string url = indexName + "/" + docId;

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "DELETE",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                null);

            if (resp != null && resp.StatusCode >= 200 && resp.StatusCode <= 299)
            { 
                Debug.WriteLine("KomodoSdk DeleteDocument returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk DeleteDocument failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Search the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">Search query.</param>
        /// <param name="result">Search result.</param>
        /// <returns>True if successful.</returns>
        public bool Search(string indexName, SearchQuery query, out SearchResult result)
        {
            result = null;
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (query == null) throw new ArgumentNullException(nameof(query));
              
            string url = indexName;
            
            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "PUT",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                Encoding.UTF8.GetBytes(Common.SerializeJson(query, true)));

            if (resp != null && resp.StatusCode >= 200 && resp.StatusCode <= 299 && resp.Data != null && resp.Data.Length > 0)
            {
                result = Common.DeserializeJson<SearchResult>(resp.Data);
                Debug.WriteLine("KomodoSdk Search returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk Search failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
            }
        }

        /// <summary>
        /// Enumerate source documents in the specified index.
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="query">Enumeration query.</param>
        /// <param name="result">Enumeration result.</param>
        /// <returns>True if successful.</returns>
        public bool Enumerate(string indexName, EnumerationQuery query, out EnumerationResult result)
        {
            result = null;
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (query == null) throw new ArgumentNullException(nameof(query));

            string url = indexName + "/enumerate";

            RestResponse resp = RestRequest.SendRequestSafe(
                _Endpoint + url,
                "application/json",
                "PUT",
                null, null, false, AcceptInvalidCertificates, _AuthHeaders,
                Encoding.UTF8.GetBytes(Common.SerializeJson(query, true)));

            if (resp != null && resp.StatusCode >= 200 && resp.StatusCode <= 299 && resp.Data != null && resp.Data.Length > 0)
            {
                result = Common.DeserializeJson<EnumerationResult>(resp.Data);
                Debug.WriteLine("KomodoSdk Search returning success");
                return true;
            }
            else
            {
                Debug.WriteLine("KomodoSdk Enumerate failed");
                if (resp != null)
                {
                    Debug.WriteLine("Response:");
                    Debug.WriteLine(resp.ToString());
                }
                else
                {
                    Debug.WriteLine("Response is null");
                }
                return false;
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
            }

            throw new ArgumentException("Unknown DocType: " + docType.ToString());
        }

        #endregion
    }
}
