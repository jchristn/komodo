using System;
using System.Collections.Generic;
using System.Text;
using BlobHelper;
using Newtonsoft.Json;

namespace Komodo.Classes
{
    /// <summary>
    /// Storage settings for storing objects.
    /// </summary>
    public class StorageSettings
    {
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
    }
}
