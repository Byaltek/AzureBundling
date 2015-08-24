using System.IO;
using System.Web.Hosting;

namespace Byaltek.Azure
{
    #region "VirtualFile"

    public class StorageVirtualFile : VirtualFile
    {
        private Stream _stream;

        public StorageVirtualFile(string virtualPath, Stream stream)
            : base(virtualPath)
        {
            _stream = stream;
        }

        public override Stream Open()
        {
            return _stream;
        }
    }

    #endregion
}
