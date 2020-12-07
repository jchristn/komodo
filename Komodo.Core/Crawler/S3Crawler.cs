using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlobHelper;
using Komodo;

namespace Komodo.Crawler
{
    /// <summary>
    /// Crawler for retrieving data from Amazon S3.
    /// </summary>
    public class S3Crawler
    {
        #region Public-Members
         
        /// <summary>
        /// The endpoint to be crawled, if using S3 compatible storage.
        /// </summary>
        public string Endpoint = null;

        /// <summary>
        /// Enable or disable SSL, if using S3 compatible storage.
        /// </summary>
        public bool Ssl = true;

        /// <summary>
        /// The S3 region.
        /// </summary>
        public AwsRegion Region = AwsRegion.USWest1;

        /// <summary>
        /// The bucket containing the object.
        /// </summary>
        public string Bucket = null;

        /// <summary>
        /// The key of the object to crawl.
        /// </summary>
        public string Key = null;

        /// <summary>
        /// The access key to use while crawling.
        /// </summary>
        public string AccessKey = null;

        /// <summary>
        /// The secret key to use while crawling.
        /// </summary>
        public string SecretKey = null;

        /// <summary>
        /// The base URL of the form https://[hostname]:[port]/{bucket}/{key}/.
        /// </summary>
        public string BaseUrl = null;

        #endregion

        #region Private-Members

        private AwsSettings _AwsSettings = null;
        private Blobs _Blobs = null;

        #endregion

        #region Constructors-and-Factories
         
        /// <summary>
        /// Instantiate the object to crawl on AWS S3.
        /// </summary>
        /// <param name="bucket">The name of the bucket.</param>
        /// <param name="key">The object key.</param>
        /// <param name="accessKey">Access key.</param>
        /// <param name="secretKey">Secret key.</param>
        /// <param name="region">The AWS region.</param>
        public S3Crawler(string bucket, string key, string accessKey, string secretKey, AwsRegion region)
        {
            Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            AccessKey = accessKey ?? throw new ArgumentNullException(nameof(accessKey));
            SecretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            Region = region;

            InitializeBlobs();
        }

        /// <summary>
        /// Instantiate the object to crawl on S3-compatible storage.
        /// </summary>
        /// <param name="endpoint">Endpoint for the S3-compatible storage, of the form http://[hostname]:[port]/.</param>
        /// <param name="ssl">Enable or disable SSL.</param>
        /// <param name="bucket">The name of the bucket.</param>
        /// <param name="key">The object key.</param>
        /// <param name="accessKey">Access key.</param>
        /// <param name="secretKey">Secret key.</param>
        /// <param name="region">The AWS region.</param>
        /// <param name="baseUrl">Base URL to use for objects, i.e. https://[bucketname].s3.[regionname].amazonaws.com/.  For non-S3 endpoints, use {bucket} and {key} to indicate where these values should be inserted, i.e. http://{bucket}.[hostname]:[port]/{key} or https://[hostname]:[port]/{bucket}/key.</param>
        public S3Crawler(string endpoint, bool ssl, string bucket, string key, string accessKey, string secretKey, AwsRegion region, string baseUrl)
        {
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            Ssl = ssl;
            Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            AccessKey = accessKey ?? throw new ArgumentNullException(nameof(accessKey));
            SecretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            Region = region;
            BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));

            InitializeBlobs();
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
        /// Retrieve the object.
        /// </summary>
        /// <returns>Crawl result.</returns>
        public CrawlResult Get()
        {
            CrawlResult ret = new CrawlResult();

            try
            {
                BlobData data = _Blobs.GetStream(Key).Result;
                ret.Metadata = CrawlResult.ObjectMetadata.FromBlobMetadata(_Blobs.GetMetadata(Key).Result);
                ret.ContentLength = data.ContentLength;
                ret.DataStream = data.Data;
                ret.Success = true;
            }
            catch (Exception e)
            {
                ret.Exception = e;
            }

            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret; 
        }

        /// <summary>
        /// Download the object to the supplied filename.
        /// </summary>
        /// <param name="filename">The filename where the object should be saved.</param>
        /// <returns>Crawl result.</returns>
        public CrawlResult Download(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            CrawlResult ret = new CrawlResult();

            try
            {
                BlobData data = _Blobs.GetStream(Key).Result;
                ret.Metadata = CrawlResult.ObjectMetadata.FromBlobMetadata(_Blobs.GetMetadata(Key).Result);
                ret.ContentLength = data.ContentLength; 

                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (data.ContentLength > 0)
                    {
                        long bytesRemaining = data.ContentLength;

                        while (bytesRemaining > 0)
                        {
                            byte[] buffer = new byte[65536];
                            int bytesRead = data.Data.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                fs.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }

                ret.Filename = filename;
                ret.DataStream = null;
                ret.Success = true;
            }
            catch (Exception e)
            {
                ret.Exception = e;
            }

            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret; 
        }

        /// <summary>
        /// Retrieve the object.
        /// </summary>
        /// <param name="result">Crawl result.</param>
        /// <returns>True if successful.</returns>
        public bool TryGet(out CrawlResult result)
        {
            result = null;

            try
            {
                result = Get();
                return result.Success;
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
        public bool TryDownload(string filename, out CrawlResult result)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            result = null;

            try
            {
                result = Download(filename);
                return result.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieve the object asynchronously.
        /// </summary>
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>Crawl result.</returns>
        public async Task<CrawlResult> GetAsync(CancellationToken token = default)
        {
            CrawlResult ret = new CrawlResult();

            try
            {
                BlobData data = await _Blobs.GetStream(Key, token).ConfigureAwait(false);
                ret.Metadata = CrawlResult.ObjectMetadata.FromBlobMetadata(await _Blobs.GetMetadata(Key, token).ConfigureAwait(false));
                ret.ContentLength = data.ContentLength;
                ret.DataStream = data.Data;
                ret.Success = true;
            }
            catch (Exception e)
            {
                ret.Exception = e;
            }

            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret;
        }

        /// <summary>
        /// Download the object asynchronously to the supplied filename.
        /// </summary>
        /// <param name="filename">The filename where the object should be saved.</param>
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>Crawl result.</returns>
        public async Task<CrawlResult> DownloadAsync(string filename, CancellationToken token = default)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            CrawlResult ret = new CrawlResult();

            try
            {
                BlobData data = await _Blobs.GetStream(Key, token).ConfigureAwait(false);
                ret.Metadata = CrawlResult.ObjectMetadata.FromBlobMetadata(await _Blobs.GetMetadata(Key, token).ConfigureAwait(false));
                ret.ContentLength = data.ContentLength;

                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (data.ContentLength > 0)
                    {
                        long bytesRemaining = data.ContentLength;

                        while (bytesRemaining > 0)
                        {
                            byte[] buffer = new byte[65536];
                            int bytesRead = await data.Data.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                await fs.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                            }
                        }
                    }
                }

                ret.Filename = filename;
                ret.DataStream = null;
                ret.Success = true;
            }
            catch (Exception e)
            {
                ret.Exception = e;
            }

            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret;
        }

        #endregion

        #region Private-Methods
         
        private void InitializeBlobs()
        {
            if (String.IsNullOrEmpty(Endpoint))
            {
                _AwsSettings = new AwsSettings(AccessKey, SecretKey, Region, Bucket);
            }
            else
            {
                _AwsSettings = new AwsSettings(Endpoint, Ssl, AccessKey, SecretKey, Region.ToString(), Bucket, BaseUrl);
            }

            _Blobs = new Blobs(_AwsSettings);
        }

        #endregion 
    }
}
