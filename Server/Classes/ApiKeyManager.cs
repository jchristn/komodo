using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SyslogLogging;
using WatsonWebserver;
using KomodoCore;

namespace KomodoServer
{
    public class ApiKeyManager
    {
        #region Private-Members

        private LoggingModule Logging;
        private List<ApiKey> ApiKeys;
        private List<ApiKeyPermission> ApiKeyPermissions;
        private readonly object ApiKeyLock;
        private readonly object ApiKeyPermissionLock;

        #endregion

        #region Constructors-and-Factories

        public ApiKeyManager(LoggingModule logging)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            Logging = logging;
            ApiKeys = new List<ApiKey>();
            ApiKeyPermissions = new List<ApiKeyPermission>();
            ApiKeyLock = new object();
            ApiKeyPermissionLock = new object();
        }

        public ApiKeyManager(LoggingModule logging, List<ApiKey> keys, List<ApiKeyPermission> perms)
        {
            if (logging == null) throw new ArgumentNullException(nameof(logging));
            Logging = logging;
            ApiKeys = new List<ApiKey>();
            ApiKeyPermissions = new List<ApiKeyPermission>();
            ApiKeyLock = new object();
            ApiKeyPermissionLock = new object();
            if (keys != null && keys.Count > 0)
            {
                ApiKeys = new List<ApiKey>(keys);
            }
            if (perms != null && perms.Count > 0)
            {
                ApiKeyPermissions = new List<ApiKeyPermission>(perms);
            }
        }

        #endregion

        #region Public-Methods

        public void Add(ApiKey curr)
        {
            if (curr == null) return;
            lock (ApiKeyLock)
            {
                ApiKeys.Add(curr);
            }
            return;
        }

        public void Remove(ApiKey curr)
        {
            if (curr == null) return;
            lock (ApiKeyLock)
            {
                if (ApiKeys.Contains(curr)) ApiKeys.Remove(curr);
            }
            return;
        }

        public List<ApiKey> GetApiKeys()
        {
            List<ApiKey> curr = new List<ApiKey>();
            lock (ApiKeyLock)
            {
                curr = new List<ApiKey>(ApiKeys);
            }
            return curr;
        }

        public ApiKey GetApiKeyByGuid(string guid)
        {
            if (String.IsNullOrEmpty(guid)) return null;
            lock (ApiKeyLock)
            {
                foreach (ApiKey curr in ApiKeys)
                {
                    if (String.Compare(curr.Guid, guid) == 0) return curr;
                }
            }
            return null;
        }

        public ApiKey GetApiKeyById(int? id)
        {
            if (id == null) return null;
            int idInternal = Convert.ToInt32(id);
            lock (ApiKeyLock)
            {
                foreach (ApiKey curr in ApiKeys)
                {
                    if (curr.ApiKeyId == idInternal) return curr;
                }
            }
            return null;
        }

        public List<ApiKeyPermission> GetPermissionsByApiKeyId(int? apiKeyId)
        {
            List<ApiKeyPermission> ret = new List<ApiKeyPermission>();
            lock (ApiKeyPermissionLock)
            {
                foreach (ApiKeyPermission curr in ApiKeyPermissions)
                {
                    if (curr.ApiKeyId == apiKeyId)
                    {
                        ret.Add(curr);
                    }
                }
            }
            return ret;
        }

        public ApiKeyPermission GetEffectiveApiKeyPermissions(int? apiKeyId, int? userMasterId)
        {
            ApiKeyPermission ret = new ApiKeyPermission();
            ret.ApiKeyPermissionId = 0;
            ret.ApiKeyId = 0;
            ret.UserMasterId = Convert.ToInt32(userMasterId);
            ret.AllowSearch = 0;
            ret.AllowCreateDocument = 0;
            ret.AllowDeleteDocument = 0;
            ret.AllowCreateIndex = 0;
            ret.AllowDeleteIndex = 0;
            
            if (apiKeyId == null)
            {
                ret.AllowSearch = 1;
                ret.AllowCreateDocument= 1;
                ret.AllowDeleteDocument = 1;
                ret.AllowCreateIndex = 1;
                ret.AllowDeleteIndex = 1;
                return ret;
            }
            else
            {
                List<ApiKeyPermission> perms = GetPermissionsByApiKeyId(apiKeyId);

                if (perms != null && perms.Count > 0)
                {
                    foreach (ApiKeyPermission curr in perms)
                    {
                        if (!Common.IsTrue(curr.Active)) continue;
                        if (!Common.IsLaterThanNow(curr.Expiration)) continue;

                        if (Common.IsTrue(curr.AllowSearch)) ret.AllowSearch = 1;
                        if (Common.IsTrue(curr.AllowCreateDocument)) ret.AllowCreateDocument= 1;
                        if (Common.IsTrue(curr.AllowDeleteDocument)) ret.AllowDeleteDocument= 1;
                        if (Common.IsTrue(curr.AllowCreateIndex)) ret.AllowCreateIndex = 1;
                        if (Common.IsTrue(curr.AllowDeleteIndex)) ret.AllowDeleteIndex = 1;
                    }
                }

                return ret;
            }
        }

        public bool VerifyApiKey(
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
                Logging.Log(LoggingModule.Severity.Warn, "VerifyApiKey unable to retrieve API key " + apiKey);
                return false;
            }

            currPermission = GetEffectiveApiKeyPermissions(currApiKey.ApiKeyId, currUserMaster.UserMasterId);
            if (currPermission == null)
            {
                Logging.Log(LoggingModule.Severity.Warn, "VerifyApiKey unable to build ApiKeyPermission object for UserMasterId " + currUserMaster.UserMasterId);
                return false;
            }

            if (currApiKey.Active == 1)
            {
                #region Check-Key-Expiration

                if (Common.IsLaterThanNow(currApiKey.Expiration))
                {
                    currUserMaster = userManager.GetUserById(currApiKey.UserMasterId);
                    if (currUserMaster == null)
                    {
                        Logging.Log(LoggingModule.Severity.Warn, "VerifyApiKey unable to find UserMasterId " + currApiKey.UserMasterId);
                        return false;
                    }

                    if (currUserMaster.Active == 1)
                    {
                        #region Check-User-Expiration

                        if (Common.IsLaterThanNow(currUserMaster.Expiration))
                        {
                            return true;
                        }
                        else
                        {
                            Logging.Log(LoggingModule.Severity.Warn, "VerifyApiKey UserMasterId " + currUserMaster.UserMasterId + " expired at " + currUserMaster.Expiration);
                            return false;
                        }

                        #endregion
                    }
                    else
                    {
                        Logging.Log(LoggingModule.Severity.Warn, "VerifyApiKey UserMasterId " + currUserMaster.UserMasterId + " marked inactive");
                        return false;
                    }
                }
                else
                {
                    Logging.Log(LoggingModule.Severity.Warn, "VerifyApiKey ApiKeyId " + currApiKey.ApiKeyId + " expired at " + currApiKey.Expiration);
                    return false;
                }

                #endregion
            }
            else
            {
                Logging.Log(LoggingModule.Severity.Warn, "VerifyApiKey ApiKeyId " + currApiKey.ApiKeyId + " marked inactive");
                return false;
            }
        }

        #endregion
    }
}
