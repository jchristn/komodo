using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using Komodo.Classes;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class Program
    {
        static async Task UserApiHandler(RequestMetadata md)
        {
            string header = "[Komodo.Server.UserApiHandler] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";

            if (md.Params.Metadata)
            {
                md.Http.Response.StatusCode = 200;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(Common.SerializeJson(md, true));
                return;
            }

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
                    if (md.Http.Request.RawUrlEntries.Count == 1
                        && !md.Params.Enumerate)
                    {
                        await PutSearchIndex(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlEntries.Count == 1
                        && md.Params.Enumerate)
                    {
                        await PutEnumerateIndex(md);
                        return;
                    }
                    break;

                case HttpMethod.POST:
                    if (md.Http.Request.RawUrlWithoutQuery.Equals("/_parse"))
                    {
                        await PostParse(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlWithoutQuery.Equals("/indices"))
                    {
                        await PostIndices(md);
                        return;
                    }

                    if (md.Http.Request.RawUrlEntries.Count == 1
                        || md.Http.Request.RawUrlEntries.Count == 2)
                    {
                        await PostIndexDocument(md);
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
                        await DeleteIndexDocument(md);
                        return;
                    }

                    break;
            }

            _Logging.Warn(header + "unknown URL " + md.Http.Request.Method + " " + md.Http.Request.RawUrlWithoutQuery);
            md.Http.Response.StatusCode = 404;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(new ErrorResponse(404, "Unknown endpoint.", null, null).ToJson(true));
            return;
        }
    }
}