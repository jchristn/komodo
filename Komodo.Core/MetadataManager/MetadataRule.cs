using System;
using System.Collections.Generic;
using System.Text;
using Komodo;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Metadata rule.
    /// </summary>
    public class MetadataRule
    {
        #region Public-Members

        /// <summary>
        /// Name of the rule.
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Description of the rule.
        /// </summary>
        public string Description { get; set; } = null;

        /// <summary>
        /// Indicates whether or not the rule is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Filter that must match for inclusion.
        /// </summary>
        public QueryFilter Required
        {
            get
            {
                return _Required;
            }
            set
            {
                if (value == null) _Required = new QueryFilter();
                else _Required = value;
            }
        }

        /// <summary>
        /// Filter that must not match for inclusion.
        /// </summary>
        public QueryFilter Exclude
        {
            get
            {
                return _Exclude;
            }
            set
            {
                if (value == null) _Exclude = new QueryFilter();
                else _Exclude = value;
            }
        }

        /// <summary>
        /// Definition of metadata document to add.
        /// </summary>
        public List<AddMetadataDocumentAction> AddMetadataDocument
        {
            get
            {
                return _AddMetadataDocument;
            }
            set
            {
                if (value == null) _AddMetadataDocument = new List<AddMetadataDocumentAction>();
                else _AddMetadataDocument = value;
            }
        }

        /// <summary>
        /// Definition of postback actions.
        /// </summary>
        public PostbackAction Postback
        {
            get
            {
                return _Postback;
            }
            set
            {
                if (value == null) _Postback = new PostbackAction();
                else _Postback = value;
            }
        }

        #endregion

        #region Private-Members

        private QueryFilter _Required = new QueryFilter();
        private QueryFilter _Exclude = new QueryFilter();
        private List<AddMetadataDocumentAction> _AddMetadataDocument = new List<AddMetadataDocumentAction>();
        private PostbackAction _Postback = new PostbackAction();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataRule()
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
