using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Caching
{
    public static class CachingTimeSpan
    {
        public static TimeSpan UserData {get;} = TimeSpan.FromMinutes(15);

    }
}
