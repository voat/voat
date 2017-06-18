using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Business.Utilities;
using Voat.Common;
using Voat.Common.Components;
using Voat.Configuration;
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
                    Debug.WriteLine("CacheHandler.Instance.Contruct({0})", handlerInfo.Type);
                    _fileManager = handlerInfo.Construct<FileManager>();
                }
                return _fileManager;
            }
            set
            {
                _fileManager = value;
            }
        }

        protected abstract string Domain { get; }

        public abstract void Delete(FileKey key);
        public abstract bool Exists(FileKey key);
        public abstract Task Upload(FileKey key, Uri contentPath, Func<Stream, Task<Stream>> preProcessor = null);
        public abstract string Uri(FileKey key, PathOptions options = null);

    }
}
