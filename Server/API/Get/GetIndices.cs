using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        private static async Task GetIndices(RequestMetadata md)
        {
            string header = md.Http.Request.SourceIp + ":" + md.Http.Request.SourcePort + " ";

            #region Get-Values

            List<Index> indices = _Index.GetIndices();
            List<string> ret = new List<string>();

            if (indices != null && indices.Count > 0)
            {
                foreach (Index curr in indices)
                {
                    ret.Add(curr.IndexName);
                }
            }

            md.Http.Response.StatusCode = 200;
            md.Http.Response.ContentType = "application/json";
            await md.Http.Response.Send(Common.SerializeJson(ret, md.Params.Pretty));
            return;

            #endregion
        }
    }
}