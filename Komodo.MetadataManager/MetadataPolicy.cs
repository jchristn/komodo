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
        
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataPolicy()
        {

        }

        private string _IndexGUID = null;
        private List<MetadataRule> _Rules = new List<MetadataRule>();
    }
}
