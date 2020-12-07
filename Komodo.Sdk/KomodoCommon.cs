using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Komodo.Sdk
{
    /// <summary>
    /// Commonly used static methods.
    /// </summary>
    public static class KomodoCommon
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static string SerializeJson(object obj, bool pretty) 
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
         
        public static T DeserializeJson<T>(string json)
        {
            if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));
            return JsonConvert.DeserializeObject<T>(json);
        }
         
        public static T DeserializeJson<T>(byte[] data) 
        {
            if (data == null || data.Length < 1) throw new ArgumentNullException(nameof(data));
            return DeserializeJson<T>(Encoding.UTF8.GetString(data));
        }
         
        public static T CopyObject<T>(object o) 
        {
            if (o == null) return default;
            string json = SerializeJson(o, false);
            T ret = DeserializeJson<T>(json);
            return ret;
        }
         
        public static byte[] StreamToBytes(Stream input)
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
         
        public static List<string> CsvToStringList(string csv)
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
         
        public static string StringListToCsv(List<string> strings)
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
         
        public static double TotalMsBetween(DateTime start, DateTime end)
        { 
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();
            TimeSpan total = end - start;
            return total.TotalMilliseconds; 
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
