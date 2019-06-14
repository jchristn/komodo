using System;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        static HttpResponse UserApiHandler(RequestMetadata md)
        {
            #region Process

            switch (md.Http.Method)
            {
                case HttpMethod.GET: 
                    if (WatsonCommon.UrlEqual(md.Http.RawUrlWithoutQuery, "/indices", false))
                    {
                        return GetIndices(md);
                    }
                    
                    if (md.Http.RawUrlEntries.Count == 1) return GetIndex(md);

                    if (md.Http.RawUrlEntries.Count == 2)
                    {
                        if (md.Http.RawUrlEntries[1].ToLower().Equals("stats")) return GetIndexStats(md); 

                        return GetIndexDocument(md);
                    } 
                    break; 

                case HttpMethod.PUT:
                    if (md.Http.RawUrlEntries.Count == 1) return PutSearchIndex(md);
                    if (md.Http.RawUrlEntries.Count == 2
                        && md.Http.RawUrlEntries[1].Equals("enumerate"))
                    {
                        return PutEnumerateIndex(md);
                    } 
                    break;

                case HttpMethod.POST:
                    if (WatsonCommon.UrlEqual(md.Http.RawUrlWithoutQuery, "/_parse", false))
                    {
                        return PostParsePreview(md);
                    }

                    if (WatsonCommon.UrlEqual(md.Http.RawUrlWithoutQuery, "/_index", false))
                    {
                        return PostIndexPreview(md);
                    }

                    if (WatsonCommon.UrlEqual(md.Http.RawUrlWithoutQuery, "/indices", false))
                    {
                        return PostIndices(md);
                    }

                    if (md.Http.RawUrlEntries.Count == 1) return PostIndexDoc(md);
                    break;

                case HttpMethod.DELETE: 

                    if (md.Http.RawUrlEntries.Count == 1) return DeleteIndex(md);
                    if (md.Http.RawUrlEntries.Count == 2) return DeleteIndexDoc(md); 
                    break; 

                case HttpMethod.HEAD: 
                    break; 

                default: 
                    _Logging.Log(LoggingModule.Severity.Warn, "UserApiHandler unknown HTTP method '" + md.Http.Method + "'");
                    return new HttpResponse(md.Http, 400, null, "application/json",
                        Encoding.UTF8.GetBytes(new ErrorResponse(400, "Unsupported method.", null).ToJson(true)));
            }

            #endregion

            #region Unknown-URL

            _Logging.Log(LoggingModule.Severity.Warn, "UserApiHandler unknown URL " + md.Http.Method + " " + md.Http.RawUrlWithoutQuery);
            return new HttpResponse(md.Http, 404, null, "application/json",
                Encoding.UTF8.GetBytes(new ErrorResponse(404, "Unknown endpoint.", null).ToJson(true)));

            #endregion
        }
    }
}