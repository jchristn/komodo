using System;
using System.Collections.Generic;
using System.Text;
using Komodo;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Error response to an API request.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int StatusCode = 0;

        /// <summary>
        /// Human-readable text.
        /// </summary>
        public string Text = null;

        /// <summary>
        /// Exception.
        /// </summary>
        public Exception ExceptionData = null;

        /// <summary>
        /// Related data.
        /// </summary>
        public object Data = null;

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="text">Human-readable text.</param>
        /// <param name="e">Exception.</param>
        /// <param name="data">Related data.</param>
        public ErrorResponse(int statusCode, string text, Exception e, object data)
        {
            StatusCode = statusCode;
            Text = text;
            ExceptionData = e;
            Data = data;
        }

        /// <summary>
        /// Provide a JSON-serialized representation of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty printing.</param>
        /// <returns>String containing JSON.</returns>
        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }
    }
}
