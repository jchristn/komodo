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

            switch (md.CurrRequest.Method)
            {
                case HttpMethod.GET: 
                    if (WatsonCommon.UrlEqual(md.CurrRequest.RawUrlWithoutQuery, "/indices", false))
                    {
                        return GetIndices(md);
                    }
                    
                    if (md.CurrRequest.RawUrlEntries.Count == 1) return GetIndex(md);

                    if (md.CurrRequest.RawUrlEntries.Count == 2)
                    {
                        if (md.CurrRequest.RawUrlEntries[1].ToLower().Equals("stats")) return GetIndexStats(md); 

                        return GetIndexDocument(md);
                    } 
                    break; 

                case HttpMethod.PUT:
                    if (md.CurrRequest.RawUrlEntries.Count == 1) return PutSearchIndex(md);
                    if (md.CurrRequest.RawUrlEntries.Count == 2
                        && md.CurrRequest.RawUrlEntries[1].Equals("enumerate"))
                    {
                        return PutEnumerateIndex(md);
                    } 
                    break;

                case HttpMethod.POST:
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

                    if (md.CurrRequest.RawUrlEntries.Count == 1) return PostIndexDoc(md);
                    break;

                case HttpMethod.DELETE: 

                    if (md.CurrRequest.RawUrlEntries.Count == 1) return DeleteIndex(md);
                    if (md.CurrRequest.RawUrlEntries.Count == 2) return DeleteIndexDoc(md); 
                    break; 

                case HttpMethod.HEAD: 
                    break; 

                default: 
                    _Logging.Log(LoggingModule.Severity.Warn, "UserApiHandler unknown HTTP method '" + md.CurrRequest.Method + "'");
                    return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                        new ErrorResponse(400, "Unsupported method.", null).ToJson(true), true); 
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