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
using System.Text.RegularExpressions;
using Voat.Utilities;

namespace Voat.Domain.Models
{
    public class UserDefinition
    {
        public string Name { get; set; }

        public IdentityType Type { get; set; } = IdentityType.User;

        public override string ToString()
        {
            return Format(Name, Type);
        }
        public static UserDefinition Create(string name, IdentityType type)
        {
            return new UserDefinition() { Name = name, Type = type };
        }
        public static UserDefinition Parse(string userName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                return null;
            }
            var matches = Regex.Matches(userName, String.Format(@"((?'prefix'@|u/|/u/|v/|/v/)?(?'name'{0}|{1}))", CONSTANTS.USER_NAME_REGEX, CONSTANTS.SUBVERSE_REGEX), RegexOptions.IgnoreCase);
            if (matches.Count != 1)
            {
                return null;
            }
            else
            {
                UserDefinition result = null;

                var match = matches[0];
                var name = match.Groups["name"].Value;
                var prefix = match.Groups["prefix"].Value;
                prefix = prefix?.ToLower();
                switch (prefix)
                {
                    case "":
                    case "@":
                    case "u/":
                    case "/u/":
                        result = new UserDefinition() { Name = name, Type = IdentityType.User };
                        break;

                    case "v/":
                    case "/v/":

                        result = new UserDefinition() { Name = name, Type = IdentityType.Subverse };
                        break;
                }

                return result;
            }
        }

        public static IEnumerable<UserDefinition> ParseMany(string userNameList, bool distinctOnly = true)
        {
            List<UserDefinition> users = new List<UserDefinition>();
            if (!String.IsNullOrEmpty(userNameList))
            {
                var userSplit = userNameList.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (userSplit.Any())
                {
                    foreach (var userName in userSplit)
                    {
                        var userDef = Parse(userName);
                        if (userDef != null)
                        {
                            if (distinctOnly && users.Any(x => x.Name.ToLower() == userDef.Name.ToLower() && x.Type == userDef.Type))
                            {
                                continue;
                            }
                            users.Add(userDef);
                        }
                    }
                }
            }

            return users;
        }

        public static string Format(string userName, IdentityType type, bool supportMarkdown = false)
        {
            return (
                type == IdentityType.Subverse ?
                "v/" + userName
                :
                (supportMarkdown ? "@" : "") + userName);
        }
    }
}
