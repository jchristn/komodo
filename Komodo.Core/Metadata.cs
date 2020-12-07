using System;
using System.Collections.Generic;
using System.Data;
using Watson.ORM.Core;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Key-value pair configuration-related data.
    /// </summary>
    [Table("metadata")]
    public class Metadata
    {
        #region Public-Members

        /// <summary>
        /// Database row ID.
        /// </summary>
        [JsonIgnore]
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; } = 0;

        /// <summary>
        /// Key.
        /// </summary>
        [JsonProperty(Order = -1)]
        [Column("configkey", false, DataTypes.Nvarchar, 64, false)]
        public string Key { get; set; } = null;

        /// <summary>
        /// Value associated with the specified key.
        /// </summary>
        [Column("configval", false, DataTypes.Nvarchar, 2048, true)]
        public string Value { get; set; } = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Metadata()
        {

        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="val">Value associated with the specified key.</param>
        public Metadata(string key, string val)
        {
            if (String.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            Key = key;
            Value = val;
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
            return Common.SerializeJson(this, pretty);
        }

        #endregion
    }
}
