using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Komodo;

namespace Komodo.Crawler
{
    /// <summary>
    /// Crawler for retrieving data from a web address.
    /// </summary>
    public class FtpCrawler
    {
        #region Public-Members
         
        /// <summary>
        /// The source URL for the object to be crawled.
        /// </summary>
        public string SourceUrl = null;  

        /// <summary>
        /// The username to use when accessing the object.
        /// </summary>
        public string Username = "anonymous";

        /// <summary>
        /// The password to use when accessing the object.
        /// </summary>
        public string Password = "anonymous@user.com";

        /// <summary>
        /// Buffer size to use while accessing a stream.
        /// </summary>
        public int StreamBufferSize
        {
            get
            {
                return _StreamBufferSize;
            }
            set
            { 
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(StreamBufferSize));
                _StreamBufferSize = value;
            }
        }

        #endregion

        #region Private-Members

        private int _StreamBufferSize = 65536;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="sourceUrl">The source URL for the object to be crawled.</param>
        public FtpCrawler(string sourceUrl)
        {
            if (String.IsNullOrEmpty(sourceUrl)) throw new ArgumentNullException(nameof(sourceUrl));
            if (!sourceUrl.StartsWith("ftp://")) throw new ArgumentException("Source URL must begin with ftp://.");

            SourceUrl = sourceUrl;
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
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(SourceUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(Username, Password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();
                ret.ContentLength = responseStream.Length;
                responseStream.CopyTo(ret.DataStream);
                response.Close();

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
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(SourceUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(Username, Password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();

                ret.Filename = filename;
                ret.ContentLength = responseStream.Length;

                long bytesRemaining = ret.ContentLength;

                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (bytesRemaining > 0)
                    {
                        while (bytesRemaining > 0)
                        {
                            byte[] buffer = new byte[65536];
                            int bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                fs.Write(buffer, 0, bytesRead);
                            }
                        }
                    }
                }

                response.Close();

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
        public bool TryDownload(string filename, out CrawlResult result)
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
        /// <param name="token">Cancellation token to cancel the request.</param>
        /// <returns>Crawl result.</returns>
        public async Task<CrawlResult> GetAsync(CancellationToken token = default)
        {
            CrawlResult ret = new CrawlResult();

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(SourceUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(Username, Password);
                FtpWebResponse response = (FtpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));

                Stream responseStream = response.GetResponseStream();
                await responseStream.CopyToAsync(ret.DataStream, _StreamBufferSize, token).ConfigureAwait(false);
                ret.ContentLength = responseStream.Length;
                response.Close();

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
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(SourceUrl);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(Username, Password);
                FtpWebResponse response = (FtpWebResponse)(await request.GetResponseAsync().ConfigureAwait(false));

                Stream responseStream = response.GetResponseStream();

                ret.Filename = filename;
                ret.ContentLength = responseStream.Length;

                long bytesRemaining = ret.ContentLength;

                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    if (bytesRemaining > 0)
                    {
                        while (bytesRemaining > 0)
                        {
                            byte[] buffer = new byte[65536];
                            int bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                            if (bytesRead > 0)
                            {
                                bytesRemaining -= bytesRead;
                                await fs.WriteAsync(buffer, 0, bytesRead, token).ConfigureAwait(false);
                            }
                        }
                    }
                }

                response.Close();

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
         
        #endregion 
    }
}
