using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Komodo.Core;
using Komodo.Server.Classes;

namespace Komodo.Server
{
    public partial class KomodoServer
    {
        public static HttpResponse GetIndices(RequestMetadata md)
        {
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

            return new HttpResponse(md.Http, true, 200, null, "application/json", Common.SerializeJson(ret, true), true);
            
            #endregion
        }
    }
}