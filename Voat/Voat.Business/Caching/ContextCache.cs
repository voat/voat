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

namespace Voat.Caching
{
    //Quick fix for trying to prevent excess task useage
    public static class ContextCache
    {
        public static T Get<T>(string key)
        {
            //CORE_PORT: HttpContext not available 
            throw new NotImplementedException("Core Port: HttpContext access");
            /*

            var context = System.Web.HttpContext.Current;
            if (context != null)
            {
                if (context.Items.Contains(key))
                {
                    return context.Items[key].Convert<T>();
                }
            }
            return default(T);
            */
        }

        public static void Set<T>(string key, T value)
        {
            //CORE_PORT: HttpContext not available 
            throw new NotImplementedException("Core Port: HttpContext access");
            /*

            var context = System.Web.HttpContext.Current;
            if (context != null)
            {
                context.Items[key] = value;
            }
            */
        }
    }
}
