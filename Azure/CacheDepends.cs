using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Web.Caching;
using System.Web.Configuration;

namespace Byaltek.Azure
{
    public class CacheDepends : CacheDependency
    {
        private Timer cacheTimer;
        private List<CacheHelper> cacheHelper = new List<CacheHelper>();
        private Storage blobStore = new Storage();
        private Object timerLock = new Object();
        private int pollTime = 60;

        /// <summary> 
        ///   Adds a Cache Dependency to the files associated with the specified virtual path.
        /// </summary> 
        /// <param name="virtualPath">An absolute virtual path.</param>
        /// <param name="virtualPathDependencies">A list of virtual files.</param>
        /// <param name="utcStart">The start time to compare files too.</param>
        /// <param name="container">The blob container.</param>
        /// <param name="pollTime">The poll time for the timer.</param>
        public CacheDepends(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart, string container, int? PollTime = null)
            : base()
        {
            foreach (string virtualDependency in virtualPathDependencies)
            {
                CacheHelper ch = new CacheHelper();
                ch.filePath = virtualDependency.TrimStart('/');
                ch.container = container;
                ch.lastModified = blobStore.BlobLastModified(ch.container, ch.filePath).DateTime;
                cacheHelper.Add(ch);
            }
            SetUtcLastModified(utcStart);
            if (Config.CachePollTime > 0)
                pollTime = Config.CachePollTime;
            cacheTimer = new Timer(new TimerCallback(CheckDependencyCallback), this, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(pollTime));
        }

        /// <summary> 
        ///   Determines whether a specified virtual file has changed
        ///   and calls NotifyDependencyChanged() if it has. 
        /// </summary> 
        protected void CheckDependencyCallback(object sender)
        {
            CacheDepends cacheDep = (CacheDepends)sender;
            lock (timerLock)
            {
                foreach (CacheHelper ch in cacheDep.cacheHelper)
                {
                    DateTime lastModified = blobStore.BlobLastModified(ch.container, ch.filePath).DateTime;
                    if (ch.lastModified != lastModified)
                    {
                        cacheDep.SetUtcLastModified(lastModified);
                        cacheDep.NotifyDependencyChanged(cacheDep, EventArgs.Empty);
                        break;
                    }
                }
            }
        }

        /// <summary> 
        ///  Calls base.DependencyDispose() to dispose of the cache dependency. 
        /// </summary> 
        protected override void DependencyDispose()
        {
            if (this.cacheTimer != null)
            {
                this.cacheTimer.Dispose();
                this.cacheTimer = null;
            }
            base.DependencyDispose();
        }
    }

    public class CacheHelper
    {
        public DateTime lastModified { get; set; }
        public string filePath { get; set; }
        public string container { get; set; }

        public CacheHelper() { }
    }

}