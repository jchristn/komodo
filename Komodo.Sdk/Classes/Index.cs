using System;
using System.Collections.Generic;
using System.Data;

namespace Komodo.Sdk.Classes
{
    /// <summary>
    /// Repository where source and parsed documents are stored.
    /// </summary>
    public class Index
    {
        #region Public-Members

        /// <summary>
        /// Globally-unique identifier.
        /// </summary>
        public string GUID { get; set; }

        /// <summary>
        /// Globally-unique identifier of the user that owns this index.
        /// </summary>
        public string OwnerGUID { get; set; }

        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Index()
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
            return KomodoCommon.SerializeJson(this, pretty);
        }

        #endregion
    }
}
