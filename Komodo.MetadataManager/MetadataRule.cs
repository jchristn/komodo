using System;
using System.Collections.Generic;
using System.Text;
using Komodo.Classes;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Metadata rule.
    /// </summary>
    public class MetadataRule
    {
        /// <summary>
        /// Name of the rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the rule.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indicates whether or not the rule is enabled.
        /// </summary>
        public bool Enabled = true;

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

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataRule()
        {

        }

        private QueryFilter _Required = new QueryFilter();
        private QueryFilter _Exclude = new QueryFilter(); 
        private List<AddMetadataDocumentAction> _AddMetadataDocument = new List<AddMetadataDocumentAction>();
        private PostbackAction _Postback = new PostbackAction();
    }
}
