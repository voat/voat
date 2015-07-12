using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Voat.Utils
{
    public static class CONSTANTS
    {
        public const string CONNECTION_LIVE = "voatEntities";
        public const string CONNECTION_READONLY = "voatEntitiesReadOnly";
        public const int DEFAULT_GUEST_PAGE_CACHE_MINUTES = 5;
    }
}