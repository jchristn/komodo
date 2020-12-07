using System;
using System.Collections.Generic;
using System.Text;
using BlobHelper;
using Newtonsoft.Json;

namespace Komodo
{
    /// <summary>
    /// Storage settings for storing objects.
    /// </summary>
    public class StorageSettings
    {
        #region Public-Members

        /// <summary>
        /// Amazon S3 settings.
        /// </summary>
        public AwsSettings Aws = null;

        /// <summary>
        /// Microsoft Azure BLOB storage settings.
        /// </summary>
        public AzureSettings Azure = null;

        /// <summary>
        /// Local filesystem storage settings.
        /// </summary>
        public DiskSettings Disk = null;

        /// <summary>
        /// Kvpbase storage server settings.
        /// </summary>
        public KvpbaseSettings Kvpbase = null;

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public StorageSettings()
        {

        }

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="aws">AWS settings.</param>
        public StorageSettings(AwsSettings aws)
        {
            if (aws == null) throw new ArgumentNullException(nameof(aws));

            Aws = aws;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="azure">Azure settings.</param>
        public StorageSettings(AzureSettings azure)
        {
            if (azure == null) throw new ArgumentNullException(nameof(azure));

            Azure = azure;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="disk">Disk settings.</param>
        public StorageSettings(DiskSettings disk)
        {
            if (disk == null) throw new ArgumentNullException(nameof(disk));

            Disk = disk;
        }

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="kvpbase">Kvpbase settings.</param>
        public StorageSettings(KvpbaseSettings kvpbase)
        {
            if (kvpbase == null) throw new ArgumentNullException(nameof(kvpbase));

            Kvpbase = kvpbase;
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
