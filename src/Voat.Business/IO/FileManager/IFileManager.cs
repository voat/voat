using System;
using System.IO;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Common.Models;
using Voat.Utilities;

namespace Voat.IO
{
    public interface IFileManager<K>
    {
        //Dear Future People: This will likely be removed - Oh snap, look what just happened!
        //Task Upload(K key, Uri contentPath, HttpResourceOptions options = null, Func<Stream, Task<Stream>> preProcessor = null);

        Task Upload(K key, Stream stream);

        string Uri(K key, PathOptions options = null);

        Task<bool> Exists(K key);

        Task<bool> Delete(K key);
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
   
    
}
