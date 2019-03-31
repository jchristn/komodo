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
    public class ApiKeyManager
    {
        #region Public-Members

        #endregion

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
                    if (String.Compare(curr.GUID, guid) == 0) return curr;
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
                        if (!Common.IsTrue(curr.Active)) continue;
                        if (!Common.IsLaterThanNow(curr.Expiration)) continue;

                        if (Common.IsTrue(curr.AllowSearch)) ret.AllowSearch = true;
                        if (Common.IsTrue(curr.AllowCreateDocument)) ret.AllowCreateDocument = true;
                        if (Common.IsTrue(curr.AllowDeleteDocument)) ret.AllowDeleteDocument = true;
                        if (Common.IsTrue(curr.AllowCreateIndex)) ret.AllowCreateIndex = true;
                        if (Common.IsTrue(curr.AllowDeleteIndex)) ret.AllowDeleteIndex = true;
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

            if (currApiKey.Active)
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

                    if (currUserMaster.Active)
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

        #region Private-Methods

        #endregion
    }
}
