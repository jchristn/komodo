﻿using System;
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
        public static HttpResponse GetIndexStats(RequestMetadata md)
        { 
            #region Process
             
            string indexName = md.Http.RawUrlEntries[0];  
            Index currIndex = _Index.GetIndexByName(indexName);
            if (currIndex == null)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexStats unable to retrieve index " + indexName);
                return new HttpResponse(md.Http, 404, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(404, "Unknown index.", null).ToJson(true)));
            }

            IndexStats stats = _Index.GetIndexStats(indexName);
            if (stats == null)
            { 
                _Logging.Log(LoggingModule.Severity.Warn, "GetIndexStats unable to retrieve statistics for index " + indexName);
                return new HttpResponse(md.Http, 500, null, "application/json",
                    Encoding.UTF8.GetBytes(new ErrorResponse(500, "Unable to retrieve statistics for index '" + indexName + "'.", null).ToJson(true)));
            }

            return new HttpResponse(md.Http, 200, null, "application/json",
                Encoding.UTF8.GetBytes(Common.SerializeJson(stats, true)));
            
            #endregion
        }
    }
}