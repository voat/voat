using System;

namespace Voat.Utilities
{
    public static class CONSTANTS
    {
        
        public const string SYSTEM_USER_NAME = "Voat";
        public const string CONNECTION_LIVE = "voatEntities";
        public const string CONNECTION_READONLY = "voatEntitiesReadOnly";
        public const int DEFAULT_GUEST_PAGE_CACHE_MINUTES = 3;
        public const string USER_NAME_REGEX = @"casualwhoaversereader|anothercuriousredditor|[a-zA-Z0-9]{1}[a-zA-Z0-9-_]{1,19}|ted\.shield|o\.o|bill\.lee"; //Backwords compat for these three users with dots in their name. See, we love them.
        public const string SUBVERSE_REGEX = "[a-zA-Z0-9]{1,20}";
        public const string SUBMISSION_ID_REGEX = @"\d*";
        public const string COMMENT_ID_REGEX = @"\d*";

        public const string SET_REGEX = SUBVERSE_REGEX + @"\-" + USER_NAME_REGEX;
        public const string HTTP_LINK_REGEX = @"([hH][tT]|[fF])[tT][pP]([sS]?)\:\/\/(?<fullDomain>([wW]{3}\.)?(?<domain>[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*))(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_=!@:\(\)]*)(?<![\.\?\-_\,]|\s{1,})";

        public const string SUBVERSE_LINK_REGEX_SHORT = @"(/?v/)(?'sub'[a-zA-Z0-9]+)";
        public const string SUBVERSE_LINK_REGEX_FULL = @"((/?v/)(?'sub'[a-zA-Z0-9]+((/(new|top(\?time=(day|week|month|year|all))?|comments/\d+(/\d+(?:/\d+(?:\d+)?)?)?)))?)(?'anchor'#(?:\d+|submissionTop))?)";

        public static string USER_HOT_LINK_REGEX
        {
            get
            {
                return String.Format(@"((?'notify'-)?(?'prefix'@|/u/)(?'user'{0}))", CONSTANTS.USER_NAME_REGEX);
            }
        }
    }
    //this is the start of localization for voat, going to start throwing messages in this class for later conversion
    public static class STRINGS
    {
        public const string DEFAULT_BIO = "Aww snap, this user did not yet write their bio. If they did, it would show up here, you know.";
    }
}
