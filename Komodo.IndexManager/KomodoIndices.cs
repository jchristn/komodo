using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Komodo.Classes;
using Komodo.Database; 
using Komodo.IndexClient;
using Komodo.Postings;

namespace Komodo.IndexManager
{
    /// <summary>
    /// Komodo index manager.
    /// </summary>
    public class KomodoIndices : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// The frequency, in seconds, by which the index manager will refresh the list of indices and update its internal index clients.
        /// </summary>
        public int RefreshIntervalSeconds
        {
            get
            {
                return _RefreshIntervalSeconds;
            }
        }

        #endregion

        #region Private-Members

        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private CancellationToken _Token;

        private StorageSettings _SourceDocsStorageSettings = null;
        private StorageSettings _ParsedDocsStorageSettings = null;
        private StorageSettings _PostingsStorageSettings = null;

        private DatabaseSettings _DatabaseSettings = null;
        private KomodoDatabase _Database = null;

        private int _RefreshIntervalSeconds = 10;
        private readonly object _IndicesLock = new object();
        private List<KomodoIndex> _Indices = new List<KomodoIndex>(); 
        
        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="dbSettings">Database settings.</param>
        /// <param name="sourceDocs">Storage settings for source documents.</param>
        /// <param name="parsedDocs">Storage settings for parsed documents.</param>
        public KomodoIndices(DatabaseSettings dbSettings, StorageSettings sourceDocs, StorageSettings parsedDocs, StorageSettings postings)
        {
            if (dbSettings == null) throw new ArgumentNullException(nameof(dbSettings));
            if (sourceDocs == null) throw new ArgumentNullException(nameof(sourceDocs));
            if (parsedDocs == null) throw new ArgumentNullException(nameof(parsedDocs));
            if (postings == null) throw new ArgumentNullException(nameof(postings));

            _SourceDocsStorageSettings = sourceDocs;
            _ParsedDocsStorageSettings = parsedDocs;
            _PostingsStorageSettings = postings;

            _Token = _TokenSource.Token;
            _DatabaseSettings = dbSettings;
            _Database = new KomodoDatabase(_DatabaseSettings);

            Task.Run(() => MonitorTask(), _Token);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Add an index.
        /// </summary>
        /// <param name="idx">Index.</param>
        /// <returns>Index.</returns>
        public KomodoIndex Add(Index idx)
        {
            if (idx == null) throw new ArgumentNullException(nameof(idx));

            lock (_IndicesLock)
            {
                if (_Indices.Exists(i => i.Name.Equals(idx.Name))) return _Indices.First(i => i.Name.Equals(idx.Name));
                KomodoIndex ki = new KomodoIndex(_DatabaseSettings, _SourceDocsStorageSettings, _ParsedDocsStorageSettings, _PostingsStorageSettings, idx);
                _Database.Insert<Index>(idx);
                _Indices.Add(ki);
                return ki;
            }
        }

        /// <summary>
        /// Check if an index exists by name.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>True if exists.</returns>
        public bool Exists(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            lock (_IndicesLock)
            {
                return _Indices.Exists(i => i.Name.Equals(name));
            } 
        }

        /// <summary>
        /// Remove an index, and optionally, destroy it.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="destroy">True if you wish to delete its data.</param>
        public async Task Remove(string name, bool destroy)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            KomodoIndex idx = null;

            lock (_IndicesLock)
            {
                if (_Indices.Exists(i => i.Name.Equals(name)))
                {
                    idx = _Indices.First(i => i.Name.Equals(name));
                    _Indices.Remove(idx);
                    _Database.DeleteByGUID<Index>(idx.GUID);
                }
            }

            if (idx != null)
            {
                if (destroy) await idx.Destroy();
                idx.Dispose();
            }
        }

        /// <summary>
        /// Retrieve all index clients.
        /// </summary>
        /// <returns>List of index clients.</returns>
        public List<KomodoIndex> Get()
        {
            lock (_IndicesLock)
            {
                return _Indices;
            }
        }

        /// <summary>
        /// Retrieve an index client.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>Index client.</returns>
        public KomodoIndex Get(string name)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

            lock (_IndicesLock)
            {
                if (_Indices.Exists(i => i.Name.Equals(name)))
                {
                    return _Indices.First(i => i.Name.Equals(name));
                }
            }

            return null;
        }

        /// <summary>
        /// Gather statistics for one or all indices.
        /// </summary>
        /// <param name="name">Optional name of the index.</param>
        /// <returns>Indices statistics.</returns>
        public IndicesStats Stats(string name)
        {
            IndicesStats ret = new IndicesStats();

            if (String.IsNullOrEmpty(name))
            {
                lock (_IndicesLock)
                {
                    foreach (KomodoIndex curr in _Indices)
                    {
                        ret.Stats.Add(curr.Stats());
                    }
                }
            }
            else
            {
                lock (_IndicesLock)
                {
                    if (_Indices.Any(i => i.Name.Equals(name)))
                    {
                        KomodoIndex idx = _Indices.First(i => i.Name.Equals(name));
                        ret.Stats.Add(idx.Stats());
                    }
                    else
                    {
                        // cannot find
                        return ret;
                    }
                } 
            }

            ret.Success = true;
            ret.Time.End = DateTime.Now.ToUniversalTime();
            return ret;
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        /// <param name="disposing">Indicate if child resources should be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // background worker
                _TokenSource.Cancel();

                lock (_IndicesLock)
                {
                    foreach (KomodoIndex curr in _Indices)
                    {
                        curr.Dispose();
                    }
                }
            }
        }
         
        private async Task MonitorTask()
        {
            // look for removed indices and remove them
            // before looking for new indices to add!
            // use case: renaming an index or changing its guid
            // could cause double connection to its database
            
            bool firstRun = true;
            while (true)
            {
                #region Delay

                if (!firstRun)
                {
                    Task.Delay(_RefreshIntervalSeconds).Wait();
                }
                else
                {
                    firstRun = false;
                }

                #endregion

                #region Gather-Records

                List<Index> indicesInDb = new List<Index>();
                List<string> removedGuids = new List<string>();
                List<string> addedGuids = new List<string>();
                List<KomodoIndex> removeQueue = new List<KomodoIndex>();
                List<Index> addQueue = new List<Index>();

                lock (_IndicesLock)
                {
                    indicesInDb = _Database.SelectMany<Index>(null, null, null, "ORDER BY id DESC");

                    #region Check-for-Removed-Indices

                    foreach (KomodoIndex index in _Indices)
                    {
                        if (!indicesInDb.Any(i => i.GUID.Equals(index.GUID)))
                        {
                            // exists in IndexManager but not in database 
                            removedGuids.Add(index.GUID);
                            removeQueue.Add(index);
                        }
                    }
                     
                    #endregion

                    #region Check-for-New-Indices
                     
                    foreach (Index index in indicesInDb)
                    {
                        if (!_Indices.Any(i => i.GUID.Equals(index.GUID)))
                        {
                            // exists in database but not in IndexManager
                            addedGuids.Add(index.GUID);
                            addQueue.Add(index);
                        }
                    }

                    #endregion 
                }

                #endregion

                #region Process-Queues

                foreach (KomodoIndex index in removeQueue)
                {
                    await Remove(index.Name, false);
                }

                foreach (Index index in addQueue)
                {
                    Add(index);
                }

                #endregion
            }
        }

        #endregion
    }
}
