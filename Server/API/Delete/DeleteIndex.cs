using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using SyslogLogging;
using WatsonWebserver;
using KomodoCore;
using RestWrapper;

namespace KomodoServer
{
    public partial class KomodoServer
    {
        public static HttpResponse DeleteIndex(RequestMetadata md)
        {
            #region Check-Input

            if (md.CurrRequest.RawUrlEntries.Count < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DeleteIndex raw URL entries does not contain exactly one item");
                return new HttpResponse(md.CurrRequest, false, 400, null, "application/json",
                    new ErrorResponse(400, "URL must contain exactly one element.", null).ToJson(true), true);
            }

            #endregion

            #region Get-Values
             
            string indexName = md.CurrRequest.RawUrlEntries[0];
            bool cleanup = false;
            if (md.CurrRequest.QuerystringEntries.ContainsKey("cleanup")) cleanup = Common.IsTrue(md.CurrRequest.QuerystringEntries["cleanup"]);
             
            Index currIndex = _Index.GetIndexByName(indexName); 
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DeleteIndex unable to retrieve index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index.", null).ToJson(true), true);
            }

            _Index.RemoveIndex(indexName, cleanup);
            return new HttpResponse(md.CurrRequest, true, 204, null, "application/json", null, true);
            
            #endregion
        }
    }
}