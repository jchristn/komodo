using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SyslogLogging;
using WatsonWebserver;
using Komodo.Core;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// User manager.
    /// </summary>
    public class UserManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private LoggingModule _Logging;
        private List<UserMaster> _Users;
        private readonly object _Lock;

        #endregion

        #region Constructors-and-Factories
        
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="logging">LoggingModule instance.</param>
        public UserManager(LoggingModule logging)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            _Logging = logging;
            _Users = new List<UserMaster>();
            _Lock = new object();
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="logging">LoggingModule instance.</param>
        /// <param name="users">List of users.</param>
        public UserManager(LoggingModule logging, List<UserMaster> users)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            _Logging = logging;
            _Users = new List<UserMaster>();
            _Lock = new object();
            if (users != null && users.Count > 0)
            {
                _Users = new List<UserMaster>(users);
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add a user.
        /// </summary>
        /// <param name="user">User object.</param>
        public void Add(UserMaster user)
        {
            if (user == null) return;
            lock (_Lock)
            {
                _Users.Add(user);
            }
            return;
        }

        /// <summary>
        /// Remove a user.
        /// </summary>
        /// <param name="user">User object.</param>
        public void Remove(UserMaster user)
        {
            if (user == null) return;
            lock (_Lock)
            {
                if (_Users.Contains(user)) _Users.Remove(user);
            }
            return;
        }

        /// <summary>
        /// Retrieve a list of users.
        /// </summary>
        /// <returns>List of users.</returns>
        public List<UserMaster> GetUsers()
        {
            List<UserMaster> curr = new List<UserMaster>();
            lock (_Lock)
            {
                curr = new List<UserMaster>(_Users);
            }
            return curr;
        }

        /// <summary>
        /// Retrieve a user by GUID.
        /// </summary>
        /// <param name="guid">GUID of the user.</param>
        /// <returns>User object.</returns>
        public UserMaster GetUserByGuid(string guid)
        {
            if (String.IsNullOrEmpty(guid)) return null;
            lock (_Lock)
            {
                foreach (UserMaster curr in _Users)
                {
                    if (String.Compare(curr.GUID, guid) == 0) return curr;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieve a user by email address.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <returns>User object.</returns>
        public UserMaster GetUserByEmail(string email)
        {
            if (String.IsNullOrEmpty(email)) return null;
            lock (_Lock)
            {
                foreach (UserMaster curr in _Users)
                {
                    if (String.Compare(curr.Email, email) == 0) return curr;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieve a user by ID.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>User object.</returns>
        public UserMaster GetUserById(int? id)
        {
            if (id == null) return null;
            int idInternal = Convert.ToInt32(id);
            lock (_Lock)
            {
                foreach (UserMaster curr in _Users)
                {
                    if (curr.UserMasterId == idInternal) return curr;
                }
            }
            return null;
        }

        /// <summary>
        /// Perform credential authentication.
        /// </summary>
        /// <param name="email">Email address.</param>
        /// <param name="password">Password.</param>
        /// <param name="user">User object.</param>
        /// <returns>True if authenticated.</returns>
        public bool AuthenticateCredentials(string email, string password, out UserMaster user)
        {
            user = null;
            if (String.IsNullOrEmpty(email)) return false;
            if (String.IsNullOrEmpty(password)) return false;

            user = GetUserByEmail(email);
            if (user == null)
            {
                _Logging.Warn("AuthenticateCredentials unable to find email " + email);
                return false;
            }

            if (String.Compare(user.Password, password) == 0)
            {
                if (user.Active)
                {
                    if (Common.IsLaterThanNow(user.Expiration))
                    {
                        return true;
                    }
                    else
                    {
                        _Logging.Warn("AuthenticateCredentials UserMasterId " + user.UserMasterId + " expired at " + user.Expiration);
                        return false;
                    }
                }
                else
                {
                    _Logging.Warn("AuthenticateCredentials UserMasterId " + user.UserMasterId + " marked inactive");
                    return false;
                }
            }
            else
            {
                _Logging.Warn("AuthenticateCredentials invalid password supplied for email " + email + " (" + password + " vs " + user.Password + ")");
                return false;
            }
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
