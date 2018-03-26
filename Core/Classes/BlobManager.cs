using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks; 
using SyslogLogging;
using RestWrapper;
using DatabaseWrapper;
using KvpbaseSDK;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace KomodoCore
{
    /// <summary>
    /// Library used to manage storage of binary large objects (BLOBs) on the local filesystem or on cloud storage.
    /// </summary>
    public class BlobManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private IndexOptions.StorageSettings _Settings;
        private LoggingModule _Logging;
        private string _StorageType;

        private IAmazonS3 _S3Client;
        private Amazon.Runtime.BasicAWSCredentials _S3Credentials;
        private Amazon.RegionEndpoint _S3Region;

        private StorageCredentials _AzureCredentials;
        private CloudStorageAccount _AzureAccount;
        private CloudBlobClient _AzureBlobClient;
        private CloudBlobContainer _AzureContainer;

        private Client _Kvpbase;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the BlobManager.
        /// </summary>
        /// <param name="options">IndexOptions containing storage configuration.</param>
        /// <param name="logging">LoggingModule.</param>
        public BlobManager(IndexOptions.StorageSettings settings, LoggingModule logging)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings)); 
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            _Settings = settings;
            _Logging = logging;

            InitializeClients();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Delete a BLOB by its ID.
        /// </summary>
        /// <param name="id">ID of the BLOB.</param>
        /// <returns>True if successful.</returns>
        public bool Delete(string id)
        { 
            if (String.IsNullOrEmpty(id)) 
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Delete invalid value or null for ID");
                return false;
            }
             
            bool success = false;

            switch (_StorageType)
            {
                case "kvpbase": 
                    success = KvpbaseDelete(id);
                    break;

                case "disk":
                    success = DiskDelete(id);
                    break;

                case "aws":
                    success = S3Delete(id);
                    break;

                case "azure":
                    success = AzureDelete(id);
                    break;
            }

            if (!success)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Delete failure condition indicated from external storage");
                return false;
            }
             
            return true;
        }

        /// <summary>
        /// Retrieve a BLOB by its ID.
        /// </summary>
        /// <param name="id">ID of the BLOB.</param>
        /// <param name="data">Byte array containing BLOB data.</param>
        /// <returns>True if successful.</returns>
        public bool Get(string id, out byte[] data)
        {
            data = null;
             
            if (String.IsNullOrEmpty(id))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Get invalid value or null supplied for ID");
                return false;
            }
                
            switch (_StorageType)
            {
                case "kvpbase":
                    return KvpbaseGet(id, out data);

                case "disk":
                    return DiskGet(id, out data);

                case "aws":
                    return S3Get(id, out data);

                case "azure":
                    return AzureGet(id, out data); 
            }

            return false;
        }

        /// <summary>
        /// Write a BLOB.
        /// </summary>
        /// <param name="id">ID of the BLOB.</param>
        /// <param name="base64">True of the supplied data is a string containing Base64-encoded data.</param>
        /// <param name="data">BLOB data.</param>
        /// <returns>True if successful.</returns>
        public bool Write(
            string id,   
            bool base64,
            object data)
        {
            if (String.IsNullOrEmpty(id))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Write no ID supplied");
                return false;
            }

            byte[] byteData;
              
            if (base64 && (data is string))
            {
                byteData = Common.Base64ToBytes((string)data);
            }
            else if (data is byte[])
            {
                byteData = (byte[])data;
            }
            else if (data is string)
            {
                byteData = Encoding.UTF8.GetBytes((string)data);
            }
            else
            {
                byteData = Common.ObjectToBytes(data);
            }
               
            bool success = false;
            string url = "";

            switch (_StorageType)
            {
                case "kvpbase":
                    success = _Kvpbase.CreateObjectWithName(null, id, "application/octet-stream", byteData, out url);
                    break;

                case "disk": 
                    success = DiskWrite(id, byteData);
                    url = DiskGenerateUrl(id);
                    break;

                case "aws":
                    success = S3Write(id, byteData);
                    url = S3GenerateUrl(id);
                    break;

                case "azure":
                    success = AzureWrite(id, byteData);
                    url = AzureGenerateUrl(id);
                    break; 
            }
             
            if (success)
            { 
                _Logging.Log(LoggingModule.Severity.Debug, "Write successfully stored BLOB data at " + url);
                return true;
            }
            else
            { 
                return false; 
            } 
        }

        #endregion

        #region Private-Methods

        private void InitializeClients()
        {
            if (_Settings.Disk != null)
            {
                _StorageType = "disk";
                // do nothing
            }
            else if (_Settings.Azure != null)
            {
                _StorageType = "azure";
                _AzureCredentials = new StorageCredentials(_Settings.Azure.AccountName, _Settings.Azure.AccessKey);
                _AzureAccount = new CloudStorageAccount(_AzureCredentials, true);
                _AzureBlobClient = new CloudBlobClient(new Uri(_Settings.Azure.Endpoint), _AzureCredentials);
                _AzureContainer = _AzureBlobClient.GetContainerReference(_Settings.Azure.Container);
            }
            else if (_Settings.Aws != null)
            {
                _StorageType = "aws";

                switch (_Settings.Aws.Region)
                {
                    case "uswest1":
                        _S3Region = Amazon.RegionEndpoint.USWest1;
                        break;
                    case "uswest2":
                        _S3Region = Amazon.RegionEndpoint.USWest2;
                        break;
                    case "useast1":
                        _S3Region = Amazon.RegionEndpoint.USEast1;
                        break;
                    default:
                        throw new ArgumentException("S3 region must be one of uswest1 uswest2 useast1");
                }

                _S3Credentials = new Amazon.Runtime.BasicAWSCredentials(_Settings.Aws.AccessKey, _Settings.Aws.SecretKey);
                _S3Client = new AmazonS3Client(_S3Credentials, _S3Region);
            }
            else if (_Settings.Kvpbase != null)
            {
                _StorageType = "kvpbase";
                _Kvpbase = new Client(_Settings.Kvpbase.UserGuid, _Settings.Kvpbase.ApiKey, _Settings.Kvpbase.Endpoint); 
            }
            else
            {
                throw new ArgumentException("Invalid Storage configuration");
            } 
        }

        #region Private-Kvpbase-Methods

        private bool KvpbaseDelete(string id)
        {
            if (_Kvpbase.DeleteObject(id))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "KvpbaseDelete success response from kvpbase for ID " + id);
                return true;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Warn, "KvpbaseDelete failure response from kvpbase for ID " + id);
                return false;
            } 
        }

        private bool KvpbaseGet(string id, out byte[] data)
        {
            data = null;

            if (_Kvpbase.GetObject(id, out data))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "KvpbaseGet retrieved " + data.Length + " bytes for ID " + id);
                return true;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Warn, "KvpbaseGet failure response from kvpbase for ID " + id);
                return false;
            } 
        }

        private bool KvpbaseWrite(string id, string contentType, byte[] data, out string writtenUrl)
        {
            writtenUrl = null;

            if (_Kvpbase.CreateObjectWithName(null, id, contentType, data, out writtenUrl))
            {
                _Logging.Log(LoggingModule.Severity.Debug, "KvpbaseWrite success response from kvpbase for ID " + id + ": " + writtenUrl);
                return true;
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Warn, "KvpbaseWrite failure response from kvpbase for ID " + id);
                return false;
            } 
        }

        #endregion

        #region Private-Disk-Methods

        private bool DiskDelete(string id)
        {
            try
            {
                File.Delete(DiskGenerateUrl(id));
                return true;
            }
            catch (Exception e)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DiskDelete unable to delete " + id + ": " + e.Message);
                return false;
            }
        }

        private bool DiskGet(string id, out byte[] data)
        {
            data = null;

            try
            {
                data = File.ReadAllBytes(DiskGenerateUrl(id));
                return true;
            }
            catch (Exception e)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DiskGet unable to read " + id + ": " + e.Message);
                return false;
            }
        }

        private bool DiskWrite(string id, byte[] data)
        {
            try
            {
                File.WriteAllBytes(DiskGenerateUrl(id), data);
                return true;
            }
            catch (Exception e)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "DiskWrite unable to write " + id + ": " + e.Message);
                return false;
            }
        }

        private string DiskGenerateUrl(string id)
        {
            return _Settings.Disk.Directory + "/" + id;
        }

        #endregion

        #region Private-S3-Methods

        private bool S3Delete(string id)
        {
            try
            {
                #region Check-for-Null-Values

                if (String.IsNullOrEmpty(id)) return false;

                #endregion

                #region Process

                IAmazonS3 client;
                Amazon.Runtime.BasicAWSCredentials cred = new Amazon.Runtime.BasicAWSCredentials(_Settings.Aws.AccessKey, _Settings.Aws.SecretKey);
                Amazon.RegionEndpoint s3Region;
                switch (_Settings.Aws.Region)
                {
                    case "uswest1":
                        s3Region = Amazon.RegionEndpoint.USWest1;
                        break;
                    case "uswest2":
                        s3Region = Amazon.RegionEndpoint.USWest2;
                        break;
                    case "useast1":
                        s3Region = Amazon.RegionEndpoint.USEast1;
                        break;
                    default:
                        return false;
                }

                using (client = new AmazonS3Client(cred, s3Region))
                {
                    DeleteObjectRequest request = new DeleteObjectRequest
                    {
                        BucketName = _Settings.Aws.Bucket,
                        Key = id
                    };

                    DeleteObjectResponse response = client.DeleteObject(request);
                    int statusCode = (int)response.HttpStatusCode;

                    if (response != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                #endregion
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool S3Get(string id, out byte[] data)
        {
            data = null;

            try
            {
                #region Process

                IAmazonS3 client;
                Amazon.Runtime.BasicAWSCredentials cred = new Amazon.Runtime.BasicAWSCredentials(_Settings.Aws.AccessKey, _Settings.Aws.SecretKey);
                Amazon.RegionEndpoint s3Region;
                switch (_Settings.Aws.Region)
                {
                    case "uswest1":
                        s3Region = Amazon.RegionEndpoint.USWest1;
                        break;
                    case "uswest2":
                        s3Region = Amazon.RegionEndpoint.USWest2;
                        break;
                    case "useast1":
                        s3Region = Amazon.RegionEndpoint.USEast1;
                        break;
                    default:
                        return false;
                }

                using (client = new AmazonS3Client(cred, s3Region))
                {
                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = _Settings.Aws.Bucket,
                        Key = id
                    };

                    using (GetObjectResponse response = client.GetObject(request))
                    using (Stream responseStream = response.ResponseStream)
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        if (response.ContentLength > 0)
                        {
                            // first copy the stream
                            data = new byte[response.ContentLength];
                            byte[] temp = new byte[2];

                            Stream bodyStream = response.ResponseStream;
                            data = Common.StreamToBytes(bodyStream);

                            int statusCode = (int)response.HttpStatusCode;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                #endregion
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool S3Write(string id, byte[] data)
        {
            try
            {
                #region Process

                IAmazonS3 client;
                Amazon.Runtime.BasicAWSCredentials cred = new Amazon.Runtime.BasicAWSCredentials(_Settings.Aws.AccessKey, _Settings.Aws.SecretKey);
                Amazon.RegionEndpoint s3Region;
                switch (_Settings.Aws.Region)
                {
                    case "uswest1":
                        s3Region = Amazon.RegionEndpoint.USWest1;
                        break;
                    case "uswest2":
                        s3Region = Amazon.RegionEndpoint.USWest2;
                        break;
                    case "useast1":
                        s3Region = Amazon.RegionEndpoint.USEast1;
                        break;
                    default:
                        return false;
                }

                using (client = new AmazonS3Client(cred, s3Region))
                {
                    Stream s = new MemoryStream(data);
                    PutObjectRequest request = new PutObjectRequest
                    {
                        BucketName = _Settings.Aws.Bucket,
                        Key = id,
                        InputStream = s,
                        ContentType = "application/octet-stream", 
                    }; 
                   
                    PutObjectResponse response = client.PutObject(request);
                    int statusCode = (int)response.HttpStatusCode;

                    if (response != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                #endregion
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string S3GenerateUrl(string id)
        {
            GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
            request.BucketName = _Settings.Aws.Bucket;
            request.Key = id;
            request.Protocol = Protocol.HTTPS;
            request.Expires = DateTime.Now.AddYears(100);
            return _S3Client.GetPreSignedURL(request);
        }

        #endregion

        #region Private-Azure-Methods

        private bool AzureDelete(string id)
        {
            try
            {
                CloudBlockBlob blockBlob = _AzureContainer.GetBlockBlobReference(id);
                OperationContext ctx = new OperationContext();
                blockBlob.Delete(DeleteSnapshotsOption.None, null, null, ctx);
                int statusCode = ctx.LastResult.HttpStatusCode;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool AzureGet(string id, out byte[] data)
        {
            data = null;

            try
            {
                CloudBlockBlob blockBlob = _AzureContainer.GetBlockBlobReference(id);
                OperationContext ctx = new OperationContext();
                Stream blobStream = new MemoryStream();
                blockBlob.DownloadToStream(blobStream, null, null, ctx);
                blobStream.Position = 0;
                data = Common.StreamToBytes(blobStream);
                blobStream.Close();

                int statusCode = ctx.LastResult.HttpStatusCode;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool AzureWrite(string id, byte[] data)
        {
            try
            {
                CloudBlockBlob blockBlob = _AzureContainer.GetBlockBlobReference(id);
                OperationContext ctx = new OperationContext();
                MemoryStream stream = new MemoryStream(data);
                blockBlob.UploadFromStream(stream, null, null, ctx);
                stream.Close();

                int statusCode = ctx.LastResult.HttpStatusCode;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string AzureGenerateUrl(string id)
        {
            return "https://" +
                _Settings.Azure.AccountName +
                ".blob.core.windows.net/" +
                _Settings.Azure.Container +
                "/" +
                id;
        }

        #endregion

        #endregion
    }
}