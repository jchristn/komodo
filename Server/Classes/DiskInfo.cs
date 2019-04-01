using System;
using System.Collections.Generic;
using System.IO;
using SyslogLogging;

namespace Komodo.Server.Classes
{
    /// <summary>
    /// Information about a visible disk.
    /// </summary>
    public class DiskInfo
    {
        #region Public-Members

        /// <summary>
        /// The name of the disk.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The volume label.
        /// </summary>
        public string VolumeLabel { get; set; }

        /// <summary>
        /// The format of the drive.
        /// </summary>
        public string DriveFormat { get; set; }

        /// <summary>
        /// The type of drive.
        /// </summary>
        public string DriveType { get; set; }

        /// <summary>
        /// Total size of the drive, in bytes.
        /// </summary>
        public long TotalSizeBytes { get; set; }

        /// <summary>
        /// Total size of the drive, in gigabytes.
        /// </summary>
        public long TotalSizeGigabytes { get; set; }

        /// <summary>
        /// Available capacity of the drive, in bytes.
        /// </summary>
        public long AvailableSizeBytes { get; set; }

        /// <summary>
        /// Available capacity of the drive, in gigabytes.
        /// </summary>
        public long AvailableSizeGigabytes { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public DiskInfo()
        {

        }

        /// <summary>
        /// Retrieve information about all visible disks.
        /// </summary>
        /// <returns>List of disk information objects.</returns>
        public static List<DiskInfo> GetAllDisks()
        {
            List<DiskInfo> ret = new List<DiskInfo>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                try
                {
                    DiskInfo curr = new DiskInfo();
                    curr.Name = drive.Name;

                    curr.VolumeLabel = drive.VolumeLabel;
                    curr.DriveFormat = drive.DriveFormat;
                    curr.DriveType = drive.DriveType.ToString();
                    curr.TotalSizeBytes = drive.TotalSize;
                    curr.TotalSizeGigabytes = drive.TotalSize / (1024 * 1024 * 1024);
                    curr.AvailableSizeBytes = drive.TotalFreeSpace;
                    curr.AvailableSizeGigabytes = drive.TotalFreeSpace / (1024 * 1024 * 1024);

                    ret.Add(curr);
                }
                catch (IOException)
                {
                    // do nothing, disk likely unvailable
                }
            }

            return ret;
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
