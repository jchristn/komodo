using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DatabaseWrapper;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Watson.ORM;
using Watson.ORM.Core;
using Komodo.Classes;  
using Komodo.Server.Classes;
using Common = Komodo.Classes.Common;
using Index = Komodo.Classes.Index;

namespace Komodo.Server
{
    public partial class Program
    {
        private static async Task GetIndex(RequestMetadata md)
        {
            string header = "[Komodo.Server] " + md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " GetIndex ";

            string name = md.Http.Request.RawUrlEntries[0];

            DbExpression e = new DbExpression(
                _ORM.GetColumnName<Index>(nameof(Index.Name)), 
                DbOperators.Equals, 
                name);

            Index idx = _ORM.SelectFirst<Index>(e);
            
            if (idx == null)
            {
                _Logging.Warn(header + "unable to find index " + name);
                md.Http.Response.StatusCode = 404;
                md.Http.Response.ContentType = "application/json";
                await md.Http.Response.Send(new ErrorResponse(404, "Unknown index.", null, null).ToJson(true));
                return;
            }

            md.Http.Response.StatusCode = 200;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(Common.SerializeJson(idx, md.Params.Pretty));
            return; 
        }
    }
}