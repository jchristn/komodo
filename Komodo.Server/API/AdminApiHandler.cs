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
        static async Task AdminApiHandler(HttpContext ctx)
        {
            string header = "[Komodo.Server.AdminApiHandler] " + ctx.Request.SourceIp + ":" + ctx.Request.SourcePort + " ";

            _Logging.Info(header + "admin API requested: " + ctx.Request.Method + " " + ctx.Request.RawUrlWithoutQuery);

            switch (ctx.Request.Method)
            {
                case HttpMethod.GET:
                    if (ctx.Request.RawUrlWithoutQuery.Equals("/admin/connections"))
                    {
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(Common.SerializeJson(_Conn.GetActiveConnections(), true));
                        return;
                    }

                    if (ctx.Request.RawUrlWithoutQuery.Equals("/admin/disks"))
                    {
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(Common.SerializeJson(DiskInfo.GetAllDisks(), true));
                        return;
                    }
                    break;
            }

            _Logging.Warn(header + "unknown endpoint " + ctx.Request.Method.ToString() + " " + ctx.Request.RawUrlWithoutQuery);
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.Send(new ErrorResponse(400, "Unknown endpoint.", null, null).ToJson(true));
            return;
        }
    }
}