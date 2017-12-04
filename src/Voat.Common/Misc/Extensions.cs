using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voat.Common
{
    public static class Extensions
    {
        public static string ShortAssemblyQualifiedName(this Type type)
        {
            return $"{type.FullName}, {type.Assembly}";
        }
       
        //public static string Standarize(this string value)
        //{
        //    if (!String.IsNullOrEmpty(value))
        //    {
        //        return value.Normalize().ToUpper();
        //    }
        //    return value;
        //}
        public static string ToDateTimeStamp(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd-HH-mm-ss.ffff");
        }
        public static T GetOrLoad<T>(ref T value, Func<T> loading, Action<T> afterLoaded = null)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                value = loading();
                if (afterLoaded != null)
                {
                    afterLoaded(value);
                }
            }
            return value;
        }
        public static string PluralizeIt(this int amount, string unit, string zeroText = null)
        {
            if (amount == 0 && !String.IsNullOrEmpty(zeroText))
            {
                return zeroText;
            }
            else
            {
                return String.Format("{0} {1}{2}", amount, unit, (amount == 1 ? "" : "s"));
            }
        }

        public static string PluralizeIt(this double amount, string unit, string zeroText = null)
        {
            if (amount == 0.0 && !String.IsNullOrEmpty(zeroText))
            {
                return zeroText;
            }
            else
            {
                return String.Format("{0} {1}{2}", (Math.Round(amount, 1)), unit, (Math.Round(amount, 1) == 1.0 ? "" : "s"));
            }

        }

        public static IEnumerable<T> GetEnumFlagsIntersect<T>(this T flags, T intersection) where T : struct, IConvertible
        {
            var setFlags = flags.GetEnumFlags();
            return setFlags.Where(x => (x.Convert<int>() & intersection.Convert<int>()) > 0).ToList();
        }
        //I don't like this
        public static IEnumerable<T> GetEnumFlags<T>(this T flags) where T : struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("The generic type parameter must be an Enum.");
            }

            if (flags.GetType() != type)
            {
                throw new ArgumentException("The generic type parameter does not match the target type.");
            }

            Enum flagsEnum = (Enum)Enum.Parse(type, flags.ToString());
            ulong flag = 1;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<T>())
            {
                ulong bits = System.Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                if (flag == bits && flagsEnum.HasFlag(value as Enum))
                {
                    yield return value;
                }
            }
        }
        //public static IEnumerable<T> GetEnumFlags<T>(this T value) where T : struct, IConvertible
        //{
        //    var type = typeof(T);
        //    if (!type.IsEnum)
        //    {
        //        throw new ArgumentException("T must be an enumerated type");
        //    }

        //    List<T> list = new List<T>();
        //    var vals = Enum.GetValues(type);
        //    int flag = (int)Enum.Parse(type, value.ToString());
        //    foreach (T v in vals)
        //    {
        //        int i = 0;
        //        if (((int)Enum.Parse(type, v.ToString()) & flag) > 0)
        //        {
        //            list.Add(v);
        //        }
        //    }
        //    return list;
        //}

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
       
        public static string SubstringMax(this string text, int count, bool ending = true, string substringAppend = "")
        {
            if (!String.IsNullOrEmpty(text))
            {
                if (text.Length > count)
                {
                    if (ending)
                    {
                        var subbed = text.Substring(0, count);
                        return subbed + substringAppend;
                    }
                    else
                    {
                        var subbed = text.Substring(text.Length - count, count);
                        return substringAppend + subbed;

                    }
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

        public static string ToQueryString(this object anyObject, bool includeEmptyArguments = false, Func<Object, String> conversion = null)
        {
            conversion = conversion ?? new Func<object, string>(x => x.ToString());

            if (anyObject != null)
            {
                List<string> keyPairs = new List<string>();
                var props = anyObject.ToKeyValuePairs();

                foreach (var prop in props)
                {
                    var pValue = prop.Value;
                    var qsValue = "";

                    if (pValue != null)
                    {
                        qsValue = conversion(pValue);
                    }
                    if (includeEmptyArguments || (!includeEmptyArguments && !String.IsNullOrEmpty(qsValue)))
                    {
                        //I don't know if this encoding is correct: Uri.EscapeUriString
                        keyPairs.Add($"{prop.Key}={Uri.EscapeDataString(qsValue)}");
                    }
                }
                var result = String.Join("&", keyPairs);
                return result;
            }
            return "";
        }
        public static Dictionary<string, object> ToKeyValuePairs(this object anyObject)
        {
            var keyValues = new Dictionary<string, object>();

            if (anyObject != null)
            {
                var props = anyObject.GetType().GetProperties();

                foreach (var prop in props)
                {
                    var pValue = prop.GetValue(anyObject);
                    keyValues.Add(prop.Name, pValue);
                }
            }
            return keyValues;
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

        public static void ForEachIndex<T>(this IEnumerable<T> items, Action<T,int> action)
        {
            if (items != null && items.Any())
            {
                int index = 0;
                foreach (var item in items)
                {
                    action(item, index);
                    index++;
                }
            }
        }
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items != null && items.Any())
            {
                foreach (var item in items)
                {
                    action(item);
                }
            }
        }
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> col)
        {
            if (col != null)
            {
                return !col.Any();
            }
            return true;
        }
        public static IEnumerable<T> ToEnumerable<T>(this T value)
        {
            var x = new List<T>();
            x.Add(value);
            return x;
        }
    }
}
