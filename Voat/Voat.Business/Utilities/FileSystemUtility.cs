using System.IO;

namespace Voat.Business.Utilities
{
    public static class FileSystemUtility
    {
        // Check if a file exists at given location.
        public static bool FileExists(string fileName, string destinationPath)
        {
            var location = Path.Combine(destinationPath, fileName);

            return (File.Exists(location));
        }
    }
}
