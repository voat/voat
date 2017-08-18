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

namespace Voat.Data
{
    public class AGGREGATE_SUBVERSE
    {
        public const string ALL = "_all";
        public const string FRONT = "_front";
        public const string ANY = "_any";
        public const string DEFAULT = "_default";

        public static bool IsAggregate(string subverse)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(subverse))
            {
                switch (subverse.ToLower())
                {
                    case AGGREGATE_SUBVERSE.ALL:
                    case AGGREGATE_SUBVERSE.FRONT:
                    case AGGREGATE_SUBVERSE.DEFAULT:
                    case AGGREGATE_SUBVERSE.ANY:
                    case "all":
                        result = true;
                        break;
                }
            }

            return result;
        }
    }
}
