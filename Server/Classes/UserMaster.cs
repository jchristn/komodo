using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SyslogLogging;
using Komodo.Core;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// User object.
    /// </summary>
    public class UserMaster
    {
        #region Public-Members

        /// <summary>
        /// ID of the user.
        /// </summary>
        public int? UserMasterId { get; set; }

        /// <summary>
        /// First name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Company name.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Email address (used for authentication).
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password (used for authentication).
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Indicates whether or not the user is a system admin.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// GUID of the user.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Indicates whether or not the account is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Time of creation.
        /// </summary>
        public DateTime? Created { get; set; }
        
        /// <summary>
        /// Time of last update.
        /// </summary>
        public DateTime? LastUpdate { get; set; }

        /// <summary>
        /// Time when the user account will expire.
        /// </summary>
        public DateTime? Expiration { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public UserMaster()
        {

        }

        /// <summary>
        /// Load a list of users from a file.
        /// </summary>
        /// <param name="filename">Filename and path.</param>
        /// <returns>List of users.</returns>
        public static List<UserMaster> FromFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!Common.FileExists(filename)) throw new FileNotFoundException(nameof(filename));

            Console.WriteLine("---");
            Console.WriteLine("Reading users from " + filename);
            string contents = Common.ReadTextFile(@filename);

            if (String.IsNullOrEmpty(contents))
            {
                Common.ExitApplication("UserMaster", "Unable to read contents of " + filename, -1);
                return null;
            }

            Console.WriteLine("Deserializing " + filename);
            List<UserMaster> ret = null;

            try
            {
                ret = Common.DeserializeJson<List<UserMaster>>(contents);
                if (ret == null)
                {
                    Common.ExitApplication("UserMaster", "Unable to deserialize " + filename + " (null)", -1);
                    return null;
                }
            }
            catch (Exception e)
            {
                LoggingModule.ConsoleException("UserMaster", "Deserialization issue with " + filename, e);
                Common.ExitApplication("UserMaster", "Unable to deserialize " + filename + " (exception)", -1);
                return null;
            }

            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
