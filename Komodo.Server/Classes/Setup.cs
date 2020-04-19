using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BlobHelper;
using DatabaseWrapper;
using Komodo.Classes;
using Komodo.Database; 
using Index = Komodo.Classes.Index;

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
            Console.WriteLine("Press ENTER to get started.");
            Console.WriteLine("");
            Console.WriteLine(Common.Line(79, "-"));
            Console.ReadLine();

            #endregion

            #region Initial-Settings

            settings.EnableConsole = true;
             
            settings.Server = new Settings.ServerSettings();
            settings.Server.HeaderApiKey = "x-api-key";
            settings.Server.AdminApiKey = "komodoadmin";
            settings.Server.ListenerPort = 9090;
            settings.Server.ListenerHostname = "127.0.0.1";

            settings.Logging = new Settings.LoggingSettings();
            settings.Logging.ConsoleLogging = true;
            settings.Logging.Header = "komodo";
            settings.Logging.SyslogServerIp = "127.0.0.1";
            settings.Logging.SyslogServerPort = 514;
            settings.Logging.MinimumLevel = 1;

            if (!Directory.Exists("./Data/")) Directory.CreateDirectory("./Data/");

            settings.Database = new Komodo.Classes.DatabaseSettings("./Data/Komodo.db");

            string tempDirectory = "./Data/Temp/";
            settings.TempStorage = new StorageSettings(new DiskSettings(tempDirectory));
            if (!Directory.Exists(tempDirectory)) Directory.CreateDirectory(tempDirectory);

            string sourceDirectory = "./Data/SourceDocuments/";
            settings.SourceDocuments = new StorageSettings(new DiskSettings(sourceDirectory));
            if (!Directory.Exists(sourceDirectory)) Directory.CreateDirectory(sourceDirectory);

            string parsedDirectory = "./Data/ParsedDocuments/";
            settings.ParsedDocuments = new StorageSettings(new DiskSettings(parsedDirectory));
            if (!Directory.Exists(parsedDirectory)) Directory.CreateDirectory(parsedDirectory);

            string postingsDirectory = "./Data/Postings/";
            settings.Postings = new StorageSettings(new DiskSettings(postingsDirectory));
            if (!Directory.Exists(postingsDirectory)) Directory.CreateDirectory(postingsDirectory);

            #endregion

            #region Initialize-Database-and-Create-Records

            KomodoDatabase db = new KomodoDatabase(settings.Database);
            Expression e = new Expression("id", Operators.GreaterThan, 0);

            User user = null;
            ApiKey apiKey = null;
            Permission perm = null;
            Index idx = null;

            if (db.SelectByFilter<User>(e, "ORDER BY id DESC") == null)
            {
                user = new User("default", "Default", "default@default.com", "default");
                user = db.Insert<User>(user);

                idx = new Index(user.GUID, "default");
                idx = db.Insert<Index>(idx);

                apiKey = new ApiKey("default", user.GUID, true);
                apiKey = db.Insert<ApiKey>(apiKey);

                perm = new Permission(idx.GUID, user.GUID, apiKey.GUID, true, true, true, true, true);
                perm = db.Insert<Permission>(perm);
            }
             
            #endregion

            #region Write-System-JSON

            File.WriteAllBytes("./System.json", Encoding.UTF8.GetBytes(Common.SerializeJson(settings, true)));

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
