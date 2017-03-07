using System;
using System.Collections.Generic;
using System.Text;

namespace Voat
{
    public static class Extensions
    {
        public static string BasePath(this Domain.Models.DomainReference domainReference, Domain.Models.SortAlgorithm? sort = null)
        {
            string path = "";
            if (domainReference != null)
            {
                switch (domainReference.Type)
                {
                    case Domain.Models.DomainType.Subverse:
                        path = String.Format("/v/{0}/{1}", domainReference.Name, sort == null ? "" : sort.Value.ToString().ToLower());
                        break;
                    case Domain.Models.DomainType.Set:
                        string p = (String.IsNullOrEmpty(domainReference.OwnerName) ? String.Format("{0}", domainReference.Name) : String.Format("{0}/{1}", domainReference.Name, domainReference.OwnerName));
                        path = String.Format("/s/{0}/{1}", p, sort == null ? "" : sort.Value.ToString().ToLower());
                        break;
                    case Domain.Models.DomainType.User:
                        path = String.Format("/u/{0}", domainReference.Name);
                        break;
                }
            }
            return path.TrimEnd('/');
        }

        //I don't like this
        public static IEnumerable<T> GetEnumFlags<T>(this T value) where T : struct, IConvertible
        {

            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            List<T> list = new List<T>();
            var vals = Enum.GetValues(type);
            int flag = (int)Enum.Parse(type, value.ToString());
            foreach (T v in vals)
            {
                int i = 0;
                if (((int)Enum.Parse(type, v.ToString()) & flag) > 0)
                {
                    list.Add(v);
                }
            }
            return list;
        }

        public static T EnsureRange<T>(this T value, T low, T high) where T : IComparable
        {
            if (value.CompareTo(low) == -1)
            {
                return low;
            }
            else if (value.CompareTo(high) == 1)
            {
                return high;
            }
            return value;
        }

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

        public static string ToQueryString(this object o, bool includeEmptyArguments = false)
        {
            if (o != null)
            {

                List<string> keyPairs = new List<string>();
                var props = o.GetType().GetProperties();

                foreach (var prop in props)
                {
                    var pValue = prop.GetValue(o);
                    var qsValue = "";

                    if (pValue != null)
                    {
                        qsValue = pValue.ToString();
                    }
                    if (includeEmptyArguments || (!includeEmptyArguments && !String.IsNullOrEmpty(qsValue)))
                    {
                        //I don't know if this encoding is correct: Uri.EscapeUriString
                        keyPairs.Add($"{prop.Name.ToLower()}={Uri.EscapeDataString(qsValue)}");
                    }
                }
                var result = String.Join("&", keyPairs);
                return result;
            }
            return "";
        }

        public static bool IsValidEnumValue<T>(int? value) where T : struct, IConvertible
        {
            var result = false;
            if (value.HasValue)
            {
                result = Enum.IsDefined(typeof(T), value.Value);
            }
            return result;
        }
        public static bool IsValidEnumValue<T>(T? value) where T : struct, IConvertible
        {
            var result = false;
            if (value.HasValue)
            {
                result = Enum.IsDefined(typeof(T), value.Value);
            }
            return result;
        }
        public static bool IsValidEnumValue<T>(T value) where T : struct, IConvertible
        {
            var result = false;
            result = Enum.IsDefined(typeof(T), value);
            return result;
        }

        public static T AssignIfValidEnumValue<T>(int value, T defaultValue) where T : struct, IConvertible
        {
            return AssignIfValidEnumValue((int?)value, defaultValue);
        }

        public static T AssignIfValidEnumValue<T>(int? value, T defaultValue) where T : struct, IConvertible
        {
            if (IsValidEnumValue<T>(value))
            {
                return (T)Enum.Parse(typeof(T), value.ToString());
            }
            else
            {
                return defaultValue;
            }
        }

        //public Dictionary<int, string> GetEnumValues(Type type)
        //{
        //    var dict = new Dictionary<int, string>();
        //}

    }
}
