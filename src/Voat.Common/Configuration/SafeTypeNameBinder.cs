using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Voat.Configuration
{
    /// <summary>
    /// Restricts Newtonsoft.Json $type resolution (TypeNameHandling) to an allow-list.
    ///
    /// TypeNameHandling.All writes and reads type names for polymorphic payloads
    /// (vote restriction/outcome options, Redis cache entries, command round-trips).
    /// Left unrestricted, any type name in the JSON is instantiated on deserialize,
    /// which is the well-known "$type gadget" attack (e.g. ObjectDataProvider RCE).
    ///
    /// Allowed here:
    ///  - inert framework primitives/value types (string, Guid, DateTime, enums, ...)
    ///  - generic collections whose type arguments are themselves allowed
    ///  - inert JSON data containers (Newtonsoft.Json.Linq.*, System.Text.Json.*)
    ///  - types owned by this code base (any "Voat*" assembly)
    /// Everything else is rejected with a JsonSerializationException.
    /// </summary>
    public class SafeTypeNameBinder : DefaultSerializationBinder
    {
        private static readonly HashSet<Type> _alwaysAllowed = new HashSet<Type>
        {
            typeof(string), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset),
            typeof(TimeSpan), typeof(Guid), typeof(Uri), typeof(object),
            typeof(System.Dynamic.ExpandoObject)
        };

        public override Type BindToType(string assemblyName, string typeName)
        {
            var type = base.BindToType(assemblyName, typeName);
            if (type != null && !IsAllowed(type))
            {
                throw new JsonSerializationException(
                    $"Type '{typeName}' from assembly '{assemblyName}' is not in the deserialization allow-list.");
            }
            return type;
        }

        public static bool IsAllowed(Type type)
        {
            if (type == null)
            {
                return true;
            }
            if (type.IsPrimitive || type.IsEnum || _alwaysAllowed.Contains(type))
            {
                return true;
            }
            if (Nullable.GetUnderlyingType(type) != null)
            {
                return IsAllowed(Nullable.GetUnderlyingType(type));
            }
            if (type.IsArray)
            {
                return IsAllowed(type.GetElementType());
            }
            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                var isCollection =
                    definition == typeof(List<>) ||
                    definition == typeof(Dictionary<,>) ||
                    definition == typeof(HashSet<>) ||
                    definition == typeof(KeyValuePair<,>) ||
                    definition == typeof(ICollection<>) ||
                    definition == typeof(IList<>) ||
                    definition == typeof(IEnumerable<>) ||
                    definition == typeof(IReadOnlyList<>) ||
                    definition == typeof(IReadOnlyCollection<>) ||
                    definition == typeof(IReadOnlyDictionary<,>) ||
                    definition == typeof(Collection<>) ||
                    definition == typeof(ObservableCollection<>);

                if (isCollection || Nullable.GetUnderlyingType(type) != null)
                {
                    foreach (var argument in type.GetGenericArguments())
                    {
                        if (!IsAllowed(argument))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }

            // Types owned by this code base (Voat.Business, Voat.Common, Voat.Data,
            // Voat.Voting, Voat.Validation, Voat.Http, test assemblies, ...).
            var assemblyName = type.Assembly.GetName().Name ?? String.Empty;
            if (assemblyName.StartsWith("Voat", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Inert JSON data containers only — not the rest of those assemblies.
            var ns = type.Namespace ?? String.Empty;
            return ns.StartsWith("Newtonsoft.Json.Linq", StringComparison.Ordinal)
                || ns.StartsWith("System.Text.Json", StringComparison.Ordinal);
        }
    }
}
