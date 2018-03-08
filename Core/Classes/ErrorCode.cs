using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomodoCore
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
        public string Id { get; private set; }

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
        public ErrorCode(string id)
        {
            SetId(id);
        }

        /// <summary>
        /// Instantiates the ErrorCode.
        /// </summary>
        /// <param name="id">The ID of the error.</param>
        /// <param name="data">Data associated with the error.</param>
        public ErrorCode(string id, object data)
        {
            SetId(id);
            SetData(data);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a JSON from the error.
        /// </summary>
        /// <returns>JSON string.</returns>
        public string ToJson()
        {
            return Common.SerializeJson(this, true);
        }

        #endregion

        #region Private-Methods

        private void SetId(string id)
        {
            switch (id)
            {
                case "MISSING_PARAM":
                    Description = "One or more parameters are missing.";
                    break;

                case "RETRIEVE_FAILED":
                    Description = "Unable to retrieve an object.";
                    break;

                case "PARSE_ERROR":
                    Description = "Unable to parse an object.";
                    break;

                case "WRITE_ERROR":
                    Description = "Unable to write an object.";
                    break;

                case "DELETE_ERROR":
                    Description = "Unable to delete an object.";
                    break;

                default:
                    Description = "Unknown error.";
                    break;
            }
        }

        private void SetData(object data)
        {
            Data = data;
        }

        #endregion
    }
}
