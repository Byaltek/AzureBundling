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
        private Storage blobStore = new Storage();

        private string Container;

        public StorageVirtualPath(string container)
            : base()
        {
            Container = container;
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
                if (blobStore.BlobExists(Container, azurePath))
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
                    MemoryStream stream = blobStore.DownloadBlob(Container, azurePath);
                    stream.Seek(0, SeekOrigin.Begin);
                    return new StorageVirtualFile(virtualPath, stream);
                }
                catch (Exception)
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
                CacheDepends azureCacheDep = new CacheDepends(virtualPath, virtualPathDependencies, utcStart, Container);
                return azureCacheDep;
            }
            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

    }

    #endregion
   
}
