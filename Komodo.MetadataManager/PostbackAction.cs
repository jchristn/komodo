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
        /// <summary>
        /// Indicates whether or not the source document metadata should be included.  Default is true.
        /// </summary>
        public bool IncludeSource = true;

        /// <summary>
        /// Indicates whether or not the parsed document metadata should be included.  Default is true.
        /// </summary>
        public bool IncludeParsed = true;

        /// <summary>
        /// Indicates whether or not the parse results from the parsed document should be included.  Default is true.
        /// </summary>
        public bool IncludeParseResult = true;

        /// <summary>
        /// Indicates whether or not the metadata document should be included.  Default is true.
        /// </summary>
        public bool IncludeMetadata = true;

        /// <summary>
        /// Indicates whether or not the rules should be included.  Default is true.
        /// </summary>
        public bool IncludeRules = true;

        /// <summary>
        /// Indicates whether or not the derived metadata documents should be included.  Default is true.
        /// </summary>
        public bool IncludeDerivedDocuments = true;

        /// <summary>
        /// List of URLs where data should be POSTed.
        /// </summary>
        public List<string> Urls = new List<string>();

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public PostbackAction()
        {

        } 
    }
}
