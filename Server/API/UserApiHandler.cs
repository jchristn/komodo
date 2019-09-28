using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        static async Task UserApiHandler(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";

            switch (md.Http.Request.Method)
            {
                case HttpMethod.GET: 
                    if (md.Http.Request.RawUrlWithoutQuery.Equals("/indices"))
                    {
                        await GetIndices(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlEntries.Count == 1)
                    {
                        await GetIndex(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlEntries.Count == 2)
                    {
                        if (md.Http.Request.RawUrlEntries[1].ToLower().Equals("stats"))
                        {
                            await GetIndexStats(md);
                            return;
                        }

                        await GetIndexDocument(md);
                        return;
                    } 
                    break; 

                case HttpMethod.PUT:
                    if (md.Http.Request.RawUrlEntries.Count == 1)
                    {
                        await PutSearchIndex(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlEntries.Count == 2
                        && md.Http.Request.RawUrlEntries[1].Equals("enumerate"))
                    {
                        await PutEnumerateIndex(md);
                        return;
                    } 
                    break;

                case HttpMethod.POST:
                    if (md.Http.Request.RawUrlWithoutQuery.Equals("/_parse"))
                    {
                        await PostParsePreview(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlWithoutQuery.Equals("/_index"))
                    {
                        await PostIndexPreview(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlWithoutQuery.Equals("/indices"))
                    {
                        await PostIndices(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlEntries.Count == 1)
                    {
                        await PostIndexDoc(md);
                        return;
                    }
                    break;

                case HttpMethod.DELETE:

                    if (md.Http.Request.RawUrlEntries.Count == 1)
                    {
                        await DeleteIndex(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlEntries.Count == 2)
                    {
                        await DeleteIndexDoc(md);
                        return;
                    }
                    
                    break;  
            }
             
            _Logging.Warn(header + "UserApiHandler unknown URL " + md.Http.Request.Method + " " + md.Http.Request.RawUrlWithoutQuery);
            md.Http.Response.StatusCode = 404;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(new ErrorResponse(404, "Unknown endpoint.", null).ToJson(true));
            return;
        }
    }
}