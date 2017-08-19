using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Domain.Command;
using Voat.Utilities;

namespace Voat.IO
{
    public abstract class FileManager : IFileManager<FileKey>
    {
        private static FileManager _fileManager = null;

        public static FileManager Instance
        {
            get
            {
                var handlerInfo = FileManagerConfigurationSettings.Instance.Handler;
                if (handlerInfo != null)
                {
                    Debug.WriteLine("FileManager.Instance.Contruct({0})", handlerInfo.Type);
                    _fileManager = handlerInfo.Construct<FileManager>();
                }
                return _fileManager;
            }
            set
            {
                _fileManager = value;
            }
        }
        public virtual bool IsMimeTypePermitted(FileType fileType, string mimeType)
        {
            return true;
        }
        public virtual CommandResponse IsUploadPermitted(string fileName, FileType fileType, string mimeType = null, long? length = null)
        {

            var result = CommandResponse.FromStatus(Status.Success);

            switch (fileType)
            {
                case FileType.Avatar:
                case FileType.Thumbnail:
                case FileType.Badge:

                    if (!fileName.IsImageExtension())
                    {
                        result = CommandResponse.FromStatus(Status.Invalid, "File type is not permitted for upload");
                    }
                    else if (!String.IsNullOrEmpty(mimeType) && !IsMimeTypePermitted(fileType, mimeType))
                    {
                        result = CommandResponse.FromStatus(Status.Invalid, "Mime type is not permitted for upload");
                    }
                    else if (length == null || length.Value == 0 || length > 100000000)
                    {
                        result = CommandResponse.FromStatus(Status.Invalid, "File length is too big or too small but we aren't saying");
                    }
                    break;
            }
            return result;
        }


        protected abstract string Domain { get; }

        public abstract Task<bool> Delete(FileKey key);
        public abstract Task<bool> Exists(FileKey key);

        public abstract Task Upload(FileKey key, Stream stream);

        public abstract string Uri(FileKey key, PathOptions options = null);

    }
}
