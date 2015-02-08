using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using System.IO.Compression;
using System.Web.Optimization;
using System.Web.Configuration;

namespace Byaltek.Azure
{
    public class GZipBundle : Bundle
    {
        public GZipBundle(string virtualPath, params IBundleTransform[] transforms)
            : base(virtualPath, null, transforms)
        { }


        public override BundleResponse CacheLookup(BundleContext context)
        {
            BundleResponse bundleResponse = base.CacheLookup(context);
            if (bundleResponse == null || context.EnableInstrumentation)
            {
                bundleResponse = this.GenerateBundleResponse(context);
                if (!context.EnableInstrumentation)
                {
                    base.UpdateCache(context, bundleResponse);
                }
            }
            return bundleResponse;
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
            base.ApplyTransforms(context, bundleContent, fileInfos);
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


    /// <summary>
    ///   Creates a GzipBundle of Javascript files using JSMinify and GZip compression
    /// </summary>
    /// <param name="virtualPath">The virtual path of the Bundle</param>
    /// <param name="container">The blob container.</param>
    public class JSBundle : GZipBundle
    {
        public JSBundle(string virtualPath, string container)
            : base(virtualPath, new IBundleTransform[] { new JsMinify(), new AzureTranform(container) })
        {
        }
    }

    /// <summary>
    ///   Creates a GzipBundle of StyleSheet files using CSSMinify and GZip compression
    /// </summary>
    /// <param name="virtualPath">The virtual path of the Bundle</param>
    /// <param name="container">The blob container.</param>
    public class CSSBundle : GZipBundle
    {
        public CSSBundle(string virtualPath, string container)
            : base(virtualPath, new IBundleTransform[] { new CssMinify(), new AzureTranform(container) })
        {
        }
    }

    public class AzureTranform : IBundleTransform
    {
        private Storage blobStore = new Storage();
        private string container;
        
        public AzureTranform(string Container)
        {
            container = Container;
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            var bundleCacheTTL = Config.BundleCacheTTL;
            var CdnPath = context.HttpContext.Request.IsSecureConnection ? Config.SecureCdnPath : Config.CdnPath;
            var blob = string.Empty;
            var content = response.Content;
            var contentType = response.ContentType == "text/css" ? "text/css" : "application/javascript";
            var file = VirtualPathUtility.GetFileName(context.BundleVirtualPath);
            var folder = VirtualPathUtility.GetDirectory(context.BundleVirtualPath).TrimStart('~', '/').TrimEnd('/');
            var ext = contentType == "text/css" ? ".css" : ".js";
            var azurePath = string.Format("{0}/{1}{2}", folder, file, ext).ToLower();
            var azureCompressedPath = string.Format("{0}/{1}/{2}{3}", folder, "compressed", file, ext).ToLower();
            if (blobStore.BlobExists(container, azurePath))
                blob = blobStore.DownloadStringBlob(container, azurePath);
            if (blob != content)
            {
                blobStore.UploadStringBlob(container, azurePath, response.Content, contentType, bundleCacheTTL);
                blobStore.CompressBlob(container, azureCompressedPath, response.Content, contentType, bundleCacheTTL);
            }
            var AcceptEncoding = context.HttpContext.Request.Headers["Accept-Encoding"].ToLowerInvariant();
            if (!string.IsNullOrEmpty(AcceptEncoding) && AcceptEncoding.Contains("gzip"))
            {
                azurePath = azureCompressedPath;
                if (blobStore.BlobExists(container, azurePath))
                    blob = blobStore.DownloadStringBlob(container, azurePath);
                content = CompressBundleResponse(content);
                if (blob != content)
                {
                    blobStore.CompressBlob(container, azureCompressedPath, response.Content, contentType, bundleCacheTTL);
                }
            }            
            var uri = string.Format("{0}{1}/{2}", CdnPath, container, azurePath);
            if (context.BundleCollection.UseCdn)
                using (var hashAlgorithm = new SHA256Managed())
                {
                    var hash = HttpServerUtility.UrlTokenEncode(hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(content)));
                    if (context.BundleCollection.GetBundleFor(context.BundleVirtualPath) != null)
                        context.BundleCollection.GetBundleFor(context.BundleVirtualPath).CdnPath = string.Format("{0}?v={1}", uri, hash);
                }
        }

        /// <summary>
        ///   Compresses a blob using gzip compression
        /// </summary>
        /// <param name="container">The blob container where this file is stored</param>
        /// <param name="remoteFileName">The path to the file in the blob container. Case sensitive. Will return false if the correct casing is not used.</param>
        /// <param name="fileContents">A string containing the files contents.</param>
        /// <param name="cacheControlMaxAgeSeconds">The amount of time to cache the blob.</param>
        public static string CompressBundleResponse(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            content = string.Empty;
            if (bytes.Length > 350)
            {
                using (var msi = new MemoryStream(bytes))
                using (var mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(mso, CompressionMode.Compress, true))
                    {
                        msi.CopyTo(gs);
                    }
                    mso.Position = 0;
                    StreamReader sr = new StreamReader(mso);
                    content = sr.ReadToEnd();
                }
            }
            return content;
        }
    }
}
