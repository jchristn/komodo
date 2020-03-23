using System;
using System.Collections.Generic;
using System.Data;
using DatabaseWrapper;
using Komodo.Classes; 

namespace Komodo.Database
{
    /// <summary>
    /// Komodo database driver.
    /// </summary>
    public class KomodoDatabase : IDisposable
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private DatabaseSettings _DatabaseSettings = null;
        private DatabaseClient _Database = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="settings">Database settings.</param>
        public KomodoDatabase(DatabaseSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _DatabaseSettings = settings;

            InitializeDatabaseClient();
            InitializeTables(); 
        }

        #endregion

        #region Internal-Methods

        /// <summary>
        /// Dispose of the object and release background workers.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// INSERT an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="entry">Object to INSERT.</param>
        /// <returns>INSERTed object.</returns>
        public T Insert<T>(T entry) where T : class
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            string tableName = DatabaseTableNameFromObject(entry);
            string key = KeyFieldFromTableName(tableName);
            Dictionary<string, object> insertVals = ObjectToInsertDictionary(entry);
            DataTable result = _Database.Insert(tableName, insertVals);
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// UPDATE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="entry">Object to UPDATE.</param>
        /// <returns>UPDATEd object.</returns>
        public T Update<T>(T entry) where T : class
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            string tableName = DatabaseTableNameFromObject(entry);
            string key = KeyFieldFromTableName(tableName);
            int id = IdValFromObject(entry, tableName);
            Dictionary<string, object> updateVals = ObjectToInsertDictionary(entry);
            Expression e = new Expression(key, DatabaseWrapper.Operators.Equals, id);
            _Database.Update(tableName, updateVals, e);
            DataTable result = _Database.Select(tableName, null, null, null, e, null);
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// DELETE an object.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="entry">Object to DELETE.</param>
        public void Delete<T>(T entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            string table = DatabaseTableNameFromObject(entry);
            string key = KeyFieldFromTableName(table);
            int id = IdValFromObject(entry, table);
            Expression e = new Expression(key, DatabaseWrapper.Operators.Equals, id);
            _Database.Delete(table, e); 
        }

        /// <summary>
        /// DELETE an object by ID.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id value.</param>
        public void DeleteById<T>(int id)
        {
            string tableName = DatabaseTableNameFromObjectType(typeof(T).Name);
            string key = KeyFieldFromTableName(tableName);
            Expression e = new Expression(key, DatabaseWrapper.Operators.Equals, id);
            _Database.Delete(tableName, e);
        }

        /// <summary>
        /// DELETE an object by its GUID.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="guid">GUID.</param>
        public void DeleteByGUID<T>(string guid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            string tableName = DatabaseTableNameFromObjectType(typeof(T).Name); 
            Expression e = new Expression("guid", DatabaseWrapper.Operators.Equals, guid);
            _Database.Delete(tableName, e);
        }

        /// <summary>
        /// DELETE objects by an Expression..
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="e">Expression.</param>
        public void DeleteByFilter<T>(Expression e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            string tableName = DatabaseTableNameFromObjectType(typeof(T).Name);
            _Database.Delete(tableName, e);
        }

        /// <summary>
        /// SELECT multiple rows.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="indexStart">Index start.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param>
        /// <param name="returnFields">List of fields to return.  If null, all fields are returned.</param>
        /// <param name="filter">Filter to apply when SELECTing rows (i.e. WHERE clause).</param>
        /// <param name="orderByClause">ORDER BY clause.</param>
        /// <returns>DataTable.</returns>
        public DataTable Select(string tableName, int? indexStart, int? maxResults, List<string> returnFields, Expression filter, string orderByClause)
        { 
            return _Database.Select(tableName, indexStart, maxResults, returnFields, filter, orderByClause);
        }

        /// <summary>
        /// SELECT an object by id.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="id">Id.</param>
        /// <returns>Object.</returns>
        public T SelectById<T>(int id) where T : class
        {
            string tableName = DatabaseTableNameFromObjectType(typeof(T).Name);
            string key = KeyFieldFromTableName(tableName);
            Expression e = new Expression(key, DatabaseWrapper.Operators.Equals, id);
            DataTable result = _Database.Select(tableName, null, null, null, e, null);
            if (result == null || result.Rows.Count < 1) return null;
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// SELECT an object by GUID.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="guid">GUID.</param>
        /// <returns>Object.</returns>
        public T SelectByGUID<T>(string guid) where T : class
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            string tableName = DatabaseTableNameFromObjectType(typeof(T).Name);
            Expression e = new Expression("guid", DatabaseWrapper.Operators.Equals, guid);
            DataTable result = _Database.Select(tableName, null, null, null, e, null);
            if (result == null || result.Rows.Count < 1) return null;
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// SELECT an object using a filter.
        /// </summary>
        /// <typeparam name="T">Type of filter.</typeparam>
        /// <param name="e">Expression by which SELECT should be filtered (i.e. WHERE clause).</param>
        /// <param name="orderByClause">ORDER BY clause.</param>
        /// <returns>Object.</returns>
        public T SelectByFilter<T>(Expression e, string orderByClause) where T : class
        {
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (String.IsNullOrEmpty(orderByClause)) throw new ArgumentNullException(nameof(orderByClause));
            string tableName = DatabaseTableNameFromObjectType(typeof(T).Name);
            string key = KeyFieldFromTableName(tableName); 
            e.PrependAnd(key, DatabaseWrapper.Operators.GreaterThan, 0); 
            DataTable result = _Database.Select(tableName, null, 1, null, e, orderByClause);
            if (result == null || result.Rows.Count < 1) return null;
            return DataTableToObject<T>(result);
        }

        /// <summary>
        /// SELECT multiple objects.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="startIndex">Start index.</param>
        /// <param name="maxResults">Maximum number of results to retrieve.</param>
        /// <param name="e">Expression by which SELECT should be filtered (i.e. WHERE clause).</param>
        /// <param name="orderByClause">ORDER BY clause.</param>
        /// <returns>List of objects.</returns>
        public List<T> SelectMany<T>(int? startIndex, int? maxResults, Expression e, string orderByClause)
        {
            if (String.IsNullOrEmpty(orderByClause)) throw new ArgumentNullException(nameof(orderByClause));
            string tableName = DatabaseTableNameFromObjectType(typeof(T).Name);
            string key = KeyFieldFromTableName(tableName);
            if (e == null) e = new Expression(key, Operators.GreaterThan, 0);
            else e.PrependAnd(new Expression(key, Operators.GreaterThan, 0));
            DataTable result = _Database.Select(tableName, startIndex, maxResults, null, e, orderByClause);
            if (result == null || result.Rows.Count < 1) return new List<T>();
            return DataTableToListObject<T>(result);
        }

        /// <summary>
        /// Execute a SQL query.
        /// </summary>
        /// <param name="query">Query.</param>
        /// <returns>DataTable.</returns>
        public DataTable Query(string query)
        {
            return _Database.Query(query);
        }

        /// <summary>
        /// Sanitize a string.
        /// </summary>
        /// <param name="str">String.</param>
        /// <returns>Sanitized string.</returns>
        public string Sanitize(string str)
        {
            if (String.IsNullOrEmpty(str)) return null;
            return _Database.SanitizeString(str);
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
                if (_Database != null) _Database.Dispose();
            }
        }

        private void InitializeDatabaseClient()
        {
            if (String.IsNullOrEmpty(_DatabaseSettings.Filename))
            {
                _Database = new DatabaseClient(
                    _DatabaseSettings.Type,
                    _DatabaseSettings.Hostname,
                    _DatabaseSettings.Port,
                    _DatabaseSettings.Username,
                    _DatabaseSettings.Password,
                    _DatabaseSettings.Instance,
                    _DatabaseSettings.DatabaseName);
            }
            else
            {
                _Database = new DatabaseClient(_DatabaseSettings.Filename);
            }
        }

        private void InitializeTables()
        {
            Queries.Tables.InitializeTables(_Database);
        }

        private string DatabaseTableNameFromObject(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (obj is ApiKey) return "apikeys";
            else if (obj is Index) return "indices";
            else if (obj is Metadata) return "metadata";
            else if (obj is Node) return "nodes";
            else if (obj is ParsedDocument) return "parseddocs";
            else if (obj is Permission) return "permissions";
            else if (obj is SourceDocument) return "sourcedocs";
            else if (obj is TermDoc) return "termdocs";
            else if (obj is TermGuid) return "termguids";
            else if (obj is User) return "users";
            throw new ArgumentException("Unknown object type: " + obj.GetType().ToString());
        }

        private string DatabaseTableNameFromObjectType(string type)
        {
            if (String.IsNullOrEmpty(type)) throw new ArgumentNullException(nameof(type));

            switch (type)
            {
                case "ApiKey":
                    return "apikeys";
                case "Index":
                    return "indices";
                case "Metadata":
                    return "metadata";
                case "Node":
                    return "nodes";
                case "ParsedDocument":
                    return "parseddocs";
                case "Permission":
                    return "permissions";
                case "SourceDocument":
                    return "sourcedocs";
                case "TermDoc":
                    return "termdocs";
                case "TermGuid":
                    return "termguids";
                case "User":
                    return "users";
                default:
                    throw new ArgumentException("Unknown type: " + type);
            }
        }

        private Dictionary<string, object> ObjectToInsertDictionary(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (obj is ApiKey) return ((ApiKey)obj).ToInsertDictionary();
            else if (obj is Index) return ((Index)obj).ToInsertDictionary();
            else if (obj is Metadata) return ((Metadata)obj).ToInsertDictionary();
            else if (obj is Node) return ((Node)obj).ToInsertDictionary();
            else if (obj is ParsedDocument) return ((ParsedDocument)obj).ToInsertDictionary();
            else if (obj is Permission) return ((Permission)obj).ToInsertDictionary();
            else if (obj is SourceDocument) return ((SourceDocument)obj).ToInsertDictionary();
            else if (obj is TermDoc) return ((TermDoc)obj).ToInsertDictionary();
            else if (obj is TermGuid) return ((TermGuid)obj).ToInsertDictionary();
            else if (obj is User) return ((User)obj).ToInsertDictionary(); 
            throw new ArgumentException("Unknown object type: " + obj.GetType().ToString());
        }

        private T DataTableToObject<T>(DataTable result) where T : class
        {
            if (result == null || result.Rows.Count < 1) return null;
            if (typeof(T) == typeof(ApiKey)) return ApiKey.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(Index)) return Index.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(Metadata)) return Metadata.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(Node)) return Node.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(ParsedDocument)) return ParsedDocument.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(Permission)) return Permission.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(SourceDocument)) return SourceDocument.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(TermDoc)) return TermDoc.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(TermGuid)) return TermGuid.FromDataRow(result.Rows[0]) as T;
            else if (typeof(T) == typeof(User)) return User.FromDataRow(result.Rows[0]) as T; 
            throw new ArgumentException("Unknown object type: " + typeof(T).Name);
        }

        private List<T> DataTableToListObject<T>(DataTable result)
        {
            if (result == null || result.Rows.Count < 1) return new List<T>();
            if (typeof(T) == typeof(ApiKey)) return ApiKey.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(Index)) return Index.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(Metadata)) return Metadata.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(Node)) return Node.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(ParsedDocument)) return ParsedDocument.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(Permission)) return Permission.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(SourceDocument)) return SourceDocument.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(TermDoc)) return TermDoc.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(TermGuid)) return TermGuid.FromDataTable(result) as List<T>;
            else if (typeof(T) == typeof(User)) return User.FromDataTable(result) as List<T>; 
            throw new ArgumentException("Unknown object type: " + typeof(T).Name);
        }

        private string KeyFieldFromTableName(string tableName)
        {
            if (String.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (tableName.Equals("apikeys")) return "id";
            else if (tableName.Equals("indices")) return "id";
            else if (tableName.Equals("metadata")) return "id";
            else if (tableName.Equals("nodes")) return "id";
            else if (tableName.Equals("parseddocs")) return "id";
            else if (tableName.Equals("permissions")) return "id";
            else if (tableName.Equals("sourcedocs")) return "id";
            else if (tableName.Equals("termdocs")) return "id";
            else if (tableName.Equals("termguids")) return "id";
            else if (tableName.Equals("users")) return "id";
            throw new ArgumentException("Unknown table: " + tableName);
        }

        private int IdValFromObject(object entry, string tableName)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (String.IsNullOrEmpty(tableName)) throw new ArgumentNullException(nameof(tableName));
            if (tableName.Equals("apikeys")) return ((ApiKey)entry).Id;
            else if (tableName.Equals("indices")) return ((Index)entry).Id;
            else if (tableName.Equals("metadata")) return ((Metadata)entry).Id;
            else if (tableName.Equals("nodes")) return ((Node)entry).Id;
            else if (tableName.Equals("parseddocs")) return ((ParsedDocument)entry).Id;
            else if (tableName.Equals("permissions")) return ((Permission)entry).Id;
            else if (tableName.Equals("sourcedocs")) return ((SourceDocument)entry).Id;
            else if (tableName.Equals("termdocs")) return ((TermDoc)entry).Id;
            else if (tableName.Equals("termguids")) return ((TermGuid)entry).Id;
            else if (tableName.Equals("users")) return ((User)entry).Id; 
            throw new ArgumentException("Unknown table: " + tableName);
        }

        private void Logger(string msg)
        {
            Console.WriteLine(msg);
        }

        #endregion
    }
}
