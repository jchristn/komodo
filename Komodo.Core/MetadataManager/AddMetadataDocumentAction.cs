using System;
using System.Collections.Generic;
using System.Text;
using Komodo;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Add metadata document action.
    /// </summary>
    public class AddMetadataDocumentAction
    {
        #region Public-Members

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
        public string Name { get; set; } = null;

        /// <summary>
        /// The title for the derived document.
        /// </summary>
        public string Title { get; set; } = null;

        /// <summary>
        /// Tags for the derived document.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// Indicate whether or not the metadata document should be parsed and indexed.
        /// </summary>
        public bool Parse { get; set; } = true;

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

        #endregion

        #region Private-Members 

        private string _IndexGUID = null;
        private List<MetadataDocumentProperty> _Properties = new List<MetadataDocumentProperty>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public AddMetadataDocumentAction()
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
