using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SyslogLogging;
using WatsonWebserver;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server.Classes
{
    internal class ApiKeyManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private LoggingModule _Logging;
        private List<ApiKey> _ApiKeys;
        private List<ApiKeyPermission> _ApiKeyPermissions;
        private readonly object _ApiKeyLock;
        private readonly object _ApiKeyPermissionLock;

        #endregion

        #region Constructors-and-Factories
         
        internal ApiKeyManager(LoggingModule logging)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            _Logging = logging;
            _ApiKeys = new List<ApiKey>();
            _ApiKeyPermissions = new List<ApiKeyPermission>();
            _ApiKeyLock = new object();
            _ApiKeyPermissionLock = new object();
        }
         
        internal ApiKeyManager(LoggingModule logging, List<ApiKey> keys, List<ApiKeyPermission> perms)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            _Logging = logging;
            _ApiKeys = new List<ApiKey>();
            _ApiKeyPermissions = new List<ApiKeyPermission>();
            _ApiKeyLock = new object();
            _ApiKeyPermissionLock = new object();
            if (keys != null && keys.Count > 0)
            {
                _ApiKeys = new List<ApiKey>(keys);
            }
            if (perms != null && perms.Count > 0)
            {
                _ApiKeyPermissions = new List<ApiKeyPermission>(perms);
            }
        }

        #endregion

        #region Internal-Methods
         
        internal void Add(ApiKey curr)
        {
            if (curr == null) return;
            lock (_ApiKeyLock)
            {
                _ApiKeys.Add(curr);
            }
            return;
        }
         
        internal void Remove(ApiKey curr)
        {
            if (curr == null) return;
            lock (_ApiKeyLock)
            {
                if (_ApiKeys.Contains(curr)) _ApiKeys.Remove(curr);
            }
            return;
        }
         
        internal List<ApiKey> GetApiKeys()
        {
            List<ApiKey> curr = new List<ApiKey>();
            lock (_ApiKeyLock)
            {
                curr = new List<ApiKey>(_ApiKeys);
            }
            return curr;
        }
         
        internal ApiKey GetApiKeyByGuid(string guid)
        {
            if (String.IsNullOrEmpty(guid)) return null;
            lock (_ApiKeyLock)
            {
                foreach (ApiKey curr in _ApiKeys)
                {
                    if (String.Compare(curr.GUID, guid) == 0) return curr;
                }
            }
            return null;
        }
         
        internal ApiKey GetApiKeyById(int? id)
        {
            if (id == null) return null;
            int idInternal = Convert.ToInt32(id);
            lock (_ApiKeyLock)
            {
                foreach (ApiKey curr in _ApiKeys)
                {
                    if (curr.ApiKeyId == idInternal) return curr;
                }
            }
            return null;
        }
         
        internal List<ApiKeyPermission> GetPermissionsByApiKeyId(int? apiKeyId)
        {
            List<ApiKeyPermission> ret = new List<ApiKeyPermission>();
            lock (_ApiKeyPermissionLock)
            {
                foreach (ApiKeyPermission curr in _ApiKeyPermissions)
                {
                    if (curr.ApiKeyId == apiKeyId)
                    {
                        ret.Add(curr);
                    }
                }
            }
            return ret;
        }
         
        internal ApiKeyPermission GetEffectiveApiKeyPermissions(int? apiKeyId, int? userMasterId)
        {
            ApiKeyPermission ret = new ApiKeyPermission();
            ret.ApiKeyPermissionId = 0;
            ret.ApiKeyId = 0;
            ret.UserMasterId = Convert.ToInt32(userMasterId);
            ret.AllowSearch = false;
            ret.AllowCreateDocument = false;
            ret.AllowDeleteDocument = false;
            ret.AllowCreateIndex = false;
            ret.AllowDeleteIndex = false;

            if (apiKeyId == null)
            {
                ret.AllowSearch = true;
                ret.AllowCreateDocument = true;
                ret.AllowDeleteDocument = true;
                ret.AllowCreateIndex = true;
                ret.AllowDeleteIndex = true;
                return ret;
            }
            else
            {
                List<ApiKeyPermission> perms = GetPermissionsByApiKeyId(apiKeyId);

                if (perms != null && perms.Count > 0)
                {
                    foreach (ApiKeyPermission curr in perms)
                    {
                        if (!curr.Active) continue;
                        if (!Common.IsLaterThanNow(curr.Expiration)) continue;

                        if (curr.AllowSearch) ret.AllowSearch = true;
                        if (curr.AllowCreateDocument) ret.AllowCreateDocument = true;
                        if (curr.AllowDeleteDocument) ret.AllowDeleteDocument = true;
                        if (curr.AllowCreateIndex) ret.AllowCreateIndex = true;
                        if (curr.AllowDeleteIndex) ret.AllowDeleteIndex = true;
                    }
                }

                return ret;
            }
        }
         
        internal bool VerifyApiKey(
            string apiKey,
            UserManager userManager,
            out UserMaster currUserMaster,
            out ApiKey currApiKey,
            out ApiKeyPermission currPermission
            )
        {
            currUserMaster = new UserMaster();
            currApiKey = new ApiKey();
            currPermission = new ApiKeyPermission();

            currApiKey = GetApiKeyByGuid(apiKey);
            if (currApiKey == null)
            {
                _Logging.Warn("VerifyApiKey unable to retrieve API key " + apiKey);
                return false;
            }

            currPermission = GetEffectiveApiKeyPermissions(currApiKey.ApiKeyId, currUserMaster.UserMasterId);
            if (currPermission == null)
            {
                _Logging.Warn("VerifyApiKey unable to build ApiKeyPermission object for UserMasterId " + currUserMaster.UserMasterId);
                return false;
            }

            if (currApiKey.Active)
            {
                #region Check-Key-Expiration

                if (Common.IsLaterThanNow(currApiKey.Expiration))
                {
                    currUserMaster = userManager.GetUserById(currApiKey.UserMasterId);
                    if (currUserMaster == null)
                    {
                        _Logging.Warn("VerifyApiKey unable to find UserMasterId " + currApiKey.UserMasterId);
                        return false;
                    }

                    if (currUserMaster.Active)
                    {
                        #region Check-User-Expiration

                        if (Common.IsLaterThanNow(currUserMaster.Expiration))
                        {
                            return true;
                        }
                        else
                        {
                            _Logging.Warn("VerifyApiKey UserMasterId " + currUserMaster.UserMasterId + " expired at " + currUserMaster.Expiration);
                            return false;
                        }

                        #endregion
                    }
                    else
                    {
                        _Logging.Warn("VerifyApiKey UserMasterId " + currUserMaster.UserMasterId + " marked inactive");
                        return false;
                    }
                }
                else
                {
                    _Logging.Warn("VerifyApiKey ApiKeyId " + currApiKey.ApiKeyId + " expired at " + currApiKey.Expiration);
                    return false;
                }

                #endregion
            }
            else
            {
                _Logging.Warn("VerifyApiKey ApiKeyId " + currApiKey.ApiKeyId + " marked inactive");
                return false;
            }
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
