using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using Komodo.Core.Enums;

namespace Komodo.Core
{
    /// <summary>
    /// Error-related data.
    /// </summary>
    public class ErrorCode
    {
        #region Public-Members

        /// <summary>
        /// The ID of the error.
        /// </summary>
        public ErrorId Id = ErrorId.NONE;
         
        /// <summary>
        /// Data associated with the error.
        /// </summary>
        public object Data = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the ErrorCode.
        /// </summary>
        public ErrorCode()
        {

        }

        /// <summary>
        /// Instantiates the ErrorCode.
        /// </summary>
        /// <param name="id">The ID of the error.</param>
        public ErrorCode(ErrorId id)
        {
            Id = id;
        }

        /// <summary>
        /// Instantiates the ErrorCode.
        /// </summary>
        /// <param name="id">The ID of the error.</param>
        /// <param name="data">Data associated with the error.</param>
        public ErrorCode(ErrorId id, object data)
        {
            Id = id;
            Data = data;
        }

        #endregion

        #region Public-Methods
        
        #endregion

        #region Private-Methods
         
        #endregion 
    }
}
