﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    internal static class DocIndexHandler
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion
        
        #region Internal-Methods

        internal static bool FromUrl(Settings currConfig, string url, string docType, out IndexedDoc idx, out RestResponse resp, out List<string> errors)
        {
            errors = new List<string>();
            idx = null;
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

            RestRequest req = new RestRequest(
                url,
                RestWrapper.HttpMethod.GET,
                null,
                "text/plain");

            req.IgnoreCertificateErrors = currConfig.Rest.AcceptInvalidCerts;

            resp = req.Send(); 
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

            ParsedHtml parsedHtml;
            ParsedJson parsedJson;
            ParsedXml parsedXml;
            
            switch (docType.ToLower())
            {
                case "html":
                    if (!DocParseHandler.FromHtmlString(Encoding.UTF8.GetString(Common.StreamToBytes(resp.Data)), out parsedHtml, out errors))
                    {
                        errors.Add("Unable to parse HTML from server.");
                        return false;
                    }
                    idx = IndexedDoc.FromHtml(parsedHtml, new IndexOptions());
                    return true;

                case "json":
                    if (!DocParseHandler.FromJsonString(Encoding.UTF8.GetString(Common.StreamToBytes(resp.Data)), out parsedJson, out errors))
                    {
                        errors.Add("Unable to parse JSON from server.");
                        return false;
                    }
                    idx = IndexedDoc.FromJson(parsedJson, new IndexOptions());
                    return true;

                case "xml":
                    if (!DocParseHandler.FromXmlString(Encoding.UTF8.GetString(Common.StreamToBytes(resp.Data)), out parsedXml, out errors))
                    {
                        errors.Add("Unable to parse XML from server.");
                        return false;
                    }
                    idx = IndexedDoc.FromXml(parsedXml, new IndexOptions());
                    return true;

                default:
                    errors.Add("Unable to process requested document type from URL.");
                    return false;
            }

            #endregion
        }

        internal static bool FromHtmlString(string data, out IndexedDoc idx, out List<string> errors)
        {
            errors = new List<string>();
            idx = null;

            #region Check-for-Null-Values
            
            if (String.IsNullOrEmpty(data))
            {
                errors.Add("No data supplied.");
                return false;
            }
            
            #endregion
            
            #region Process

            ParsedHtml parsedHtml;
            if (!DocParseHandler.FromHtmlString(data, out parsedHtml, out errors))
            {
                errors.Add("Unable to parse supplied HTML.");
                return false;
            }
            idx = IndexedDoc.FromHtml(parsedHtml, new IndexOptions());
            if (idx != null)
            {
                return true;
            }
            else
            {
                errors.Add("Unable to create document index.");
                return false;
            }

            #endregion
        }

        internal static bool FromHtmlFile(string filename, out IndexedDoc idx, out List<string> errors)
        {
            errors = new List<string>();
            idx = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(filename))
            {
                errors.Add("No filename supplied.");
                return false;
            }

            #endregion

            #region Process

            ParsedHtml parsedHtml;
            if (!DocParseHandler.FromHtmlFile(filename, out parsedHtml, out errors))
            {
                errors.Add("Unable to parse HTML from file.");
                return false;
            }
            idx = IndexedDoc.FromHtml(parsedHtml, new IndexOptions());
            if (idx != null)
            {
                return true;
            }
            else
            {
                errors.Add("Unable to create document index.");
                return false;
            }

            #endregion
        }

        internal static bool FromJsonString(string data, out IndexedDoc idx, out List<string> errors)
        {
            errors = new List<string>();
            idx = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(data))
            {
                errors.Add("No data supplied.");
                return false;
            }

            #endregion

            #region Process

            ParsedJson parsedJson;
            if (!DocParseHandler.FromJsonString(data, out parsedJson, out errors))
            {
                errors.Add("Unable to parse supplied JSON.");
                return false;
            }
            idx = IndexedDoc.FromJson(parsedJson, new IndexOptions());
            if (idx != null)
            {
                return true;
            }
            else
            {
                errors.Add("Unable to create document index.");
                return false;
            }

            #endregion
        }

        internal static bool FromJsonFile(string filename, out IndexedDoc idx, out List<string> errors)
        {
            errors = new List<string>();
            idx = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(filename))
            {
                errors.Add("No filename supplied.");
                return false;
            }

            #endregion

            #region Process

            ParsedJson parsedJson;
            if (!DocParseHandler.FromJsonFile(filename, out parsedJson, out errors))
            {
                errors.Add("Unable to parse JSON from file.");
                return false;
            }
            idx = IndexedDoc.FromJson(parsedJson, new IndexOptions());
            if (idx != null)
            {
                return true;
            }
            else
            {
                errors.Add("Unable to create document index.");
                return false;
            }

            #endregion
        }

        internal static bool FromXmlString(string data, out IndexedDoc idx, out List<string> errors)
        {
            errors = new List<string>();
            idx = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(data)) 
            {
                errors.Add("No data supplied.");
                return false;
            }

            #endregion

            #region Process

            ParsedXml parsedXml;
            if (!DocParseHandler.FromXmlString(data, out parsedXml, out errors))
            {
                errors.Add("Unable to parse supplied XML.");
                return false;
            }
            idx = IndexedDoc.FromXml(parsedXml, new IndexOptions());
            if (idx != null)
            {
                return true;
            }
            else
            {
                errors.Add("Unable to create document index.");
                return false;
            }

            #endregion
        }

        internal static bool FromXmlFile(string filename, out IndexedDoc idx, out List<string> errors)
        {
            errors = new List<string>();
            idx = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(filename))
            {
                errors.Add("No filename supplied.");
                return false;
            }

            #endregion

            #region Process

            ParsedXml parsedXml;
            if (!DocParseHandler.FromXmlFile(filename, out parsedXml, out errors))
            {
                errors.Add("Unable to parse XML from file.");
                return false;
            }
            idx = IndexedDoc.FromXml(parsedXml, new IndexOptions());
            if (idx != null)
            {
                return true;
            }
            else
            {
                errors.Add("Unable to create document index.");
                return false;
            }

            #endregion
        }

        internal static bool FromSqlQuery(
            string dbtype, string dbserver, int dbport, string dbuser, string dbpass, string dbinstance, string dbname, string dbquery, 
            out IndexedDoc idx, out List<string> errors)
        {
            errors = new List<string>();
            idx = null;

            #region Check-for-Null-Values

            if (String.IsNullOrEmpty(dbtype)
                || String.IsNullOrEmpty(dbserver) 
                || String.IsNullOrEmpty(dbuser) 
                || String.IsNullOrEmpty(dbpass) 
                || string.IsNullOrEmpty(dbname) 
                || String.IsNullOrEmpty(dbquery))
            {
                errors.Add("Request must include database type, server, port, user, password, database name, and query.");
                return false;
            }

            #endregion

            #region Process

            ParsedSql parsedSql;
            if (!DocParseHandler.FromSqlQuery(dbtype, dbserver, dbport, dbuser, dbpass, dbname, dbinstance, dbquery, out parsedSql, out errors))
            {
                errors.Add("Unable to parse SQL from query.");
                return false;
            }
            idx = IndexedDoc.FromSql(parsedSql, new IndexOptions());
            if (idx != null)
            {
                return true;
            }
            else
            {
                errors.Add("Unable to create document index.");
                return false;
            }

            #endregion
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}