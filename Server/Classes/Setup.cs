﻿using System;
using System.Collections.Generic;
using System.Text;
using SyslogLogging;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server.Classes
{
    public class Setup
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

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

            DateTime timestamp = DateTime.Now;
            Config currConfig = new Config();
            
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
             
            currConfig.DocumentationUrl = "https://github.com/jchristn/komodo"; 
            currConfig.EnableConsole = true;
            
            #endregion

            #region Set-Defaults-for-Config-Sections

            #region Files

            currConfig.Files = new Config.FilesSettings();
            currConfig.Files.ApiKey = "./ApiKey.json";
            currConfig.Files.ApiKeyPermission = "./ApiKeyPermission.json";
            currConfig.Files.UserMaster = "./UserMaster.json";
            currConfig.Files.Indices = "./Indices.json";

            #endregion

            #region Debug

            currConfig.Debug = new Config.DebugSettings();
            currConfig.Debug.Database = false;

            #endregion

            #region Server

            currConfig.Server = new Config.ServerSettings();
            currConfig.Server.HeaderApiKey = "x-api-key";
            currConfig.Server.HeaderEmail = "x-email";
            currConfig.Server.HeaderPassword = "x-password";
            currConfig.Server.HeaderVersion = "x-version";
            currConfig.Server.AdminApiKey = "komodoadmin";
            currConfig.Server.ListenerPort = 9090;
            currConfig.Server.ListenerHostname = "127.0.0.1";

            #endregion
             
            #region Logging

            currConfig.Logging = new Config.LoggingSettings();
            currConfig.Logging.ConsoleLogging = true;
            currConfig.Logging.Header = "komodo";
            currConfig.Logging.SyslogServerIp = "127.0.0.1";
            currConfig.Logging.SyslogServerPort = 514;
            currConfig.Logging.LogHttpRequests = false;
            currConfig.Logging.LogHttpResponses = false;
            currConfig.Logging.MinimumLevel = 1;

            #endregion

            #region REST

            currConfig.Rest = new Config.RestSettings();
            currConfig.Rest.AcceptInvalidCerts = true;
            currConfig.Rest.UseWebProxy = false;

            #endregion

            #region Indexer

            currConfig.Indexer = new Config.IndexerSettings();
            currConfig.Indexer.IndexerIntervalMs = 5000;

            #endregion

            #endregion

            #region System-Config

            if (
                Common.FileExists("./System.json")
                )
            {
                Console.WriteLine("System configuration file already exists.");
                if (Common.InputBoolean("Do you wish to overwrite this file", true))
                {
                    Common.DeleteFile("./System.json");
                    if (!Common.WriteFile("./System.json", Common.SerializeJson(currConfig, true), false))
                    {
                        Common.ExitApplication("Setup", "Unable to write System.json", -1);
                        return;
                    }
                }
            }
            else
            {
                if (!Common.WriteFile("./System.json", Common.SerializeJson(currConfig, true), false))
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
                    currApiKey.Active = true;
                    currApiKey.ApiKeyId = 1;
                    currApiKey.Created = timestamp;
                    currApiKey.LastUpdate = timestamp;
                    currApiKey.Expiration = timestamp.AddYears(100);
                    currApiKey.GUID = "default";
                    currApiKey.Notes = "Created by setup script";
                    currApiKey.UserMasterId = 1;
                    apiKeys.Add(currApiKey);

                    currPerm = new ApiKeyPermission();
                    currPerm.Active = true;
                    currPerm.AllowSearch = true;
                    currPerm.AllowCreateDocument = true;
                    currPerm.AllowDeleteDocument = true;
                    currPerm.AllowCreateIndex = true;
                    currPerm.AllowDeleteIndex = true;
                    currPerm.AllowReindex = true;
                    currPerm.ApiKeyId = 1;
                    currPerm.ApiKeyPermissionId = 1;
                    currPerm.Created = timestamp;
                    currPerm.LastUpdate = timestamp;
                    currPerm.Expiration = timestamp.AddYears(100);
                    currPerm.GUID = "default";
                    currPerm.Notes = "Created by setup script";
                    currPerm.UserMasterId = 1;
                    permissions.Add(currPerm);

                    currUser = new UserMaster();
                    currUser.Active = true;
                    currUser.Address1 = "123 Some Street";
                    currUser.Cellphone = "408-555-1212";
                    currUser.City = "San Jose";
                    currUser.CompanyName = "Default Company";
                    currUser.Country = "USA";
                    currUser.FirstName = "First";
                    currUser.LastName = "Last";
                    currUser.Email = "default@default.com";
                    currUser.IsAdmin = true;
                    currUser.NodeId = 0;
                    currUser.Password = "default";
                    currUser.PostalCode = "95128";
                    currUser.State = "CA";
                    currUser.UserMasterId = 1;
                    currUser.GUID = "default";
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
                    Console.WriteLine("  GUID     : " + currUser.GUID);
                    Console.WriteLine("  API Key  : " + currApiKey.GUID);
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
                currApiKey.Active = true;
                currApiKey.ApiKeyId = 1;
                currApiKey.Created = timestamp;
                currApiKey.LastUpdate = timestamp;
                currApiKey.Expiration = timestamp.AddYears(100);
                currApiKey.GUID = "default";
                currApiKey.Notes = "Created by setup script";
                currApiKey.UserMasterId = 1;
                apiKeys.Add(currApiKey);

                currPerm = new ApiKeyPermission();
                currPerm.Active = true;
                currPerm.AllowSearch = true;
                currPerm.AllowCreateDocument = true;
                currPerm.AllowDeleteDocument = true;
                currPerm.AllowCreateIndex = true;
                currPerm.AllowDeleteIndex = true;
                currPerm.ApiKeyId = 1;
                currPerm.ApiKeyPermissionId = 1;
                currPerm.Created = timestamp;
                currPerm.LastUpdate = timestamp;
                currPerm.Expiration = timestamp.AddYears(100);
                currPerm.GUID = "default";
                currPerm.Notes = "Created by setup script";
                currPerm.UserMasterId = 1;
                permissions.Add(currPerm);

                currUser = new UserMaster();
                currUser.Active = true;
                currUser.Address1 = "123 Some Street";
                currUser.Cellphone = "408-555-1212";
                currUser.City = "San Jose";
                currUser.CompanyName = "Default Company";
                currUser.Country = "USA";
                currUser.FirstName = "First";
                currUser.LastName = "Last";
                currUser.Email = "default@default.com";
                currUser.IsAdmin = true;
                currUser.NodeId = 0;
                currUser.Password = "default";
                currUser.PostalCode = "95128";
                currUser.State = "CA";
                currUser.UserMasterId = 1;
                currUser.GUID = "default";
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
                Console.WriteLine("  GUID     : " + currUser.GUID);
                Console.WriteLine("  API Key  : " + currApiKey.GUID);
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
            Console.WriteLine("http://localhost:" + currConfig.Server.ListenerPort + "/loopback");
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to start.");
            Console.WriteLine("");
            Console.ReadLine();
            
            #endregion
        }

        #endregion
    }
}