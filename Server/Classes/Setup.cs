using System;
using System.Collections.Generic;
using System.Text;
using SyslogLogging;
using KomodoCore;

namespace KomodoServer
{
    public class Setup
    {
        public Setup()
        {
            RunSetup();
        }

        private void RunSetup()
        {
            #region Variables

            DateTime timestamp = DateTime.Now;
            Config currConfig = new Config();
            string separator = "";
            string workingDir = "";
            
            ApiKey currApiKey = new ApiKey();
            List<ApiKey> apiKeys = new List<ApiKey>();

            ApiKeyPermission currPerm = new ApiKeyPermission();
            List<ApiKeyPermission> permissions = new List<ApiKeyPermission>();

            UserMaster currUser = new UserMaster();
            List<UserMaster> users = new List<UserMaster>();

            #endregion

            #region Welcome

            //          1         2         3         4         5         6         7
            // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
            Console.WriteLine("Thank you for using Komodo!  We'll put together a basic system configuration");
            Console.WriteLine("so you can be up and running quickly.");
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to get started.");
            Console.WriteLine("");
            Console.WriteLine(Common.Line(79, "-"));
            Console.ReadLine();

            #endregion

            #region Initial-Settings

            currConfig.ProductVersion = "1.0.0";
            currConfig.DocumentationUrl = "http://www.komodosearch.com/docs/";

            int platform = (int)Environment.OSVersion.Platform;
            if ((platform == 4) || (platform == 6) || (platform == 128))
            {
                currConfig.Environment = "linux";
                currConfig.EnvironmentSeparator = "/";
                separator = "/";
            }
            else
            {
                currConfig.Environment = "windows";
                currConfig.EnvironmentSeparator = "\\";
                separator = "\\";
            }

            currConfig.EnableConsole = 1;

            workingDir = Environment.CurrentDirectory + separator;

            #endregion

            #region Set-Defaults-for-Config-Sections

            #region Files

            currConfig.Files = new Config.FilesSettings();
            currConfig.Files.ApiKey = workingDir + "ApiKey.json";
            currConfig.Files.ApiKeyPermission = workingDir + "ApiKeyPermission.json";
            currConfig.Files.UserMaster = workingDir + "UserMaster.json";
            currConfig.Files.IndicesDatabase = workingDir + "Indices.db";

            #endregion

            #region Debug

            currConfig.Debug = new Config.DebugSettings();
            currConfig.Debug.Database = 0;

            #endregion

            #region Server

            currConfig.Server = new Config.ServerSettings();
            currConfig.Server.HeaderApiKey = "x-api-key";
            currConfig.Server.HeaderEmail = "x-email";
            currConfig.Server.HeaderPassword = "x-password";
            currConfig.Server.HeaderVersion = "x-version";
            currConfig.Server.AdminApiKey = "komodoadmin";
            currConfig.Server.ListenerPort = 9090;

            switch (currConfig.Environment)
            {
                case "linux":
                    Console.WriteLine("");
                    //          1         2         3         4         5         6         7
                    // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
                    Console.WriteLine("IMPORTANT: for Linux and Mac environments, Komodo can only receive requests on");
                    Console.WriteLine("one hostname.  The hostname you set here must either be the hostname in the URL");
                    Console.WriteLine("used by the requestor, or, set in the HOST header of each request.");
                    Console.WriteLine("");
                    Console.WriteLine("If you set the hostname to 'localhost', this node will ONLY receive and handle");
                    Console.WriteLine("requests destined to 'localhost', i.e. it will only handle local requests.");
                    Console.WriteLine("");

                    currConfig.Server.ListenerHostname = Common.InputString("On which hostname shall this node listen?", "localhost", false);
                    break;

                case "windows":
                    currConfig.Server.ListenerHostname = "+";
                    break;
            }

            #endregion
             
            #region Logging

            currConfig.Logging = new Config.LoggingSettings();
            currConfig.Logging.ConsoleLogging = 1;
            currConfig.Logging.Header = "komodo";
            currConfig.Logging.SyslogServerIp = "127.0.0.1";
            currConfig.Logging.SyslogServerPort = 514;
            currConfig.Logging.LogHttpRequests = 0;
            currConfig.Logging.LogHttpResponses = 0;
            currConfig.Logging.MinimumLevel = 1;

            #endregion

            #region REST

            currConfig.Rest = new Config.RestSettings();
            currConfig.Rest.AcceptInvalidCerts = 1;
            currConfig.Rest.UseWebProxy = 0;

            #endregion

            #region Indexer

            currConfig.Indexer = new Config.IndexerSettings();
            currConfig.Indexer.IndexerIntervalMs = 5000;

            #endregion

            #endregion

            #region System-Config

            if (
                Common.FileExists(workingDir + "System.json")
                )
            {
                Console.WriteLine("System configuration file already exists.");
                if (Common.InputBoolean("Do you wish to overwrite this file", true))
                {
                    Common.DeleteFile(workingDir + "System.json");
                    if (!Common.WriteFile(workingDir + "System.json", Common.SerializeJson(currConfig, true), false))
                    {
                        Common.ExitApplication("Setup", "Unable to write System.json", -1);
                        return;
                    }
                }
            }
            else
            {
                if (!Common.WriteFile(workingDir + "System.json", Common.SerializeJson(currConfig, true), false))
                {
                    Common.ExitApplication("Setup", "Unable to write System.json", -1);
                    return;
                }
            }

            #endregion

            #region Users-API-Keys-and-Permissions

            if (
                Common.FileExists(currConfig.Files.ApiKey)
                || Common.FileExists(currConfig.Files.ApiKeyPermission)
                || Common.FileExists(currConfig.Files.UserMaster)
                )
            {
                Console.WriteLine("Configuration files already exist for API keys, users, and/or permissions.");
                if (Common.InputBoolean("Do you wish to overwrite these files", true))
                {
                    Common.DeleteFile(currConfig.Files.ApiKey);
                    Common.DeleteFile(currConfig.Files.ApiKeyPermission);
                    Common.DeleteFile(currConfig.Files.UserMaster);

                    Console.WriteLine("Creating new configuration files for API keys, users, and permissions.");

                    currApiKey = new ApiKey();
                    currApiKey.Active = 1;
                    currApiKey.ApiKeyId = 1;
                    currApiKey.Created = timestamp;
                    currApiKey.LastUpdate = timestamp;
                    currApiKey.Expiration = timestamp.AddYears(100);
                    currApiKey.Guid = "default";
                    currApiKey.Notes = "Created by setup script";
                    currApiKey.UserMasterId = 1;
                    apiKeys.Add(currApiKey);

                    currPerm = new ApiKeyPermission();
                    currPerm.Active = 1;
                    currPerm.AllowSearch = 1;
                    currPerm.AllowCreateDocument = 1;
                    currPerm.AllowDeleteDocument = 1;
                    currPerm.AllowCreateIndex = 1;
                    currPerm.AllowDeleteIndex = 1;
                    currPerm.AllowReindex = 1;
                    currPerm.ApiKeyId = 1;
                    currPerm.ApiKeyPermissionId = 1;
                    currPerm.Created = timestamp;
                    currPerm.LastUpdate = timestamp;
                    currPerm.Expiration = timestamp.AddYears(100);
                    currPerm.Guid = "default";
                    currPerm.Notes = "Created by setup script";
                    currPerm.UserMasterId = 1;
                    permissions.Add(currPerm);

                    currUser = new UserMaster();
                    currUser.Active = 1;
                    currUser.Address1 = "123 Some Street";
                    currUser.Cellphone = "408-555-1212";
                    currUser.City = "San Jose";
                    currUser.CompanyName = "Default Company";
                    currUser.Country = "USA";
                    currUser.FirstName = "First";
                    currUser.LastName = "Last";
                    currUser.Email = "default@default.com";
                    currUser.IsAdmin = 1;
                    currUser.NodeId = 0;
                    currUser.Password = "default";
                    currUser.PostalCode = "95128";
                    currUser.State = "CA";
                    currUser.UserMasterId = 1;
                    currUser.Guid = "default";
                    currUser.Created = timestamp;
                    currUser.LastUpdate = timestamp;
                    currUser.Expiration = timestamp.AddYears(100);
                    users.Add(currUser);

                    if (!Common.WriteFile(currConfig.Files.ApiKey, Common.SerializeJson(apiKeys, true), false))
                    {
                        Common.ExitApplication("Setup", "Unable to write " + currConfig.Files.ApiKey, -1);
                        return;
                    }

                    if (!Common.WriteFile(currConfig.Files.ApiKeyPermission, Common.SerializeJson(permissions, true), false))
                    {
                        Common.ExitApplication("Setup", "Unable to write " + currConfig.Files.ApiKeyPermission, -1);
                        return;
                    }

                    if (!Common.WriteFile(currConfig.Files.UserMaster, Common.SerializeJson(users, true), false))
                    {
                        Common.ExitApplication("Setup", "Unable to write " + currConfig.Files.UserMaster, -1);
                        return;
                    }

                    Console.WriteLine("We have created your first user account and permissions.");
                    Console.WriteLine("  Email    : " + currUser.Email);
                    Console.WriteLine("  Password : " + currUser.Password);
                    Console.WriteLine("  GUID     : " + currUser.Guid);
                    Console.WriteLine("  API Key  : " + currApiKey.Guid);
                    Console.WriteLine("");
                    Console.WriteLine("This was done by creating the following files:");
                    Console.WriteLine("  " + currConfig.Files.UserMaster);
                    Console.WriteLine("  " + currConfig.Files.ApiKey);
                    Console.WriteLine("  " + currConfig.Files.ApiKeyPermission);
                    Console.WriteLine("");
                }
                else
                {
                    Console.WriteLine("Existing files were left in tact.");
                }
            }
            else
            {
                currApiKey = new ApiKey();
                currApiKey.Active = 1;
                currApiKey.ApiKeyId = 1;
                currApiKey.Created = timestamp;
                currApiKey.LastUpdate = timestamp;
                currApiKey.Expiration = timestamp.AddYears(100);
                currApiKey.Guid = "default";
                currApiKey.Notes = "Created by setup script";
                currApiKey.UserMasterId = 1;
                apiKeys.Add(currApiKey);

                currPerm = new ApiKeyPermission();
                currPerm.Active = 1;
                currPerm.AllowSearch = 1;
                currPerm.AllowCreateDocument = 1;
                currPerm.AllowDeleteDocument = 1;
                currPerm.AllowCreateIndex = 1;
                currPerm.AllowDeleteIndex = 1;
                currPerm.ApiKeyId = 1;
                currPerm.ApiKeyPermissionId = 1;
                currPerm.Created = timestamp;
                currPerm.LastUpdate = timestamp;
                currPerm.Expiration = timestamp.AddYears(100);
                currPerm.Guid = "default";
                currPerm.Notes = "Created by setup script";
                currPerm.UserMasterId = 1;
                permissions.Add(currPerm);

                currUser = new UserMaster();
                currUser.Active = 1;
                currUser.Address1 = "123 Some Street";
                currUser.Cellphone = "408-555-1212";
                currUser.City = "San Jose";
                currUser.CompanyName = "Default Company";
                currUser.Country = "USA";
                currUser.FirstName = "First";
                currUser.LastName = "Last";
                currUser.Email = "default@default.com";
                currUser.IsAdmin = 1;
                currUser.NodeId = 0;
                currUser.Password = "default";
                currUser.PostalCode = "95128";
                currUser.State = "CA";
                currUser.UserMasterId = 1;
                currUser.Guid = "default";
                currUser.Created = timestamp;
                currUser.LastUpdate = timestamp;
                currUser.Expiration = timestamp.AddYears(100);
                users.Add(currUser);

                if (!Common.WriteFile(currConfig.Files.ApiKey, Common.SerializeJson(apiKeys, true), false))
                {
                    Common.ExitApplication("Setup", "Unable to write " + currConfig.Files.ApiKey, -1);
                    return;
                }

                if (!Common.WriteFile(currConfig.Files.ApiKeyPermission, Common.SerializeJson(permissions, true), false))
                {
                    Common.ExitApplication("Setup", "Unable to write " + currConfig.Files.ApiKeyPermission, -1);
                    return;
                }

                if (!Common.WriteFile(currConfig.Files.UserMaster, Common.SerializeJson(users, true), false))
                {
                    Common.ExitApplication("Setup", "Unable to write " + currConfig.Files.UserMaster, -1);
                    return;
                }

                Console.WriteLine("We have created your first user account and permissions.");
                Console.WriteLine("  Email    : " + currUser.Email);
                Console.WriteLine("  Password : " + currUser.Password);
                Console.WriteLine("  GUID     : " + currUser.Guid);
                Console.WriteLine("  API Key  : " + currApiKey.Guid);
                Console.WriteLine("");
                Console.WriteLine("This was done by creating the following files:");
                Console.WriteLine("  " + currConfig.Files.UserMaster);
                Console.WriteLine("  " + currConfig.Files.ApiKey);
                Console.WriteLine("  " + currConfig.Files.ApiKeyPermission);
                Console.WriteLine("");
            }

            #endregion
               
            #region Wrap-Up

            //          1         2         3         4         5         6         7
            // 12345678901234567890123456789012345678901234567890123456789012345678901234567890

            Console.WriteLine("");
            Console.WriteLine("All finished!");
            Console.WriteLine("");
            Console.WriteLine("If you ever want to return to this setup wizard, just re-run the application");
            Console.WriteLine("from the terminal with the 'setup' argument.");
            Console.WriteLine("");
            Console.WriteLine("Verify Komodo is running in your browser:");
            Console.WriteLine("");

            switch (currConfig.Environment)
            {
                case "linux":
                    Console.WriteLine("http://" + currConfig.Server.ListenerHostname + ":" + currConfig.Server.ListenerPort + "/loopback");
                    break;

                case "windows":
                    Console.WriteLine("http://localhost:" + currConfig.Server.ListenerPort + "/loopback");
                    break;
            }
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to start.");
            Console.WriteLine("");
            Console.ReadLine();
            
            #endregion
        }
    }
}