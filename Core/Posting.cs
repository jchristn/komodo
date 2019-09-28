using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komodo.Core
{
    /// <summary>
    /// Inverted index entry.
    /// </summary>
    public class Posting
    {
        #region Public-Members

        /// <summary>
        /// Database ID.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Term.
        /// </summary>
        public string Term { get; set; }
        
        /// <summary>
        /// Document ID.
        /// </summary>
        public string DocumentId { get; set; }
         
        /// <summary>
        /// The frequency with which the term was found.
        /// </summary>
        public long Frequency { get; set; }

        /// <summary>
        /// The character positions where the term was found.
        /// </summary>
        public List<long> Positions { get; set; }

        /// <summary>
        /// Time at which the posting was created in the database.
        /// </summary>
        public DateTime? Created { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the Posting object.
        /// </summary>
        public Posting()
        {

        }

        /// <summary>
        /// Convert a DataRow to a Posting.
        /// </summary>
        /// <param name="row">DataRow.</param>
        /// <returns>Posting.</returns>
        public static Posting FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            Posting ret = new Posting();
            ret.Id = Convert.ToInt32(row["Id"]);
            ret.DocumentId = row["DocumentId"].ToString(); 
            ret.Frequency = Convert.ToInt64(row["Frequency"]);
            ret.Positions = Common.DeserializeJson<List<long>>(row["Positions"].ToString());
            ret.Created = Convert.ToDateTime(row["Created"].ToString());
            return ret;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Returns a human-readable string version of the object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            string ret = "";
            ret += "[" + Term + " frequency " + Frequency + "]: ";
            if (Positions != null)
            {
                int added = 0;
                foreach (long curr in Positions)
                {
                    if (added == 0) ret += curr;
                    else ret += "," + curr;
                    added++;
                }
            }
            return ret;
        }

        #endregion

        #region Private-Methods

        #endregion 
    }
}
