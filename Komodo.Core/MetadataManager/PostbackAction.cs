using System;
using System.Collections.Generic;
using System.Text;

namespace Komodo.MetadataManager
{
    /// <summary>
    /// Postback action.
    /// </summary>
    public class PostbackAction
    {
        #region Public-Members

        /// <summary>
        /// Indicates whether or not the source document metadata should be included.  Default is true.
        /// </summary>
        public bool IncludeSource { get; set; } = true;

        /// <summary>
        /// Indicates whether or not the parsed document metadata should be included.  Default is true.
        /// </summary>
        public bool IncludeParsed { get; set; } = true;

        /// <summary>
        /// Indicates whether or not the parse results from the parsed document should be included.  Default is true.
        /// </summary>
        public bool IncludeParseResult { get; set; } = true;

        /// <summary>
        /// Indicates whether or not the metadata document should be included.  Default is true.
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;

        /// <summary>
        /// Indicates whether or not the rules should be included.  Default is true.
        /// </summary>
        public bool IncludeRules { get; set; } = true;

        /// <summary>
        /// Indicates whether or not the derived metadata documents should be included.  Default is true.
        /// </summary>
        public bool IncludeDerivedDocuments { get; set; } = true;

        /// <summary>
        /// List of URLs where data should be POSTed.
        /// </summary>
        public List<string> Urls { get; set; } = new List<string>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public PostbackAction()
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
