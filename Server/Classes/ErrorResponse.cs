using System;
using KomodoCore;

namespace KomodoServer
{
    public class ErrorResponse
    {
        #region Public-Members

        public bool Success;
        public int HttpStatus;
        public string HttpText;
        public string Text;
        public object Data;

        #endregion

        #region Constructors-and-Factories

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

        public string ToJson(bool pretty)
        {
            return Common.SerializeJson(this, pretty);
        }

        #endregion
    }
}
