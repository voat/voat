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
using System.Text;
using Voat.Configuration;

namespace Voat.Utilities
{
    public static class IpHash
    {
        // encrypt user IP (one-way) before storing it in database
        //HMAC-SHA256 keyed with VoatSettings.SigningKey: an unsalted hash of an IPv4
        //address can be brute-forced offline over the whole 32-bit space; with a server
        //secret the values are not reversible without the key. Set SigningKey in
        //production (see .env.example) — with an empty key this degrades to plain hashing.
        public static string CreateHash(string ip)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(VoatSettings.Instance.SigningKey ?? String.Empty)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(ip)));
            }
        }
    }
}
