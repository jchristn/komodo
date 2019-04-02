using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqliteWrapper;
using SyslogLogging;

namespace Komodo.Core
{
    /// <summary>
    /// Class that manages indices.
    /// </summary>
    public class IndexManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private LoggingModule _Logging;
         
        private string _IndicesFilename;

        private List<Index> _Indices;
        private List<IndexClient> _IndexClients;
        private readonly object _IndicesLock;
        private readonly object _IndexClientLock;
        
        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the IndexManager.
        /// </summary>
        public IndexManager()
        {

        }

        /// <summary>
        /// Instantiates the IndexManager.
        /// </summary>
        /// <param name="indicesFilename">The file containing the list of indices.</param> 
        /// <param name="logging">Logging module.</param>
        public IndexManager(string indicesFilename, LoggingModule logging)
        {
            if (String.IsNullOrEmpty(indicesFilename)) throw new ArgumentNullException(nameof(indicesFilename));
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            _Logging = logging;
            _IndicesFilename = indicesFilename;

            _Indices = new List<Index>();
            _IndexClients = new List<IndexClient>();

            _IndicesLock = new object();
            _IndexClientLock = new object(); 

            LoadIndicesFile();
            InitializeIndexClients();

            _Logging.Log(LoggingModule.Severity.Info, "IndexManager started");
        }

        #endregion

        #region Public-Index-Methods

        /// <summary>
        /// Retrieve the list of indices.
        /// </summary>
        /// <returns>List of Index objects.</returns>
        public List<Index> GetIndices()
        {
            lock (_IndicesLock)
            {
                List<Index> ret = new List<Index>(_Indices);
                return ret;
            }
        }

        /// <summary>
        /// Retrieve an index by name.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>Index.</returns>
        public Index GetIndexByName(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            indexName = indexName.ToLower();

            lock (_IndicesLock)
            {
                foreach (Index currIndex in _Indices)
                {
                    if (currIndex.IndexName.ToLower().Equals(indexName)) return currIndex;
                }

                _Logging.Log(LoggingModule.Severity.Warn, "IndexManager GetIndexByName index " + indexName + " does not exist");
                return null;
            }
        }

        /// <summary>
        /// Check if an index exists.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>True if exists.</returns>
        public bool IndexExists(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            indexName = indexName.ToLower();

            lock (_IndicesLock)
            {
                foreach (Index currIndex in _Indices)
                {
                    if (currIndex.IndexName.ToLower().Equals(indexName)) return true;
                }

                _Logging.Log(LoggingModule.Severity.Warn, "IndexManager IndexExists index " + indexName + " does not exist");
                return false;
            }
        }

        /// <summary>
        /// Add a new index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="error">Human-readable error string.</param>
        /// <returns>True if successful.</returns>
        public bool AddIndex(Index index)
        { 
            try
            { 
                if (index == null)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "IndexManager AddIndex index cannot be null");
                    return false;
                }

                index.IndexName = index.IndexName.ToLower();
                Index currIndex = GetIndexByName(index.IndexName);
                if (currIndex != null)
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "IndexManager AddIndex index " + index.IndexName + " already exists, reusing");
                    return true;
                }

                lock (_IndicesLock)
                {
                    _Indices.Add(index);
                    if (!Common.WriteFile(_IndicesFilename, Encoding.UTF8.GetBytes(Common.SerializeJson(_Indices, true))))
                    {
                        _Logging.Log(LoggingModule.Severity.Warn, "IndexManager AddIndex unable to write new index to " + _IndicesFilename);
                        return false;
                    }
                }

                LoadIndicesFile();

                IndexClient idxClient = new IndexClient(index, _Logging);

                lock (_IndexClientLock)
                {
                    _IndexClients.Add(idxClient);
                }

                _Logging.Log(LoggingModule.Severity.Info, "IndexManager AddIndex index " + index.IndexName + " added");
                return true;
            }
            catch (Exception e)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexManager AddIndex index " + index.IndexName + " failed due to exception, cleaning up");
                _Logging.LogException("IndexManager", "AddIndex", e);

                RemoveIndex(index.IndexName, true);
                return false;
            }
        }

        /// <summary>
        /// Remove an index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="cleanup">True if you wish to delete the index directory and files.</param>
        public void RemoveIndex(string indexName, bool cleanup)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            indexName = indexName.ToLower();

            _Logging.Log(LoggingModule.Severity.Info, "IndexManager RemoveIndex removing index " + indexName);

            Index currIndex = GetIndexByName(indexName);
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexManager RemoveIndex index " + indexName + " does not exist");
                return;
            }

            lock (_IndicesLock)
            {
                List<Index> updated = new List<Index>();

                foreach (Index tempIndex in _Indices)
                {
                    if (!tempIndex.IndexName.ToLower().Equals(indexName)) updated.Add(currIndex);
                }

                if (!Common.WriteFile(_IndicesFilename, Encoding.UTF8.GetBytes(Common.SerializeJson(updated, true))))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "IndexManager RemoveIndex unable to remove index " + indexName + " from " + _IndicesFilename);
                    return;
                }
            }

            LoadIndicesFile();

            lock (_IndexClientLock)
            {
                IndexClient currIndexClient = _IndexClients.Where(x => x.Name.Equals(indexName)).FirstOrDefault();

                if (cleanup && currIndexClient != null)
                {
                    currIndexClient.Destroy();
                }

                if (currIndexClient != null && currIndexClient != default(IndexClient))
                {
                    currIndexClient.Dispose();
                }

                _IndexClients = _IndexClients.Where(x => !x.Name.Equals(indexName)).ToList();
            }

            if (cleanup)
            {
                if (!Common.DeleteDirectory(currIndex.RootDirectory, true))
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "IndexManager RemoveIndex unable to remove root directory for index " + indexName);
                }
            }

            return;
        }
         
        /// <summary>
        /// Retrieve the index client for an index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>IndexClient.</returns>
        public IndexClient GetIndexClient(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            indexName = indexName.ToLower();
            if (!IndexExists(indexName)) return null;

            lock (_IndexClientLock)
            {
                IndexClient curr = _IndexClients.Where(x => x.Name.ToLower().Equals(indexName)).FirstOrDefault();
                if (curr == default(IndexClient)) return null;
                return curr;
            }
        }

        /// <summary>
        /// Retrieve statistics for the specified index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>Index statistics.</returns>
        public IndexStats GetIndexStats(string indexName)
        { 
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            indexName = indexName.ToLower();

            IndexClient currClient = GetIndexClient(indexName);
            if (currClient == null || currClient == default(IndexClient))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexManager GetIndexStats index " + indexName + " does not exist");
                return null;
            }

            IndexStats stats = currClient.GetIndexStats();
            return stats;
        }

        #endregion

        #region Private-Index-Methods

        private void LoadIndicesFile()
        {
            lock (_IndicesLock)
            {
                if (!Common.FileExists(_IndicesFilename))
                {
                    if (!Common.WriteFile(_IndicesFilename, Encoding.UTF8.GetBytes(Common.SerializeJson(new List<object>(), true))))
                    {
                        _Logging.Log(LoggingModule.Severity.Warn, "IndexManager unable to write new file " + _IndicesFilename + ", exiting");
                        Common.ExitApplication("IndexManager", "Unable to write indices file " + _IndicesFilename, -1);
                        return;
                    }
                }

                _Indices = Common.DeserializeJson<List<Index>>(Common.ReadBinaryFile(_IndicesFilename));
                _Logging.Log(LoggingModule.Severity.Debug, "IndexManager intialized with " + _Indices.Count + " indices");
            }
        }

        private void InitializeIndexClients()
        {
            lock (_IndicesLock)
            {
                lock (_IndexClientLock)
                {
                    foreach (Index currIndex in _Indices)
                    {
                        IndexClient currClient = new IndexClient(currIndex, _Logging);
                        _IndexClients.Add(currClient);
                    }
                }
            } 
        }
           
        #endregion
    }
}
