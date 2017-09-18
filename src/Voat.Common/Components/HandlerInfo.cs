using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Voat.Common.Configuration
{
    public class HandlerInfo
    {
        public bool Enabled { get; set; }
        /// <summary>
        /// Friendly Name for this type
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Full TypeName for type to be constructed and loaded via Reflection
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Constructor arguments. Formatted string parsed by ArgumentParser.Parse(value)
        /// </summary>
        public string Arguments { get; set; }
        /// <summary>
        /// Property values for setting state after type is constructed. Formatted value string parsed by ArgumentParser.Parse(value)
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        public T Construct<T>()
        {
            var type = System.Type.GetType(this.Type);
            if (type != null)
            {
                return Construct<T>(type, Arguments, Properties);
            }
            throw new InvalidOperationException(String.Format("Can not find type: {0}", this.Type));
        }
        public static T Construct<T>(Type type, string arguments, Dictionary<string, string> properties = null)
        {
            T t = default(T);
            if (type != null)
            {
                object[] args = ArgumentParser.Parse(arguments);
                t = (T)Activator.CreateInstance(type, args);
                if (properties != null && properties.Count > 0)
                {
                    foreach (var key in properties.Keys)
                    {
                        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.IgnoreCase;
                        var propertyInfo = type.GetProperty(key, flags);
                        if (propertyInfo != null)
                        {
                            var valueString = properties[key];
                            var argument = ArgumentParser.Parse(valueString);
                            propertyInfo.SetValue(t, argument[0]);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Can not find property {key} on type {type.Name}");
                        }
                    }
                }
            }
            return t;
        }
    }
}
