using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Sdk.Classes
{
    /// <summary>
    /// A filter for the query.
    /// </summary>
    public class QueryFilter
    {
        #region Public-Members

        /// <summary>
        /// List of terms upon which to match.
        /// </summary>
        public List<string> Terms
        {
            get
            {
                return _Terms;
            }
            set
            {
                if (value == null) _Terms = new List<string>();
                else _Terms = value;
            }
        }

        /// <summary>
        /// List of filters upon which to match.
        /// </summary>
        public List<SearchFilter> Filter
        {
            get
            {
                return _Filter;
            }
            set
            {
                if (value == null) _Filter = new List<SearchFilter>();
                else _Filter = value;
            }
        }

        #endregion

        #region Private-Members

        private List<string> _Terms = new List<string>();
        private List<SearchFilter> _Filter = new List<SearchFilter>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public QueryFilter()
        {
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
