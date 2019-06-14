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
        public static HttpResponse GetIndex(RequestMetadata md)
        {
            #region Check-Input

            if (md.Http.RawUrlEntries.Count < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndex raw URL entries does not contain exactly one item");
                return new HttpResponse(md.Http, 400, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(400, "URL must contain exactly one element.", null).ToJson(true)));
            }

            #endregion

            #region Get-Values
            
            string indexName = md.Http.RawUrlEntries[0];
            Index ret = _Index.GetIndexByName(indexName);
            
            if (ret == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndex unable to retrieve index " + indexName);
                return new HttpResponse(md.Http, 404, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(404, "Unknown index.", null).ToJson(true)));
            }

            return new HttpResponse(md.Http, 200, null, "application/json",
                Encoding.UTF8.GetBytes(Common.SerializeJson(ret, md.Params.Pretty)));
            
            #endregion
        }
    }
}