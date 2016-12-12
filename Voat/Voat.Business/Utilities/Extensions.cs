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
                return Utilities.Formatting.StripWhiteSpace(text);
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
        public static bool IsGenericType(this Type type, Type genericType)
        {
            var result = false;
            if (type != null && genericType != null)
            {
                result = type.GetGenericTypeDefinition() == genericType.GetGenericTypeDefinition();
            }
            return result;
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
        public static bool IsDefault<T>(this T type)
        {
            var result = false;

            var defaultValue = default(T);

            result = System.Object.Equals(type, defaultValue);

            return result;
        }

        //Entire purpose of this routine is to provide a proper error message upon an invalid cast
        public static T Convert<T>(this object type)
        {
            try
            {
                return (T)(object)type;
            }
            catch (Exception ex)
            {

                string typeName = "How do I get type of variable type?";
                throw new InvalidCastException($"Can not convert {typeName} to {typeof(T).Name}", ex);
            }
        }

        //Entire purpose of this routine is to provide a proper error message upon an invalid cast
        public static T Convert<T, V>(this V type)
        {
            try
            {
                return (T)(object)type;
            }
            catch (Exception ex)
            {

                //string typeName = type == null ? "null" : type.GetType().Name;
                string typeName = typeof(V).Name;
                throw new InvalidCastException($"Can not convert {typeName} to {typeof(T).Name}", ex);
            }
        }
    }
}
