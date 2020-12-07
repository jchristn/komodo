using System;
using System.Collections.Generic;
using System.Text;
using DatabaseWrapper;
using SyslogLogging;
using Watson.ORM;
using Watson.ORM.Core;
using Komodo;

namespace Komodo.Server.Classes
{
    internal class AuthManager
    { 
        private WatsonORM _ORM = null;

        internal AuthManager(WatsonORM orm)
        {
            if (orm == null) throw new ArgumentNullException(nameof(orm)); 
            _ORM = orm;
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

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<Permission>(nameof(Permission.Id)), 
                DbOperators.GreaterThan, 
                0);

            switch (permType)
            {
                case PermissionType.Search:
                    e.PrependAnd(new DbExpression(_ORM.GetColumnName<Permission>(nameof(Permission.AllowSearch)), DbOperators.Equals, 1));
                    break;
                case PermissionType.CreateDocument:
                    e.PrependAnd(new DbExpression(_ORM.GetColumnName<Permission>(nameof(Permission.AllowCreateDocument)), DbOperators.Equals, 1));
                    break;
                case PermissionType.DeleteDocument:
                    e.PrependAnd(new DbExpression(_ORM.GetColumnName<Permission>(nameof(Permission.AllowDeleteDocument)), DbOperators.Equals, 1));
                    break;
                case PermissionType.CreateIndex:
                    e.PrependAnd(new DbExpression(_ORM.GetColumnName<Permission>(nameof(Permission.AllowCreateIndex)), DbOperators.Equals, 1));
                    break;
                case PermissionType.DeleteIndex:
                    e.PrependAnd(new DbExpression(_ORM.GetColumnName<Permission>(nameof(Permission.AllowDeleteIndex)), DbOperators.Equals, 1));
                    break;
                default:
                    throw new ArgumentException("Unknown permission type: " + permType.ToString());
            }

            Permission p = _ORM.SelectFirst<Permission>(e);
            if (p != null && p != default(Permission)) return p;
            return null;
        }

        private ApiKey GetApiKey(string apiKey, out User user)
        {
            if (String.IsNullOrEmpty(apiKey)) throw new ArgumentNullException(nameof(apiKey));
            user = null;

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<ApiKey>(nameof(ApiKey.GUID)), 
                DbOperators.Equals, 
                apiKey);

            e.PrependAnd(_ORM.GetColumnName<ApiKey>(nameof(ApiKey.Active)), DbOperators.Equals, true);
            ApiKey key = _ORM.SelectFirst<ApiKey>(e);
            if (key == null || key == default(ApiKey)) return null;

            e = new DbExpression(
                _ORM.GetColumnName<User>(nameof(User.GUID)), 
                DbOperators.Equals, 
                key.UserGUID);

            user = _ORM.SelectFirst<User>(e);
            if (user == null || user == default(User)) return null;
            return key;
        }

        private User GetUser(string email, string passwordMd5)
        {
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (String.IsNullOrEmpty(passwordMd5)) throw new ArgumentNullException(nameof(passwordMd5));

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<User>(nameof(User.Id)), 
                DbOperators.GreaterThan, 
                0);

            e.PrependAnd(_ORM.GetColumnName<User>(nameof(User.Email)), DbOperators.Equals, email);
            e.PrependAnd(_ORM.GetColumnName<User>(nameof(User.PasswordMd5)), DbOperators.Equals, passwordMd5);
            User user = _ORM.SelectFirst<User>(e);
            if (user != null && user != default(User)) return user;
            return null;
        }
    }
}
