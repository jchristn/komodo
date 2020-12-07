using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using Komodo;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    partial class Program
    {
        static async Task AdminApiHandler(HttpContext ctx)
        {
            string header = "[Komodo.Server.AdminApiHandler] " + ctx.Request.Source.IpAddress + ":" + ctx.Request.Source.Port + " ";

            _Logging.Info(header + "admin API requested: " + ctx.Request.Method + " " + ctx.Request.Url.RawWithoutQuery);

            switch (ctx.Request.Method)
            {
                case HttpMethod.GET:  
                    if (ctx.Request.Url.RawWithoutQuery.Equals("/admin/disks"))
                    {
                        ctx.Response.StatusCode = 200;
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.Send(Common.SerializeJson(DiskInfo.GetAllDisks(), true));
                        return;
                    }
                    break;
            }

            _Logging.Warn(header + "unknown endpoint " + ctx.Request.Method.ToString() + " " + ctx.Request.Url.RawWithoutQuery);
            ctx.Response.StatusCode = 400;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.Send(new ErrorResponse(400, "Unknown endpoint.", null, null).ToJson(true));
            return;
        }
    }
}