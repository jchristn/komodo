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
        public static HttpResponse DeleteIndex(RequestMetadata md)
        {
            #region Check-Input

            if (md.Http.RawUrlEntries.Count < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DeleteIndex raw URL entries does not contain exactly one item");
                return new HttpResponse(md.Http, false, 400, null, "application/json",
                    new ErrorResponse(400, "URL must contain exactly one element.", null).ToJson(true), true);
            }

            #endregion

            #region Get-Values
             
            string indexName = md.Http.RawUrlEntries[0];             
            Index currIndex = _Index.GetIndexByName(indexName); 
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DeleteIndex unable to retrieve index " + indexName);
                return new HttpResponse(md.Http, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index.", null).ToJson(true), true);
            }

            _Index.RemoveIndex(indexName, md.Params.Cleanup);
            return new HttpResponse(md.Http, true, 204, null, "application/json", null, true);
            
            #endregion
        }
    }
}