using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Voat.Common
{
    public class DiscoveredType<A> where A : Attribute
    {
        public Type Type { get; set; }
        public A Attribute { get; set; }
    }
    public static class TypeDiscovery
    {
        public static IEnumerable<DiscoveredType<A>> DiscoverTypes<A>(IEnumerable<Assembly> assemblies, bool inherit = false, Type subClassRestriction = null) where A : Attribute
        {
            var discovered = new List<DiscoveredType<A>>();

            if (assemblies != null && assemblies.Count() > 0)
            {
                foreach (Assembly assembly in assemblies)
                {
                    var tempDiscovered = new List<DiscoveredType<A>>();
                    if (assembly != null)
                    {
                        var types = assembly.GetTypes();
                        if (types != null && types.Length > 0)
                        {
                            foreach (Type type in types)
                            {
                                //Ensure a rule type
                                if (subClassRestriction == null || (subClassRestriction != null && type.IsSubclassOf(subClassRestriction)))
                                {
                                    var attribute = type.GetCustomAttribute<A>(inherit);
                                    if (attribute != null)
                                    {
                                        tempDiscovered.Add(new DiscoveredType<A>() { Type = type, Attribute = attribute });
                                    }
                                }
                            }
                        }
                    }
                    discovered.AddRange(tempDiscovered);
                }
            }
            return discovered;
        }
    }
}
