using System;
using System.Collections.Generic;
using System.Text;
using Watson.ORM;
using Watson.ORM.Core;

namespace Komodo.Classes
{
    /// <summary>
    /// Database settings.
    /// </summary>
    public class DbSettings
    {
        /// <summary>
        /// Type of database: Mysql, Postgresql, Sqlite, SqlServer.
        /// </summary>
        public DbType Type { get; set; }

        /// <summary>
        /// For Sqlite, the database filename.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Database server hostname.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Database server port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Database username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Database password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// For SQL Server Express databases, the instance.
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Database name.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Instantiate the object for Sqlite only.
        /// </summary>
        /// <param name="filename">Database filename.</param>
        public DbSettings(string filename)
        {
            Filename = filename ?? throw new ArgumentNullException(nameof(filename));
            Type = DbType.Sqlite;
        }

        /// <summary>
        /// Instantiate the object for Mysql, Postgresql, or SqlServer.
        /// </summary>
        /// <param name="dbType">Type of database.</param>
        /// <param name="hostname">Database server hostname.</param>
        /// <param name="port">Database server port.</param>
        /// <param name="username">Database username.</param>
        /// <param name="password">Database password.</param>
        /// <param name="database">Database name.</param>
        public DbSettings(DbType dbType, string hostname, int port, string username, string password, string database)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0) throw new ArgumentException("Port must be zero or greater.");
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (String.IsNullOrEmpty(database)) throw new ArgumentNullException(nameof(database));

            if (dbType == DbType.Sqlite) throw new ArgumentException("Use the filename constructor for Sqlite databases.");

            Type = dbType;
            Hostname = hostname;
            Port = port;
            Username = username;
            Password = password;
            DatabaseName = database;
        }

        /// <summary>
        /// Instantiate the object for Mysql, Postgresql, or SqlServer.
        /// </summary>
        /// <param name="dbType">Type of database.</param>
        /// <param name="hostname">Database server hostname.</param>
        /// <param name="port">Database server port.</param>
        /// <param name="username">Database username.</param>
        /// <param name="password">Database password.</param>
        /// <param name="instance">Instance.</param>
        /// <param name="database">Database name.</param>
        public DbSettings(DbType dbType, string hostname, int port, string username, string password, string instance, string database)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0) throw new ArgumentException("Port must be zero or greater.");
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (String.IsNullOrEmpty(database)) throw new ArgumentNullException(nameof(database));

            if (dbType == DbType.Sqlite) throw new ArgumentException("Use the filename constructor for Sqlite databases.");

            Type = dbType;
            Hostname = hostname;
            Port = port;
            Username = username;
            Password = password;
            Instance = instance;
            DatabaseName = database;
        }

        /// <summary>
        /// Instantiate the object for SqlServer, i.e. for SQL Server or SQL Server Express.
        /// </summary>
        /// <param name="hostname">Database server hostname.</param>
        /// <param name="port">Database server port.</param>
        /// <param name="username">Database username.</param>
        /// <param name="password">Database password.</param>
        /// <param name="instance">Instance name.</param>
        /// <param name="database">Database name.</param>
        public DbSettings(string hostname, int port, string username, string password, string instance, string database)
        {
            if (String.IsNullOrEmpty(hostname)) throw new ArgumentNullException(nameof(hostname));
            if (port < 0) throw new ArgumentException("Port must be zero or greater.");
            if (String.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username));
            if (String.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password));
            if (String.IsNullOrEmpty(database)) throw new ArgumentNullException(nameof(database));

            Type = DbType.SqlServer;
            Hostname = hostname;
            Port = port;
            Username = username;
            Password = password;
            Instance = instance;
            DatabaseName = database;
        }

        /// <summary>
        /// Convert Komodo database settings to Watson.ORM.Core.DatabaseSettings.
        /// </summary>
        /// <returns></returns>
        public DatabaseSettings ToDatabaseSettings()
        { 
            switch (Type)
            {
                case DbType.Mysql:
                    return new DatabaseSettings(DbTypes.Mysql, Hostname, Port, Username, Password, DatabaseName);
                case DbType.Postgresql:
                    return new DatabaseSettings(DbTypes.Postgresql, Hostname, Port, Username, Password, DatabaseName);
                case DbType.Sqlite:
                    return new DatabaseSettings(Filename);
                case DbType.SqlServer:
                    return new DatabaseSettings(Hostname, Port, Username, Password, Instance, DatabaseName);
                default:
                    throw new ArgumentException("Unknown database type: " + Type.ToString());
            }
        }
    }

    /// <summary>
    /// Database type.
    /// </summary>
    public enum DbType
    {
        /// <summary>
        /// MySQL.
        /// </summary>
        Mysql,
        /// <summary>
        /// PostgreSQL.
        /// </summary>
        Postgresql,
        /// <summary>
        /// Sqlite.
        /// </summary>
        Sqlite,
        /// <summary>
        /// SQL Server including Express.
        /// </summary>
        SqlServer
    }
}
