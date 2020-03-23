using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Komodo.Classes;

namespace Komodo.Classes
{
    /// <summary>
    /// Object that has been uploaded to Komodo.
    /// </summary>
    public class SourceDocument
    {
        /// <summary>
        /// Database row ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user that owns the document record.
        /// </summary>
        public string OwnerGUID { get; set; }

        /// <summary>
        /// The globally-unique identifier of the index.
        /// </summary>
        public string IndexGUID { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The title of the object.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The tags associated with the object.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary>
        public DocType Type { get; set; }

        /// <summary>
        /// The URL from which the content was retrieved.
        /// </summary>
        public string SourceURL { get; set; }

        /// <summary>
        /// The content type of the object.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The content length of the source document.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// The MD5 hash of the source content.
        /// </summary>
        public string Md5 { get; set; }

        /// <summary>
        /// The timestamp from when the entry was created.
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// The timestamp from when the document was indexed.
        /// </summary>
        public DateTime? Indexed { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SourceDocument()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns the document record.</param>
        /// <param name="indexGuid">The globally-unique identifier of the index.</param>
        /// <param name="name">The name of the object.</param>
        /// <param name="title">The title of the object.</param>
        /// <param name="tags">The tags associated with the object.</param>
        /// <param name="docType">The type of document.</param>
        /// <param name="sourceUrl">The URL from which the content was retrieved.</param>
        /// <param name="contentType">The content type of the object.</param>
        /// <param name="contentLength">The content length of the source document.</param>
        /// <param name="md5">The MD5 hash of the source content.</param>
        public SourceDocument(string ownerGuid, string indexGuid, string name, string title, List<string> tags, DocType docType, string sourceUrl, string contentType, long contentLength, string md5)
        {
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (contentLength < 0) throw new ArgumentException("Content length must be zero or greater.");

            GUID = Guid.NewGuid().ToString();
            OwnerGUID = ownerGuid;
            IndexGUID = indexGuid;
            Name = name;
            Title = title;
            Tags = tags;
            Type = docType;
            SourceURL = sourceUrl;
            ContentType = contentType;
            ContentLength = contentLength;
            Md5 = md5;
            Created = DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns the document record.</param>
        /// <param name="indexGuid">The globally-unique identifier of the index.</param>
        /// <param name="name">The name of the object.</param>
        /// <param name="title">The title of the object.</param>
        /// <param name="tags">The tags associated with the object.</param>
        /// <param name="docType">The type of document.</param>
        /// <param name="sourceUrl">The URL from which the content was retrieved.</param>
        /// <param name="contentType">The content type of the object.</param>
        /// <param name="contentLength">The content length of the source document.</param>
        /// <param name="md5">The MD5 hash of the source content.</param>
        public SourceDocument(string guid, string ownerGuid, string indexGuid, string name, string title, List<string> tags, DocType docType, string sourceUrl, string contentType, long contentLength, string md5)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (contentLength < 0) throw new ArgumentException("Content length must be zero or greater.");

            if (!String.IsNullOrEmpty(guid)) GUID = guid;
            else GUID = Guid.NewGuid().ToString();

            OwnerGUID = ownerGuid;
            IndexGUID = indexGuid;
            Name = name;
            Title = title;
            Tags = tags;
            Type = docType;
            SourceURL = sourceUrl;
            ContentType = contentType;
            ContentLength = contentLength;
            Md5 = md5;
            Created = DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Create a database insertable dictionary from the object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("guid", GUID);
            ret.Add("ownerguid", OwnerGUID);
            ret.Add("indexguid", IndexGUID);
            ret.Add("name", Name);
            ret.Add("title", Title);
            ret.Add("tags", Common.StringListToCsv(Tags));
            ret.Add("doctype", Type.ToString());
            ret.Add("sourceurl", SourceURL);
            ret.Add("contenttype", ContentType);
            ret.Add("contentlength", ContentLength);
            ret.Add("md5", Md5);
            ret.Add("created", Created);
            ret.Add("indexed", Indexed);
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static SourceDocument FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            SourceDocument ret = new SourceDocument();
             
            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("ownerguid") && row["ownerguid"] != null && row["ownerguid"] != DBNull.Value)
                ret.OwnerGUID = row["ownerguid"].ToString();

            if (row.Table.Columns.Contains("indexguid") && row["indexguid"] != null && row["indexguid"] != DBNull.Value)
                ret.IndexGUID = row["indexguid"].ToString();

            if (row.Table.Columns.Contains("name") && row["name"] != null && row["name"] != DBNull.Value)
                ret.Name = row["name"].ToString();

            if (row.Table.Columns.Contains("title") && row["title"] != null && row["title"] != DBNull.Value)
                ret.Title = row["title"].ToString();

            if (row.Table.Columns.Contains("tags") && row["tags"] != null && row["tags"] != DBNull.Value)
                ret.Tags = Common.CsvToStringList(row["tags"].ToString());

            if (row.Table.Columns.Contains("doctype") && row["doctype"] != null && row["doctype"] != DBNull.Value)
                ret.Type = (DocType)(Enum.Parse(typeof(DocType), row["doctype"].ToString()));

            if (row.Table.Columns.Contains("sourceurl") && row["sourceurl"] != null && row["sourceurl"] != DBNull.Value)
                ret.SourceURL = row["sourceurl"].ToString();

            if (row.Table.Columns.Contains("contenttype") && row["contenttype"] != null && row["contenttype"] != DBNull.Value)
                ret.ContentType = row["contenttype"].ToString();

            if (row.Table.Columns.Contains("contentlength") && row["contentlength"] != null && row["contentlength"] != DBNull.Value)
                ret.ContentLength = Convert.ToInt64(row["contentlength"].ToString());

            if (row.Table.Columns.Contains("md5") && row["md5"] != null && row["md5"] != DBNull.Value)
                ret.Md5 = row["md5"].ToString();
             
            if (row.Table.Columns.Contains("created") && row["created"] != null && row["created"] != DBNull.Value) 
                ret.Created = Convert.ToDateTime(row["created"].ToString()); 

            if (row.Table.Columns.Contains("indexed") && row["indexed"] != null && row["indexed"] != DBNull.Value)
                ret.Indexed = Convert.ToDateTime(row["indexed"].ToString());

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<SourceDocument> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<SourceDocument> ret = new List<SourceDocument>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(SourceDocument.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
