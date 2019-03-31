using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

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
        public ErrorId Id { get; set; }

        /// <summary>
        /// Human-readable description of the error.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Data associated with the error.
        /// </summary>
        public object Data { get; private set; } 

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
