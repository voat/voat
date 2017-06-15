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
using System.Threading.Tasks;

namespace Voat.Tests.Infrastructure
{
    public class SUBVERSES
    {
        //Known Subverses
        public const string Unit = "unit";
        public const string Anon = "anon";
        public const string Whatever = "whatever";
        public const string AuthorizedOnly = "AuthorizedOnly";
        public const string MinCCP = "MinCCP";
        public const string Private = "private";
        public const string AskVoat = "AskVoat";
        public const string News = "news";
        public const string NSFW = "NSFW";
        public const string AllowAnon = "AllowAnon";
        public const string Disabled = "Disabled";
    }

    public class USERNAMES
    {
        //Known Usernames
        public const string Unit = "unit";
        public const string Anon = "anon";
        public const string User0CCP = "User0CCP";
        public const string User50CCP = "User50CCP";
        public const string User100CCP = "User100CCP";
        public const string User500CCP = "User500CCP";
    }
    public class CONSTANTS
    {

        public const string UNIT_TEST_USER_TEMPLATE = "UnitTestUser{0}";
        public const string TEST_USER_TEMPLATE = "TestUser{0}";
    }
}
