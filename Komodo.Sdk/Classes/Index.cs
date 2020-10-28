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

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Index()
        {

        } 
    }
}
