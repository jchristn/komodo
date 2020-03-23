using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Commonly-used methods in Komodo and supporting applications.
    /// </summary>
    public static class Common
    { 
        #region Input

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool InputBoolean(string question, bool yesDefault)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Console.Write(question);

            if (yesDefault) Console.Write(" [Y/n]? ");
            else Console.Write(" [y/N]? ");

            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput))
            {
                if (yesDefault) return true;
                return false;
            }

            userInput = userInput.ToLower();

            if (yesDefault)
            {
                if (
                    (String.Compare(userInput, "n") == 0)
                    || (String.Compare(userInput, "no") == 0)
                   )
                {
                    return false;
                }

                return true;
            }
            else
            {
                if (
                    (String.Compare(userInput, "y") == 0)
                    || (String.Compare(userInput, "yes") == 0)
                   )
                {
                    return true;
                }

                return false;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string InputString(string question, string defaultAnswer, bool allowNull)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            while (true)
            {
                Console.Write(question);

                if (!String.IsNullOrEmpty(defaultAnswer))
                {
                    Console.Write(" [" + defaultAnswer + "]");
                }

                Console.Write(" ");

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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static int InputInteger(string question, int defaultAnswer, bool positiveOnly, bool allowZero)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            while (true)
            {
                Console.Write(question);
                Console.Write(" [" + defaultAnswer + "] ");

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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<string> InputStringList(string question, bool allowEmpty)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            List<string> ret = new List<string>();

            while (true)
            {
                Console.Write(question);

                Console.Write(" ");

                string userInput = Console.ReadLine();

                if (String.IsNullOrEmpty(userInput))
                {
                    if (ret.Count < 1 && !allowEmpty) continue;
                    return ret;
                }

                ret.Add(userInput);
            }
        }

        #endregion

        #region Serialization

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string SerializeJson(object obj, bool pretty)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static T DeserializeJson<T>(string json)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine("Exception while deserializing:");
                Console.WriteLine(json);
                Console.WriteLine("");
                throw e;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static T DeserializeJson<T>(byte[] data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
            return DeserializeJson<T>(Encoding.UTF8.GetString(data));
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static T CopyObject<T>(object o)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (o == null) return default(T);
            string json = SerializeJson(o, false);
            T ret = DeserializeJson<T>(json);
            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string XmlEscape(string val)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(val)) return null;
            XmlDocument doc = new XmlDocument();
            var node = doc.CreateElement("root");
            node.InnerText = val;
            return node.InnerXml;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string SanitizeXml(string xml)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(xml)) return null;

            string ret = "";
            XmlDocument doc = new XmlDocument();
            using (StringReader sr = new StringReader(xml))
            {
                using (XmlTextReader xtr = new XmlTextReader(sr) { Namespaces = false })
                {
                    doc.LoadXml(xml);
                }
            }

            if (doc == null) return null;

            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter xtw = XmlWriter.Create(sw))
                {
                    doc.WriteTo(xtw);
                    xtw.Flush();

                    ret = sw.GetStringBuilder().ToString();
                }
            }

            if (String.IsNullOrEmpty(ret)) return null;

            // remove all namespaces
            XElement xe = XmlRemoveNamespace(XElement.Parse(xml));

            // remove null fields from string
            Regex rgx = new Regex("\\n*\\s*<([\\w_]+)></([\\w_]+)>\\n*");

            return rgx.Replace(xe.ToString(), "");
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string QueryXml(string xml, string path)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (String.IsNullOrEmpty(xml)) return null;
                if (String.IsNullOrEmpty(path)) return null;

                string sanitized = SanitizeXml(xml);
                StringReader sr = new StringReader(sanitized);
                XPathDocument xpd = new XPathDocument(sr);
                XPathNavigator xpn = xpd.CreateNavigator();
                XPathNodeIterator xni = xpn.Select(path);
                string response = null;

                while (xni.MoveNext())
                {
                    if (xni.Current.SelectSingleNode("*") != null)
                    {
                        response = QueryXmlProcessChildren(xni);
                    }
                    else
                    {
                        response = xni.Current.Value;
                    }
                }

                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static XElement XmlRemoveNamespace(XElement xml)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                xml.RemoveAttributes();
                if (!xml.HasElements)
                {
                    XElement xe = new XElement(xml.Name.LocalName);
                    xe.Value = xml.Value;

                    foreach (XAttribute attribute in xml.Attributes())
                        xe.Add(attribute);

                    return xe;
                }
                return new XElement(xml.Name.LocalName, xml.Elements().Select(el => XmlRemoveNamespace(el)));
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string QueryXmlProcessChildren(XPathNodeIterator xpni)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                XPathNodeIterator child = xpni.Current.SelectChildren(XPathNodeType.All);

                while (child.MoveNext())
                {
                    if (child.Current.SelectSingleNode("*") != null) QueryXmlProcessChildren(child);
                }

                return child.Current.Value;
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region Conversion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<T> GenericToSpecificList<T>(List<object> source)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (source == null) return null;

            List<T> retList = new List<T>();
            int count = 0;

            foreach (object curr in source)
            {
                retList.Add((T)curr);
                count++;
            }

            return retList;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<object> DataTableToListObject(DataTable dt, string objType)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            //
            // Must pass in the fully-qualified class name including namespace
            //
            // i.e. Namespace.ClassName
            //

            if (dt == null) return null;
            if (dt.Rows.Count < 1) return null;
            if (String.IsNullOrEmpty(objType)) return null;

            List<object> retList = new List<object>();
            int count = 0;

            foreach (DataRow currRow in dt.Rows)
            {
                object ret = Activator.CreateInstance(Type.GetType(objType));
                if (ret == null) return null;

                foreach (PropertyInfo prop in ret.GetType().GetProperties())
                {
                    #region Process-Each-Property

                    PropertyInfo tempProp = prop;

                    switch (prop.PropertyType.ToString().ToLower().Trim())
                    {
                        case "system.string":
                            string valStr = currRow[prop.Name].ToString().Trim();
                            tempProp.SetValue(ret, valStr, null);
                            break;

                        case "system.int32":
                        case "system.nullable`1[system.int32]":
                            int valInt32 = 0;
                            if (Int32.TryParse(currRow[prop.Name].ToString(), out valInt32)) tempProp.SetValue(ret, valInt32, null);
                            break;

                        case "system.int64":
                        case "system.nullable`1[system.int64]":
                            long valInt64 = 0;
                            if (Int64.TryParse(currRow[prop.Name].ToString(), out valInt64)) tempProp.SetValue(ret, valInt64, null);
                            break;

                        case "system.decimal":
                        case "system.nullable`1[system.decimal]":
                            decimal valDecimal = 0m;
                            if (Decimal.TryParse(currRow[prop.Name].ToString(), out valDecimal)) tempProp.SetValue(ret, valDecimal, null);
                            break;

                        case "system.datetime":
                        case "system.nullable`1[system.datetime]":
                            DateTime datetime = DateTime.Now;
                            if (DateTime.TryParse(currRow[prop.Name].ToString(), out datetime)) tempProp.SetValue(ret, datetime, null);
                            break;

                        default:
                            break;
                    }

                    #endregion
                }

                count++;
                retList.Add(ret);
            }

            return retList;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static object DataTableToObject(DataTable dt, string objType)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (dt == null) return null;
            if (dt.Rows.Count != 1) return null;
            if (String.IsNullOrEmpty(objType)) return null;

            object ret = new object();

            foreach (DataRow dr in dt.Rows)
            {
                ret = Activator.CreateInstance(Type.GetType(objType));
                if (ret == null)
                {
                    return null;
                }

                foreach (PropertyInfo prop in ret.GetType().GetProperties())
                {
                    PropertyInfo tempProp = prop;

                    switch (prop.PropertyType.ToString().ToLower().Trim())
                    {
                        case "system.string":
                            string valStr = dr[prop.Name].ToString().Trim();
                            tempProp.SetValue(ret, valStr, null);
                            break;

                        case "system.int32":
                        case "system.nullable`1[system.int32]":
                            int valInt32 = 0;
                            if (Int32.TryParse(dr[prop.Name].ToString(), out valInt32)) tempProp.SetValue(ret, valInt32, null);
                            break;

                        case "system.int64":
                        case "system.nullable`1[system.int64]":
                            long valInt64 = 0;
                            if (Int64.TryParse(dr[prop.Name].ToString(), out valInt64)) tempProp.SetValue(ret, valInt64, null);
                            break;

                        case "system.decimal":
                        case "system.nullable`1[system.decimal]":
                            decimal valDecimal = 0m;
                            if (Decimal.TryParse(dr[prop.Name].ToString(), out valDecimal)) tempProp.SetValue(ret, valDecimal, null);
                            break;

                        case "system.datetime":
                        case "system.nullable`1[system.datetime]":
                            DateTime datetime = DateTime.Now;
                            if (DateTime.TryParse(dr[prop.Name].ToString(), out datetime)) tempProp.SetValue(ret, datetime, null);
                            break;

                        default:
                            break;
                    }
                }

                break;
            }

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static T DataTableToObject<T>(this DataTable table) where T : new()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                return item;
            }

            return default(T);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static IList<T> DataTableToList<T>(this DataTable table) where T : new()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static IList<T> DataTableToList<T>(this DataTable table, Dictionary<string, string> mappings) where T : new()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties, mappings);
                result.Add(item);
            }

            return result;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (row[property.Name] is System.DBNull) continue;
                property.SetValue(item, row[property.Name], null);
            }
            return item;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties, Dictionary<string, string> mappings) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (mappings.ContainsKey(property.Name))
                    property.SetValue(item, row[mappings[property.Name]], null);
            }
            return item;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<dynamic> DataTableToListDynamic(DataTable dt)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            List<dynamic> ret = new List<dynamic>();
            if (dt == null || dt.Rows.Count < 1) return ret;

            foreach (DataRow curr in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                foreach (DataColumn col in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[col.ColumnName] = curr[col];
                }
                ret.Add(dyn);
            }

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static dynamic DataTableToDynamic(DataTable dt)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            dynamic ret = new ExpandoObject();
            if (dt == null || dt.Rows.Count < 1) return ret;

            foreach (DataRow curr in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)ret;
                    dic[col.ColumnName] = curr[col];
                }

                return ret;
            }

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<Dictionary<string, object>> DataTableToListDictionary(DataTable dt)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
            if (dt == null || dt.Rows.Count < 1) return ret;

            foreach (DataRow curr in dt.Rows)
            {
                Dictionary<string, object> currDict = new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    currDict.Add(col.ColumnName, curr[col]);
                }

                ret.Add(currDict);
            }

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static Dictionary<string, object> DataTableToDictionary(DataTable dt)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            if (dt == null || dt.Rows.Count < 1) return ret;

            foreach (DataRow curr in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    ret.Add(col.ColumnName, curr[col]);
                }

                return ret;
            }

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static byte[] Base64ToBytes(string data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                return System.Convert.FromBase64String(data);
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string Base64ToUTF8(string data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (String.IsNullOrEmpty(data)) return null;
                byte[] bytes = System.Convert.FromBase64String(data);
                return System.Text.UTF8Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string BytesToBase64(byte[] data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (data == null) return null;
            if (data.Length < 1) return null;
            return System.Convert.ToBase64String(data);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string UTF8ToBase64(string data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (String.IsNullOrEmpty(data)) return null;
                byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(data);
                return System.Convert.ToBase64String(bytes);
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static byte[] ObjectToBytes(object obj)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (obj == null) return null;
                if (obj is byte[]) return (byte[])obj;

                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream();
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static byte[] StreamToBytes(Stream input)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!input.CanRead) throw new InvalidOperationException("Input stream is not readable");

            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;

                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<string> CsvToStringList(string csv)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(csv))
            {
                return null;
            }

            List<string> ret = new List<string>();

            string[] array = csv.Split(',');

            if (array != null && array.Length > 0)
            {
                foreach (string curr in array)
                {
                    if (String.IsNullOrEmpty(curr)) continue;
                    ret.Add(curr.Trim());
                }
            }

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string StringListToCsv(List<string> strings)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (strings == null || strings.Count < 1) return null;

            int added = 0;
            string ret = "";

            foreach (string curr in strings)
            {
                if (added == 0)
                {
                    ret += curr;
                }
                else
                {
                    ret += "," + curr;
                }

                added++;
            }

            return ret;
        }

        #endregion

        #region Misc

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string StringRemove(string original, string remove)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(original)) return null;
            if (String.IsNullOrEmpty(remove)) return original;

            int index = original.IndexOf(remove);
            string ret = (index < 0)
                ? original
                : original.Remove(index, remove.Length);

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string Line(int count, string fill)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (count < 1) return "";

            string ret = "";
            for (int i = 0; i < count; i++)
            {
                ret += fill;
            }

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string RandomString(int num_char)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            string ret = "";
            if (num_char < 1) return null;
            int valid = 0;
            Random random = new Random((int)DateTime.Now.Ticks);
            int num = 0;

            for (int i = 0; i < num_char; i++)
            {
                num = 0;
                valid = 0;
                while (valid == 0)
                {
                    num = random.Next(126);
                    if (((num > 47) && (num < 58)) ||
                        ((num > 64) && (num < 91)) ||
                        ((num > 96) && (num < 123)))
                    {
                        valid = 1;
                    }
                }
                ret += (char)num;
            }

            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static double TotalMsFrom(DateTime start)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                DateTime end = DateTime.Now;
                return TotalMsBetween(start, end);
            }
            catch (Exception)
            {
                return -1;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static double TotalMsBetween(DateTime start, DateTime end)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                start = start.ToUniversalTime();
                end = end.ToUniversalTime();
                TimeSpan total = end - start;
                return total.TotalMilliseconds;
            }
            catch (Exception)
            {
                return -1;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool IsLaterThanNow(DateTime? dt)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                DateTime curr = Convert.ToDateTime(dt);
                return Common.IsLaterThanNow(curr);
            }
            catch (Exception)
            {
                return false;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool IsLaterThanNow(DateTime dt)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (DateTime.Compare(dt, DateTime.Now) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Environment

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static void ExitApplication(string method, string text, int returnCode)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Console.WriteLine("---");
            Console.WriteLine("");
            Console.WriteLine("The application has exited.");
            Console.WriteLine("");
            Console.WriteLine("  Requested by : " + method);
            Console.WriteLine("  Reason text  : " + text);
            Console.WriteLine("");
            Console.WriteLine("---");
            Environment.Exit(returnCode);
            return;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetPathSeparator(string environment)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(environment)) throw new ArgumentNullException(nameof(environment));

            switch (environment)
            {
                case "windows":
                    return @"\";

                case "mac":
                case "linux":
                    return "/";

                default:
                    throw new ArgumentException("Unknown environment: " + environment);
            }
        }

        #endregion

        #region Dictionary

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static Dictionary<string, string> AddToDictionary(string key, string val, Dictionary<string, string> existing)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            if (existing == null)
            {
                ret.Add(key, val);
                return ret;
            }
            else
            {
                existing.Add(key, val);
                return existing;
            }
        }

        #endregion

        #region IsTrue

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool IsTrue(int? val)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (val == null) return false;
            if (Convert.ToInt32(val) == 1) return true;
            return false;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool IsTrue(int val)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (val == 1) return true;
            return false;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool IsTrue(bool val)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return val;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool IsTrue(bool? val)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (val == null) return false;
            return Convert.ToBoolean(val);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool IsTrue(string val)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(val)) return false;
            val = val.ToLower().Trim();
            int valInt = 0;
            if (Int32.TryParse(val, out valInt)) if (valInt == 1) return true;
            if (String.Compare(val, "true") == 0) return true;
            return false;
        }

        #endregion

        #region Crypto

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string Md5(byte[] data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (data == null) return null;

            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
            string ret = sb.ToString();
            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string Md5(string data)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (String.IsNullOrEmpty(data)) return null;

            MD5 md5 = MD5.Create();
            byte[] dataBytes = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hash = md5.ComputeHash(dataBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
            string ret = sb.ToString();
            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string Md5(Stream stream)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (stream == null || !stream.CanRead) return null;

            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
            string ret = sb.ToString();
            return ret;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string Md5File(string filename)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    byte[] hash = md5.ComputeHash(stream); 
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
                    string ret = sb.ToString();
                    return ret;
                }
            }
        }

        #endregion

        #region Directory

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool CreateDirectory(string dir)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Directory.CreateDirectory(dir);
            return true;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool DirectoryExists(string dir)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                return Directory.Exists(dir);
            }
            catch (Exception)
            {
                return false;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<string> GetSubdirectoryList(string directory, bool recursive)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                /*
                 * Prepends the 'directory' variable to the name of each directory already
                 * so each is immediately usable from the resultant list
                 * 
                 * Does NOT append a slash
                 * Does NOT include the original directory in the list
                 * Does NOT include child files
                 * 
                 * i.e. 
                 * C:\code\kvpbase
                 * C:\code\kvpbase\src
                 * C:\code\kvpbase\test
                 * 
                 */

                string[] folders;

                if (recursive)
                {
                    folders = Directory.GetDirectories(@directory, "*", SearchOption.AllDirectories);
                }
                else
                {
                    folders = Directory.GetDirectories(@directory, "*", SearchOption.TopDirectoryOnly);
                }

                List<string> folderList = new List<string>();

                foreach (string folder in folders)
                {
                    folderList.Add(folder);
                }

                return folderList;
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool DeleteDirectory(string dir, bool recursive)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            if (!Directory.Exists(dir)) return true;

            if (!recursive)
            {
                Directory.Delete(dir);
                return true;
            }
            else
            {
                DirectoryInfo di = new DirectoryInfo(dir);

                foreach (FileInfo fi in di.GetFiles())
                {
                    fi.Delete();
                }

                foreach (DirectoryInfo subdir in di.GetDirectories())
                {
                    DeleteDirectory(subdir.FullName, true);
                }

                Directory.Delete(dir, true);
                return true;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool RenameDirectory(string from, string to)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (String.IsNullOrEmpty(from)) return false;
                if (String.IsNullOrEmpty(to)) return false;
                if (String.Compare(from, to) == 0) return true;
                Directory.Move(from, to);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool MoveDirectory(string from, string to)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (String.IsNullOrEmpty(from)) return false;
                if (String.IsNullOrEmpty(to)) return false;
                if (String.Compare(from, to) == 0) return true;
                Directory.Move(from, to);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool WalkDirectory(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            string environment,
            int depth,
            string directory,
            bool prependFilename,
            out List<string> subdirectories,
            out List<string> files,
            out long bytes,
            bool recursive)
        {
            subdirectories = new List<string>();
            files = new List<string>();
            bytes = 0;

            try
            {
                subdirectories = Common.GetSubdirectoryList(directory, false);
                files = Common.GetFileList(environment, directory, prependFilename);

                if (files != null && files.Count > 0)
                {
                    foreach (String currFile in files)
                    {
                        FileInfo fi = new FileInfo(currFile);
                        bytes += fi.Length;
                    }
                }

                List<string> queueSubdirectories = new List<string>();
                List<string> queueFiles = new List<string>();
                long queueBytes = 0;

                if (recursive)
                {
                    if (subdirectories == null || subdirectories.Count < 1) return true;
                    depth += 2;

                    foreach (string curr in subdirectories)
                    {
                        List<string> childSubdirectories = new List<string>();
                        List<string> childFiles = new List<string>();
                        long childBytes = 0;

                        WalkDirectory(
                            environment,
                            depth,
                            curr,
                            prependFilename,
                            out childSubdirectories,
                            out childFiles,
                            out childBytes,
                            true);

                        if (childSubdirectories != null)
                            foreach (string childSubdir in childSubdirectories)
                                queueSubdirectories.Add(childSubdir);

                        if (childFiles != null)
                            foreach (string childFile in childFiles)
                                queueFiles.Add(childFile);

                        queueBytes += childBytes;
                    }
                }

                if (queueSubdirectories != null)
                    foreach (string queueSubdir in queueSubdirectories)
                        subdirectories.Add(queueSubdir);

                if (queueFiles != null)
                    foreach (string queueFile in queueFiles)
                        files.Add(queueFile);

                bytes += queueBytes;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool DirectoryStatistics(
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
            DirectoryInfo dirinfo,
            bool recursive,
            out long bytes,
            out int files,
            out int subdirs)
        {
            bytes = 0;
            files = 0;
            subdirs = 0;

            try
            {
                FileInfo[] fis = dirinfo.GetFiles();
                files = fis.Length;

                foreach (FileInfo fi in fis)
                {
                    bytes += fi.Length;
                }

                // Add subdirectory sizes
                DirectoryInfo[] subdirinfos = dirinfo.GetDirectories();

                if (recursive)
                {
                    foreach (DirectoryInfo subdirinfo in subdirinfos)
                    {
                        subdirs++;
                        long subdirBytes = 0;
                        int subdirFiles = 0;
                        int subdirSubdirectories = 0;

                        if (Common.DirectoryStatistics(subdirinfo, recursive, out subdirBytes, out subdirFiles, out subdirSubdirectories))
                        {
                            bytes += subdirBytes;
                            files += subdirFiles;
                            subdirs += subdirSubdirectories;
                        }
                    }
                }
                else
                {
                    subdirs = subdirinfos.Length;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region File

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool DeleteFile(string filename)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                File.Delete(@filename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool FileExists(string filename)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            return File.Exists(filename);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static List<string> GetFileList(string environment, string directory, bool prependFilename)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                /*
                 * 
                 * Returns only the filename unless prepend_filename is set
                 * If prepend_filename is set, directory is prepended
                 * 
                 */

                string separator = GetPathSeparator(environment);
                DirectoryInfo info = new DirectoryInfo(directory);
                FileInfo[] files = info.GetFiles().OrderBy(p => p.CreationTime).ToArray();
                List<string> fileList = new List<string>();

                foreach (FileInfo file in files)
                {
                    if (prependFilename) fileList.Add(directory + separator + file.Name);
                    else fileList.Add(file.Name);
                }

                return fileList;
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool WriteFile(string filename, string content, bool append)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            using (StreamWriter writer = new StreamWriter(filename, append))
            {
                writer.WriteLine(content);
            }
            return true;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool WriteFile(string filename, byte[] content)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            File.WriteAllBytes(filename, content); return true;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool WriteFile(string filename, byte[] content, int pos)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            using (Stream stream = new FileStream(filename, System.IO.FileMode.OpenOrCreate))
            {
                stream.Seek(pos, SeekOrigin.Begin);
                stream.Write(content, 0, content.Length);
            }
            return true;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string ReadTextFile(string filename)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                return File.ReadAllText(@filename);
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static byte[] ReadBinaryFile(string filename, int from, int len)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (len < 1) return null;
                if (from < 0) return null;

                byte[] ret = new byte[len];
                using (BinaryReader reader = new BinaryReader(new FileStream(filename, System.IO.FileMode.Open)))
                {
                    reader.BaseStream.Seek(from, SeekOrigin.Begin);
                    reader.Read(ret, 0, len);
                }

                return ret;
            }
            catch (Exception)
            {
                return null;
            }
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static byte[] ReadBinaryFile(string filename)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                return File.ReadAllBytes(@filename);
            }
            catch (Exception)
            {
                return null;
            }
        }
         
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string GetFileExtension(string filename)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (String.IsNullOrEmpty(filename)) return null;
                return Path.GetExtension(filename);
            }
            catch (Exception)
            {
                return null;
            }
        }
         
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool RenameFile(string from, string to)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (String.IsNullOrEmpty(from)) return false;
                if (String.IsNullOrEmpty(to)) return false;

                if (String.Compare(from, to) == 0) return true;
                File.Move(from, to);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
         
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool MoveFile(string from, string to)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                if (String.IsNullOrEmpty(from)) return false;
                if (String.IsNullOrEmpty(to)) return false;

                if (String.Compare(from, to) == 0) return true;
                File.Move(from, to);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
         
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static bool VerifyFileReadAccess(string filename) 
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            try
            {
                using (FileStream stream = File.Open(filename, System.IO.FileMode.Open, FileAccess.Read))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }

        #endregion 
    }
}
