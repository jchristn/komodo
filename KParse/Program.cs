using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Komodo.Core;
using Komodo.Core.Enums;
using RestWrapper;

namespace KParse
{
    class Program
    {
        static DocType _ContentType = DocType.Unknown;
        static string _InFile = null;
        static string _OutFile = null;

        static string _InContent = null;
        static string _OutContent = null;

        static Crawler _Crawler = null;

        static void Main(string[] args)
        {
            #region Process-Arguments
            
            if (args != null && args.Length > 0)
            {
                foreach (string currArg in args)
                {
                    if (currArg.StartsWith("-type="))
                    {
                        _ContentType = (DocType)(Enum.Parse(typeof(DocType), currArg.Substring(6)));
                    }
                    
                    if (currArg.StartsWith("-infile="))
                    {
                        _InFile = currArg.Substring(8);
                    }

                    if (currArg.StartsWith("-outfile="))
                    {
                        _OutFile = currArg.Substring(9);
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

            if (String.IsNullOrEmpty(_InFile))
            {
                Console.WriteLine("Either the input URL or input file must be specified.");
                Usage();
                return;
            }
             
            if (_ContentType != DocType.Json
                && _ContentType != DocType.Html
                && _ContentType != DocType.Xml
                && _ContentType != DocType.Text)
            {
                Console.WriteLine("Invalid content type.");
                Usage();
                return;
            }

            #endregion

            #region Load-Content

            _Crawler = new Crawler(_InFile, _ContentType);
            _InContent = Encoding.UTF8.GetString(_Crawler.RetrieveBytes());
            if (String.IsNullOrEmpty(_InContent))
            {
                Console.WriteLine("No data retrieved.");
                return;
            }

            #endregion

            #region Parse-Content

            switch (_ContentType)
            {
                case DocType.Html:
                    ParsedHtml html = new ParsedHtml();
                    html.LoadString(_InContent, _InFile);
                    _OutContent = SerializeJson(html, true);
                    break;

                case DocType.Json:
                    ParsedJson json = new ParsedJson();
                    json.LoadString(_InContent, _InFile);
                    _OutContent = SerializeJson(json, true);
                    break;

                case DocType.Xml:
                    ParsedXml xml = new ParsedXml();
                    xml.LoadString(_InContent, _InFile);
                    _OutContent = SerializeJson(xml, true);
                    break;

                case DocType.Text:
                    ParsedText text = new ParsedText();
                    text.LoadString(_InContent, _InFile);
                    _OutContent = SerializeJson(text, true);
                    break;

                default:
                    Console.WriteLine("Invalid content type.");
                    Usage();
                    return;
            }

            #endregion

            #region Write-Output

            if (String.IsNullOrEmpty(_OutContent))
            {
                Console.WriteLine("No content returned from parsing.");
                return;
            }

            if (!String.IsNullOrEmpty(_OutFile))
            {
                File.WriteAllBytes(_OutFile, Encoding.UTF8.GetBytes(_OutContent));
            }
            else
            {
                Console.WriteLine(_OutContent);
            }

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
            Console.WriteLine("KParse - crawl and retrieve, then flatten and parse");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("  C:\\> KParse [arguments]");
            Console.WriteLine("");
            Console.WriteLine("Where [arguments] includes:");
            Console.WriteLine("  -type=[type]     Specify the incoming data type");
            Console.WriteLine("                   Valid values: Json Xml Html Text");
            Console.WriteLine("  -infile=[file]   Specify the URL or file where data can be retrieved");
            Console.WriteLine("  -outfile=[file]  Specify the file where results should be written");
            Console.WriteLine("                   If outfile is not specified, output is sent to console");
            Console.WriteLine("");
        }

        static string SerializeJson(object obj, bool pretty)
        {
            if (obj == null) return null;
            string json;

            if (pretty)
            {
                json = JsonConvert.SerializeObject(
                  obj,
                  Newtonsoft.Json.Formatting.Indented,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                  });
            }
            else
            {
                json = JsonConvert.SerializeObject(obj,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc
                  });
            }

            return json;
        }
    }
}
