using System;
using System.IO;
using System.Collections;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace Byaltek.Azure
{
    #region "VirtualPathProvider"

    public class StorageVirtualPath : VirtualPathProvider
    {
        private Storage blobStore;
        private string container;
        int pollTime = 300;

        /// <summary>
        /// The Azure Account name to be used for the connection to Azure
        /// </summary>
        public string AzureAccountName { get; set; }

        /// <summary>
        /// The AccessKey to be used for the connection to Azure
        /// </summary>
        public string AzureAccessKey { get; set; }

        public StorageVirtualPath(string Container)
            : base()
        {
            container = Container;
            AzureAccountName = Config.AzureAccountName;
            AzureAccessKey = Config.AzureAccessKey;
            if (!string.IsNullOrEmpty(AzureAccountName) && !string.IsNullOrEmpty(AzureAccessKey))
            {
                blobStore = new Storage(AzureAccountName, AzureAccessKey);
            }
            else
            {
                throw new ArgumentNullException("AzureAccountName and AzureAccessKey are required!");
            }
        }
        
        public StorageVirtualPath(string Container, string azureAccountName, string azureAccessKey, int? PollTime = null)
            : base()
        {
            container = Container;
            this.AzureAccountName = azureAccountName;
            this.AzureAccessKey = azureAccessKey;
            if (PollTime.HasValue)
                pollTime = PollTime.Value;
            if (!string.IsNullOrEmpty(AzureAccountName) && !string.IsNullOrEmpty(AzureAccessKey))
            {
                blobStore = new Storage(AzureAccountName, AzureAccessKey);
            }
            else
            {
                throw new ArgumentNullException("AzureAccountName and AzureAccessKey are required!");
            }
        }
        
        /// <summary> 
        ///   Determines whether a specified virtual path is within 
        ///   the virtual file system. 
        /// </summary> 
        /// <param name="virtualPath">An absolute virtual path.</param>
        /// <returns> 
        ///   true if the virtual path is within the  
        ///   virtual file sytem; otherwise, false. 
        /// </returns> 
        public bool IsPathVirtual(string virtualPath)
        {
            string checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
            return checkPath.StartsWith("~/", StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool FileExists(string virtualPath)
        {
            if (IsPathVirtual(virtualPath))
            {
                string azurePath = virtualPath.TrimStart('~', '/');
                if (blobStore.BlobExists(container, azurePath))
                    return true;
                return Previous != null ? Previous.FileExists(virtualPath) : false;
            }
            else
            {
                return Previous != null ? Previous.FileExists(virtualPath) : false;
            }
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            // Check if the file exists on blob storage
            if (IsPathVirtual(virtualPath))
            {
                string azurePath = virtualPath.TrimStart('~', '/');
                try
                {
                    MemoryStream stream = blobStore.DownloadBlob(container, azurePath);
                    stream.Position = 0;
                    return new StorageVirtualFile(virtualPath, stream);
                }
                catch
                {
                    return Previous != null ? Previous.GetFile(virtualPath) : null;
                }
            }
            else
                return Previous != null ? Previous.GetFile(virtualPath) : null;
        }

        public override bool DirectoryExists(string virtualDir)
        {
            if (IsPathVirtual(virtualDir))
            {
                string azurePath = virtualDir.TrimStart('~', '/');
                if (blobStore.BlobDirectoryExists(container, azurePath))
                    return true;
                return Previous != null ? Previous.DirectoryExists(virtualDir) : false;
            }
            else
            {
                return Previous != null ? Previous.DirectoryExists(virtualDir) : false;
            }
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            return base.GetDirectory(virtualDir);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (IsPathVirtual(virtualPath))
            {
                CacheDepends azureCacheDep = new CacheDepends(virtualPath, virtualPathDependencies, utcStart, container, pollTime);
                return azureCacheDep;
            }
            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

    }

    #endregion
   
}
