using System;
using System.Collections.Generic;
using System.IO;
using SyslogLogging;
using KomodoCore;
using WatsonWebserver;

namespace KomodoServer
{
    public class RequestMetadata
    {
        #region Public-Members

        public HttpRequest CurrRequest { get; set; }
        public UserMaster CurrUser { get; set; }
        public ApiKey CurrApiKey { get; set; }
        public ApiKeyPermission CurrPerm { get; set; }

        #endregion

        #region Constructors-and-Factories

        public RequestMetadata()
        {

        }
        
        #endregion
    }
}
