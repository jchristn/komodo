using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlobHelper;

namespace Komodo.Crawler
{
    /// <summary>
    /// Crawler for retrieving data from Azure BLOB storage.
    /// </summary>
    public class AzureBlobCrawler
    {
        #region Public-Members

        /// <summary>
        /// The name of the account.
        /// </summary>
        public string AccountName = null;

        /// <summary>
        /// The endpoint to be crawled.
        /// </summary>
        public string Endpoint = null;

        /// <summary>
        /// The container containing the object.
        /// </summary>
        public string Container = null;

        /// <summary>
        /// The key of the object to crawl.
        /// </summary>
        public string Key = null;

        /// <summary>
        /// The access key to use while crawling.
        /// </summary>
        public string AccessKey = null;
         
        #endregion

        #region Private-Members

        private AzureSettings _AzureSettings = null;
        private Blobs _Blobs = null;

        #endregion

        #region Constructors-and-Factories
         
        /// <summary>
        /// Instantiate the object to crawl on Azure BLOB storage.
        /// </summary>
        /// <param name="accountName">The name of the account.</param>
        /// <param name="container">Name of the container.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="accessKey">Access key.</param>
        /// <param name="key">The object key.</param>
        public AzureBlobCrawler(string accountName, string container, string endpoint, string accessKey, string key)
        {
            AccountName = accountName ?? throw new ArgumentNullException(nameof(accountName));
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            AccessKey = accessKey ?? throw new ArgumentNullException(nameof(accessKey)); 

            InitializeBlobs();
        }
          
        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve the object.
        /// </summary>
        /// <returns>Crawl result.</returns>
        public AzureBlobCrawlResult Get()
        {
            AzureBlobCrawlResult ret = new AzureBlobCrawlResult();

            try
            {
                BlobData data = _Blobs.GetStream(Key).Result;
                ret.Metadata = ObjectMetadata.FromBlobMetadata(_Blobs.GetMetadata(Key).Result);
                ret.ContentLength = data.ContentLength;
                ret.DataStream = data.Data;
                ret.Success = true;
            }
            catch (Exception)
            {

            }

            ret.Time.End = DateTime.Now;
            return ret; 
        }

        /// <summary>
        /// Download the object to the supplied filename.
        /// </summary>
        /// <param name="filename">The filename where the object should be saved.</param>
        /// <returns>Crawl result.</returns>
        public AzureBlobCrawlResult Download(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            AzureBlobCrawlResult ret = new AzureBlobCrawlResult();

            try
            {
                BlobData data = _Blobs.GetStream(Key).Result;
                ret.Metadata = ObjectMetadata.FromBlobMetadata(_Blobs.GetMetadata(Key).Result);
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
            catch (Exception)
            {

            }

            ret.Time.End = DateTime.Now;
            return ret; 
        }

        /// <summary>
        /// Retrieve the object.
        /// </summary>
        /// <param name="result">Crawl result.</param>
        /// <returns>True if successful.</returns>
        public bool TryGet(out AzureBlobCrawlResult result)
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
        public bool TryDownload(string filename, out AzureBlobCrawlResult result)
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
        /// <returns>Crawl result.</returns>
        public async Task<AzureBlobCrawlResult> GetAsync()
        {
            AzureBlobCrawlResult ret = new AzureBlobCrawlResult();

            try
            {
                BlobData data = await _Blobs.GetStream(Key);
                ret.Metadata = ObjectMetadata.FromBlobMetadata(await _Blobs.GetMetadata(Key));
                ret.ContentLength = data.ContentLength;
                ret.DataStream = data.Data;
                ret.Success = true;
            }
            catch (Exception)
            {

            }

            ret.Time.End = DateTime.Now;
            return ret;
        }

        /// <summary>
        /// Download the object asynchronously to the supplied filename.
        /// </summary>
        /// <param name="filename">The filename where the object should be saved.</param>
        /// <returns>Crawl result.</returns>
        public async Task<AzureBlobCrawlResult> DownloadAsync(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            AzureBlobCrawlResult ret = new AzureBlobCrawlResult();

            try
            {
                BlobData data = await _Blobs.GetStream(Key);
                ret.Metadata = ObjectMetadata.FromBlobMetadata(await _Blobs.GetMetadata(Key));
                ret.ContentLength = data.ContentLength;

                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (data.ContentLength > 0)
                    {
                        long bytesRemaining = data.ContentLength;

                        while (bytesRemaining > 0)
                        {
                            byte[] buffer = new byte[65536];
                            int bytesRead = await data.Data.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                await fs.WriteAsync(buffer, 0, bytesRead);
                            }
                        }
                    }
                }

                ret.Filename = filename;
                ret.DataStream = null;
                ret.Success = true;
            }
            catch (Exception)
            {

            }

            ret.Time.End = DateTime.Now;
            return ret;
        }

        #endregion

        #region Private-Methods
         
        private void InitializeBlobs()
        {
            _AzureSettings = new AzureSettings(AccountName, AccessKey, Endpoint, Container);
            _Blobs = new Blobs(_AzureSettings);
        }

        #endregion 
    }
}
