namespace Byaltek.Azure
{
    public class GZipBundleConfig
    {
        #region "Properties"

        /// <summary>
        /// Name of the Azure Storage container where the files will be read from, and the bundles will be published to.
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Virtual path of the Azure Storage file path where the files will be read from, and the bundles will be published to.
        /// </summary>
        public string VirtualPath { get; set; }

        /// <summary>
        /// Account name of the Azure Storage account where the files will be read from, and the bundles will be published to.
        /// </summary>
        public string AzureAccountName { get; set; }

        /// <summary>
        /// Access key for the Azure Storage account where the files will be read from, and the bundles will be published to.
        /// </summary>
        public string AzureAccessKey { get; set; }

        /// <summary>
        /// If you use a custom domain for your Azure CDN, supply that here. If you will not be using a custom domain, supply the 
        /// default CDN name from Azure. Do not include the blob container name. If you will be using blob storage to serve the 
        /// files rather than a CDN, supply the path for blob storage.
        /// </summary>
        /// <example>
        /// http://cdn.yourdomain.com/
        /// </example>
        public string CdnPath { get; set; }

        /// <summary>
        /// Azure does not yet allow https to be used with custom domains on the CDN, so supply the default CDN path here for use in 
        /// https scenarios. If you will be using blob storage to serve the files rather than a CDN, supply the https path for blob storage.
        /// </summary>
        /// <example>
        /// https://az217414.vo.msecnd.net/
        /// </example>
        public string SecureCdnPath { get; set; }

        /// <summary>
        /// The value in seconds for how often you want the source files in the Azure storage account to be checked for changes. By default,
        /// it is set to zero and will check on every request.
        /// </summary>
        public int CachePollTime { get; set; }

        /// <summary>
        /// The value that will be set in the cache control header for the bundle. This will control how often the CDN checks for a new version
        /// and how long the file is cached on the client browser.
        /// </summary>
        public int BundleCacheTTL { get; set; }

        /// <summary>
        /// Boolean value checks to see if gzip compression should be appied to the bundles. Default is false.
        /// </summary>
        public bool? UseCompression { get; set; }


        /// <summary>
        /// Creates the storage account to be used with Azure blob storage to get the files for bundles.
        /// </summary>
        public Storage BlobStorage { get; private set; }

        #endregion

        /// <summary>
        /// Loads the default configuration object for the bundle.
        /// </summary>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig()
        {
            LoadConfig(this);
        }

        /// <summary>
        /// Creates a copy of the configuration object for the bundle.
        /// </summary>
        /// <param name="clone"></param>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig(GZipBundleConfig clone)
        {
            LoadConfig(this);
        }

        /// <summary>
        /// Creates a configuration object for the bundle. Expects to find global values for missing properties in either web.config or a json file.
        /// </summary>
        /// <param name="virtualPath">the virtual path on Azure for the bundle.</param>
        /// <param name="container">the container on Azure to look for the bundled file.</param>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig(string virtualPath, string container)
        {
            Container = container;
            VirtualPath = virtualPath;
            LoadConfig(this);
        }

        /// <summary>
        /// Creates a configuration object for the bundle. Expects to find global values for missing properties in either web.config or a json file.
        /// </summary>
        /// <param name="virtualPath">the virtual path on Azure for the bundle.</param>
        /// <param name="container">the container on Azure to look for the bundled file.</param>
        /// <param name="cdnPath">If you use a custom domain for your Azure CDN, supply that here. If you will not be using a custom domain, supply the 
        /// default CDN name from Azure. Do not include the blob container name. If you will be using blob storage to serve the 
        /// files rather than a CDN, supply the path for blob storage.</param>
        /// <example>
        /// http://cdn.yourdomain.com/
        /// </example>
        /// <param name="secureCdnPath">Azure does not yet allow https to be used with custom domains on the CDN, so supply the default CDN path here for use in 
        /// https scenarios. If you will be using blob storage to serve the files rather than a CDN, supply the https path for blob storage.</param>
        /// <example>
        /// https://az217414.vo.msecnd.net/
        /// </example>
        /// <param name="useCompression">Boolean value determining whether or not to use gzip compression.</param>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig(string virtualPath, string container, string cdnPath, string secureCdnPath, bool? useCompression)
        {
            Container = container;
            VirtualPath = virtualPath;
            CdnPath = cdnPath;
            SecureCdnPath = secureCdnPath;
            UseCompression = useCompression.HasValue ? useCompression.Value : UseCompression;
            LoadConfig(this);
        }

        /// <summary>
        /// Creates a configuration object for the bundle. Expects to find global values for missing properties in either web.config or a json file.
        /// </summary>
        /// <param name="virtualPath">the virtual path on Azure for the bundle.</param>
        /// <param name="container">the container on Azure to look for the bundled file.</param>
        /// <param name="bundleCacheTTL">The value that will be set in the cache control header for the bundle.</param>
        /// <param name="cachePollTime">The value that is set for the cache to check for an update to the files.</param>
        /// <param name="useCompression">Boolean value determining whether or not to use gzip compression.</param>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig(string virtualPath, string container, int bundleCacheTTL, int cachePollTime, bool? useCompression)
        {
            Container = container;
            VirtualPath = virtualPath;
            BundleCacheTTL = bundleCacheTTL;
            CachePollTime = cachePollTime;
            UseCompression = useCompression.HasValue ? useCompression.Value : UseCompression;
            LoadConfig(this);
        }

        /// <summary>
        /// Creates a configuration object for the bundle. Expects to find global values for missing properties in either web.config or a json file.
        /// </summary>
        /// <param name="virtualPath">the virtual path on Azure for the bundle.</param>
        /// <param name="container">the container on Azure to look for the bundled file.</param>
        /// <param name="cdnPath">If you use a custom domain for your Azure CDN, supply that here. If you will not be using a custom domain, supply the 
        /// default CDN name from Azure. Do not include the blob container name. If you will be using blob storage to serve the 
        /// files rather than a CDN, supply the path for blob storage.</param>
        /// <example>
        /// http://cdn.yourdomain.com/
        /// </example>
        /// <param name="secureCdnPath">Azure does not yet allow https to be used with custom domains on the CDN, so supply the default CDN path here for use in 
        /// https scenarios. If you will be using blob storage to serve the files rather than a CDN, supply the https path for blob storage.</param>
        /// <example>
        /// https://az217414.vo.msecnd.net/
        /// </example>
        /// <param name="bundleCacheTTL">The value that will be set in the cache control header for the bundle.</param>
        /// <param name="cachePollTime">The value that is set for the cache to check for an update to the files.</param>
        /// <param name="useCompression">Boolean value determining whether or not to use gzip compression.</param>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig(string virtualPath, string container, string cdnPath, string secureCdnPath, int bundleCacheTTL, int cachePollTime, bool? useCompression)
        {
            Container = container;
            VirtualPath = virtualPath;
            CdnPath = cdnPath;
            SecureCdnPath = secureCdnPath;
            BundleCacheTTL = bundleCacheTTL;
            CachePollTime = cachePollTime;
            UseCompression = useCompression.HasValue ? useCompression.Value : UseCompression;
            LoadConfig(this);
        }

        /// <summary>
        /// Creates a configuration object for the bundle. Expects to find global values for missing properties in either web.config or a json file.
        /// </summary>
        /// <param name="virtualPath">the virtual path on Azure for the bundle.</param>
        /// <param name="container">the container on Azure to look for the bundled file.</param>
        /// <param name="azureAccountName">Account name of the Azure Storage account where the files will be read from, and the bundles will be published to.</param>
        /// <param name="azureAccessKey">Access key for the Azure Storage account where the files will be read from, and the bundles will be published to.</param>
        /// <param name="cdnPath">If you use a custom domain for your Azure CDN, supply that here. If you will not be using a custom domain, supply the 
        /// default CDN name from Azure. Do not include the blob container name. If you will be using blob storage to serve the 
        /// files rather than a CDN, supply the path for blob storage.</param>
        /// <example>
        /// http://cdn.yourdomain.com/
        /// </example>
        /// <param name="secureCdnPath">Azure does not yet allow https to be used with custom domains on the CDN, so supply the default CDN path here for use in 
        /// https scenarios. If you will be using blob storage to serve the files rather than a CDN, supply the https path for blob storage.</param>
        /// <example>
        /// https://az217414.vo.msecnd.net/
        /// </example>
        /// <param name="bundleCacheTTL">The value that will be set in the cache control header for the bundle.</param>
        /// <param name="useCompression">Boolean value determining whether or not to use gzip compression.</param>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig(string virtualPath, string container, string azureAccountName, string azureAccessKey, string cdnPath, string secureCdnPath, int bundleCacheTTL, bool? useCompression)
        {
            Container = container;
            VirtualPath = virtualPath;
            AzureAccessKey = azureAccessKey;
            AzureAccountName = azureAccountName;
            CdnPath = cdnPath;
            SecureCdnPath = secureCdnPath;
            BundleCacheTTL = bundleCacheTTL;
            UseCompression = useCompression.HasValue ? useCompression.Value : UseCompression;
            LoadConfig(this);
        }

        /// <summary>
        /// Creates a configuration object for the bundle
        /// </summary>
        /// <param name="virtualPath">the virtual path on Azure for the bundle.</param>
        /// <param name="container">the container on Azure to look for the bundled file.</param>
        /// <param name="azureAccountName">Account name of the Azure Storage account where the files will be read from, and the bundles will be published to.</param>
        /// <param name="azureAccessKey">Access key for the Azure Storage account where the files will be read from, and the bundles will be published to.</param>
        /// <param name="cdnPath">If you use a custom domain for your Azure CDN, supply that here. If you will not be using a custom domain, supply the 
        /// default CDN name from Azure. Do not include the blob container name. If you will be using blob storage to serve the 
        /// files rather than a CDN, supply the path for blob storage.</param>
        /// <example>
        /// http://cdn.yourdomain.com/
        /// </example>
        /// <param name="secureCdnPath">Azure does not yet allow https to be used with custom domains on the CDN, so supply the default CDN path here for use in 
        /// https scenarios. If you will be using blob storage to serve the files rather than a CDN, supply the https path for blob storage.</param>
        /// <example>
        /// https://az217414.vo.msecnd.net/
        /// </example>
        /// <param name="bundleCacheTTL">The value that will be set in the cache control header for the bundle.</param>
        /// <param name="cachePollTime">The value that is set for the cache to check for an update to the files.</param>
        /// <param name="useCompression">Boolean value determining whether or not to use gzip compression.</param>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig(string virtualPath, string container, string azureAccountName, string azureAccessKey, string cdnPath, string secureCdnPath, int bundleCacheTTL, int cachePollTime, bool? useCompression)
        {
            Container = container;
            VirtualPath = virtualPath;
            AzureAccessKey = azureAccessKey;
            AzureAccountName = azureAccountName;
            CdnPath = cdnPath;
            SecureCdnPath = secureCdnPath;
            BundleCacheTTL = bundleCacheTTL;
            CachePollTime = cachePollTime;
            UseCompression = useCompression.HasValue ? useCompression.Value : UseCompression;
            LoadConfig(this);
        }

        /// <summary>
        /// The config class has default values set, that can be set by web.config or a json file. 
        /// GZipBundleConfig allows each bundle to have its configuration set on a per bundle basis instead of globally.
        /// Uses global values for missing properties from either web.config or a json file.
        /// </summary>
        /// <param name="clone">GZipBundleConfig to use to build the configuration from.</param>
        /// <returns>Bundle Configuration</returns>
        public GZipBundleConfig LoadConfig(GZipBundleConfig clone)
        {
            Container = clone.Container;
            VirtualPath = clone.VirtualPath;
            AzureAccessKey = string.IsNullOrEmpty(clone.AzureAccessKey) ? Config.AzureAccessKey : clone.AzureAccessKey;
            AzureAccountName = string.IsNullOrEmpty(clone.AzureAccountName) ? Config.AzureAccountName : clone.AzureAccountName;
            CdnPath = string.IsNullOrEmpty(clone.CdnPath) ? Config.CdnPath : clone.CdnPath;
            SecureCdnPath = string.IsNullOrEmpty(clone.SecureCdnPath) ? Config.SecureCdnPath : clone.SecureCdnPath;
            BundleCacheTTL = clone.BundleCacheTTL == 0 ? Config.BundleCacheTTL : clone.BundleCacheTTL;
            CachePollTime = clone.CachePollTime == 0 ? Config.CachePollTime : clone.CachePollTime;
            UseCompression = clone.UseCompression.HasValue ? clone.UseCompression.Value : Config.UseCompression;
            BlobStorage = string.IsNullOrEmpty(clone.AzureAccountName) || string.IsNullOrEmpty(clone.AzureAccessKey) ? Config.BlobStorage : new Storage(clone.AzureAccountName, clone.AzureAccessKey);
            return this;
        }        

    }
}
