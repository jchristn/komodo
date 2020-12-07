using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RestWrapper;

namespace Komodo.Sdk
{
    /// <summary>
    /// Komodo exception.
    /// </summary>
    public class KomodoException : Exception
    {
        #region Public-Members

        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int StatusCode = 0;

        /// <summary>
        /// Response data from Komodo.
        /// </summary>
        public byte[] ResponseData = null;

        /// <summary>
        /// The type of exception.
        /// </summary>
        public ExceptionType Type = ExceptionType.Unknown;

        #endregion

        #region Constructors-and-Factories

        internal static KomodoException FromRestResponse(RestResponse resp)
        {
            KomodoException e = new KomodoException();

            if (resp == null)
            {
                e.StatusCode = 0;
                e.ResponseData = null;
                e.Type = ExceptionType.CannotConnect;
                return e;
            }

            if (resp.StatusCode >= 500)
            {
                e.StatusCode = resp.StatusCode;
                if (resp.ContentLength > 0) e.ResponseData = KomodoCommon.StreamToBytes(resp.Data);
                else e.ResponseData = null;
                e.Type = ExceptionType.InternalServerError;
                return e;
            }

            if (resp.StatusCode == 413)
            {
                e.StatusCode = resp.StatusCode;
                if (resp.ContentLength > 0) e.ResponseData = KomodoCommon.StreamToBytes(resp.Data);
                else e.ResponseData = null;
                e.Type = ExceptionType.PayloadTooLarge;
                return e;
            }

            if (resp.StatusCode == 409)
            {
                e.StatusCode = resp.StatusCode;
                if (resp.ContentLength > 0) e.ResponseData = KomodoCommon.StreamToBytes(resp.Data);
                else e.ResponseData = null;
                e.Type = ExceptionType.Conflict;
                return e;
            }

            if (resp.StatusCode == 404)
            {
                e.StatusCode = resp.StatusCode;
                if (resp.ContentLength > 0) e.ResponseData = KomodoCommon.StreamToBytes(resp.Data);
                else e.ResponseData = null;
                e.Type = ExceptionType.NotFound;
                return e;
            }

            if (resp.StatusCode == 401)
            {
                e.StatusCode = resp.StatusCode;
                if (resp.ContentLength > 0) e.ResponseData = KomodoCommon.StreamToBytes(resp.Data);
                else e.ResponseData = null;
                e.Type = ExceptionType.Unauthorized;
                return e;
            }

            if (resp.StatusCode == 400)
            {
                e.StatusCode = resp.StatusCode;
                if (resp.ContentLength > 0) e.ResponseData = KomodoCommon.StreamToBytes(resp.Data);
                else e.ResponseData = null;
                e.Type = ExceptionType.BadRequest;
                return e;
            }

            return null;
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

    /// <summary>
    /// The type of Komodo exception.
    /// </summary>
    public enum ExceptionType
    {
        /// <summary>
        /// Unknown exception type.
        /// </summary>
        Unknown,
        /// <summary>
        /// Could not connect to the server or could not retrieve a response.
        /// </summary> 
        CannotConnect,
        /// <summary>
        /// A server-side error was encountered.
        /// </summary>
        InternalServerError,
        /// <summary>
        /// Request data too large.
        /// </summary>
        PayloadTooLarge,
        /// <summary>
        /// A conflict exists, for example, attempting to write an object using a key that already exists.
        /// </summary>
        Conflict,
        /// <summary>
        /// The requested resource was not found.
        /// </summary>
        NotFound,
        /// <summary>
        /// You were not authorized to perform the request.
        /// </summary>
        Unauthorized,
        /// <summary>
        /// Your request was malformed.
        /// </summary>
        BadRequest
    }
}
