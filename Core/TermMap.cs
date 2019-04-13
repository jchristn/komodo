using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Komodo.Core
{
    public class TermMap
    {
        #region Public-Members

        public int Id { get; set; }
        public string Term { get; set; }
        public string GUID { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastUpdate { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        public TermMap()
        {

        }

        public static TermMap FromDataRow(DataRow row)
        {
            if (row == null) throw new ArgumentNullException(nameof(row));

            TermMap ret = new TermMap();
            ret.Id = Convert.ToInt32(row["Id"]);
            ret.Term = row["Term"].ToString();
            ret.GUID = row["GUID"].ToString();
            ret.Created = Convert.ToDateTime(row["Created"].ToString());
            ret.LastUpdate = Convert.ToDateTime(row["LastUpdate"].ToString());
            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
