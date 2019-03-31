using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SyslogLogging;
using WatsonWebserver;
using Komodo.Core;

namespace Komodo.Server.Classes
{
    public class UserManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private LoggingModule Logging;
        private List<UserMaster> Users;
        private readonly object UserLock;

        #endregion

        #region Constructors-and-Factories

        public UserManager(LoggingModule logging)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            Logging = logging;
            Users = new List<UserMaster>();
            UserLock = new object();
        }

        public UserManager(LoggingModule logging, List<UserMaster> curr)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            Logging = logging;
            Users = new List<UserMaster>();
            UserLock = new object();
            if (curr != null && curr.Count > 0)
            {
                Users = new List<UserMaster>(curr);
            }
        }

        #endregion

        #region Public-Methods

        public void Add(UserMaster curr)
        {
            if (curr == null) return;
            lock (UserLock)
            {
                Users.Add(curr);
            }
            return;
        }

        public void Remove(UserMaster curr)
        {
            if (curr == null) return;
            lock (UserLock)
            {
                if (Users.Contains(curr)) Users.Remove(curr);
            }
            return;
        }

        public List<UserMaster> GetUsers()
        {
            List<UserMaster> curr = new List<UserMaster>();
            lock (UserLock)
            {
                curr = new List<UserMaster>(Users);
            }
            return curr;
        }

        public UserMaster GetUserByGuid(string guid)
        {
            if (String.IsNullOrEmpty(guid)) return null;
            lock (UserLock)
            {
                foreach (UserMaster curr in Users)
                {
                    if (String.Compare(curr.GUID, guid) == 0) return curr;
                }
            }
            return null;
        }

        public UserMaster GetUserByEmail(string email)
        {
            if (String.IsNullOrEmpty(email)) return null;
            lock (UserLock)
            {
                foreach (UserMaster curr in Users)
                {
                    if (String.Compare(curr.Email, email) == 0) return curr;
                }
            }
            return null;
        }

        public UserMaster GetUserById(int? id)
        {
            if (id == null) return null;
            int idInternal = Convert.ToInt32(id);
            lock (UserLock)
            {
                foreach (UserMaster curr in Users)
                {
                    if (curr.UserMasterId == idInternal) return curr;
                }
            }
            return null;
        }

        public bool AuthenticateCredentials(string email, string password, out UserMaster curr)
        {
            curr = null;
            if (String.IsNullOrEmpty(email)) return false;
            if (String.IsNullOrEmpty(password)) return false;

            curr = GetUserByEmail(email);
            if (curr == null)
            {
                Logging.Log(LoggingModule.Severity.Warn, "AuthenticateCredentials unable to find email " + email);
                return false;
            }

            if (String.Compare(curr.Password, password) == 0)
            {
                if (curr.Active)
                {
                    if (Common.IsLaterThanNow(curr.Expiration))
                    {
                        return true;
                    }
                    else
                    {
                        Logging.Log(LoggingModule.Severity.Warn, "AuthenticateCredentials UserMasterId " + curr.UserMasterId + " expired at " + curr.Expiration);
                        return false;
                    }
                }
                else
                {
                    Logging.Log(LoggingModule.Severity.Warn, "AuthenticateCredentials UserMasterId " + curr.UserMasterId + " marked inactive");
                    return false;
                }
            }
            else
            {
                Logging.Log(LoggingModule.Severity.Warn, "AuthenticateCredentials invalid password supplied for email " + email + " (" + password + " vs " + curr.Password + ")");
                return false;
            }
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
