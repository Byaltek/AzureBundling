using System;
using System.IO;
using System.Web.Hosting;
using Newtonsoft.Json.Linq;
using System.Web.Configuration;

namespace Byaltek.Azure
{
    public class Config
    {
        #region "Properties"

        private static string _azureAccountName = string.Empty;
        private static string _azureAccessKey = string.Empty;
        private static string _cdnPath = string.Empty;
        private static string _secureCdnPath = string.Empty;
        private static int _cachePollTime = 0;
        private static int _bundleCacheTTL = 0;
        private static bool _useCompression = false;
        private static Storage _blobStorage;

        /// <summary>
        /// Account name of the Azure Storage account where the files will be read from, and the bundles will be published to.
        /// </summary>
        public static string AzureAccountName
        {
            get
            {
                LoadConfig();
                return _azureAccountName;
            }
        }
        /// <summary>
        /// Access key for the Azure Storage account where the files will be read from, and the bundles will be published to.
        /// </summary>
        public static string AzureAccessKey
        {
            get
            {
                LoadConfig();
                return _azureAccessKey;
            }
        }
        /// <summary>
        /// If you use a custom domain for your Azure CDN, supply that here. If you will not be using a custom domain, supply the 
        /// default CDN name from Azure. Do not include the blob container name. If you will be using blob storage to serve the 
        /// files rather than a CDN, supply the path for blob storage.
        /// </summary>
        /// <example>
        /// http://cdn.yourdomain.com/
        /// </example>
        public static string CdnPath
        {
            get
            {
                LoadConfig();
                return _cdnPath;
            }
        }
        /// <summary>
        /// Azure does not yet allow https to be used with custom domains on the CDN, so supply the default CDN path here for use in 
        /// https scenarios. If you will be using blob storage to serve the files rather than a CDN, supply the https path for blob storage.
        /// </summary>
        /// <example>
        /// https://az217414.vo.msecnd.net/
        /// </example>
        public static string SecureCdnPath
        {
            get
            {
                LoadConfig();
                return _secureCdnPath;
            }
        }
        /// <summary>
        /// The value in seconds for how often you want the source files in the Azure storage account to be checked for changes. By default,
        /// it is set to zero and will check on every request.
        /// </summary>
        public static int CachePollTime
        {
            get
            {
                LoadConfig();
                return _cachePollTime;
            }
        }
        /// <summary>
        /// The value that will be set in the cache control header for the bundle. This will control how often the CDN checks for a new version
        /// and how long the file is cached on the client browser.
        /// </summary>
        public static int BundleCacheTTL
        {
            get
            {
                LoadConfig();
                return _bundleCacheTTL;
            }
        }

        /// <summary>
        /// Boolean value checks to see if gzip compression should be appied to the bundles.
        /// </summary>
        public static bool UseCompression
        {
            get
            {
                LoadConfig();
                return _useCompression;
            }
        }

        /// <summary>
        /// Creates the storage account to be used with Azure blob storage to get the files for bundles. Default is false.
        /// </summary>
        public static Storage BlobStorage
        {
            get
            {
                if (string.IsNullOrEmpty(_azureAccountName) || string.IsNullOrEmpty(_azureAccessKey))
                    return new Storage();
                return _blobStorage = new Storage(_azureAccountName, _azureAccessKey);
            }
        }

        #endregion

        /// <summary>
        /// If configuration values are not yet set, it checks for the appropriate keys in the web.config app settings, and if those do not
        /// exist, it looks for them in a Config.json file in the site root.
        /// </summary>
        public static void LoadConfig()
        {
            try
            {
                if (_azureAccountName == null || _azureAccessKey == null || _secureCdnPath == null || _cdnPath == null || _cachePollTime == 0 || _bundleCacheTTL == 0 || _useCompression == false)
                {
                    if (WebConfigurationManager.AppSettings["AzureAccountName"] != null)
                    {
                        _azureAccountName = WebConfigurationManager.AppSettings["AzureAccountName"].ToString();
                    }
                    else
                    {
                        _azureAccountName = GetJson(jsonString, "AzureAccountName");
                    }
                    if (WebConfigurationManager.AppSettings["AzureAccessKey"] != null)
                    {

                        _azureAccessKey = WebConfigurationManager.AppSettings["AzureAccessKey"].ToString();
                    }
                    else
                    {
                        _azureAccessKey = GetJson(jsonString, "AzureAccessKey");
                    }
                    if (WebConfigurationManager.AppSettings["SecureCdnPath"] != null)
                    {
                        _secureCdnPath = WebConfigurationManager.AppSettings["SecureCdnPath"].ToString();
                    }
                    else
                    {
                        _secureCdnPath = GetJson(jsonString, "SecureCdnPath");
                    }
                    if (WebConfigurationManager.AppSettings["CdnPath"] != null)
                    {
                        _cdnPath = WebConfigurationManager.AppSettings["CdnPath"].ToString();
                    }
                    else
                    {
                        _cdnPath = GetJson(jsonString, "CdnPath");
                    }
                    if (WebConfigurationManager.AppSettings["CachePollTime"] != null)
                    {
                        int oldCachePollTime = _cachePollTime;
                        if (!Int32.TryParse(WebConfigurationManager.AppSettings["CachePollTime"], out _cachePollTime))
                        {
                            _cachePollTime = oldCachePollTime;
                        }
                    }
                    else
                    {
                        int oldCachePollTime = _cachePollTime;
                        if (!Int32.TryParse(GetJson(jsonString, "CachePollTime"), out _cachePollTime))
                        {
                            _cachePollTime = oldCachePollTime;
                        }
                    }
                    if (WebConfigurationManager.AppSettings["BundleCacheTTL"] != null)
                    {
                        int oldBundleCacheTTL = _bundleCacheTTL;
                        if (!Int32.TryParse(WebConfigurationManager.AppSettings["BundleCacheTTL"], out _bundleCacheTTL))
                        {
                            _bundleCacheTTL = oldBundleCacheTTL;
                        }
                    }
                    else
                    {
                        int oldBundleCacheTTL = _bundleCacheTTL;
                        if (!Int32.TryParse(GetJson(jsonString, "BundleCacheTTL"), out _bundleCacheTTL))
                        {
                            _bundleCacheTTL = oldBundleCacheTTL;
                        }
                    }
                    if (WebConfigurationManager.AppSettings["UseCompression"] != null)
                    {
                        bool oldUseCompression = _useCompression;
                        if (!Boolean.TryParse(WebConfigurationManager.AppSettings["UseCompression"], out _useCompression))
                            _useCompression = oldUseCompression;
                    }
                    else
                    {
                        bool oldUseCompression = _useCompression;
                        if (!Int32.TryParse(GetJson(jsonString, "UseCompression"), out _bundleCacheTTL))
                        {
                            _useCompression = oldUseCompression;
                        }
                    }
                }
            }
            catch
            {
                throw new ArgumentNullException("One or more necessary Configuration settings are missing.");
            }
        }

        /// <summary>
        /// Reads the settings from the Config.json file in the site root
        /// </summary>
        private static string jsonString
        {
            get
            {
                if (WebConfigurationManager.AppSettings["JsonConfig"] != null)
                {

                    if (File.Exists(HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["JsonConfig"].ToString())))
                        using (StreamReader r = new StreamReader(HostingEnvironment.MapPath(WebConfigurationManager.AppSettings["jsonConfig"].ToString())))
                        {
                            return r.ReadToEnd();
                        }
                }
                else
                {
                    if (File.Exists(HostingEnvironment.MapPath("~/Config.json")))
                        using (StreamReader r = new StreamReader(HostingEnvironment.MapPath("~/Config.json")))
                        {
                            return r.ReadToEnd();
                        }
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the specified setting from the Config.json file
        /// </summary>
        /// <param name="toParse">String containing JSON file</param>
        /// <param name="property">Setting name to retrieve</param>
        /// <returns>Strind value of the setting</returns>
        private static string GetJson(string toParse, string property)
        {
            if (!string.IsNullOrEmpty(toParse))
                return JObject.Parse(toParse).GetValue(property).ToString();
            return null;
        }
    }
}

