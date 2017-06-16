using System;
using System.Collections.Generic;
using System.Text;

namespace Voat.Common
{
    public static class ValidationExtensions
    {
        public static void EnsureNotNull<T>(this T variable, string name = null) where T: class
        {
            if (variable == null)
            {
                var nameOfItem = name == null ? "variable" : name;
                throw new ArgumentNullException($"{nameOfItem} can not be null");
            }
        } 


    }
}
