using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Komodo;

namespace Komodo.Crawler
{ 
    /// <summary>
    /// Crawler for retrieving data from the file system.
    /// </summary>
    public class FileCrawler
    {
        #region Public-Members

        /// <summary>
        /// The path and filename to be crawled.
        /// </summary>
        public string Filename = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories
         
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="filename">The path and filename to be crawled.</param>
        public FileCrawler(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            Filename = filename; 
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
        /// Close and dispose the DataStream when done.
        /// </summary>
        /// <returns>Crawl result.</returns>
        public CrawlResult Get()
        {
            CrawlResult ret = new CrawlResult();

            try
            {
                ret.Metadata = CrawlResult.ObjectMetadata.FromFileInfo(new FileInfo(Filename));
                ret.ContentLength = new FileInfo(Filename).Length;
                ret.DataStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);
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
                ret.Metadata = CrawlResult.ObjectMetadata.FromFileInfo(new FileInfo(Filename));
                ret.ContentLength = new FileInfo(Filename).Length;

                using (FileStream source = new FileStream(Filename, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream target = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite))
                    {
                        source.CopyTo(target);
                    }
                }

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
        /// Close and dispose the DataStream when done.
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

        #endregion

        #region Private-Methods

        #endregion
    }
}
