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
using System.Text.RegularExpressions;

namespace Voat.Common.Configuration
{
    public static class ArgumentParser
    {
        /// <summary>
        /// Simple formatter for parsing argument lists in the form of [Type](Value);[Type](Value).
        /// </summary>
        /// <param name="argumentValue">The string to be converted to object[]</param>
        public static object[] Parse(string argumentValue)
        {
            List<object> objectList = new List<object>();

            if (!String.IsNullOrEmpty(argumentValue))
            {
                string regEx = @"\[(?<type>[\w\.\s,\]\[`]+)\]\((?<value>[^(]*)\)";
                List<string> argumentPairs = new List<string>();

                //manually split arg list if propery delimited
                var delim = ");[";
                int lastIndex = 0;
                int index = argumentValue.IndexOf(delim);
                if (index >= 0)
                {
                    while (index >= 0)
                    {
                        var argumentPair = argumentValue.Substring(lastIndex, index - lastIndex + 1);
                        argumentPairs.Add(argumentPair);
                        lastIndex = index + delim.Length - 1;
                        index = argumentValue.IndexOf(delim, index + delim.Length);
                    }
                    //add tailing
                    if (lastIndex > 0)
                    {
                        var argumentPair = argumentValue.Substring(lastIndex, argumentValue.Length - lastIndex);
                        argumentPairs.Add(argumentPair);
                    }
                }
                else
                {
                    argumentPairs.Add(argumentValue);
                }

                //parse
                foreach (string arg in argumentPairs)
                {
                    var matches = Regex.Matches(arg, regEx);
                    foreach (Match m in matches)
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
                                        //Check for Parse(string) method and invoke it if found (This covers System.TimeSpan parsing)
                                        var parseMethod = type.GetMethod("Parse", new[] { typeof(string) });
                                        if (parseMethod != null)
                                        {

                                            var parsedObject = parseMethod.Invoke(type, new object[] { value });
                                            objectList.Add(parsedObject);
                                        }
                                        else
                                        {
                                            var o = Activator.CreateInstance(type, new object[] { value });
                                            objectList.Add(o);
                                        }
                                    }
                                }
                                else if (type.IsEnum)
                                {
                                    objectList.Add(Enum.Parse(type, value, true));
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
                    //else
                    //{
                    //    throw new ArgumentException(String.Format("Can not parse: {0}", arg), argumentValue);
                    //}
                }
            }
            return objectList.ToArray();
        }
    }
}
