using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KomodoCore;
using RestWrapper;

namespace KomodoCli
{
    class KomodoCli
    {
        static string ContentType = null;
        static string InFile = null;
        static string OutFile = null;

        static string InContent = null;
        static string OutContent = null;

        static Crawler Crawler = null;

        static void Main(string[] args)
        {
            #region Process-Arguments
            
            if (args != null && args.Length > 0)
            {
                foreach (string currArg in args)
                {
                    if (currArg.StartsWith("-type="))
                    {
                        ContentType = currArg.Substring(6);
                    }
                    
                    if (currArg.StartsWith("-infile="))
                    {
                        InFile = currArg.Substring(8);
                    }

                    if (currArg.StartsWith("-outfile="))
                    {
                        OutFile = currArg.Substring(9);
                    }
                }
            }
            else
            {
                Usage();
                return;
            }

            #endregion

            #region Validate-Arguments

            if (String.IsNullOrEmpty(InFile))
            {
                Console.WriteLine("Either the input URL or input file must be specified.");
                Usage();
                return;
            }
            
            if (String.IsNullOrEmpty(ContentType))
            {
                Console.WriteLine("Content type must be specified.");
                Usage();
                return;
            }

            if (!ContentType.Equals("json")
                && !ContentType.Equals("html")
                && !ContentType.Equals("xml")
                && !ContentType.Equals("text"))
            {
                Console.WriteLine("Invalid content type.");
                Usage();
                return;
            }

            #endregion

            #region Load-Content
            
            Crawler = new Crawler(InFile, ContentType);
            InContent = Crawler.RetrieveString();
            if (String.IsNullOrEmpty(InContent))
            {
                Console.WriteLine("No data retrieved.");
                return;
            }

            #endregion

            #region Parse-Content

            switch (ContentType)
            {
                case "html":
                    ParsedHtml html = new ParsedHtml();
                    html.LoadString(InContent, InFile);
                    OutContent = Common.SerializeJson(html, true);
                    break;

                case "json":
                    ParsedJson json = new ParsedJson();
                    json.LoadString(InContent, InFile);
                    OutContent = Common.SerializeJson(json, true);
                    break;

                case "xml":
                    ParsedXml xml = new ParsedXml();
                    xml.LoadString(InContent, InFile);
                    OutContent = Common.SerializeJson(xml, true);
                    break;

                case "text":
                    ParsedText text = new ParsedText();
                    text.LoadString(InContent, InFile);
                    OutContent = Common.SerializeJson(text, true);
                    break;

                default:
                    Console.WriteLine("Invalid content type.");
                    Usage();
                    return;
            }

            #endregion

            #region Write-Output

            if (String.IsNullOrEmpty(OutContent))
            {
                Console.WriteLine("No content returned from parsing.");
                return;
            }

            File.WriteAllBytes(OutFile, Encoding.UTF8.GetBytes(OutContent));
            Console.WriteLine("Success.");
            return;

            #endregion
        }

        static void Welcome()
        {
            string ret =
                Environment.NewLine +
                Environment.NewLine +
                "oooo                                                    .o8            " + Environment.NewLine +
                "`888                                                    888            " + Environment.NewLine +
                " 888  oooo   .ooooo.  ooo. .oo.  .oo.    .ooooo.   .oooo888   .ooooo.  " + Environment.NewLine +
                " 888 .8P'   d88' `88b `888P'Y88bP'Y88b  d88' `88b d88' `888  d88' `88b " + Environment.NewLine +
                " 888888.    888   888  888   888   888  888   888 888   888  888   888 " + Environment.NewLine +
                " 888 `88b.  888   888  888   888   888  888   888 888   888  888   888 " + Environment.NewLine +
                "o888o o888o `Y8bod8P' o888o o888o o888o `Y8bod8P' `Y8bod88P  `Y8bod8P' " + Environment.NewLine +
                Environment.NewLine;

            Console.WriteLine(ret);
        }

        static void Usage()
        {
            // Console.WriteLine("34567890123456789012345678901234567890123456789012345678901234567890123456789");
            Console.WriteLine("");
            Console.WriteLine("KomodoCli - crawl and retrieve, then flatten and parse");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("  C:\\> KomodoCli [arguments]");
            Console.WriteLine("");
            Console.WriteLine("Where [arguments] includes:");
            Console.WriteLine("  -type=[type]     Specify the incoming data type");
            Console.WriteLine("                   Valid values: json xml html text");
            Console.WriteLine("  -infile=[file]   Specify the URL or file where data can be retrieved");
            Console.WriteLine("  -outfile=[file]  Specify the file where results should be written");
            Console.WriteLine("");
        }
    }
}
