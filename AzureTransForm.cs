using System;
using System.Text;
using System.Security.Cryptography;
using System.Web;
using System.Web.Optimization;

namespace Byaltek.Azure
{
    public class AzureTransform : IBundleTransform
    {
        private GZipBundleConfig _config;

        [Obsolete("Use GZipBundleConfig to configure transform.")]
        public AzureTransform(string container, string azureAccountName = null, string azureAccessKey = null, string cdnPath = null, string secureCdnPath = null, int? bundleCacheTTL = null, int? cachePollTime = null, bool? useCompression = null)
        {
            _config = new GZipBundleConfig("", container, azureAccountName, azureAccessKey, cdnPath, secureCdnPath, bundleCacheTTL.HasValue ? bundleCacheTTL.Value : 0, cachePollTime.HasValue ? cachePollTime.Value : 0, useCompression);
        }

        public AzureTransform(GZipBundleConfig config)
        {
            _config = config;
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            var CdnPath = context.HttpContext.Request.IsSecureConnection ? _config.SecureCdnPath : _config.CdnPath;
            var blob = string.Empty;
            var content = response.Content;
            var contentType = response.ContentType == "text/css" ? "text/css" : "text/javascript";
            var file = VirtualPathUtility.GetFileName(context.BundleVirtualPath);
            var folder = VirtualPathUtility.GetDirectory(context.BundleVirtualPath).TrimStart('~', '/').TrimEnd('/');
            var ext = contentType == "text/css" ? ".css" : ".js";
            var azurePath = string.Format("{0}/{1}{2}", folder, file, ext).ToLower();
            var azureCompressedPath = string.Format("{0}/{1}/{2}{3}", folder, "compressed", file, ext).ToLower();
            if (_config.BlobStorage.BlobExists(_config.Container, azurePath))
                blob = _config.BlobStorage.DownloadStringBlob(_config.Container, azurePath);
            if (blob != content)
            {
                _config.BlobStorage.UploadStringBlob(_config.Container, azurePath, response.Content, contentType, _config.BundleCacheTTL);
                _config.BlobStorage.CompressBlob(_config.Container, azureCompressedPath, response.Content, contentType, _config.BundleCacheTTL);
            }
            var AcceptEncoding = context.HttpContext.Request.Headers["Accept-Encoding"].ToLowerInvariant();
            if (!string.IsNullOrEmpty(AcceptEncoding) && AcceptEncoding.Contains("gzip") && _config.UseCompression.Value)
            {
                azurePath = azureCompressedPath;
                if (_config.BlobStorage.BlobExists(_config.Container, azurePath))
                    blob = _config.BlobStorage.DownloadStringBlob(_config.Container, azurePath);
                content = content.CompressString();
                if (blob != content)
                {
                    _config.BlobStorage.CompressBlob(_config.Container, azureCompressedPath, response.Content, contentType, _config.BundleCacheTTL);
                }
            }
            var uri = string.Format("{0}{1}/{2}", CdnPath, _config.Container, azurePath);
            if (context.BundleCollection.UseCdn)
                using (var hashAlgorithm = new SHA256Managed())
                {
                    var hash = HttpServerUtility.UrlTokenEncode(hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(content)));
                    if (context.BundleCollection.GetBundleFor(context.BundleVirtualPath) != null)
                        context.BundleCollection.GetBundleFor(context.BundleVirtualPath).CdnPath = string.Format("{0}?v={1}", uri, hash);
                }
        }

    }
}
