using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voat.Caching
{
    //Quick fix for trying to prevent excess task useage
    public static class ContextCache
    {
        public static T Get<T>(string key)
        {
            var context = System.Web.HttpContext.Current;
            if (context != null)
            {
                if (context.Items.Contains(key))
                {
                    return context.Items[key].Convert<T>();
                }
            }
            return default(T);
        }

        public static void Set<T>(string key, T value)
        {
            var context = System.Web.HttpContext.Current;
            if (context != null)
            {
                context.Items[key] = value;
            }
        }
    }
}
