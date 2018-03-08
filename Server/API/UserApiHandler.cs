using System;
using System.Net;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using KomodoCore;

namespace KomodoServer
{
    public partial class KomodoServer
    {
        static HttpResponse UserApiHandler(RequestMetadata md)
        {
            #region Process

            switch (md.CurrRequest.Method.ToLower())
            {
                case "get":
                    #region get

                    if (WatsonCommon.UrlEqual(md.CurrRequest.RawUrlWithoutQuery, "/indices", false))
                    {
                        return GetIndices(md);
                    }

                    return GetIndex(md);
                   
                #endregion

                case "put":
                    #region put

                    return PutSearchIndex(md);

                #endregion

                case "post":
                    #region post

                    if (WatsonCommon.UrlEqual(md.CurrRequest.RawUrlWithoutQuery, "/_parse", false))
                    {
                        return PostParsePreview(md);
                    }

                    if (WatsonCommon.UrlEqual(md.CurrRequest.RawUrlWithoutQuery, "/_index", false))
                    {
                        return PostIndexPreview(md);
                    }

                    if (WatsonCommon.UrlEqual(md.CurrRequest.RawUrlWithoutQuery, "/indices", false))
                    {
                        return PostIndices(md);
                    }

                    return PostIndexDoc(md); 

                #endregion

                case "delete":
                    #region delete

                    return DeleteIndex(md);
                     
                    #endregion

                case "head":
                    #region head
                    
                    break;

                #endregion

                default:
                    #region default

                    _Logging.Log(LoggingModule.Severity.Warn, "UserApiHandler unknown HTTP method '" + md.CurrRequest.Method + "'");
                    return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unsupported method.", null).ToJson(true), true);

                    #endregion
            }

            #endregion

            #region Unknown-URL

            _Logging.Log(LoggingModule.Severity.Warn, "UserApiHandler unknown URL " + md.CurrRequest.Method + " " + md.CurrRequest.RawUrlWithoutQuery);
            return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                new ErrorResponse(404, "Unknown endpoint.", null).ToJson(true), true);

            #endregion
        }
    }
}