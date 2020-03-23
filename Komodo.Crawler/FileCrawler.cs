using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        /// Retrieve the object.
        /// </summary>
        /// <returns>Crawl result.</returns>
        public FileCrawlResult Get()
        {
            FileCrawlResult ret = new FileCrawlResult(); 
            ret.ContentLength = new FileInfo(Filename).Length;
            ret.FileStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        /// <summary>
        /// Download the object to the supplied filename.
        /// </summary>
        /// <param name="filename">The filename where the object should be saved.</param>
        /// <returns>Crawl result.</returns>
        public FileCrawlResult Download(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            FileCrawlResult ret = new FileCrawlResult();
            ret.ContentLength = new FileInfo(Filename).Length;

            using (FileStream source = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                using (FileStream target = new FileStream(filename, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    source.CopyTo(target);
                }
            }

            ret.FileStream = null;
            ret.Success = true;
            ret.Time.End = DateTime.Now;
            return ret;
        }

        /// <summary>
        /// Retrieve the object.
        /// </summary>
        /// <param name="result">Crawl result.</param>
        /// <returns>True if successful.</returns>
        public bool TryGet(out FileCrawlResult result)
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
        public bool TryDownload(string filename, out FileCrawlResult result)
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
