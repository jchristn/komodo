﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks; 
using RestWrapper; 

namespace Komodo.Crawler
{
    /// <summary>
    /// Crawler for retrieving data from a web address.
    /// </summary>
    public class HttpCrawler
    {
        #region Public-Members

        /// <summary>
        /// The HTTP method to use.
        /// </summary>
        public Komodo.Crawler.HttpMethod Method = Komodo.Crawler.HttpMethod.GET;

        /// <summary>
        /// The source URL for the object to be crawled.
        /// </summary>
        public string SourceUrl = null; 

        /// <summary>
        /// The headers to apply to the HTTP request.
        /// </summary>
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        /// <summary>
        /// The username to use when accessing the object.
        /// </summary>
        public string Username = null;

        /// <summary>
        /// The password to use when accessing the object.
        /// </summary>
        public string Password = null;

        /// <summary>
        /// Specify whether or not HTTP credentials should be encoded.
        /// </summary>
        public bool EncodeCredentials = false;

        /// <summary>
        /// Specify whether or not the HTTP client should accept invalid certificates or those that cannot be verified.
        /// </summary>
        public bool IgnoreCertificateErrors = true;

        /// <summary>
        /// The data to send when accessing the object.
        /// </summary>
        public byte[] Data = null;

        #endregion

        #region Private-Members

        private RestRequest _RestRequest = null;
        private RestResponse _RestResponse = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="sourceUrl">The source URL for the object to be crawled.</param>
        public HttpCrawler(string sourceUrl)
        {
            if (String.IsNullOrEmpty(sourceUrl)) throw new ArgumentNullException(nameof(sourceUrl));
            if (!sourceUrl.StartsWith("http://") && !sourceUrl.StartsWith("https://")) throw new ArgumentException("Source URL must begin with either http:// or https://."); 

            SourceUrl = sourceUrl; 
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve the object.
        /// </summary>
        /// <returns>Crawl result.</returns>
        public HttpCrawlResult Get()
        {
            _RestRequest = new RestRequest(
                SourceUrl,
                ConvertHttpMethod(Method),
                Headers,
                null);

            SetRestRequestValues();
             
            if (Data == null || Data.Length < 1) _RestResponse = _RestRequest.Send();
            else _RestResponse = _RestRequest.Send(Data);

            HttpCrawlResult ret = new HttpCrawlResult();

            if (_RestResponse != null)
            {
                if (_RestResponse.StatusCode >= 200 && _RestResponse.StatusCode <= 299) ret.Success = true;
                ret.ContentLength = _RestResponse.ContentLength;
                ret.DataStream = _RestResponse.Data;
                ret.Headers = _RestResponse.Headers;
            }
             
            ret.Time.End = DateTime.Now;
            return ret;
        }

        /// <summary>
        /// Download the object to the supplied filename.
        /// </summary>
        /// <param name="filename">The filename where the object should be saved.</param>
        /// <returns>Crawl result.</returns>
        public HttpCrawlResult Download(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            _RestRequest = new RestRequest(
                SourceUrl,
                ConvertHttpMethod(Method),
                Headers,
                null);

            SetRestRequestValues();

            if (Data == null || Data.Length < 1) _RestResponse = _RestRequest.Send();
            else _RestResponse = _RestRequest.Send(Data);

            HttpCrawlResult ret = new HttpCrawlResult();

            if (_RestResponse != null)
            {
                if (_RestResponse.StatusCode >= 200 && _RestResponse.StatusCode <= 299)
                {
                    ret.Success = true;
                    ret.Filename = filename;

                    using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        if (_RestResponse.ContentLength > 0)
                        {
                            long bytesRemaining = _RestResponse.ContentLength;

                            while (bytesRemaining > 0)
                            {
                                byte[] buffer = new byte[65536];
                                int bytesRead = _RestResponse.Data.Read(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    bytesRemaining -= bytesRead;
                                    fs.Write(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                }

                ret.ContentLength = _RestResponse.ContentLength;
                ret.DataStream = null;
                ret.Headers = _RestResponse.Headers;
            }

            ret.Time.End = DateTime.Now;
            return ret;
        }

        /// <summary>
        /// Retrieve the object.
        /// </summary>
        /// <param name="result">Crawl result.</param>
        /// <returns>True if successful.</returns>
        public bool TryGet(out HttpCrawlResult result)
        {
            result = null;

            try
            {
                result = Get();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Download the object to the supplied filename.
        /// </summary>
        /// <param name="filename">The filename where the object should be saved.</param>
        /// <param name="result">Crawl result.</param>
        /// <returns>True if successful.</returns>
        public bool TryDownload(string filename, out HttpCrawlResult result)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            result = null;

            try
            {
                result = Download(filename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieve the object asynchronously.
        /// </summary>
        /// <returns>Crawl result.</returns>
        public async Task<HttpCrawlResult> GetAsync()
        {
            _RestRequest = new RestRequest(
                SourceUrl,
                ConvertHttpMethod(Method),
                Headers,
                null);

            SetRestRequestValues();

            if (Data == null || Data.Length < 1) _RestResponse = await _RestRequest.SendAsync();
            else _RestResponse = await _RestRequest.SendAsync(Data);

            HttpCrawlResult ret = new HttpCrawlResult();

            if (_RestResponse != null)
            {
                if (_RestResponse.StatusCode >= 200 && _RestResponse.StatusCode <= 299) ret.Success = true;
                ret.ContentLength = _RestResponse.ContentLength;
                ret.DataStream = _RestResponse.Data;
                ret.Headers = _RestResponse.Headers;
            }

            ret.Time.End = DateTime.Now;
            return ret;
        }

        /// <summary>
        /// Download the object asynchronously to the supplied filename.
        /// </summary>
        /// <param name="filename">The filename where the object should be saved.</param>
        /// <returns>Crawl result.</returns>
        public async Task<HttpCrawlResult> DownloadAsync(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            _RestRequest = new RestRequest(
                SourceUrl,
                ConvertHttpMethod(Method),
                Headers,
                null);

            SetRestRequestValues();

            if (Data == null || Data.Length < 1) _RestResponse = await _RestRequest.SendAsync();
            else _RestResponse = await _RestRequest.SendAsync(Data);

            HttpCrawlResult ret = new HttpCrawlResult();

            if (_RestResponse != null)
            {
                if (_RestResponse.StatusCode >= 200 && _RestResponse.StatusCode <= 299)
                {
                    ret.Success = true;
                    ret.Filename = filename;

                    using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        if (_RestResponse.ContentLength > 0)
                        {
                            long bytesRemaining = _RestResponse.ContentLength;

                            while (bytesRemaining > 0)
                            {
                                byte[] buffer = new byte[65536];
                                int bytesRead = await _RestResponse.Data.ReadAsync(buffer, 0, buffer.Length);
                                if (bytesRead > 0)
                                {
                                    bytesRemaining -= bytesRead;
                                    await fs.WriteAsync(buffer, 0, bytesRead);
                                }
                            }
                        }
                    }
                }

                ret.ContentLength = _RestResponse.ContentLength;
                ret.DataStream = null;
                ret.Headers = _RestResponse.Headers;
            }

            ret.Time.End = DateTime.Now;
            return ret;
        }

        #endregion

        #region Private-Methods

        private void SetRestRequestValues()
        {
            _RestRequest.User = Username;
            _RestRequest.Password = Password;
            _RestRequest.EncodeCredentials = EncodeCredentials;
            _RestRequest.IgnoreCertificateErrors = IgnoreCertificateErrors;
        }

        private RestWrapper.HttpMethod ConvertHttpMethod(HttpMethod method)
        {
            switch (method)
            {
                case HttpMethod.GET:
                    return RestWrapper.HttpMethod.GET;
                case HttpMethod.PUT:
                    return RestWrapper.HttpMethod.PUT;
                case HttpMethod.POST:
                    return RestWrapper.HttpMethod.POST;
                case HttpMethod.DELETE:
                    return RestWrapper.HttpMethod.DELETE;
                case HttpMethod.PATCH:
                    return RestWrapper.HttpMethod.PATCH;
                default:
                    throw new ArgumentException("Unsupported HTTP method.");
            }
        }

        #endregion 
    }
}
