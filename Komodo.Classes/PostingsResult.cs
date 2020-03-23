using System;
using System.Collections.Generic;
using System.Text;
using Komodo.Classes;

namespace Komodo.Classes
{
    /// <summary>
    /// Results from generating postings.
    /// </summary>
    public class PostingsResult
    {
        /// <summary>
        /// Indicates if the postings generator was successful.
        /// </summary>
        public bool Success = false;

        /// <summary>
        /// Start and end timestamps.
        /// </summary>
        public Timestamps Time = new Timestamps();

        /// <summary>
        /// Postings.
        /// </summary>
        public List<Posting> Postings = new List<Posting>();

        /// <summary>
        /// Terms.
        /// </summary>
        public List<string> Terms = new List<string>();

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public PostingsResult()
        {

        }
    }
}
