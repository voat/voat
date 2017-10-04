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

namespace Voat.Utilities
{
    public static class CONSTANTS
    {
        public const string ADMIN_AREA = "6310C05A-E33A-4DD5-BC3B-15C20EF02C9C";

        public const string SYSTEM_USER_NAME = "Voat";
        public const string CONNECTION_USERS = "voatUsers";
        public const string CONNECTION_LIVE = "ReadWrite";
        public const string CONNECTION_READONLY = "ReadOnly";
        public const int DEFAULT_GUEST_PAGE_CACHE_MINUTES = 3;
        public const string USER_NAME_REGEX = @"ted\.shield|o\.o|bill\.lee|casualwhoaversereader|anothercuriousredditor|[a-zA-Z0-9]{1}([a-zA-Z0-9-_]{1,19})?"; //Backwords compat for these three users with dots in their name. See, we love them.
        public const string SUBVERSE_REGEX = "[a-zA-Z0-9]{1,20}";
        public const string SUBMISSION_ID_REGEX = @"\d*";
        public const string COMMENT_ID_REGEX = @"\d*";
        public const string NSFW_FLAG = "nsfw|nsfl";
        public const string REQUEST_VERIFICATION_HEADER_NAME = "VoatRequestVerificationToken";
        public const string ACCEPTABLE_LEADS = @"(?<=\s{1,}|^|\(|\[|\>)";
        public const string ACCEPTABLE_TRAILING = @"(?<![\.\?\,]|\s{1,})";

        //public const string SET_REGEX = SUBVERSE_REGEX + @"\-" + USER_NAME_REGEX;
        public static readonly string SET_REGEX = $"(?<name>{SUBVERSE_REGEX})(\\{SET_SEPERATOR}(?<ownerName>{USER_NAME_REGEX}))?";

        public const string SET_SEPERATOR = "@";

        //matches url after protocol
        public const string HOST_AND_PATH_LINK_REGEX = @"(?<fullDomain>([wW]{3}\.)?(?<domain>[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*))(:(?<port>[0-9]*))?(?<path>((\/[.\w\$\-_#]*)*))?\??(?<query>[a-zA-Z0-9\*\-\.\?\,\'\/\\\+&amp;%\$#_=!@:\(\)~]*)";
        public const string PROTOCOL_LESS_LINK_REGEX = @"(\/\/|%2[fF]%2[fF])" + HOST_AND_PATH_LINK_REGEX;
        public const string HTTP_LINK_REGEX = @"(?<protocol>([hH][tT]|[fF])[tT][pP]([sS]?))(\:|%3[aA])" + PROTOCOL_LESS_LINK_REGEX;

        //used for apps OAuth redirects - no named capture or backtracking in js regex
        public const string URI_LINK_REGEX_UI = @"([a-zA-Z0-9_-]+)(\:|%3[aA])(\/\/|%2[fF]%2[fF])([wW]{3}\.)?[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)[a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_=!@:\(\)]*";

        public const string SET_LINK_REGEX_SHORT = @"/?(?<prefix>s)/(?<name>[a-zA-Z0-9]+(@(?<owner>[a-zA-Z0-9]+))?)";

        public const string SUBVERSE_LINK_REGEX_SHORT = @"/?(?<prefix>v)/(?<name>[a-zA-Z0-9]+(@(?<owner>[a-zA-Z0-9]+))?)";
        public const string SUBVERSE_LINK_REGEX_FULL = "((" + SUBVERSE_LINK_REGEX_SHORT + @"(?<fullPath>(((/(new|top(\?time=(day|week|month|year|all))?|(comments/)?\d+(/\d+(?:/\d+(?:\d+)?)?)?)))?)(?<anchor>#(?:\d+|submissionTop))?)))";

        public static string USER_HOT_LINK_REGEX
        {
            get
            {
                return String.Format(@"((?<notify>-)?(?<prefix>@|/u/)(?'user'{0}))", CONSTANTS.USER_NAME_REGEX);
            }
        }

        public const bool AWAIT_CAPTURE_CONTEXT = true;

    }
    //this is the start of localization for voat, going to start throwing messages in this class for later conversion
    public static class STRINGS
    {
        public const string DEFAULT_BIO = "Aww snap, this user did not yet write their bio. If they did, it would show up here, you know.";
    }
}
