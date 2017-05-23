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
using System.Security.Cryptography;

namespace Voat.Utilities
{
    public class HashHandler
    {
        public static bool CommentExists(string content)
        {
            if (!String.IsNullOrEmpty(content) && content.Length > 0)
            {
            }

            return false;
        }

        private static string Compute(params string[] parts)
        {
            string checkBlock = String.Concat(parts);
            var alg = SHA1Managed.Create();
            return Convert.ToBase64String(alg.ComputeHash(System.Text.Encoding.Unicode.GetBytes(checkBlock)));
        }
    }
}
