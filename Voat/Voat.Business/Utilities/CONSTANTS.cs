namespace Voat.Utilities
{
    public static class CONSTANTS
    {
        public const string CONNECTION_LIVE = "voatEntities";
        public const string CONNECTION_READONLY = "voatEntitiesReadOnly";
        public const int DEFAULT_GUEST_PAGE_CACHE_MINUTES = 3;
        public const string USER_NAME_REGEX = "[a-zA-Z0-9]{1}[A-Za-z0-9-_]{1,19}";
        public const string HTTP_LINK_REGEX = @"(ht|f)tp(s?)\:\/\/(?<domain>[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*)(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_=!@:]*)(?<![\.\?\-_\,]|\s{1,})";
    }
}