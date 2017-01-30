using System.IO;

namespace Voat.Utilities
{
    public static class FileSystemUtility
    {
        // Check if a file exists at given location.
        public static bool FileExists(string fileName, string destinationPath)
        {
            var fullPath = Path.Combine(destinationPath, fileName);
            var fileInfo = new FileInfo(fullPath);
            if (fileInfo.Exists && fileInfo.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
