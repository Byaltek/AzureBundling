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
        int pollTime = 60;

        public StorageVirtualPath(string Container, string azureAccountName = null, string azureAccessKey = null, int? PollTime = null)
            : base()
        {
            container = Container;
            if(!string.IsNullOrEmpty(azureAccountName) && !string.IsNullOrEmpty(azureAccessKey))
            {
                blobStore = new Storage(azureAccountName, azureAccessKey);               
            }
            else
            {
                blobStore = new Storage();
            }
            if (PollTime.HasValue)
                pollTime = PollTime.Value;
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
            return base.DirectoryExists(virtualDir);
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
