using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Byaltek.Azure
{
    public class Storage
    {
        public Storage()
        {
            this.AzureAccountName = Config.AzureAccountName;
            this.AzureAccessKey = Config.AzureAccessKey;
            if (string.IsNullOrEmpty(AzureAccountName) || string.IsNullOrEmpty(AzureAccessKey))
                throw new ArgumentNullException("AzureAccountName and AzureAccessKey are required!");
        }

        public Storage(string azureAccountName, string azureAccessKey)
        {
            this.AzureAccountName = azureAccountName;
            this.AzureAccessKey = azureAccessKey;
            if (string.IsNullOrEmpty(AzureAccountName) || string.IsNullOrEmpty(AzureAccessKey))
                throw new ArgumentNullException("AzureAccountName and AzureAccessKey are required!");
        }

        /// <summary>
        /// The Azure Account name to be used for the connection to Azure
        /// </summary>
        public string AzureAccountName { get; set; }

        /// <summary>
        /// The AccessKey to be used for the connection to Azure
        /// </summary>
        public string AzureAccessKey { get; set; }


        /// <summary>
        /// The full connection string to the Azure blob storage
        /// </summary>
        public string ConnectionString
        {
            get { return string.Format("DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1}", AzureAccountName, AzureAccessKey); }
        }

        /// <summary>
        /// Checks if the specified file exists in the container.
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used.</param>
        /// <returns></returns>
        public DateTimeOffset BlobLastModified(string container, string fileName)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
                blockBlob.FetchAttributes();
                if (blockBlob.Properties.LastModified.HasValue)
                    return blockBlob.Properties.LastModified.Value;
                return DateTime.UtcNow;
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Checks if the specified file exists in the container.
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used.</param>
        /// <returns></returns>
        public bool BlobExists(string container, string fileName)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
                return blockBlob.Exists();
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Checks if the specified file exists in the container.
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used.</param>
        /// <returns></returns>
        public async Task<DateTimeOffset> BlobLastModifiedAsync(string container, string fileName)
        {
            try
            {
                //create the cloud blob client
                var blobClient = StorageAccount.CreateCloudBlobClient();
                //get the cloud blob reference
                var blobContainer = blobClient.GetContainerReference(container);
                var cloudBlob = blobContainer.GetBlockBlobReference(fileName);                
                await cloudBlob.FetchAttributesAsync();
                if (cloudBlob.Properties.LastModified.HasValue)
                    return cloudBlob.Properties.LastModified.Value;
                return DateTime.UtcNow;
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                return DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Checks if the specified file exists in the container.
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used.</param>
        /// <returns></returns>
        public async Task<bool> BlobExistsAsync(string container, string fileName)
        {
            try
            {
                //create the cloud blob client
                var blobClient = StorageAccount.CreateCloudBlobClient();
                //get the cloud blob reference
                var blobContainer = blobClient.GetContainerReference(container);
                var cloudBlob = blobContainer.GetBlockBlobReference(fileName);
                return await cloudBlob.ExistsAsync();
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Uploads a local file to blob storage
        /// </summary>
        /// <param name="container">The blob container where this file will be stored</param>
        /// <param name="localFileName">The full path to the local file</param>
        /// <param name="remoteFileName">The remote name of the file</param>
        /// <param name="contentType">the content type of the file</param>
        /// <param name="cacheControlTTL">The number of seconds set against the 'public, max-age=XXXX' cache control property (default is 3600)</param>
        /// <param name="uploadTimeout">the number of seconds to wait before timing out (default is 90)</param>
        /// <param name="doCompression">whether or not to apply gzip compression</param>
        public void UploadBlob(string container, string localFileName, string remoteFileName, string contentType = null, int? cacheControlTTL = 3600, int? uploadTimeout = 90, bool? doCompression = false)
        {
            using (var fileStream = File.OpenRead(localFileName))
            {
                this.UploadBlob(container, remoteFileName, fileStream, contentType, cacheControlTTL, uploadTimeout, doCompression);
            }
        }


        /// <summary>
        /// Uploads a local file to blob storage
        /// </summary>
        /// <param name="container">The blob container where this file will be stored</param>
        /// <param name="remoteFileName">The remote name of the file</param>
        /// <param name="fileContents">the stream contents of the file</param>
        /// <param name="contentType">the content type of the file</param>
        /// <param name="cacheControlTTL">The number of seconds set against the 'public, max-age=XXXX' cache control property (default is 3600)</param>
        /// <param name="uploadTimeout">the number of seconds to wait before timing out (default is 90)</param>
        /// <param name="doCompression">whether or not to apply gzip compression</param>
        public void UploadBlob(string container, string remoteFileName, Stream fileContents, string contentType = null, int? cacheControlTTL = 3600, int? uploadTimeout = 90, bool? doCompression = false)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(remoteFileName);
                BlobRequestOptions blobRequestOptions = new BlobRequestOptions() { ServerTimeout = TimeSpan.FromSeconds(Convert.ToInt16(uploadTimeout)), MaximumExecutionTime = TimeSpan.FromSeconds(Convert.ToInt16(uploadTimeout)) };
                blockBlob.UploadFromStream(fileContents, null, blobRequestOptions);
                blockBlob.Properties.ContentType = !string.IsNullOrEmpty(contentType) ? contentType : this.ContentType(remoteFileName);
                blockBlob.Properties.CacheControl = string.Format("public, max-age={0}", cacheControlTTL);
                if (doCompression.HasValue && doCompression == true)
                    blockBlob.Properties.ContentEncoding = "gzip";
                blockBlob.SetProperties();
                if (doCompression.HasValue && doCompression == true)
                    CompressBlob(container, remoteFileName, DownloadStringBlob(container, remoteFileName), contentType, cacheControlTTL);
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                throw;
            }

        }

        /// <summary>
        /// Uploads a local file to blob storage
        /// </summary>
        /// <param name="container">The blob container where this file will be stored</param>
        /// <param name="localFileName">The full path to the local file</param>
        /// <param name="remoteFileName">The remote name of the file</param>
        /// <param name="contentType">the content type of the file</param>
        /// <param name="cacheControlTTL">The number of seconds set against the 'public, max-age=XXXX' cache control property (default is 3600)</param>
        /// <param name="uploadTimeout">the number of seconds to wait before timing out (default is 90)</param>
        /// <param name="doCompression">whether or not to apply gzip compression</param>
        public async Task UploadBlobAsync(string container, string localFileName, string remoteFileName, string contentType = null, int? cacheControlTTL = 3600, int? uploadTimeout = 90, bool? doCompression = false)
        {
            using (var fileStream = File.OpenRead(localFileName))
            {
                await this.UploadBlobAsync(container, remoteFileName, fileStream, contentType, cacheControlTTL, uploadTimeout, doCompression);
            }
        }

        /// <summary>
        /// Uploads a local file to blob storage
        /// </summary>
        /// <param name="container">The blob container where this file will be stored</param>
        /// <param name="localFileName">The full path to the local file</param>
        /// <param name="fileContents">the stream contents of the file</param>
        /// <param name="contentType">the content type of the file</param>
        /// <param name="cacheControlTTL">The number of seconds set against the 'public, max-age=XXXX' cache control property (default is 3600)</param>
        /// <param name="uploadTimeout">the number of seconds to wait before timing out (default is 90)</param>
        /// <param name="doCompression">whether or not to apply gzip compression</param>
        public async Task UploadBlobAsync(string container, string remoteFileName, Stream fileContents, string contentType = null, int? cacheControlTTL = 3600, int? uploadTimeout = 90, bool? doCompression = false)
        {
            try
            {
                var blobClient = StorageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(container);
                var blockBlob = blobContainer.GetBlockBlobReference(remoteFileName);
                var blobRequestOptions = new BlobRequestOptions() { ServerTimeout = TimeSpan.FromSeconds(Convert.ToInt16(uploadTimeout)), MaximumExecutionTime = TimeSpan.FromSeconds(Convert.ToInt16(uploadTimeout)) };
                await blockBlob.UploadFromStreamAsync(fileContents, null, blobRequestOptions, new OperationContext());
                blockBlob.Properties.ContentType = !string.IsNullOrEmpty(contentType) ? contentType : this.ContentType(remoteFileName);
                blockBlob.Properties.CacheControl = string.Format("public, max-age={0}", cacheControlTTL);
                if (doCompression.HasValue && doCompression == true)
                    blockBlob.Properties.ContentEncoding = "gzip";
                blockBlob.SetProperties();
                if (doCompression.HasValue && doCompression == true)
                    CompressBlob(container, remoteFileName, DownloadStringBlob(container, remoteFileName), contentType, cacheControlTTL);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Uploading blob was cancelled: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Uploading blob failed: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Uploads a string as a blob 
        /// </summary>
        /// <param name="container">The blob container where this file will be stored</param>
        /// <param name="localFileName">The full path to the local file</param>
        /// <param name="fileContents">the string contents of the file</param>
        /// <param name="contentType">the content type of the file</param>
        /// <param name="cacheControlTTL">The number of seconds set against the 'public, max-age=XXXX' cache control property (default is 3600)</param>
        /// <param name="uploadTimeout">the number of seconds to wait before timing out (default is 90)</param>
        /// <param name="doCompression">whether or not to apply gzip compression</param>
        public void UploadStringBlob(string container, string remoteFileName, string fileContents, string contentType, int? cacheControlTTL = 3600, int? uploadTimeout = 90, bool? doCompression = false)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(remoteFileName);
                BlobRequestOptions blobRequestOptions = new BlobRequestOptions() { ServerTimeout = TimeSpan.FromSeconds(Convert.ToInt16(uploadTimeout)), MaximumExecutionTime = TimeSpan.FromSeconds(Convert.ToInt16(uploadTimeout)) };

                blockBlob.UploadText(fileContents, null, null, blobRequestOptions);
                blockBlob.Properties.ContentType = contentType;
                blockBlob.Properties.CacheControl = string.Format("public, max-age={0}", cacheControlTTL);
                if (doCompression.HasValue && doCompression == true)
                    blockBlob.Properties.ContentEncoding = "gzip";
                blockBlob.SetProperties();
                if (doCompression.HasValue && doCompression == true)
                    CompressBlob(container, remoteFileName, fileContents, contentType, cacheControlTTL);
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                throw;
            }

        }

        /// <summary>
        /// Uploads a string as a blob 
        /// </summary>
        /// <param name="container">The blob container where this file will be stored</param>
        /// <param name="localFileName">The full path to the local file</param>
        /// <param name="fileContents">the string contents of the file</param>
        /// <param name="contentType">the content type of the file</param>
        /// <param name="cacheControlTTL">The number of seconds set against the 'public, max-age=XXXX' cache control property (default is 3600)</param>
        /// <param name="uploadTimeout">the number of seconds to wait before timing out (default is 90)</param>
        /// <param name="doCompression">whether or not to apply gzip compression</param>
        public async Task UploadStringBlobAsync(string container, string remoteFileName, string fileContents, string contentType, int? cacheControlTTL = 3600, int? uploadTimeout = 90, bool? doCompression = false)
        {
            try
            {
                var blobClient = StorageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(container);
                var blockBlob = blobContainer.GetBlockBlobReference(remoteFileName);
                var blobRequestOptions = new BlobRequestOptions() { ServerTimeout = TimeSpan.FromSeconds(Convert.ToInt16(uploadTimeout)), MaximumExecutionTime = TimeSpan.FromSeconds(Convert.ToInt16(uploadTimeout)) };

                await blockBlob.UploadTextAsync(fileContents, null, null, blobRequestOptions, new OperationContext());
                blockBlob.Properties.ContentType = contentType;
                blockBlob.Properties.CacheControl = string.Format("public, max-age={0}", cacheControlTTL);
                if (doCompression.HasValue && doCompression == true)
                    blockBlob.Properties.ContentEncoding = "gzip";
                blockBlob.SetProperties();
                if (doCompression.HasValue && doCompression == true)
                    CompressBlob(container, remoteFileName, fileContents, contentType, cacheControlTTL);
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                throw;
            }

        }

        /// <summary>
        ///   Compresses a blob using gzip compression
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="remoteFileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used.</param>
        /// <param name="fileContents">A string containing the files contents.</param>
        /// <param name="contentType">The ContentType of the file.</param>
        /// <param name="cacheControlMaxAgeSeconds">The amount of time to cache the blob (Default is 1 month).</param>
        public void CompressBlob(string container, string fileName, string fileContents, string contentType, int? cacheControlMaxAgeSeconds = 86400)
        {
            string cacheControlHeader = "public, max-age=" + cacheControlMaxAgeSeconds.ToString();

            // only create gzip copies for css and js files
            string extension = Path.GetExtension(fileName);
            if (extension != ".css" && extension != ".js")
                return;

            // see if the gzip version already exists
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
            CloudBlockBlob gzipBlob = blobContainer.GetBlockBlobReference(fileName);
            var bytes = Encoding.UTF8.GetBytes(fileContents);
            if (bytes.Length > 350)
            {
                MemoryStream mso = null;
                using (var msi = new MemoryStream(bytes))
                    try
                    {
                        mso = new MemoryStream();
                        using (var gs = new GZipStream(mso, CompressionMode.Compress, true))
                        {
                            msi.CopyTo(gs);
                        }
                        var compressed = new byte[mso.Length];
                        compressed = mso.ToArray();
                        //maximum level for gzip is about 95% so make sure file is at least 5%
                        if (compressed.Length > (bytes.Length * .05))
                        {
                            gzipBlob.UploadFromByteArray(compressed, 0, compressed.Length);
                            gzipBlob.Properties.ContentEncoding = "gzip";
                        }
                    }
                    finally
                    {
                        if (mso != null)
                            mso.Dispose();
                    }
            }
            else
            {
                //compression failed so load original bytes so displaying file doesn't fail
                gzipBlob.UploadFromByteArray(bytes, 0, bytes.Length);
            }
            gzipBlob.Properties.CacheControl = cacheControlHeader;
            gzipBlob.Properties.ContentType = contentType;
            gzipBlob.SetProperties();
        }

        /// <summary>
        ///   Compresses a blob using gzip compression asyncronously
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="remoteFileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used.</param>
        /// <param name="fileContents">A string containing the files contents.</param>
        /// <param name="contentType">The ContentType of the file.</param>
        /// <param name="cacheControlMaxAgeSeconds">The amount of time to cache the blob (Default is 1 month).</param>
        public async Task CompressBlobAsync(string container, string fileName, string fileContents, string contentType, int? cacheControlMaxAgeSeconds = 86400)
        {
            try
            {
                string cacheControlHeader = "public, max-age=" + cacheControlMaxAgeSeconds.ToString();

                // only create gzip copies for css and js files
                string extension = Path.GetExtension(fileName);
                if (extension != ".css" && extension != ".js")
                    return;

                // see if the gzip version already exists
                var blobClient = StorageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(container);
                var gzipBlob = blobContainer.GetBlockBlobReference(fileName);
                var bytes = Encoding.UTF8.GetBytes(fileContents);
                if (bytes.Length > 350)
                {
                    MemoryStream mso = null;
                    using (var msi = new MemoryStream(bytes))
                        try
                        {
                            mso = new MemoryStream();
                            using (var gs = new GZipStream(mso, CompressionMode.Compress, true))
                            {
                                await msi.CopyToAsync(gs);
                            }
                            var compressed = new byte[mso.Length];
                            compressed = mso.ToArray();
                            //maximum level for gzip is about 95% so make sure file is at least 5%
                            if (compressed.Length > (bytes.Length * .05))
                            {
                                await gzipBlob.UploadFromByteArrayAsync(compressed, 0, compressed.Length);
                                gzipBlob.Properties.ContentEncoding = "gzip";
                            }
                        }
                        finally
                        {
                            if (mso != null)
                                mso.Dispose();
                        }
                }
                else
                {
                    //compression failed so load original bytes so displaying file doesn't fail
                    await gzipBlob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                }
                gzipBlob.Properties.CacheControl = cacheControlHeader;
                gzipBlob.Properties.ContentType = contentType;
                gzipBlob.SetProperties();

            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Uploading blob was cancelled: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Uploading blob failed: {0}", ex.Message);
            }
        }

        /// <summary>
        /// downloads a blob
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used</param>
        /// <returns>memorystream containing blob data</returns>
        public MemoryStream DownloadBlob(string container, string fileName)
        {
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
            MemoryStream strm = new MemoryStream();
            blockBlob.DownloadToStream(strm);
            return strm;
        }

        /// <summary>
        /// downloads a blob and saves it to disk
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used</param>
        /// <param name="localFileName">the local path to save the blob</param>
        /// <returns>saves the blob to disk</returns>
        public void DownloadBlob(string container, string remoteFileName, string localFileName)
        {
            // Create the blob client.
            CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
            // Retrieve reference to a blob named "photo1.jpg".
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(remoteFileName);

            // Save blob contents to a file.
            using (var fileStream = System.IO.File.OpenWrite(localFileName))
            {
                blockBlob.DownloadToStream(fileStream);
            }
        }


        /// <summary>
        /// downloads a blob asyncronously
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used</param>
        /// <returns>memorystream containing blob data</returns>
        public async Task<MemoryStream> DownloadBlobAsync(string container, string remoteFileName)
        {
            try
            {
                var blobClient = StorageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(container);
                var blockBlob = blobContainer.GetBlockBlobReference(remoteFileName);
                MemoryStream strm = new MemoryStream();
                await blockBlob.DownloadToStreamAsync(strm);
                return strm;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Downloading blob was cancelled: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Downloading blob failed: {0}", ex.Message);
            }
            return new MemoryStream();
        }

        /// <summary>
        /// downloads a blob asyncronously and saves it to disk
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used</param>
        /// <param name="localFileName">the local path to save the blob</param>
        /// <returns>saves the blob to disk</returns>
        public async Task DownloadBlobAsync(string container, string remoteFileName, string localFileName)
        {
            try
            {
                // Create the blob client.
                var blobClient = StorageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(container);
                // Retrieve reference to a blob named "photo1.jpg".
                var blockBlob = blobContainer.GetBlockBlobReference(remoteFileName);

                // Save blob contents to a file.
                using (var fileStream = System.IO.File.OpenWrite(localFileName))
                {
                    await blockBlob.DownloadToStreamAsync(fileStream);
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("Downloading blob was cancelled: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Downloading blob failed: {0}", ex.Message);
            }
        }

        /// <summary>
        /// downloads a blob
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used</param>
        /// <returns>a string representation of the blob</returns>
        public string DownloadStringBlob(string container, string fileName)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
                return blockBlob.DownloadText();
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                return null;
            }
        }

        /// <summary>
        /// downloads a blob asyncronously
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used</param>
        /// <returns>a string representation of the blob</returns>
        public async Task<string> DownloadStringBlobAsync(string container, string fileName)
        {
            try
            {
                var blobClient = StorageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(container);
                var blockBlob = blobContainer.GetBlockBlobReference(fileName);
                return await blockBlob.DownloadTextAsync();
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                return null;
            }
        }

        /// <summary>
        /// Deletes the blob from the container
        /// </summary>
        /// <param name="Container"></param>
        /// <param name="RemoteFileName"></param>
        public void DeleteBlob(string container, string fileName)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
                blockBlob.Delete();
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                throw;
            }
        }

        /// <summary>
        /// Deletes the blob from the container asyncronously
        /// </summary>
        /// <param name="Container"></param>
        /// <param name="RemoteFileName"></param>
        public async Task DeleteBlobAsync(string container, string fileName)
        {
            try
            {
                var blobClient = StorageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(container);
                var blockBlob = blobContainer.GetBlockBlobReference(fileName);
                await blockBlob.DeleteAsync();
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                throw;
            }
        }

        /// <summary>
        /// Checks if the specified file exists in the container and returns it.
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="fileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used.</param>
        /// <returns></returns>
        public CloudBlockBlob GetBlob(string container, string fileName)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);

                if (blockBlob.Exists())
                    return blockBlob;
                return null;
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                return null;
            }
        }

        /// <summary>
        /// Returns a list of all blobs in a subdirectory in a container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public bool BlobDirectoryExists(string container, string dir)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlobDirectory blobDir = blobContainer.GetDirectoryReference(dir);
                return true;
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                return false;
            }
        }

        /// <summary>
        /// Deletes all blobs in a subdirectory of a container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public void DeleteBlobDirectory(string container, string dir)
        {
            try
            {
                CloudBlobClient blobClient = StorageAccount.CreateCloudBlobClient();
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(container);
                CloudBlobDirectory blobDir = blobContainer.GetDirectoryReference(dir);
                foreach (IListBlobItem blob in blobDir.ListBlobs(true, BlobListingDetails.None))
                {
                    DeleteBlob(container, dir + "/" + Path.GetFileName(blob.Uri.LocalPath));
                }
            }
            catch (StorageException ex)
            {
                var requestInformation = ex.RequestInformation;
                Trace.WriteLine(requestInformation.HttpStatusMessage);
                throw;
            }
        }

        #region Privates

        private CloudStorageAccount StorageAccount
        {
            get
            {
                try
                {
                    return CloudStorageAccount.Parse(ConnectionString);
                }
                catch (StorageException ex)
                {
                    var requestInformation = ex.RequestInformation;
                    Trace.WriteLine(requestInformation.HttpStatusMessage);
                    throw;
                }
            }
        }

        /// <summary>
        ///   Gets the content type for the specified FileName
        /// </summary>       
        private string ContentType(string FileName)
        {
            FileName = FileName.ToLower();
            string contentType = "application/octet-stream";
            switch (Path.GetExtension(FileName))
            {
                case ".png":
                    contentType = "image/png";
                    break;
                case ".jpg":
                    contentType = "image/jpeg";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".css":
                    contentType = "text/css";
                    break;
                case ".htm":
                    contentType = "text/html";
                    break;
                case ".js":
                    contentType = "text/javascript";
                    break;
                case ".xml":
                    contentType = "text/xml";
                    break;
            }
            return contentType;
        }

        #endregion

    }
}
