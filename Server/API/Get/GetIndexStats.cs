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
        public static HttpResponse GetIndexStats(RequestMetadata md)
        { 
            #region Process
             
            string indexName = md.CurrRequest.RawUrlEntries[0];  
            Index currIndex = _Index.GetIndexByName(indexName);
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexStats unable to retrieve index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 404, null, "application/json",
                    new ErrorResponse(404, "Unknown index.", null).ToJson(true), true);
            }

            IndexStats stats = _Index.GetIndexStats(indexName);
            if (stats == null)
            { 
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexStats unable to retrieve statistics for index " + indexName);
                return new HttpResponse(md.CurrRequest, false, 500, null, "application/json",
                    new ErrorResponse(500, "Unable to retrieve statistics for index '" + indexName + "'.", null).ToJson(true), true);
            }
             
            return new HttpResponse(md.CurrRequest, true, 200, null, "application/json", Common.SerializeJson(stats, true), true); 
            
            #endregion
        }
    }
}