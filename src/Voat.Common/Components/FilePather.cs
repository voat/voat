using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Voat.Common.Components
{
    public class FilePather
    {
        private string _physicalPathRoot = null;
        public static FilePather Instance { get; set; }

        public FilePather(string physicalPathRoot)
        {
            if (!Directory.Exists(physicalPathRoot))
            {
                throw new DirectoryNotFoundException($"Can not find: {physicalPathRoot}");
            }
            _physicalPathRoot = physicalPathRoot;
        }

        public string LocalPath(params string[] relativeParts)
        {
            var result = Path.Combine(ProcessRelative(_physicalPathRoot, relativeParts).ToArray());
            return result;
        }
        //public string UrlPath(string relativePath)
        //{
        //    return relativePath;
        //}

        private IEnumerable<string> ProcessRelative(string root, string[] relativePaths)
        {
            List<string> paths = new List<string>();
            paths.Add(root);
            paths.AddRange(relativePaths.ToPathParts());
            return paths;
        }

    }
}
