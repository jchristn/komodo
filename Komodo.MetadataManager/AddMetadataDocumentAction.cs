using System;
using System.Collections.Generic;
using System.Text;
using Komodo.Classes;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Add metadata document action.
    /// </summary>
    public class AddMetadataDocumentAction
    { 
        /// <summary>
        /// GUID for the index where the metadata should be stored.
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
                else _IndexGUID = value;
            }
        }

        /// <summary>
        /// The name for the derived document.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The title for the derived document.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Tags for the derived document.
        /// </summary>
        public List<string> Tags = new List<string>();

        /// <summary>
        /// Indicate whether or not the metadata document should be parsed and indexed.
        /// </summary>
        public bool Parse = true;

        /// <summary>
        /// Document properties for the metadata document.
        /// </summary>
        public List<MetadataDocumentProperty> Properties
        {
            get
            {
                return _Properties;
            }
            set
            {
                if (value == null) _Properties = new List<MetadataDocumentProperty>();
                else _Properties = value;
            }
        }
         
        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public AddMetadataDocumentAction()
        {

        }

        private string _IndexGUID = null;
        private List<MetadataDocumentProperty> _Properties = new List<MetadataDocumentProperty>();
    }
}
