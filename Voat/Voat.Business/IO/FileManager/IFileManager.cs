using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Common.Components;
using Voat.Configuration;

namespace Voat.IO
{
    public interface IFileManager<K>
    {
        Task Upload(K key, Uri contentPath, Func<Stream, Task<Stream>> preProcessor = null);

        string Uri(K key, PathOptions options = null);

        bool Exists(K key);

        void Delete(K key);
    }
    
    public class FileKey
    {
        public FileKey() { }

        public FileKey(string id, FileType fileType)
        {
            this.ID = id;
            this.FileType = fileType;
        }
        public string ID { get; set; }
        //public string Name { get; set; }
        public FileType FileType {get; set;}
    }
    public enum FileType
    {
        Badge,
        Avatar,
        Thumbnail
    }
    
}
