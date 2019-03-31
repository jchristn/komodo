using System;
using System.Collections.Generic;
using System.Text;

namespace KomodoCore
{
    /// <summary>
    /// A search filter.
    /// </summary>
    public class SearchFilter
    { 
        /// <summary>
        /// The field upon which to match.
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// The condition by which the parsed document's content is evaluated against the supplied value.
        /// </summary>
        public SearchCondition Condition { get; set; }

        /// <summary>
        /// The value to be evaluated using the specified condition against the parsed document's content.
        /// </summary>
        public string Value { get; set; } 
    }
}
