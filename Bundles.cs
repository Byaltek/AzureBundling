using System.Web;
using System.Web.Optimization;
using System.Text;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;

namespace Byaltek.Azure
{
    public class GZipBundle : Bundle
    {
        private GZipBundleConfig _config;

        public GZipBundle(GZipBundleConfig config, params IBundleTransform[] transforms)
            : base(config.VirtualPath, null, transforms)
        {
            _config = config;
        }

        [Obsolete("Use GZipBundleConfig to configure bundle.")]
        public GZipBundle(string virtualPath, string container, string cdnPath = null, string secureCdnPath = null, bool? useCompression = null, params IBundleTransform[] transforms)
            : base(virtualPath, null, transforms)
        {
            _config = new GZipBundleConfig(virtualPath, container, cdnPath, secureCdnPath, useCompression.Value);
        }

        /// <summary>
        /// Processes the bundle request to generate the response.
        /// </summary>
        /// <param name="context">The <see cref="BundleContext"/> object that contains state for both the framework configuration and the HTTP request.</param>
        /// <returns>A <see cref="BundleResponse"/> object containing the processed bundle contents.</returns>
        public override BundleResponse GenerateBundleResponse(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            IEnumerable<BundleFile> fileInfos = this.EnumerateFiles(context);
            fileInfos = context.BundleCollection.IgnoreList.FilterIgnoredFiles(context, fileInfos);
            fileInfos = this.Orderer.OrderFiles(context, fileInfos);
            string bundleContent = this.Builder.BuildBundleContent(this, context, fileInfos);
            BundleResponse bundleResponse = new BundleResponse(bundleContent, fileInfos);
            return ApplyTransforms(context, bundleContent, fileInfos);
        }

        public override BundleResponse CacheLookup(BundleContext context)
        {
            BundleResponse bundleResponse = base.CacheLookup(context);
            if (bundleResponse == null || context.EnableInstrumentation)
            {
                bundleResponse = this.GenerateBundleResponse(context);
                if (!context.EnableInstrumentation)
                {
                    UpdateCache(context, bundleResponse);
                }
            }
            else
            {
                var contentType = bundleResponse.ContentType == "text/css" ? "text/css" : "text/javascript";
                var file = VirtualPathUtility.GetFileName(context.BundleVirtualPath);
                var folder = VirtualPathUtility.GetDirectory(context.BundleVirtualPath).TrimStart('~', '/').TrimEnd('/');
                var ext = contentType == "text/css" ? ".css" : ".js";
                var azureCompressedPath = string.Format("{0}/{1}/{2}{3}", folder, "compressed", file, ext).ToLower();
                var AcceptEncoding = context.HttpContext.Request.Headers["Accept-Encoding"].ToLowerInvariant();
                if (!string.IsNullOrEmpty(AcceptEncoding) && AcceptEncoding.Contains("gzip") && _config.UseCompression.Value)
                {
                    if (!_config.BlobStorage.BlobExists(_config.Container, azureCompressedPath))
                        _config.BlobStorage.CompressBlob(_config.Container, azureCompressedPath, bundleResponse.Content, contentType, _config.BundleCacheTTL);
                }
            }
            return bundleResponse;
        }

        /// <summary>
        /// Used to determine the cache key to store the response for a particular bundle request
        /// </summary>
        /// <param name="context"></param>
        public override string GetCacheKey(BundleContext context)
        {
            if (context.HttpContext == null)
            {
                return base.GetCacheKey(context);
            }
            return string.Format("System.Web.Optimization.Bundle:{0}{1}", context.BundleVirtualPath, context.HttpContext.Request.IsSecureConnection == true ? "ssl" : "");
        }
    }

    
    public class JSBundle : GZipBundle
    {
        /// <summary>
        /// Creates a GzipBundle of Javascript files using JsMinify and optional gzip compression
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
        [Obsolete("Use GZipBundleConfig to configure bundle.")]
        public JSBundle(string virtualPath, string container, string azureAccountName = null, string azureAccessKey = null, 
            string cdnPath = null, string secureCdnPath = null, int? bundleCacheTTL = null, int? cachePollTime = null, bool? useCompression = null)
            : base(new GZipBundleConfig(virtualPath, container, azureAccountName, azureAccessKey, cdnPath, secureCdnPath, bundleCacheTTL.HasValue ? bundleCacheTTL.Value : 0, cachePollTime.HasValue ? cachePollTime.Value : 0, useCompression), 
                  new IBundleTransform[] { new JsMinify(),
                      new AzureTransform(new GZipBundleConfig(virtualPath, container, azureAccountName, azureAccessKey, cdnPath, secureCdnPath, bundleCacheTTL.HasValue ? bundleCacheTTL.Value : 0, cachePollTime.HasValue ? cachePollTime.Value : 0, useCompression)) })
        {
        }

        /// <summary>
        /// Creates a GzipBundle of StyleSheet files using JsMinify and optional gzip compression
        /// </summary>
        /// <param name="config">Configures the bundle properties.</param>
        public JSBundle(GZipBundleConfig config)
            : base(config, new IBundleTransform[] { new JsMinify(), new AzureTransform(config) })
        {
        }
    }
   
    public class CSSBundle : GZipBundle
    {
        /// <summary>
        /// Creates a GzipBundle of StyleSheet files using CssMinify and optional gzip compression
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
        [Obsolete("Use GZipBundleConfig to configure bundle.")]
        public CSSBundle(string virtualPath, string container, string azureAccountName = null, string azureAccessKey = null, 
            string cdnPath = null, string secureCdnPath = null, int? bundleCacheTTL = null, int? cachePollTime = null, bool? useCompression = null)
            : base(new GZipBundleConfig(virtualPath, container, azureAccountName, azureAccessKey, cdnPath, secureCdnPath, bundleCacheTTL.HasValue ? bundleCacheTTL.Value : 0, cachePollTime.HasValue ? cachePollTime.Value : 0, useCompression), 
                  new IBundleTransform[] { new CssMinify(),
                      new AzureTransform(new GZipBundleConfig(virtualPath, container, azureAccountName, azureAccessKey, cdnPath, secureCdnPath, bundleCacheTTL.HasValue ? bundleCacheTTL.Value : 0, cachePollTime.HasValue ? cachePollTime.Value : 0,useCompression)) })
        {
        }

        /// <summary>
        /// Creates a GzipBundle of StyleSheet files using CssMinify and optional gzip compression
        /// </summary>
        /// <param name="config">Configures the bundle properties.</param>
        public CSSBundle(GZipBundleConfig config)
            : base(config, new IBundleTransform[] { new CssMinify(), new AzureTransform(config) })
        {
        }
    }
}
