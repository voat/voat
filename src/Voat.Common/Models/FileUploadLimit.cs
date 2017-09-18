using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Voat.Common.Models
{
    public class FileUploadLimit
    {
        public FileType Type { get; set; }
        public long ByteLimit { get; set; } = 0;

        public string[] ExtensionTypes { get; set; } = new string[] { };
        public string[] MimeTypes { get; set; } = new string[] { };


        public bool IsExtensionAllowed(string fileName)
        {
            if (ExtensionTypes.Any())
            {
                var file = Path.GetExtension(fileName);
                return ExtensionTypes.Any(x => x.IsEqual(file));
            }
            return true;
        }
        public bool IsMimeTypeAllowed(string mimeType)
        {
            if (!String.IsNullOrEmpty(mimeType) && MimeTypes.Any())
            {
                //TODO: Need to evaluate combo mimetypes like image/png image/jpg ext (I don't even know if there are valid)
                return MimeTypes.Any(x => x.IsEqual(mimeType));
            }
            return true;
        }
    }
}
