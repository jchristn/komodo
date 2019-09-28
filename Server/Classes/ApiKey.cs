using System;
using System.Collections.Generic;
using System.IO;
using SyslogLogging;
using Komodo.Core;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// API key used to authorize access to APIs.
    /// </summary>
    public class ApiKey
    {
        #region Public-Members

        /// <summary>
        /// ID of the API key.
        /// </summary>
        public int? ApiKeyId { get; set; }

        /// <summary>
        /// ID of the user to which this API key is mapped.
        /// </summary>
        public int? UserMasterId { get; set; }

        /// <summary>
        /// The API key's GUID, i.e. this is used as the value when supplying the API key to the server.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Notes associated with the API key.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Indicates whether or not the API key is active and valid.
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
        /// Expiration timestamp for the API key.
        /// </summary>
        public DateTime? Expiration { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public ApiKey()
        {

        }

        /// <summary>
        /// Load API keys from file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>List of ApiKey.</returns>
        public static List<ApiKey> FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!Common.FileExists(filename)) throw new FileNotFoundException(nameof(filename));
             
            Console.WriteLine("Reading API keys from " + filename);
            string contents = Common.ReadTextFile(@filename);

            if (String.IsNullOrEmpty(contents))
            {
                Common.ExitApplication("ApiKey", "Unable to read contents of " + filename, -1);
                return null;
            }
             
            return Common.DeserializeJson<List<ApiKey>>(contents); 
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
