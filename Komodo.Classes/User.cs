using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Classes
{
    /// <summary>
    /// Accounts which, through API keys, have access to Komodo.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Name of the user.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// MD5 hash of the user's password.
        /// </summary>
        public string PasswordMd5 { get; set; }

        /// <summary>
        /// Indicates whether or not the account is able to be used.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public User()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="name">Name of the user.</param>
        /// <param name="email">Email address of the user.</param>
        /// <param name="passwordMd5">MD5 hash of the user's password.</param>
        public User(string name, string email, string passwordMd5)
        {
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (String.IsNullOrEmpty(passwordMd5)) throw new ArgumentNullException(nameof(passwordMd5));

            GUID = Guid.NewGuid().ToString();
            Name = name;
            Email = email;
            PasswordMd5 = passwordMd5;
            Active = true;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="name">Name of the user.</param>
        /// <param name="email">Email address of the user.</param>
        /// <param name="passwordMd5">MD5 hash of the user's password.</param>
        public User(string guid, string name, string email, string passwordMd5)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (String.IsNullOrEmpty(email)) throw new ArgumentNullException(nameof(email));
            if (String.IsNullOrEmpty(passwordMd5)) throw new ArgumentNullException(nameof(passwordMd5));

            GUID = guid;
            Name = name;
            Email = email;
            PasswordMd5 = passwordMd5;
            Active = true;
        }

        /// <summary>
        /// Create a database insertable dictionary from the object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("guid", GUID);
            ret.Add("name", Name);
            ret.Add("email", Email);
            ret.Add("passwordmd5", PasswordMd5);
            ret.Add("active", Convert.ToInt32(Active));
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static User FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            User ret = new User();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("name") && row["name"] != null && row["name"] != DBNull.Value)
                ret.Name = row["name"].ToString();

            if (row.Table.Columns.Contains("email") && row["email"] != null && row["email"] != DBNull.Value)
                ret.Email = row["email"].ToString();

            if (row.Table.Columns.Contains("passwordmd5") && row["passwordmd5"] != null && row["passwordmd5"] != DBNull.Value)
                ret.PasswordMd5 = row["passwordmd5"].ToString();

            if (row.Table.Columns.Contains("active") && row["active"] != null && row["active"] != DBNull.Value)
                ret.Active = Convert.ToBoolean(row["active"]);

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<User> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<User> ret = new List<User>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(User.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
