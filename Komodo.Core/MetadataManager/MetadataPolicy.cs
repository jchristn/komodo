using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Metadata policy.
    /// </summary>
    public class MetadataPolicy
    {
        #region Public-Members

        /// <summary>
        /// Index GUID.
        /// </summary>
        public string IndexGUID
        {
            get
            {
                return _IndexGUID;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(IndexGUID));
                _IndexGUID = value;
            }
        }

        /// <summary>
        /// Metadata rules.
        /// </summary>
        public List<MetadataRule> Rules
        {
            get
            {
                return _Rules;
            }
            set
            {
                if (value == null) _Rules = new List<MetadataRule>();
                else _Rules = value;
            }
        }

        #endregion

        #region Private-Members

        private string _IndexGUID = null;
        private List<MetadataRule> _Rules = new List<MetadataRule>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataPolicy()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        #endregion
    }
}
