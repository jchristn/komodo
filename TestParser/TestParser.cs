using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Komodo.Core;

namespace KomodoTestParser
{
    class TestParser
    {
        static void Main(string[] args)
        {
            bool runForever = true;
            string url = "";
            string filename = "";
            string data = "";

            string dbType = "mysql";
            string serverHostname = "localhost";
            int serverPort = 3306;
            string user = "root";
            string pass = "password";
            string instance = "";
            string databaseName = "mydatabase";
            string query = "SELECT * FROM mytable;";

            ParsedHtml parsedHtml = new ParsedHtml();
            ParsedJson parsedJson = new ParsedJson();
            ParsedXml parsedXml = new ParsedXml();
            ParsedSql parsedSql = new ParsedSql();
            ParsedText parsedText = new ParsedText();
            IndexOptions options = new IndexOptions();
            IndexedDoc idx = new IndexedDoc();

            Welcome();

            while (runForever)
            {
                Console.Write("Komodo [? for help]: ");
                string userInput = Console.ReadLine();
                if (String.IsNullOrEmpty(userInput)) continue;

                switch (userInput.ToLower())
                {
                    case "?":
                        Menu();
                        break;

                    case "q":
                        runForever = false;
                        break;

                    #region Parsers

                    #region HTML

                    case "parse_html_from_file":
                        filename = InputString("Filename", null, false);
                        parsedHtml = new ParsedHtml();
                        if (!parsedHtml.LoadFile(filename))
                        {
                            Console.WriteLine("Failed to parse: " + filename);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + filename);
                            Console.WriteLine(parsedHtml.ToString());
                        }
                        break;

                    case "parse_html_from_string":
                        data = InputString("Data", null, false);
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedHtml = new ParsedHtml();
                        if (!parsedHtml.LoadString(data, url))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedHtml.ToString());
                        }
                        break;

                    case "parse_html_from_url":
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedHtml = new ParsedHtml();
                        if (!parsedHtml.LoadUrl(url))
                        {
                            Console.WriteLine("Failed to parse: " + url);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + url);
                            Console.WriteLine(parsedHtml.ToString());
                        }
                        break;

                    #endregion

                    #region JSON

                    case "parse_json_from_file":
                        filename = InputString("Filename", null, false);
                        parsedJson = new ParsedJson();
                        if (!parsedJson.LoadFile(filename))
                        {
                            Console.WriteLine("Failed to parse: " + filename);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + filename);
                            Console.WriteLine(parsedJson.ToString());
                        }
                        break;

                    case "parse_json_from_string":
                        data = InputString("Data", null, false);
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedJson = new ParsedJson();
                        if (!parsedJson.LoadString(data, url))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedJson.ToString());
                        }
                        break;

                    case "parse_json_from_url":
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedJson = new ParsedJson();
                        if (!parsedJson.LoadUrl(url))
                        {
                            Console.WriteLine("Failed to parse: " + url);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + url);
                            Console.WriteLine(parsedJson.ToString());
                        }
                        break;

                    #endregion

                    #region XML

                    case "parse_xml_from_file":
                        filename = InputString("Filename", null, false);
                        parsedXml = new ParsedXml();
                        if (!parsedXml.LoadFile(filename))
                        {
                            Console.WriteLine("Failed to parse: " + filename);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + filename);
                            Console.WriteLine(parsedXml.ToString());
                        }
                        break;

                    case "parse_xml_from_string":
                        data = InputString("Data", null, false);
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedXml = new ParsedXml();
                        if (!parsedXml.LoadString(data, url))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedXml.ToString());
                        }
                        break;

                    case "parse_xml_from_url":
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedXml = new ParsedXml();
                        if (!parsedXml.LoadUrl(url))
                        {
                            Console.WriteLine("Failed to parse: " + url);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + url);
                            Console.WriteLine(parsedXml.ToString());
                        }
                        break;

                    #endregion

                    #region Text

                    case "parse_text_from_file":
                        filename = InputString("Filename", null, false);
                        parsedText = new ParsedText();
                        if (!parsedText.LoadFile(filename))
                        {
                            Console.WriteLine("Failed to parse: " + filename);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + filename);
                            Console.WriteLine(parsedText.ToString());
                        }
                        break;

                    case "parse_text_from_string":
                        data = InputString("Data", null, false);
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedText = new ParsedText();
                        if (!parsedText.LoadString(data, url))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedText.ToString());
                        }
                        break;

                    case "parse_text_from_url":
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedText = new ParsedText();
                        if (!parsedText.LoadUrl(url))
                        {
                            Console.WriteLine("Failed to parse: " + url);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + url);
                            Console.WriteLine(parsedText.ToString());
                        }
                        break;

                    #endregion

                    #region SQL

                    case "parse_sql_from_query":
                        dbType = InputString("Database Type", dbType, false);
                        serverHostname = InputString("Hostname", serverHostname, false);
                        serverPort = InputInteger("Port", serverPort, true, false);
                        user = InputString("User", user, false);
                        pass = InputString("Password", pass, false);
                        instance = InputString("Instance", instance, true);
                        databaseName = InputString("Database Name", databaseName, false);
                        query = InputString("Query", query, false);

                        parsedSql = new ParsedSql();
                        if (!parsedSql.LoadDatabase(dbType, serverHostname, serverPort, user, pass, instance, databaseName, query))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedSql.ToString());
                        }
                        break;

                    #endregion

                    #endregion

                    #region Index-Builders

                    #region HTML

                    case "idx_html_from_file":
                        filename = InputString("Filename", null, false);
                        parsedHtml = new ParsedHtml();
                        if (!parsedHtml.LoadFile(filename))
                        {
                            Console.WriteLine("Failed to parse: " + filename);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + filename);
                            Console.WriteLine(parsedHtml.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromHtml(parsedHtml, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    case "idx_html_from_string":
                        data = InputString("Data", null, false);
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedHtml = new ParsedHtml();
                        if (!parsedHtml.LoadString(data, url))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedHtml.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromHtml(parsedHtml, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    case "idx_html_from_url":
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedHtml = new ParsedHtml();
                        if (!parsedHtml.LoadUrl(url))
                        {
                            Console.WriteLine("Failed to parse: " + url);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + url);
                            Console.WriteLine(parsedHtml.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromHtml(parsedHtml, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    #endregion

                    #region JSON

                    case "idx_json_from_file":
                        filename = InputString("Filename", null, false);
                        parsedJson = new ParsedJson();
                        if (!parsedJson.LoadFile(filename))
                        {
                            Console.WriteLine("Failed to parse: " + filename);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + filename);
                            Console.WriteLine(parsedJson.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromJson(parsedJson, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    case "idx_json_from_string":
                        data = InputString("Data", null, false);
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedJson = new ParsedJson();
                        if (!parsedJson.LoadString(data, url))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedJson.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromJson(parsedJson, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    case "idx_json_from_url":
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedJson = new ParsedJson();
                        if (!parsedJson.LoadUrl(url))
                        {
                            Console.WriteLine("Failed to parse: " + url);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + url);
                            Console.WriteLine(parsedJson.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromJson(parsedJson, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    #endregion

                    #region XML

                    case "idx_xml_from_file":
                        filename = InputString("Filename", null, false);
                        parsedXml = new ParsedXml();
                        if (!parsedXml.LoadFile(filename))
                        {
                            Console.WriteLine("Failed to parse: " + filename);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + filename);
                            Console.WriteLine(parsedXml.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromXml(parsedXml, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    case "idx_xml_from_string":
                        data = InputString("Data", null, false);
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedXml = new ParsedXml();
                        if (!parsedXml.LoadString(data, url))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedXml.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromXml(parsedXml, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    case "idx_xml_from_url":
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedXml = new ParsedXml();
                        if (!parsedXml.LoadUrl(url))
                        {
                            Console.WriteLine("Failed to parse: " + url);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + url);
                            Console.WriteLine(parsedXml.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromXml(parsedXml, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    #endregion

                    #region Text

                    case "idx_text_from_file":
                        filename = InputString("Filename", null, false);
                        parsedText = new ParsedText();
                        if (!parsedText.LoadFile(filename))
                        {
                            Console.WriteLine("Failed to parse: " + filename);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + filename);
                            Console.WriteLine(parsedText.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromText(parsedText, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    case "idx_text_from_string":
                        data = InputString("Data", null, false);
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedText = new ParsedText();
                        if (!parsedText.LoadString(data, url))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedText.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromText(parsedText, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    case "idx_text_from_url":
                        url = InputString("URL", "http://www.cnn.com", false);
                        parsedText = new ParsedText();
                        if (!parsedText.LoadUrl(url))
                        {
                            Console.WriteLine("Failed to parse: " + url);
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: " + url);
                            Console.WriteLine(parsedText.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromText(parsedText, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    #endregion

                    #region SQL

                    case "idx_sql_from_query":
                        dbType = InputString("Database Type", dbType, false);
                        serverHostname = InputString("Hostname", serverHostname, false);
                        serverPort = InputInteger("Port", serverPort, true, false);
                        user = InputString("User", user, false);
                        pass = InputString("Password", pass, false);
                        instance = InputString("Instance", instance, true);
                        databaseName = InputString("Database Name", databaseName, false);
                        query = InputString("Query", query, false);

                        parsedSql = new ParsedSql();
                        if (!parsedSql.LoadDatabase(dbType, serverHostname, serverPort, user, pass, instance, databaseName, query))
                        {
                            Console.WriteLine("Failed to parse input data");
                        }
                        else
                        {
                            Console.WriteLine("Successful parse: ");
                            Console.WriteLine(parsedSql.ToString());
                            Console.WriteLine("Indexing");
                            idx = IndexedDoc.FromSql(parsedSql, options);
                            if (idx != null)
                            {
                                Console.WriteLine("Successful index");
                                Console.WriteLine(idx.ToString());
                            }
                        }
                        break;

                    #endregion

                    #endregion

                    default:
                        break;
                }
            }
        }

        static void Menu()
        {
            Console.WriteLine("--- Komodo Test Application ---");
            Console.WriteLine("Parsers");
            Console.WriteLine("  parse_html_from_file   parse_html_from_string   parse_html_from_url");
            Console.WriteLine("  parse_json_from_file   parse_json_from_string   parse_json_from_url");
            Console.WriteLine("  parse_xml_from_file    parse_xml_from_string    parse_xml_from_url");
            Console.WriteLine("  parse_text_from_file   parse_text_from_string   parse_text_from_url");
            Console.WriteLine("  parse_sql_from_query");
            Console.WriteLine("");
            Console.WriteLine("Index Builders");
            Console.WriteLine("  idx_html_from_file     idx_html_from_string     idx_html_from_url");
            Console.WriteLine("  idx_json_from_file     idx_json_from_string     idx_json_from_url");
            Console.WriteLine("  idx_xml_from_file      idx_xml_from_string      idx_xml_from_url");
            Console.WriteLine("  idx_text_from_file     idx_text_from_string     idx_text_from_url");
            Console.WriteLine("  idx_sql_from_query");
            Console.WriteLine("---");
        }

        static void Welcome()
        {
            string ret =
                Environment.NewLine +
                Environment.NewLine +
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░" + Environment.NewLine +
                "░░░█░█░█▀▀█░█▀▄▀█░█▀▀█░█▀▀▄░█▀▀█░░░" + Environment.NewLine +
                "░░░█▀▄░█░░█░█░▀░█░█░░█░█░░█░█░░█░░░" + Environment.NewLine +
                "░░░▀░▀░▀▀▀▀░▀░░░▀░▀▀▀▀░▀▀▀░░▀▀▀▀░░░" + Environment.NewLine +
                "░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░" + Environment.NewLine +
                Environment.NewLine;

            Console.WriteLine(ret);
        }

        static string InputString(string question, string defaultAnswer, bool allowNull)
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(": ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                    if (allowNull) return null;
                    else continue;
                }

                return userInput;
            }
        }

        static int InputInteger(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
        {
            while (true)
            {
                Console.Write(question);
                Console.Write(" [" + defaultAnswer + "]: ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    return defaultAnswer;
                }

                int ret = 0;
                if (!Int32.TryParse(userInput, out ret))
                {
                    Console.WriteLine("Please enter a valid integer.");
                    continue;
                }

                if (ret == 0)
                {
                    if (allowZero)
                    {
                        return 0;
                    }
                }

                if (ret < 0)
                {
                    if (positiveOnly)
                    {
                        Console.WriteLine("Please enter a value greater than zero.");
                        continue;
                    }
                }

                return ret;
            }
        }
    }
}
