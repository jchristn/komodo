using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqliteWrapper;
using SyslogLogging;

namespace KomodoCore
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

        private bool _DbDebug;
        private string _DbFilename;
        private DatabaseClient _SqlIndices;

        private readonly object _IndexClientLock;
        private List<IndexClient> _IndexClients;

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
        /// <param name="dbFilename">The file containing the indices database.</param>
        /// <param name="dbDebug">Enable or disable database debugging.</param>
        /// <param name="logging">Logging module.</param>
        public IndexManager(string dbFilename, bool dbDebug, LoggingModule logging)
        {
            if (String.IsNullOrEmpty(dbFilename)) throw new ArgumentNullException(nameof(dbFilename));
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            _Logging = logging;
            _DbFilename = dbFilename;
            _DbDebug = dbDebug;

            _SqlIndices = new DatabaseClient(_DbFilename, _DbDebug);
            CreateIndicesTable();

            _IndexClients = new List<IndexClient>();
            _IndexClientLock = new object();
            _IndexClientLock = new List<IndexClient>();
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
            List<Index> ret = new List<Index>();
            DataTable result = _SqlIndices.Select("Indices", null, null, null, null, null);

            if (result != null && result.Rows.Count > 0)
            {
                ret = Index.ListFromDataTable(result);
            }

            return ret;
        }

        /// <summary>
        /// Retrieve an index by name.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>Index.</returns>
        public Index GetIndexByName(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            Expression e = new Expression("Name", Operators.Equals, indexName);
            DataTable result = _SqlIndices.Select("Indices", null, 1, null, e, null);
            if (result != null && result.Rows.Count > 0)
            {
                return Index.FromDataTable(result);
            }

            _Logging.Log(LoggingModule.Severity.Warn, "IndexManager GetIndexByName index " + indexName + " does not exist");
            return null;
        }

        /// <summary>
        /// Check if an index exists.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>True if exists.</returns>
        public bool IndexExists(string indexName)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            Expression e = new Expression("Name", Operators.Equals, indexName);
            DataTable result = _SqlIndices.Select("Indices", null, null, null, e, null);
            if (result != null && result.Rows.Count > 0) return true;
            else return false;
        }

        /// <summary>
        /// Add a new index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="rootDirectory">The root directory for the index.</param>
        /// <param name="options">The options for the index.</param>
        public void AddIndex(string indexName, string rootDirectory, IndexOptions options)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));
            if (String.IsNullOrEmpty(rootDirectory)) throw new ArgumentNullException(nameof(rootDirectory));
            if (options == null) throw new ArgumentNullException(nameof(options));

            Index currIndex = GetIndexByName(indexName);
            if (currIndex != null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexManager AddIndex index " + indexName + " already exists, reusing");
                return;
            }

            AddIndexToDatabase(indexName, rootDirectory, options);

            IndexClient idxClient = new IndexClient(indexName, rootDirectory, _DbDebug, options, _Logging);

            lock (_IndexClientLock)
            {
                _IndexClients.Add(idxClient);
            }

            _Logging.Log(LoggingModule.Severity.Info, "IndexManager AddIndex index " + indexName + " added");
        }

        /// <summary>
        /// Remove an index.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <param name="cleanup">True if you wish to delete the index directory and files.</param>
        public void RemoveIndex(string indexName, bool cleanup)
        {
            if (String.IsNullOrEmpty(indexName)) throw new ArgumentNullException(nameof(indexName));

            _Logging.Log(LoggingModule.Severity.Info, "IndexManager RemoveIndex removing index " + indexName);

            Index currIndex = GetIndexByName(indexName);
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "IndexManager RemoveIndex index " + indexName + " does not exist");
                return;
            }
             
            RemoveIndexFromDatabase(indexName);

            lock (_IndexClientLock)
            {
                IndexClient currIndexClient = _IndexClients.Where(x => x.Name.Equals(indexName)).FirstOrDefault();
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
            if (!IndexExists(indexName)) return null;

            lock (_IndexClientLock)
            {
                IndexClient curr = _IndexClients.Where(x => x.Name.Equals(indexName)).FirstOrDefault();
                if (curr == default(IndexClient)) return null;
                return curr;
            }
        }

        #endregion
          
        #region Private-Index-Methods

        private void InitializeIndexClients()
        {
            string query = "SELECT * FROM Indices";
            DataTable result = _SqlIndices.Query(query);
            if (result != null && result.Rows.Count > 0)
            { 
                foreach (DataRow currRow in result.Rows)
                {
                    lock (_IndexClientLock)
                    { 
                        IndexClient currClient = new IndexClient(
                            currRow["Name"].ToString(), 
                            currRow["Directory"].ToString(), 
                            _DbDebug,
                            Common.DeserializeJson<IndexOptions>(currRow["Options"].ToString()),
                            _Logging);

                        _IndexClients.Add(currClient);
                    }
                }
            }
        }

        private void CreateIndicesTable()
        {
            string indicesTableQuery =
                "CREATE TABLE IF NOT EXISTS Indices " +
                "(" +
                "  Id                INTEGER PRIMARY KEY, " +
                "  Name              VARCHAR(128), " +
                "  Directory         VARCHAR(256), " +
                "  Options           TEXT " +
                ")";

            _SqlIndices.Query(indicesTableQuery);
        }

        private void AddIndexToDatabase(string indexName, string rootDirectory, IndexOptions options)
        {
            Dictionary<string, object> insertDict = new Dictionary<string, object>();
            insertDict.Add("Name", indexName);
            insertDict.Add("Directory", rootDirectory);
            insertDict.Add("Options", Common.SerializeJson(options, false));

            _SqlIndices.Insert("Indices", insertDict);
            return;
        }

        private void RemoveIndexFromDatabase(string indexName)
        {
            Expression e = new Expression("Name", Operators.Equals, indexName);
            _SqlIndices.Delete("Indices", e);
            return;
        }

        #endregion
    }
}
