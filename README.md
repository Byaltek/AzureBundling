# AzureBundling
This package allows Bundles to be created using files from Azure. 
The Bundles can be either CSS or JS Bundles that are minified and a seperate Gzip 
compressed file will be created as well. It sets the CdnPath of the Bundle to the 
compressed file if the Request Accepts gzip. Otherwise the CdnPath is set to the 
non-compressed file. The bundle is also created in memory for complete fallback protection.

There are 6 setting values that can be set in either the config.json file or the web.config file of your website.
AzureAccountName(string) - Your Azure Account Name.
AzureAccessKey(string) - Your Access Key.
CdnPath(string) - Url for your blob storage.
SecureCdnPath(string) - SecureUrl for your blob storage.
CachePollTime(int) - Poll time for custom cache dependency (seconds).
BundleCacheTTL(int) - max-age to cache bundle.

When creating a bundle in Visual Studio you must set the BundleTable VirtualPathProvider.
BundleTable.VirtualPathProvider = new StorageVirtualPath(AzureContainer);
Then you declare your Bundle as you normally would. Instead of StyleBundle or ScriptBundle use JSBundle or CSSBundle.
Declaration includes Bundle virtualPath - "~/bundles/common" and container - "AzureContainer".
bundles.Add(new JSBundle("~/bundles/common", "AzureContainer").Include("~/scripts/common/main.js", "~/scripts/common/site.js"));
The bundle will be minified and saved back to Azure with the correct extension added. (.css or .js)
A folder will be created inside your folder named compressed where a gzip compressed file will be created with the same name.
The file is then cached in-memory and served to the client with the CdnPath - cdn.mysite.com/AzureContainer/bundles/compressed/common.js
Automatically falls back to in-memory file if cdn fails. Creates a cache dependency so if you change any file in the bundle on Azure,
the bundle will be recreated depending on the CachePollTime. Set the BundleCacheTTL to whatever far off time you want and changes
will force invalidation to all requests.
