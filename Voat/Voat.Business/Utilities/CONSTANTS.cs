namespace Voat.Utilities
{
    public static class CONSTANTS
    {
        public const string CONNECTION_LIVE = "voatEntities";
        public const string CONNECTION_READONLY = "voatEntitiesReadOnly";
        public const int DEFAULT_GUEST_PAGE_CACHE_MINUTES = 3;
        public const string USER_NAME_REGEX = @"[a-zA-Z0-9]{1}[a-zA-Z0-9-_]{1,19}|ted\.shield|o\.o|bill\.lee"; //Backwords compat for these three users with dots in their name. See, we love them.
        public const string SUBVERSE_REGEX = "[a-zA-Z0-9]{1,20}";
        public const string SET_REGEX = SUBVERSE_REGEX + @"\-" + USER_NAME_REGEX;
        public const string HTTP_LINK_REGEX = @"(ht|f)tp(s?)\:\/\/(?<domain>[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*)(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_=!@:\(\)]*)(?<![\.\?\-_\,]|\s{1,})";
    }
}