using System;
using System.Collections.Generic;
using System.Text;
using DatabaseWrapper;
using SyslogLogging;
using Komodo.Classes;
using Komodo.Database;

namespace Komodo.Server.Classes
{
    internal class AuthManager
    { 
        private KomodoDatabase _Database = null;

        internal AuthManager(KomodoDatabase db)
        {
            if (db == null) throw new ArgumentNullException(nameof(db)); 
            _Database = db;
        }

        internal bool Authenticate(string apiKey, out User user)
        {
            if (String.IsNullOrEmpty(apiKey)) throw new ArgumentNullException(nameof(apiKey));
            user = null;
            ApiKey key = GetApiKey(apiKey, out user);
            if (key != null && key != default(ApiKey)) return true;
            return false;
        } 

        internal bool Authenticate(string email, string passwordMd5)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (String.IsNullOrEmpty(passwordMd5)) throw new ArgumentNullException(nameof(passwordMd5));
            User user = GetUser(email, passwordMd5);
            if (user != null && user != default(User)) return true;
            return false;
        }

        internal bool AuthenticateAndAuthorize(string apiKey, PermissionType permType, out User user, out ApiKey key, out Permission perm)
        {
            if (String.IsNullOrEmpty(apiKey)) throw new ArgumentNullException(nameof(apiKey));  
            perm = GetPermission(apiKey, permType, out user, out key);
            if (perm != null) return true;
            return false; 
        }

        private Permission GetPermission(string apiKey, PermissionType permType, out User user, out ApiKey key)
        {
            if (String.IsNullOrEmpty(apiKey)) throw new ArgumentNullException(nameof(apiKey));
            key = GetApiKey(apiKey, out user);
            if (key == null || user == null) return null;

            Expression e = new Expression("id", Operators.GreaterThan, 0);

            switch (permType)
            {
                case PermissionType.Search:
                    e.PrependAnd(new Expression("allowsearch", Operators.Equals, 1));
                    break;
                case PermissionType.CreateDocument:
                    e.PrependAnd(new Expression("allowcreatedoc", Operators.Equals, 1));
                    break;
                case PermissionType.DeleteDocument:
                    e.PrependAnd(new Expression("allowdeletedoc", Operators.Equals, 1));
                    break;
                case PermissionType.CreateIndex:
                    e.PrependAnd(new Expression("allowcreateindex", Operators.Equals, 1));
                    break;
                case PermissionType.DeleteIndex:
                    e.PrependAnd(new Expression("allowdeleteindex", Operators.Equals, 1));
                    break;
                default:
                    throw new ArgumentException("Unknown permission type: " + permType.ToString());
            }

            Permission p = _Database.SelectByFilter<Permission>(e, "ORDER BY id DESC");
            if (p != null && p != default(Permission)) return p;
            return null;
        }

        private ApiKey GetApiKey(string apiKey, out User user)
        {
            if (String.IsNullOrEmpty(apiKey)) throw new ArgumentNullException(nameof(apiKey));
            user = null;
            Expression e = new Expression("guid", Operators.Equals, apiKey);
            e.PrependAnd("active", Operators.Equals, 1);
            ApiKey key = _Database.SelectByFilter<ApiKey>(e, "ORDER BY id DESC");
            if (key == null || key == default(ApiKey)) return null;

            e = new Expression("guid", Operators.Equals, key.UserGUID);
            user = _Database.SelectByFilter<User>(e, "ORDER BY id DESC");
            if (user == null || user == default(User)) return null;
            return key;
        }

        private User GetUser(string email, string passwordMd5)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (String.IsNullOrEmpty(passwordMd5)) throw new ArgumentNullException(nameof(passwordMd5));
            Expression e = new Expression("id", Operators.GreaterThan, 0);
            e.PrependAnd("email", Operators.Equals, email);
            e.PrependAnd("passwordmd5", Operators.Equals, passwordMd5); 
            User user = _Database.SelectByFilter<User>(e, "ORDER BY id DESC");
            if (user != null && user != default(User)) return user;
            return null;
        }
    }
}
