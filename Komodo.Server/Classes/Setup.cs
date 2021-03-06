﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BlobHelper;
using SyslogLogging;
using Watson.ORM;
using Watson.ORM.Core;
using Komodo;
using Common = Komodo.Common;
using DbSettings = Komodo.DbSettings;
using Index = Komodo.Index;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Perform requisite setup tasks to start the Komodo server.
    /// </summary>
    public class Setup
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the setup wizard.
        /// </summary>
        public Setup()
        {
            RunSetup();
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        private void RunSetup()
        {
            #region Variables

            DateTime timestamp = DateTime.Now.ToUniversalTime();
            Settings settings = new Settings();

            #endregion

            #region Welcome

            Console.WriteLine(
                Environment.NewLine +
                Environment.NewLine +
                "oooo                                                    .o8            " + Environment.NewLine +
                "`888                                                    888            " + Environment.NewLine +
                " 888  oooo   .ooooo.  ooo. .oo.  .oo.    .ooooo.   .oooo888   .ooooo.  " + Environment.NewLine +
                " 888 .8P'   d88' `88b `888P'Y88bP'Y88b  d88' `88b d88' `888  d88' `88b " + Environment.NewLine +
                " 888888.    888   888  888   888   888  888   888 888   888  888   888 " + Environment.NewLine +
                " 888 `88b.  888   888  888   888   888  888   888 888   888  888   888 " + Environment.NewLine +
                "o888o o888o `Y8bod8P' o888o o888o o888o `Y8bod8P' `Y8bod88P  `Y8bod8P' " + Environment.NewLine +
                Environment.NewLine +
                Environment.NewLine);

            // ________________         1         2         3         4         5         6         7
            // ________________12345678901234567890123456789012345678901234567890123456789012345678901234567890
            Console.WriteLine("Thank you for using Komodo!  We'll put together a basic system configuration");
            Console.WriteLine("so you can be up and running quickly.");
            Console.WriteLine(""); 
            Console.WriteLine(Common.Line(79, "-"));
            Console.WriteLine("");

            #endregion

            #region Initial-Settings

            settings.EnableConsole = true;
             
            settings.Server = new Settings.ServerSettings();
            settings.Server.HeaderApiKey = "x-api-key";
            settings.Server.AdminApiKey = "komodoadmin";
            settings.Server.ListenerPort = 9090;
            settings.Server.ListenerHostname = "localhost";

            settings.Logging = new Settings.LoggingSettings();
            settings.Logging.ConsoleLogging = true;
            settings.Logging.Header = "komodo";
            settings.Logging.SyslogServerIp = "127.0.0.1";
            settings.Logging.SyslogServerPort = 514;
            settings.Logging.MinimumLevel = Severity.Info;
            settings.Logging.FileLogging = true;
            settings.Logging.FileDirectory = "./logs/";
            settings.Logging.Filename = "Komodo.log";

            if (!Directory.Exists("./data/")) Directory.CreateDirectory("./data/");
            if (!Directory.Exists("./logs/")) Directory.CreateDirectory("./logs/");

            settings.Database = new DbSettings("./data/komodo.db");

            string tempDirectory = "./data/temp/";
            settings.TempStorage = new StorageSettings(new DiskSettings(tempDirectory));
            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

            string sourceDirectory = "./data/source/";
            settings.SourceDocuments = new StorageSettings(new DiskSettings(sourceDirectory));
            if (!Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);

            string parsedDirectory = "./data/parsed/";
            settings.ParsedDocuments = new StorageSettings(new DiskSettings(parsedDirectory));
            if (!Directory.Exists(parsedDirectory)) Directory.CreateDirectory(parsedDirectory);
             
            string postingsDirectory = "./data/postings/";
            settings.Postings = new StorageSettings(new DiskSettings(postingsDirectory));
            if (!Directory.Exists(postingsDirectory)) Directory.CreateDirectory(postingsDirectory);

            #endregion

            #region Initialize-Database-and-Create-Records

            WatsonORM orm = new WatsonORM(settings.Database.ToDatabaseSettings());

            orm.InitializeDatabase();
            orm.InitializeTable(typeof(ApiKey));
            orm.InitializeTable(typeof(Index));
            orm.InitializeTable(typeof(Metadata));
            orm.InitializeTable(typeof(MetadataDocument));
            orm.InitializeTable(typeof(Node));
            orm.InitializeTable(typeof(ParsedDocument));
            orm.InitializeTable(typeof(Permission));
            orm.InitializeTable(typeof(PostingsDocument));
            orm.InitializeTable(typeof(SourceDocument));
            orm.InitializeTable(typeof(TermDoc));
            orm.InitializeTable(typeof(TermGuid));
            orm.InitializeTable(typeof(User));

            DbExpression e = new DbExpression("id", DbOperators.GreaterThan, 0);

            User user = null;
            ApiKey apiKey = null;
            Permission perm = null;
            Index idx = null;

            List<User> users = orm.SelectMany<User>(e);
            if (users == null || users.Count < 1)
            {
                Console.WriteLine("| Creating first user 'default'");
                user = new User("default", "Default", "default@default.com", "default");
                user = orm.Insert<User>(user);
            } 
            else
            {
                Console.WriteLine("| Users already exist, not creating default user");
            }

            List<Index> indices = orm.SelectMany<Index>(e);
            if (indices == null || indices.Count < 1)
            {
                Console.WriteLine("| Creating first index 'default'");
                idx = new Index(user.GUID, "default");
                idx = orm.Insert<Index>(idx);
            }
            else
            {
                Console.WriteLine("| Indices already exist, not creating default index");
            }

            List<ApiKey> keys = orm.SelectMany<ApiKey>(e);
            if (keys == null || keys.Count < 1)
            {
                Console.WriteLine("| Creating first API key 'default'");
                apiKey = new ApiKey("default", user.GUID, true);
                apiKey = orm.Insert<ApiKey>(apiKey);
            }
            else
            {
                Console.WriteLine("| API keys already exist, not creating default API key");
            }

            List<Permission> perms = orm.SelectMany<Permission>(e);
            if (perms == null || perms.Count < 1)
            {
                Console.WriteLine("| Creating first permission 'default'");
                perm = new Permission(idx.GUID, user.GUID, apiKey.GUID, true, true, true, true, true);
                perm = orm.Insert<Permission>(perm);
            }
            else
            {
                Console.WriteLine("| Permissions already exist, not creating default permissions");
            }
             
            #endregion

            #region Write-System-JSON

            File.WriteAllBytes("./system.json", Encoding.UTF8.GetBytes(Common.SerializeJson(settings, true)));

            #endregion
             
            #region Wrap-Up

            string baseUrl = "http://localhost:" + settings.Server.ListenerPort;

            // ________________         1         2         3         4         5         6         7
            // ________________12345678901234567890123456789012345678901234567890123456789012345678901234567890
            Console.WriteLine("");
            Console.WriteLine("All finished!");
            Console.WriteLine("");
            Console.WriteLine("If you ever want to return to this setup wizard, just re-run the application");
            Console.WriteLine("from the terminal with the 'setup' argument.");
            Console.WriteLine("");
            Console.WriteLine("Verify Komodo is running in your browser using the following URL:");
            Console.WriteLine("");
            Console.WriteLine("  " + baseUrl);  
            Console.WriteLine("");
            Console.WriteLine("We've created your first index for you called 'First'.  Try POSTing a JSON");
            Console.WriteLine("document to the index using the API key 'default' using the URL:");
            Console.WriteLine("");
            Console.WriteLine("  " + baseUrl + "/default?type=json&name=My+First+Document&x-api-key=default"); 
            Console.WriteLine(""); 

            #endregion
        }

        #endregion
    }
}
