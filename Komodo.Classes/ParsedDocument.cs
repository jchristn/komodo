using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Komodo.Classes;

namespace Komodo.Classes
{
    /// <summary>
    /// Object that has been parsed by Komodo.
    /// </summary>
    public class ParsedDocument
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
        /// Globally-unique identifier of the source document.
        /// </summary>
        public string SourceDocumentGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user that owns the document record.
        /// </summary>
        public string OwnerGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the index.
        /// </summary>
        public string IndexGUID { get; set; }

        /// <summary>
        /// The type of document.
        /// </summary>
        public DocType Type { get; set; }

        /// <summary>
        /// The content length of the parsed document.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// The number of terms in the parsed document.
        /// </summary>
        public long Terms { get; set; }

        /// <summary>
        /// The number of postings in the parsed document.
        /// </summary>
        public long Postings { get; set; }

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
        public ParsedDocument()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="sourceDocGuid">Globally-unique identifier of the source document.</param>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns the document record.</param>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="docType">The type of document.</param>
        /// <param name="contentLength">The content length of the parsed document.</param>
        /// <param name="terms">The number of terms in the parsed document.</param>
        /// <param name="postings">The number of postings in the parsed document.</param>
        public ParsedDocument(string sourceDocGuid, string ownerGuid, string indexGuid, DocType docType, long contentLength, long terms, long postings)
        {
            if (String.IsNullOrEmpty(sourceDocGuid)) throw new ArgumentNullException(nameof(sourceDocGuid));
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (contentLength < 0) throw new ArgumentException("Content length must be zero or greater.");
            if (terms < 0) throw new ArgumentException("Terms count must be zero or greater.");
            if (postings < 0) throw new ArgumentException("Postings count must be zero or greater.");

            GUID = Guid.NewGuid().ToString();
            SourceDocumentGUID = sourceDocGuid;
            OwnerGUID = ownerGuid;
            IndexGUID = indexGuid;
            Type = docType;
            ContentLength = contentLength;
            Terms = terms;
            Postings = postings;
            Created = DateTime.Now.ToUniversalTime(); 
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="sourceDocGuid">Globally-unique identifier of the source document.</param>
        /// <param name="ownerGuid">Globally-unique identifier of the user that owns the document record.</param>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="docType">The type of document.</param>
        /// <param name="terms">The number of terms in the parsed document.</param>
        /// <param name="postings">The number of postings in the parsed document.</param>
        /// <param name="contentLength">The content length of the parsed document.</param>
        public ParsedDocument(string guid, string sourceDocGuid, string ownerGuid, string indexGuid, DocType docType, long contentLength, long terms, long postings)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(sourceDocGuid)) throw new ArgumentNullException(nameof(sourceDocGuid));
            if (String.IsNullOrEmpty(ownerGuid)) throw new ArgumentNullException(nameof(ownerGuid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (contentLength < 0) throw new ArgumentException("Content length must be zero or greater.");
            if (terms < 0) throw new ArgumentException("Terms count must be zero or greater.");
            if (postings < 0) throw new ArgumentException("Postings count must be zero or greater.");

            GUID = guid;
            SourceDocumentGUID = sourceDocGuid;
            OwnerGUID = ownerGuid;
            IndexGUID = indexGuid;
            Type = docType;
            ContentLength = contentLength;
            Terms = terms;
            Postings = postings;
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
            ret.Add("sourcedocguid", SourceDocumentGUID);
            ret.Add("ownerguid", OwnerGUID);
            ret.Add("indexguid", IndexGUID);
            ret.Add("doctype", Type.ToString());
            ret.Add("contentlength", ContentLength);
            ret.Add("terms", Terms);
            ret.Add("postings", Postings);
            ret.Add("created", Created);
            ret.Add("indexed", Indexed);
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static ParsedDocument FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            ParsedDocument ret = new ParsedDocument();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("sourcedocguid") && row["sourcedocguid"] != null && row["sourcedocguid"] != DBNull.Value)
                ret.SourceDocumentGUID = row["sourcedocguid"].ToString();

            if (row.Table.Columns.Contains("ownerguid") && row["ownerguid"] != null && row["ownerguid"] != DBNull.Value)
                ret.OwnerGUID = row["ownerguid"].ToString();

            if (row.Table.Columns.Contains("indexguid") && row["indexguid"] != null && row["indexguid"] != DBNull.Value)
                ret.IndexGUID = row["indexguid"].ToString();

            if (row.Table.Columns.Contains("doctype") && row["doctype"] != null && row["doctype"] != DBNull.Value)
                ret.Type = (DocType)(Enum.Parse(typeof(DocType), row["doctype"].ToString()));

            if (row.Table.Columns.Contains("contentlength") && row["contentlength"] != null && row["contentlength"] != DBNull.Value)
                ret.ContentLength = Convert.ToInt64(row["contentlength"].ToString());

            if (row.Table.Columns.Contains("terms") && row["terms"] != null && row["terms"] != DBNull.Value)
                ret.Terms = Convert.ToInt64(row["terms"].ToString());

            if (row.Table.Columns.Contains("postings") && row["postings"] != null && row["postings"] != DBNull.Value)
                ret.Postings = Convert.ToInt64(row["postings"].ToString());

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
        public static List<ParsedDocument> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<ParsedDocument> ret = new List<ParsedDocument>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(ParsedDocument.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
