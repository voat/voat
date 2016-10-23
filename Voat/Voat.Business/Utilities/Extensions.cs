using System;

namespace Voat
{
    public static class Extensions
    {
        /// <summary>
        /// Runs case insensitive compare
        /// </summary>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <returns></returns>
        public static bool IsEqual(this string string1, string string2)
        {
            return String.Equals(string1, string2, StringComparison.OrdinalIgnoreCase);
        }

        public static string TrimSafe(this string text)
        {
            if (!String.IsNullOrEmpty(text))
            {
                return text.Trim();
            }
            return text;
        }

        public static string SubstringMax(this string text, int count)
        {
            if (!String.IsNullOrEmpty(text))
            {
                if (text.Length > count)
                {
                    return text.Substring(0, count);
                }
            }
            return text;
        }

        public static bool HasInterface(this Type type, Type typeToFind)
        {
            var result = false;
            if (type != null && typeToFind != null)
            {
                result = typeToFind.IsAssignableFrom(type);
                if (!result && typeToFind.IsGenericType)
                {
                    var genericType = typeToFind.GetGenericTypeDefinition();
                    var interfaces = type.GetInterfaces();
                    foreach (var i in interfaces)
                    {
                        if (i.IsGenericType)
                        {
                            if (i.GetGenericTypeDefinition() == typeToFind)
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
