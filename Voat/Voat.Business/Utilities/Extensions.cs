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
using System.Globalization;
using System.Text;
using Voat.Domain.Models;

namespace Voat
{
    public static class Extensions
    {

        public static string ToYesNo(this bool value)
        {
            return value ? "Yes" : "No";
        }
        public static string ToYesNo(this bool? value, string nullValue)
        {
            if (value.HasValue)
            {
                return value.Value.ToYesNo();
            }
            return nullValue;
        }

        public static string BasePath(this Domain.Models.DomainReference domainReference, Domain.Models.SortAlgorithm? sort = null)
        {
            string path = "";
            if (domainReference != null)
            {
                switch (domainReference.Type)
                {
                    case Domain.Models.DomainType.Subverse:
                        path = String.Format("/v/{0}/{1}", domainReference.Name, sort == null ? "" : sort.Value.ToString().ToLower());
                        break;
                    case Domain.Models.DomainType.Set:
                        path = String.Format("{0}/{1}", Utilities.VoatPathHelper.BasePath(domainReference), sort == null ? "" : sort.Value.ToString().ToLower());
                        break;
                    case Domain.Models.DomainType.User:
                        path = String.Format("/u/{0}", domainReference.Name);
                        break;
                }
            }
            return path.TrimEnd('/');
        }


        //public Dictionary<int, string> GetEnumValues(Type type)
        //{
        //    var dict = new Dictionary<int, string>();
        //}
        public static string ToChatTimeDisplay(this DateTime dateTime) 
        {
            CultureInfo ci = CultureInfo.InvariantCulture;

            return dateTime.ToString("hh:mm:ss", ci) + " UTC";
  
        }

       
    }
}
