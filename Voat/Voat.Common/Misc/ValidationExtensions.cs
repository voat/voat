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

        public static void EnsureNotNullOrEmpty(this string variable, string name = null)
        {
            if (String.IsNullOrEmpty(variable))
            {
                var nameOfItem = name == null ? "variable" : name;
                throw new ArgumentOutOfRangeException($"{nameOfItem} can not be null or empty");
            }
        }
    }
}
