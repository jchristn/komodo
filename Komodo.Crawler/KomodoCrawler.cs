using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlobHelper;
using Komodo.Classes;

namespace Komodo.Crawler
{
    /// <summary>
    /// Crawler for retrieving data from a Komodo instance.
    /// </summary>
    public class KomodoCrawler
    {
        #region Public-Members

        /// <summary>
        /// The GUID of the index.
        /// </summary>
        public string IndexGuid = null;

        /// <summary>
        /// The endpoint to be crawled.
        /// </summary>
        public string Endpoint = null;
         
        /// <summary>
        /// The key of the object to crawl.
        /// </summary>
        public string Key = null;

        /// <summary>
        /// The access key to use while crawling.
        /// </summary>
        public string ApiKey = null;
         
        #endregion

        #region Private-Members

        private KomodoSettings _KomodoSettings = null;
        private Blobs _Blobs = null;

        #endregion

        #region Constructors-and-Factories
         
        /// <summary>
        /// Instantiate the object to crawl on Azure BLOB storage.
        /// </summary>
        /// <param name="endpoint">The endpoint.</param> 
        /// <param name="indexGuid">The GUID of the index.</param>
        /// <param name="apiKey">API key.</param>
        /// <param name="key">The object key.</param>
        public KomodoCrawler(string endpoint, string indexGuid, string apiKey, string key)
        {
            IndexGuid = indexGuid ?? throw new ArgumentNullException(nameof(indexGuid));
            Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
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
        /// <returns>Crawl result.</returns>
        public async Task<CrawlResult> GetAsync()
        {
            CrawlResult ret = new CrawlResult();

            try
            {
                BlobData data = await _Blobs.GetStream(Key);
                ret.Metadata = CrawlResult.ObjectMetadata.FromBlobMetadata(await _Blobs.GetMetadata(Key));
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
        /// <returns>Crawl result.</returns>
        public async Task<CrawlResult> DownloadAsync(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            CrawlResult ret = new CrawlResult();

            try
            {
                BlobData data = await _Blobs.GetStream(Key);
                ret.Metadata = CrawlResult.ObjectMetadata.FromBlobMetadata(await _Blobs.GetMetadata(Key));
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
            _KomodoSettings = new KomodoSettings(Endpoint, IndexGuid, ApiKey);
            _Blobs = new Blobs(_KomodoSettings);
        }

        #endregion 
    }
}
