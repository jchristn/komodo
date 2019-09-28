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
        private List<Index> _Indices = new List<Index>();
        private List<IndexClient> _Clients = new List<IndexClient>();
        private readonly object _IndicesLock = new object();
        private readonly object _ClientsLock = new object();
        
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

            DateTime ts = DateTime.Now;

            _Logging = logging;
            _IndicesFilename = indicesFilename;

            _Logging.Info("IndexManager starting");
             
            LoadIndicesFile();
            InitializeIndexClients();

            _Logging.Info("IndexManager started [" + Common.TotalMsFrom(ts) + "ms]");
        }

        #endregion

        #region Public-Methods

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
                Index index = _Indices.Where(i => i.IndexName.ToLower().Equals(indexName)).FirstOrDefault();
                if (index == null || index == default(Index)) return null;
                return index;
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
                if (_Indices.Exists(i => i.IndexName.ToLower().Equals(indexName))) return true;
                return false;
            }
        }

        /// <summary>
        /// Add a new index.
        /// </summary>
        /// <param name="index">The index.</param> 
        /// <returns>True if successful.</returns>
        public bool AddIndex(Index index)
        { 
            try
            {
                if (index == null) return false;

                index.IndexName = index.IndexName.ToLower();
                Index currIndex = GetIndexByName(index.IndexName);
                if (currIndex != null) return true;

                lock (_IndicesLock)
                {
                    _Indices.Add(index);
                    if (!Common.WriteFile(_IndicesFilename, Encoding.UTF8.GetBytes(Common.SerializeJson(_Indices, true))))
                    { 
                        return false;
                    }
                }

                LoadIndicesFile();

                IndexClient idxClient = new IndexClient(index, _Logging);

                lock (_ClientsLock)
                {
                    _Clients.Add(idxClient);
                }
                 
                return true;
            }
            catch (Exception)
            { 
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
             
            Index currIndex = GetIndexByName(indexName);
            if (currIndex == null) return;

            lock (_IndicesLock)
            {
                List<Index> updated = new List<Index>();

                foreach (Index tempIndex in _Indices)
                {
                    if (!tempIndex.IndexName.ToLower().Equals(indexName)) updated.Add(currIndex);
                }

                if (!Common.WriteFile(_IndicesFilename, Encoding.UTF8.GetBytes(Common.SerializeJson(updated, true))))
                { 
                    return;
                }
            }

            LoadIndicesFile();

            lock (_ClientsLock)
            {
                IndexClient currIndexClient = _Clients.Where(x => x.Name.Equals(indexName)).FirstOrDefault();

                if (cleanup && currIndexClient != null)
                {
                    currIndexClient.Destroy();
                }

                if (currIndexClient != null && currIndexClient != default(IndexClient))
                {
                    currIndexClient.Dispose();
                }

                _Clients = _Clients.Where(x => !x.Name.Equals(indexName)).ToList();
            }

            if (cleanup)
            {
                if (!Common.DeleteDirectory(currIndex.RootDirectory, true))
                {
                    _Logging.Warn("IndexManager RemoveIndex unable to remove root directory for index " + indexName);
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

            lock (_ClientsLock)
            {
                IndexClient curr = _Clients.Where(x => x.Name.ToLower().Equals(indexName)).FirstOrDefault();
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
                return null;
            }

            IndexStats stats = currClient.GetIndexStats();
            return stats;
        }

        #endregion

        #region Private-Methods

        private void LoadIndicesFile()
        {
            lock (_IndicesLock)
            {
                if (!Common.FileExists(_IndicesFilename))
                {
                    if (!Common.WriteFile(_IndicesFilename, Encoding.UTF8.GetBytes(Common.SerializeJson(new List<object>(), true))))
                    { 
                        Common.ExitApplication("IndexManager", "Unable to write indices file " + _IndicesFilename, -1);
                        return;
                    }
                }

                _Indices = Common.DeserializeJson<List<Index>>(Common.ReadBinaryFile(_IndicesFilename)); 
            }
        }

        private void InitializeIndexClients()
        {
            lock (_IndicesLock)
            {
                lock (_ClientsLock)
                {
                    foreach (Index currIndex in _Indices)
                    {
                        IndexClient currClient = new IndexClient(currIndex, _Logging);
                        _Clients.Add(currClient);
                    }
                }
            } 
        }
           
        #endregion
    }
}
