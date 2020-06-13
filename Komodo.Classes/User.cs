using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;

namespace Komodo.Classes
{
    /// <summary>
    /// Accounts which, through API keys, have access to Komodo.
    /// </summary>
    [Table("users")]
    public class User
    { 
        /// <summary>
        /// Database row ID.
        /// </summary>
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        [Column("guid", false, DataTypes.Nvarchar, 64, false)]
        public string GUID { get; set; }

        /// <summary>
        /// Name of the user.
        /// </summary>
        [Column("name", false, DataTypes.Nvarchar, 64, false)]
        public string Name { get; set; }

        /// <summary>
        /// Email address of the user.
        /// </summary>
        [Column("email", false, DataTypes.Nvarchar, 64, false)]
        public string Email { get; set; }

        /// <summary>
        /// MD5 hash of the user's password.
        /// </summary>
        [Column("passwordmd5", false, DataTypes.Nvarchar, 64, false)]
        public string PasswordMd5 { get; set; }

        /// <summary>
        /// Indicates whether or not the account is able to be used.
        /// </summary>
        [Column("active", false, DataTypes.Boolean, false)]
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
    }
}
