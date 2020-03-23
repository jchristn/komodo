using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.Classes
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
        public string Field
        {
            get
            {
                return _Field;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Field));
                _Field = value;
            }
        }

        /// <summary>
        /// The condition by which the parsed document's content is evaluated against the supplied value.
        /// </summary>
        public SearchCondition Condition = SearchCondition.Equals;

        /// <summary>
        /// The value to be evaluated using the specified condition against the parsed document's content.
        /// When using GreaterThan, GreaterThanOrEqualTo, LessThan, or LessThanOrEqualTo, the value supplied must be convertible to decimal.
        /// </summary>
        public string Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (Condition == SearchCondition.GreaterThan
                    || Condition == SearchCondition.GreaterThanOrEqualTo
                    || Condition == SearchCondition.LessThan
                    || Condition == SearchCondition.LessThanOrEqualTo)
                {
                    if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

                    decimal testDecimal = 0m;
                    if (!Decimal.TryParse(value, out testDecimal))
                    {
                        throw new ArgumentException("Value must be convertible to decimal when using GreaterThan, GreaterThanOrEqualTo, LessThan, or LessThanOrEqualTo.");
                    }
                }

                _Value = value;
            }
        }

        #endregion

        #region Private-Members

        private string _Field = "";
        private string _Value = "";

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
