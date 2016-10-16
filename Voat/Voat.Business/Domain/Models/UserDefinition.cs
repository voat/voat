using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public static UserDefinition Parse(string userName)
        {
            Match subverseSenderMatch = Regex.Match(userName, CONSTANTS.SUBVERSE_LINK_REGEX_SHORT, RegexOptions.IgnoreCase);
            if (subverseSenderMatch.Success)
            {
                var subverse = subverseSenderMatch.Groups["sub"].Value;
                return new UserDefinition() { Name = subverse, Type = IdentityType.Subverse };
            }
            else
            {
                return new UserDefinition() { Name = userName, Type = IdentityType.User };
            }
        }

        public static string Format(string userName, IdentityType type)
        {
            return (type == IdentityType.Subverse ? "v/" + userName : userName);
        }
    }
}
