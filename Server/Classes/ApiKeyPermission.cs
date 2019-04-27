using System;
using System.Collections.Generic;
using System.IO;
using SyslogLogging;
using Komodo.Core;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Permissions associated with an API key and user.
    /// </summary>
    public class ApiKeyPermission
    {
        #region Public-Members

        /// <summary>
        /// ID of the permission object.
        /// </summary>
        public int ApiKeyPermissionId { get; set; }

        /// <summary>
        /// ID of the user.
        /// </summary>
        public int UserMasterId { get; set; }

        /// <summary>
        /// ID of the API key.
        /// </summary>
        public int ApiKeyId { get; set; }

        /// <summary>
        /// Notes about the permissions.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Allow or disallow search.
        /// </summary>
        public bool AllowSearch { get; set; }

        /// <summary>
        /// Allow or disallow document creation.
        /// </summary>
        public bool AllowCreateDocument { get; set; }

        /// <summary>
        /// Allow or disallow document deletion.
        /// </summary>
        public bool AllowDeleteDocument { get; set; }

        /// <summary>
        /// Allow or disallow index creation.
        /// </summary>
        public bool AllowCreateIndex { get; set; }

        /// <summary>
        /// Allow or disallow index deletion.
        /// </summary>
        public bool AllowDeleteIndex { get; set; }

        /// <summary>
        /// Allow or disallow re-indexing.
        /// </summary>
        public bool AllowReindex { get; set; }

        /// <summary>
        /// GUID of the permission object.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Indicates whether or not the permission is active and valid.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Creation time.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// Time of last update.
        /// </summary>
        public DateTime? LastUpdate { get; set; }

        /// <summary>
        /// Expiration timestamp for the permission.
        /// </summary>
        public DateTime? Expiration { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ApiKeyPermission()
        {

        }

        /// <summary>
        /// Load API key permissions from file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>List of ApiKeyPermission.</returns>
        public static List<ApiKeyPermission> FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!Common.FileExists(filename)) throw new FileNotFoundException(nameof(filename));
             
            Console.WriteLine("Reading API key permissions from " + filename);
            string contents = Common.ReadTextFile(@filename);

            if (String.IsNullOrEmpty(contents))
            {
                Common.ExitApplication("ApiKeyPermission", "Unable to read contents of " + filename, -1);
                return null;
            }
             
            List<ApiKeyPermission> ret = null;

            try
            {
                ret = Common.DeserializeJson<List<ApiKeyPermission>>(contents);
                if (ret == null)
                {
                    Common.ExitApplication("ApiKeyPermission", "Unable to deserialize " + filename + " (null)", -1);
                    return null;
                }
            }
            catch (Exception e)
            {
                LoggingModule.ConsoleException("ApiKeyPermission", "Deserialization issue with " + filename, e);
                Common.ExitApplication("ApiKeyPermission", "Unable to deserialize " + filename + " (exception)", -1);
                return null;
            }

            return ret;
        }

        /// <summary>
        /// Retrieve default permit permissions.
        /// </summary>
        /// <param name="curr">The user.</param>
        /// <returns>ApiKeyPermission.</returns>
        public static ApiKeyPermission DefaultPermit(UserMaster curr)
        {
            if (curr == null) throw new ArgumentNullException(nameof(curr));
            ApiKeyPermission ret = new ApiKeyPermission();
            ret.ApiKeyPermissionId = 0;
            ret.ApiKeyId = 0;
            ret.UserMasterId = Convert.ToInt32(curr.UserMasterId);
            ret.AllowSearch = true;
            ret.AllowCreateDocument = true;
            ret.AllowDeleteDocument = true;
            ret.AllowCreateIndex = true;
            ret.AllowDeleteIndex = true;
            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
