using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Core
{
    /// <summary>
    /// A search filter.
    /// </summary>
    public class SearchFilter
    {
        #region Public-Members

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

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public SearchFilter()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="field">Field.</param>
        /// <param name="condition">SearchCondition.</param>
        /// <param name="value">Value.</param>
        public SearchFilter(string field, SearchCondition condition, string value)
        {
            if (String.IsNullOrEmpty(field)) throw new ArgumentNullException(nameof(field));

            Field = field;
            Condition = condition;
            Value = value;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
