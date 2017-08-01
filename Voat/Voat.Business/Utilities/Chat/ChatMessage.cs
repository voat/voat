#region LICENSE

/*
    
    Copyright(c) Voat, Inc.

    This file is part of Voat.

    This source file is subject to version 3 of the GPL license,
    that is bundled with this package in the file LICENSE, and is
    available online at http://www.gnu.org/licenses/gpl-3.0.txt;
    you may not use this file except in compliance with the License.

    Software distributed under the License is distributed on an
    "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express
    or implied. See the License for the specific language governing
    rights and limitations under the License.

    All Rights Reserved.

*/

#endregion LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;

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
            message = message.StripWhiteSpace();
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
