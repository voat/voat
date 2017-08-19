module ActivePatterns

open System
open System.Collections.Generic
open System.Reflection
open System.Collections
open System.Linq.Expressions
open System.Text.RegularExpressions

let (|Dictionary|_|) (o: obj) =
    if (o :? IDictionary) then
        Some(o :?> IDictionary)
    else
        None

let (|PrimativeType|ListType|DictionaryType|ObjectType|) (t: Type) =
    if ((t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Nullable<_>>) || t.IsPrimitive || t = typeof<String> || t.IsEnum || t.GetInterface("IComparable") <> null) then
        PrimativeType
    elif (typeof<IEnumerable<_>>.IsAssignableFrom(t) || typeof<IList>.IsAssignableFrom(t)) then
        ListType
    elif (typeof<IDictionary>.IsAssignableFrom(t)) then
        DictionaryType
    else
        ObjectType

let (|LessThan|_|) k value = if value < k then Some() else None

let (|MoreThan|_|) k value = if value > k then Some() else None