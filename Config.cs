using System;
using System.IO;
using System.Web.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web.Configuration;

namespace Byaltek
{
    public static class Config
    {
        #region "Properties"

        private static string azureAccountName = null;
        private static string azureAccessKey = null;
        private static string cdnPath = null;
        private static string secureCdnPath = null;
        private static int cachePollTime = 0;
        private static int bundleCacheTTL = 0;

        /// <summary>
        /// Account name of the Azure Storage account where the files will be read from, and the bundles will be published to.
        /// </summary>
        public static string AzureAccountName
        {
            get
            {
                LoadConfig();
                return azureAccountName;
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
                return azureAccessKey;
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
                return cdnPath;
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
                return secureCdnPath;
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
                return cachePollTime;
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
                return bundleCacheTTL;
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
                if (azureAccountName == null || azureAccessKey == null || secureCdnPath == null || cdnPath == null || cachePollTime == 0 || bundleCacheTTL == 0)
                {
                    if (WebConfigurationManager.AppSettings["AzureAccountName"] != null)
                    {
                        azureAccountName = WebConfigurationManager.AppSettings["AzureAccountName"].ToString();
                    }
                    else
                    {
                        azureAccountName = GetJson(jsonString, "AzureAccountName");
                    }
                    if (WebConfigurationManager.AppSettings["AzureAccessKey"] != null)
                    {

                        azureAccessKey = WebConfigurationManager.AppSettings["AzureAccessKey"].ToString();
                    }
                    else
                    {
                        azureAccessKey = GetJson(jsonString, "AzureAccessKey");
                    }
                    if (WebConfigurationManager.AppSettings["SecureCdnPath"] != null)
                    {
                        secureCdnPath = WebConfigurationManager.AppSettings["SecureCdnPath"].ToString();
                    }
                    else
                    {
                        secureCdnPath = GetJson(jsonString, "SecureCdnPath");
                    }
                    if (WebConfigurationManager.AppSettings["CdnPath"] != null)
                    {
                        cdnPath = WebConfigurationManager.AppSettings["CdnPath"].ToString();
                    }
                    else
                    {
                        cdnPath = GetJson(jsonString, "CdnPath");
                    }
                    if (WebConfigurationManager.AppSettings["CachePollTime"] != null)
                    {
                        int oldCachePollTime = cachePollTime;
                        if (!Int32.TryParse(WebConfigurationManager.AppSettings["CachePollTime"], out cachePollTime))
                        {
                            cachePollTime = oldCachePollTime;
                        }
                    }
                    else
                    {
                        int oldCachePollTime = cachePollTime;
                        if (!Int32.TryParse(GetJson(jsonString, "CachePollTime"), out cachePollTime))
                        {
                            cachePollTime = oldCachePollTime;
                        }
                    }
                    if (WebConfigurationManager.AppSettings["BundleCacheTTL"] != null)
                    {
                        int oldBundleCacheTTL = bundleCacheTTL;
                        if (!Int32.TryParse(WebConfigurationManager.AppSettings["BundleCacheTTL"], out bundleCacheTTL))
                        {
                            bundleCacheTTL = oldBundleCacheTTL;
                        }
                    }
                    else
                    {
                        int oldBundleCacheTTL = bundleCacheTTL;
                        if (!Int32.TryParse(GetJson(jsonString, "BundleCacheTTL"), out bundleCacheTTL))
                        {
                            bundleCacheTTL = oldBundleCacheTTL;
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
                if (File.Exists(HostingEnvironment.MapPath("~/Config.json")))
                    using (StreamReader r = new StreamReader(HostingEnvironment.MapPath("~/Config.json")))
                    {
                        return r.ReadToEnd();
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
