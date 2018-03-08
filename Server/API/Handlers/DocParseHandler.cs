using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using KomodoCore;
using RestWrapper;
using DatabaseWrapper;

namespace KomodoServer
{
    public static class DocParseHandler
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion
        
        #region Public-Methods

        public static bool FromUrl(Config currConfig, string url, string docType, out object parsed, out RestResponse resp, out List<string> errors)
        {
            errors = new List<string>();
            parsed = null;
            resp = null;

            #region Check-for-Null-Values

            if (currConfig == null)
            {
                errors.Add("No configuration file supplied.");
                return false;
            }

            if (String.IsNullOrEmpty(url))
            {
                errors.Add("No URL supplied.");
                return false;
            }

            if (String.IsNullOrEmpty(docType))
            {
                errors.Add("No document type supplied.");
                return false;
            }

            if (!docType.ToLower().Equals("html")
                && !docType.ToLower().Equals("json")
                && !docType.ToLower().Equals("xml"))
            {
                errors.Add("Unable to process requested document type from URL.");
                return false;
            }

            #endregion

            #region Retrieve-Data

            resp = RestRequest.SendRequestSafe(url, "text/plain", "GET", null, null, false,
                Common.IsTrue(currConfig.Rest.AcceptInvalidCerts), null, null);

            if (resp == null)
            {
                errors.Add("Unable to retrieve response from URL.");
                return false;
            }

            if (resp.StatusCode != 200)
            {
                errors.Add("Non-200 response retrieved from server.");
                return false;
            }

            if (resp.Data == null || resp.Data.Length < 1)
            {
                errors.Add("No data returned from server.");
                return false;
            }

            #endregion

            #region Process-by-Doctype

            ParsedHtml html = new ParsedHtml();
            ParsedJson json = new ParsedJson();
            ParsedXml xml = new ParsedXml();
            bool success = false;

            switch (docType.ToLower())
            {
                case "html":
                    success = html.LoadUrl(url);
                    if (success) parsed = html;
                    else errors.Add("Unable to parse HTML from supplied URL");
                    break;

                case "json":
                    success = json.LoadUrl(url);
                    if (success) parsed = json;
                    else errors.Add("Unable to parse JSON from supplied URL");
                    break;

                case "xml":
                    success = xml.LoadUrl(url);
                    if (success) parsed = xml;
                    else errors.Add("Unable to parse XML from supplied URL");
                    break;
            }

            return success;

            #endregion
        }

        public static bool FromHtmlString(string html, out ParsedHtml parsed, out List<string> errors)
        {
            errors = new List<string>();
            parsed = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(html))
            {
                errors.Add("No HTML provided");
                return false;
            }

            #endregion

            #region Process

            parsed = new ParsedHtml();
            bool success = parsed.LoadString(html, "undefined");
            if (success) return true;
            errors.Add("Unable to parse supplied HTML");
            return false;

            #endregion
        }

        public static bool FromHtmlFile(string filename, out ParsedHtml parsed, out List<string> errors)
        {
            errors = new List<string>();
            parsed = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(filename))
            {
                errors.Add("No filename provided");
                return false;
            }

            #endregion

            #region Read-File

            if (!File.Exists(filename))
            {
                errors.Add("File does not exist");
                return false;
            }

            if (!Common.VerifyFileReadAccess(filename))
            {
                errors.Add("Unable to read file");
                return false;
            }

            string html = Common.ReadTextFile(filename);
            if (String.IsNullOrEmpty(html))
            {
                errors.Add("No data in file");
                return false;
            }

            #endregion

            #region Process

            parsed = new ParsedHtml();
            bool success = parsed.LoadString(html, "undefined");
            if (success) return true;
            errors.Add("Unable to parse HTML from file");
            return false;

            #endregion
        }

        public static bool FromJsonString(string json, out ParsedJson parsed, out List<string> errors)
        {
            errors = new List<string>();
            parsed = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(json))
            {
                errors.Add("No JSON provided");
                return false;
            }

            #endregion

            #region Process

            parsed = new ParsedJson();
            bool success = parsed.LoadString(json, "undefined");
            if (success) return true;
            errors.Add("Unable to parse supplied JSON");
            return false;

            #endregion
        }

        public static bool FromJsonFile(string filename, out ParsedJson parsed, out List<string> errors)
        {
            errors = new List<string>();
            parsed = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(filename))
            {
                errors.Add("No filename provided");
                return false;
            }

            #endregion

            #region Read-File

            if (!File.Exists(filename))
            {
                errors.Add("File does not exist");
                return false;
            }

            if (!Common.VerifyFileReadAccess(filename))
            {
                errors.Add("Unable to read file");
                return false;
            }

            string json = Common.ReadTextFile(filename);
            if (String.IsNullOrEmpty(json))
            {
                errors.Add("No data in file");
                return false;
            }

            #endregion

            #region Process

            parsed = new ParsedJson();
            bool success = parsed.LoadString(json, "undefined");
            if (success) return true;
            errors.Add("Unable to parse JSON from file");
            return false;

            #endregion
        }

        public static bool FromXmlString(string xml, out ParsedXml parsed, out List<string> errors)
        {
            errors = new List<string>();
            parsed = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(xml))
            {
                errors.Add("No XML provided");
                return false;
            }

            #endregion

            #region Process

            parsed = new ParsedXml();
            bool success = parsed.LoadString(xml, "undefined");
            if (success) return true;
            errors.Add("Unable to parse supplied XML");
            return false;

            #endregion
        }

        public static bool FromXmlFile(string filename, out ParsedXml parsed, out List<string> errors)
        {
            errors = new List<string>();
            parsed = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(filename))
            {
                errors.Add("No filename provided");
                return false;
            }

            #endregion

            #region Read-File

            if (!File.Exists(filename))
            {
                errors.Add("File does not exist");
                return false;
            }

            if (!Common.VerifyFileReadAccess(filename))
            {
                errors.Add("Unable to read file");
                return false;
            }

            string xml = Common.ReadTextFile(filename);
            if (String.IsNullOrEmpty(xml))
            {
                errors.Add("No data in file");
                return false;
            }

            #endregion

            #region Process

            parsed = new ParsedXml();
            bool success = parsed.LoadString(xml, "undefined");
            if (success) return true;
            errors.Add("Unable to parse XML from file");
            return false;

            #endregion
        }

        public static bool FromSqlQuery(
            string dbType,
            string dbServer,
            int dbPort,
            string dbUser,
            string dbPass,
            string dbName,
            string dbInstance,
            string query,
            out ParsedSql parsed,
            out List<string> errors)
        {
            errors = new List<string>();
            parsed = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(dbType))
            {
                errors.Add("No database type provided");
                return false;
            }

            if (!dbType.ToLower().Equals("mssql") && !dbType.ToLower().Equals("mysql"))
            {
                errors.Add("Invalid database type provided");
                return false;
            }

            if (String.IsNullOrEmpty(dbServer))
            {
                errors.Add("No database server name provided");
                return false;
            }

            if (dbPort <= 0)
            {
                errors.Add("Invalid database server port provided");
                return false;
            }

            if (String.IsNullOrEmpty(dbUser) || String.IsNullOrEmpty(dbPass))
            {
                errors.Add("Incomplete database authentication material provided");
                return false;
            }

            if (String.IsNullOrEmpty(query))
            {
                errors.Add("No query provided");
                return false;
            }

            #endregion

            #region Process

            parsed = new ParsedSql();
            bool success = parsed.LoadDatabase(dbType, dbServer, dbPort, dbUser, dbPass, dbInstance, dbName, query);
            if (success) return true;
            errors.Add("Unable to parse SQL from query");
            return false;

            #endregion
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}