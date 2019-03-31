using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Core
{
    /// <summary>
    /// A filter for the query.
    /// </summary>
    public class QueryFilter
    { 
        /// <summary>
        /// List of terms upon which to match.
        /// </summary>
        public List<string> Terms { get; set; }

        /// <summary>
        /// List of filters upon which to match.
        /// </summary>
        public List<SearchFilter> Filter { get; set; }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public QueryFilter()
        {
            Terms = new List<string>();
            Filter = new List<SearchFilter>();
        } 
    }
}
