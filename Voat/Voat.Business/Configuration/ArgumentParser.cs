using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Voat.Configuration
{
    public static class ArgumentParser
    {
        /// <summary>
        /// Simple formatter for parsing argument lists in the form of [Type](Value),[Type](Value).
        /// </summary>
        /// <param name="argumentValue">The string to be converted to object[]</param>
        public static object[] Parse(string argumentValue)
        {
            List<object> objectList = new List<object>();

            if (!String.IsNullOrEmpty(argumentValue))
            {
                string regEx = @"\[(?<type>[\w\.\s,\]\[`]+)\]\((?<value>[^(]*)\)";
                string[] args = argumentValue.Split(';');
                foreach (string arg in args)
                {
                    var m = Regex.Match(arg, regEx);
                    if (m.Success)
                    {
                        string typeString = m.Groups["type"].Value;
                        string value = m.Groups["value"].Value;
                        if (value.Equals("null", StringComparison.OrdinalIgnoreCase))
                        {
                            value = null;
                        }

                        var type = Type.GetType(typeString);
                        if (type != null)
                        {
                            //Support only primitives, enums, and strings right now.
                            if (type.IsPrimitive || type == typeof(string) || type.IsEnum || type.BaseType == typeof(ValueType))
                            {
                                if (type == typeof(string))
                                {
                                    objectList.Add(value);
                                }
                                else if (type.IsPrimitive || type.BaseType == typeof(ValueType))
                                {
                                    var typeCode = Type.GetTypeCode(type);
                                    if (typeCode != TypeCode.Object)
                                    {
                                        objectList.Add(System.Convert.ChangeType(value, typeCode));
                                    }
                                    else
                                    {
                                        var o = Activator.CreateInstance(type, new object[] { value });
                                        objectList.Add(o);
                                    }
                                }
                                else if (type.IsEnum)
                                {
                                    objectList.Add(Enum.Parse(type, value));
                                }

                            }
                            else
                            {
                                //partial implementation. It will null non-primatives but not construct non-primatives. 
                                if (String.IsNullOrEmpty(value))
                                {
                                    //We have a potentially nullable non-primative, attempt it.
                                    var x = Convert.ChangeType(null, type);
                                    objectList.Add(x);
                                }
                                else
                                {
                                    throw new ArgumentException($"Type {type.FullName} is not supported", "arguments");
                                }
                            }
                        }
                        else
                        {
                            //This shouldn't ever happen...
                            throw new ArgumentException($"Type {typeString} not found");
                        }
                    }
                    else
                    {
                        throw new ArgumentException(String.Format("Can not parse: {0}", arg), argumentValue);
                    }
                }
            }
            return objectList.ToArray();
        }
    }
}
