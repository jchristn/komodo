using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Property for a metadata document.
    /// </summary>
    public class MetadataDocumentProperty
    {
        #region Public-Members

        /// <summary>
        /// The action to take to define the value for this property.
        /// </summary>
        public PropertyValueAction ValueAction { get; set; } = PropertyValueAction.Static;

        /// <summary>
        /// The property from the parse result of the source document.
        /// </summary>
        public string SourceProperty
        {
            get
            {
                return _SourceProperty;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
                else _SourceProperty = value;
            }
        }

        /// <summary>
        /// The key to use in the metadata document.
        /// </summary>
        public string Key
        {
            get
            {
                return _Key;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));
                else _Key = value;
            }
        }

        /// <summary>
        /// For static value actions, the value to use.
        /// </summary>
        public string Value { get; set; } = null;

        #endregion

        #region Private-Members

        private string _SourceProperty = null;
        private string _Key = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public MetadataDocumentProperty()
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
