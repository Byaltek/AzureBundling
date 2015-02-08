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

        public static string AzureAccountName
        {
            get
            {
                LoadConfig();
                return azureAccountName;
            }
        }
        public static string AzureAccessKey
        {
            get
            {
                LoadConfig();
                return azureAccessKey;
            }
        }
        public static string CdnPath
        {
            get
            {
                LoadConfig();
                return cdnPath;
            }
        }
        public static string SecureCdnPath
        {
            get
            {
                LoadConfig();
                return secureCdnPath;
            }
        }
        public static int CachePollTime
        {
            get
            {
                LoadConfig();
                return cachePollTime;
            }
        }
        public static int BundleCacheTTL
        {
            get
            {
                LoadConfig();
                return bundleCacheTTL;
            }
        }

        #endregion

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
                        cachePollTime = Convert.ToInt32(WebConfigurationManager.AppSettings["CachePollTime"]);
                    }
                    else
                    {
                        cachePollTime = Convert.ToInt32(GetJson(jsonString, "CachePollTime"));
                    }
                    if (WebConfigurationManager.AppSettings["BundleCacheTTL"] != null)
                    {
                        bundleCacheTTL = Convert.ToInt32(WebConfigurationManager.AppSettings["BundleCacheTTL"]);
                    }
                    else
                    {
                        bundleCacheTTL = Convert.ToInt32(GetJson(jsonString, "BundleCacheTTL"));
                    }
                }
            }
            catch
            {
                throw new ArgumentNullException("One or more necessary Configuration settings are missing.");
            }
        }

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

        private static string GetJson(string toParse, string property)
        {
            if (!string.IsNullOrEmpty(toParse))
                return JObject.Parse(toParse).GetValue(property).ToString();
            return null;
        }
    }
}
