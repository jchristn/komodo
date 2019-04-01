using System;
using Komodo.Core;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Object used when conveying an error in an API response.
    /// </summary>
    public class ErrorResponse
    {
        #region Public-Members

        /// <summary>
        /// Indicates whether or not the request succeeded.
        /// </summary>
        public bool Success;

        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int HttpStatus;

        /// <summary>
        /// Text description of the HTTP status code.
        /// </summary>
        public string HttpText;

        /// <summary>
        /// Human-readable text.
        /// </summary>
        public string Text;

        /// <summary>
        /// Error-related data.
        /// </summary>
        public object Data;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="status">HTTP status code.</param>
        /// <param name="text">Human-readable text.</param>
        /// <param name="data">Error-related data.</param>
        public ErrorResponse(
            int status,
            string text,
            object data)
        {
            Data = data;
            HttpStatus = status;
            Text = text;

            // set http_status
            switch (HttpStatus)
            {
                case 200:
                    HttpText = "OK";
                    break;

                case 201:
                    HttpText = "Created";
                    break;

                case 301:
                    HttpText = "Moved Permanently";
                    break;

                case 302:
                    HttpText = "Moved Temporarily";
                    break;

                case 304:
                    HttpText = "Not Modified";
                    break;

                case 400:
                    HttpText = "Bad Request";
                    break;

                case 401:
                    HttpText = "Unauthorized";
                    break;

                case 403:
                    HttpText = "Forbidden";
                    break;

                case 404:
                    HttpText = "Not Found";
                    break;

                case 405:
                    HttpText = "Method Not Allowed";
                    break;

                case 409:
                    HttpText = "Conflict";
                    break;

                case 423:
                    HttpText = "Locked";
                    break;

                case 429:
                    HttpText = "Too Many Requests";
                    break;

                case 500:
                    HttpText = "Internal Server Error";
                    break;

                case 501:
                    HttpText = "Not Implemented";
                    break;

                case 503:
                    HttpText = "Service Unavailable";
                    break;

                default:
                    HttpText = "Unknown";
                    break;
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Serialize the object to JSON.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
