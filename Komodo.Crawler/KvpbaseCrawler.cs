using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlobHelper;

namespace Komodo.Crawler
{
    /// <summary>
    /// Crawler for retrieving data from Kvpbase.
    /// </summary>
    public class KvpbaseCrawler
    {
        #region Public-Members
         
        /// <summary>
        /// The endpoint to be crawled.
        /// </summary>
        public string Endpoint = null;

        /// <summary>
        /// GUID of the user.
        /// </summary>
        public string UserGuid = null;

        /// <summary>
        /// The container containing the object.
        /// </summary>
        public string Container = null;

        /// <summary>
        /// The key of the object to crawl.
        /// </summary>
        public string Key = null;

        /// <summary>
        /// The API key to use while crawling.
        /// </summary>
        public string ApiKey = null;
         
        #endregion

        #region Private-Members

        private KvpbaseSettings _KvpbaseSettings = null;
        private Blobs _Blobs = null;

        #endregion

        #region Constructors-and-Factories
         
        /// <summary>
        /// Instantiate the object to crawl on Kvpbase.
        /// </summary>
        /// <param name="accountName">The name of the account.</param>
        /// <param name="container">Name of the container.</param>
        /// <param name="endpoint">The endpoint.</param>
        /// <param name="apiKey">API key.</param>
        /// <param name="key">The object key.</param>
        public KvpbaseCrawler(string endpoint, string userGuid, string container, string apiKey, string key)
        { 
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            UserGuid = userGuid ?? throw new ArgumentNullException(nameof(userGuid));
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey)); 

            InitializeBlobs();
        }
          
        #endregion

        #region Public-Methods

        /// <summary>
        /// Retrieve the object.
        /// </summary>
        /// <returns>Crawl result.</returns>
        public KvpbaseCrawlResult Get()
        {
            KvpbaseCrawlResult ret = new KvpbaseCrawlResult();

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
        public KvpbaseCrawlResult Download(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            KvpbaseCrawlResult ret = new KvpbaseCrawlResult();

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
        public bool TryGet(out KvpbaseCrawlResult result)
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
        public bool TryDownload(string filename, out KvpbaseCrawlResult result)
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
        public async Task<KvpbaseCrawlResult> GetAsync()
        {
            KvpbaseCrawlResult ret = new KvpbaseCrawlResult();

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
        public async Task<KvpbaseCrawlResult> DownloadAsync(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            KvpbaseCrawlResult ret = new KvpbaseCrawlResult();

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
            _KvpbaseSettings = new KvpbaseSettings(Endpoint, UserGuid, Container, ApiKey);
            _Blobs = new Blobs(_KvpbaseSettings);
        }

        #endregion 
    }
}
