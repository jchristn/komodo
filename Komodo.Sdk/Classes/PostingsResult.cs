﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Komodo.Sdk.Classes
{
    /// <summary>
    /// Results from generating postings.
    /// </summary>
    public class PostingsResult
    {
        #region Public-Members

        /// <summary>
        /// Indicates if the postings generator was successful.
        /// </summary>
        [JsonProperty(Order = -1)]
        public bool Success = false;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// Normalized parse result.
        /// </summary>
        [JsonProperty(Order = 990)]
        public ParseResult Normalized = new ParseResult();

        /// <summary>
        /// Postings options.
        /// </summary>
        [JsonProperty(Order = 991)]
        public PostingsOptions PostingsOptions = null;

        /// <summary>
        /// Terms.
        /// </summary>
        [JsonProperty(Order = 992)]
        public List<string> Terms = new List<string>();

        /// <summary>
        /// Postings.
        /// </summary>
        [JsonProperty(Order = 993)]
        public List<Posting> Postings = new List<Posting>();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public PostingsResult()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Return a JSON string of this object.
        /// </summary>
        /// <param name="pretty">Enable or disable pretty print.</param>
        /// <returns>JSON string.</returns>
        public string ToJson(bool pretty)
        {
            return KomodoCommon.SerializeJson(this, pretty);
        }

        #endregion
    }
}
