using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SyslogLogging;
using Komodo.Core;

namespace Komodo.Server.Classes
{
    public class UserMaster
    {
        #region Public-Members

        public int? UserMasterId { get; set; }
        public int? NodeId { get; set; }
        public int? ExpirationSec { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Cellphone { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        
        public bool IsAdmin { get; set; }
        public string GUID { get; set; }
        public bool Active { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? Expiration { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        public UserMaster()
        {

        }

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
