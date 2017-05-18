using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Voat
{
    public class ChatMessage
    {
        public string RoomID { get; set; }
        public UserContext User { get; set; }
        public string Message { get; set; }
        public DateTime CreationDate { get; set; }

        private static Dictionary<string, string> replacements = new Dictionary<string, string>() {
            { @"\n", "" },
            { @"\r", "" },
            { @"^([\#\>](\s+)?){1,}", "" },
            { @"(\-\s+){2,}(\-(\s+)?)", "" }, //three or more - followed by spaces
            { @"(\*\s+){2,}(\*(\s+)?)", "" } //three or more * followed by spaces
        };

        public static string SanitizeInput(string message)
        {
            message = message.TrimSafe();
            if (!String.IsNullOrEmpty(message))
            {
                message = message.SubstringMax(500);
                message = replacements.Aggregate(message, (value, keyPair) => Regex.Replace(value, keyPair.Key, keyPair.Value));
                message = message.TrimSafe();
            }

            return message;
        }

    }

}
