using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Classes
{
    /// <summary>
    /// Maps a term to a given document within a given index.
    /// </summary>
    public class TermDoc
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
        /// Globally-unique identifier of the index.
        /// </summary>
        public string IndexGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the term.
        /// </summary>
        public string TermGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the source document.
        /// </summary>
        public string SourceDocumentGUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the parsed document.
        /// </summary>
        public string ParsedDocumentGUID { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public TermDoc()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="termGuid">Globally-unique identifier of the term.</param>
        /// <param name="sourceDocGuid">Globally-unique identifier of the source document.</param>
        /// <param name="parsedDocGuid">Globally-unique identifier of the parsed document.</param>
        public TermDoc(string indexGuid, string termGuid, string sourceDocGuid, string parsedDocGuid)
        {
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(termGuid)) throw new ArgumentNullException(nameof(termGuid));
            if (String.IsNullOrEmpty(sourceDocGuid)) throw new ArgumentNullException(nameof(sourceDocGuid));
            if (String.IsNullOrEmpty(parsedDocGuid)) throw new ArgumentNullException(nameof(parsedDocGuid));

            GUID = Guid.NewGuid().ToString();
            IndexGUID = indexGuid;
            TermGUID = termGuid;
            SourceDocumentGUID = sourceDocGuid;
            ParsedDocumentGUID = parsedDocGuid;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="guid">Globally-unique identifier.</param>
        /// <param name="indexGuid">Globally-unique identifier of the index.</param>
        /// <param name="termGuid">Globally-unique identifier of the term.</param>
        /// <param name="sourceDocGuid">Globally-unique identifier of the source document.</param>
        /// <param name="parsedDocGuid">Globally-unique identifier of the parsed document.</param>
        public TermDoc(string guid, string indexGuid, string termGuid, string sourceDocGuid, string parsedDocGuid)
        {
            if (String.IsNullOrEmpty(guid)) throw new ArgumentNullException(nameof(guid));
            if (String.IsNullOrEmpty(indexGuid)) throw new ArgumentNullException(nameof(indexGuid));
            if (String.IsNullOrEmpty(termGuid)) throw new ArgumentNullException(nameof(termGuid));
            if (String.IsNullOrEmpty(sourceDocGuid)) throw new ArgumentNullException(nameof(sourceDocGuid));
            if (String.IsNullOrEmpty(parsedDocGuid)) throw new ArgumentNullException(nameof(parsedDocGuid));

            GUID = guid;
            IndexGUID = indexGuid;
            TermGUID = termGuid;
            SourceDocumentGUID = sourceDocGuid;
            ParsedDocumentGUID = parsedDocGuid;
        }

        /// <summary>
        /// Create a database insertable dictionary from the object.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> ToInsertDictionary()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("guid", GUID);
            ret.Add("indexguid", IndexGUID);
            ret.Add("termguid", TermGUID);
            ret.Add("sourcedocguid", SourceDocumentGUID);
            ret.Add("parseddocguid", ParsedDocumentGUID);
            return ret;
        }

        /// <summary>
        /// Create the object from a DataRow.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Instance.</returns>
        public static TermDoc FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            TermDoc ret = new TermDoc();

            if (row.Table.Columns.Contains("id") && row["id"] != null && row["id"] != DBNull.Value)
                ret.Id = Convert.ToInt32(row["id"]);

            if (row.Table.Columns.Contains("guid") && row["guid"] != null && row["guid"] != DBNull.Value)
                ret.GUID = row["guid"].ToString();

            if (row.Table.Columns.Contains("indexguid") && row["indexguid"] != null && row["indexguid"] != DBNull.Value)
                ret.IndexGUID = row["indexguid"].ToString();

            if (row.Table.Columns.Contains("termguid") && row["termguid"] != null && row["termguid"] != DBNull.Value)
                ret.TermGUID = row["termguid"].ToString();

            if (row.Table.Columns.Contains("sourcedocguid") && row["sourcedocguid"] != null && row["sourcedocguid"] != DBNull.Value)
                ret.SourceDocumentGUID = row["sourcedocguid"].ToString();

            if (row.Table.Columns.Contains("parseddocguid") && row["parseddocguid"] != null && row["parseddocguid"] != DBNull.Value)
                ret.ParsedDocumentGUID = row["parseddocguid"].ToString();

            return ret;
        }

        /// <summary>
        /// Create a list from a DataTable.
        /// </summary>
        /// <param name="table">DataTable.</param>
        /// <returns>List of instances.</returns>
        public static List<TermDoc> FromDataTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            List<TermDoc> ret = new List<TermDoc>();
            if (table.Rows != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    ret.Add(TermDoc.FromDataRow(row));
                }
            }
            return ret;
        }
    }
}
