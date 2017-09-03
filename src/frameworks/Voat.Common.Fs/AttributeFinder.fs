namespace Voat.Common.Fs

open System
open System.Collections.Generic
open System.Reflection

type AttributeFinder() =
    //Storage
    static let _cache = new Dictionary<Type, Dictionary<Type, Dictionary<MemberInfo, List<Attribute>>>>()

    static let getValidatableType(objType:Type) =
        [objType]
        |> Seq.append(objType.GetInterfaces())
        |> Seq.pick(fun interfaceType ->
            match interfaceType with
            | interfaceType when interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() = typedefof<IDictionary<_, _>> -> Some(interfaceType.GenericTypeArguments.[1])
            | interfaceType when interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() = typedefof<IEnumerable<_>> -> Some(interfaceType.GenericTypeArguments.[0])
            | oType when interfaceType = objType -> Some(oType)
            | _ -> None)

    static let getValidatableProperties(objType: Type) =
            objType.GetProperties(BindingFlags.Instance ||| BindingFlags.Public)
            |> Seq.filter(fun x -> x.PropertyType.IsClass && not x.PropertyType.IsPrimitive && x.PropertyType <> typeof<string>)
            |> Seq.sortBy(fun x -> x.Name) //Sort by name, might need to take out for performance later

    static let getGroupedValidatableProperties(objType: Type) =
            getValidatableProperties objType
            |> Seq.map(fun p -> (p, getValidatableType p.PropertyType))

    static member private filterResults<'t when 't :> Attribute>(validations: Dictionary<Type, Dictionary<MemberInfo, List<Attribute>>>, filter: Func<Attribute, bool>) =
        validations
        |> Seq.map(fun typeEntry ->
            match typeEntry with
            | KeyValue(t, typeatts) ->
                    typeatts
                    |> Seq.map(fun entry ->
                        match entry with
                        | KeyValue(mi, atts) ->
                            atts
                            |> Seq.filter(fun att -> filter.Invoke(att))
                            |> Seq.groupBy(fun att -> mi)
                            |> Seq.map(fun(key, value) ->
                                let x = new List<Attribute>()
                                Seq.iter(fun v -> x.Add(v)) value
                                (key, x)
                            )
                    )
                    |> Seq.collect(fun mi -> mi)
                    |> Seq.fold(fun dict mi ->
                        match mi with
                        | (mi, value) ->
                            let typeDict = dict :> Dictionary<Type, Dictionary<MemberInfo, List<Attribute>>>
                            match typeDict.ContainsKey(t) with
                            | true -> typeDict.[t].Add(mi, value)
                            | false ->
                                let newMemberInfo = new Dictionary<MemberInfo, List<Attribute>>()
                                newMemberInfo.Add(mi, value)
                                typeDict.Add(t, newMemberInfo)
                            dict
                    ) (new Dictionary<Type, Dictionary<MemberInfo, List<Attribute>>>())
        )
        |> Seq.find(fun x -> true)

    static member FindValidatableProperties(objType: Type) =
            getGroupedValidatableProperties objType

    static member Find<'t when 't :> Attribute>(objectType: Type, recursiveLoad: bool, useCache: bool, filter: Func<Attribute, bool>) =

        let attType = typeof<'t>
        //TODO: Need to try to use IDictionary
        let result = new Dictionary<Type, Dictionary<MemberInfo, List<Attribute>>>() 

        let getValidationDictionary objectType =
            match useCache && _cache.ContainsKey(attType) && _cache.Item(attType).ContainsKey(objectType) with
                | true -> _cache.Item(attType).Item(objectType)
                | false ->
                    let atts = new Dictionary<MemberInfo, List<Attribute>>()

                    objectType.GetProperties(BindingFlags.Instance ||| BindingFlags.Public)
                    |> Seq.map(fun propinfo -> (propinfo, propinfo.GetCustomAttributes(recursiveLoad)))
                    |> Seq.collect(fun(propinfo, attributes) -> Seq.map(fun a -> (propinfo, a)) attributes)
                    |> Seq.filter(fun(propinfo, attribute) -> attribute :? 't)
                    |> Seq.map(fun(propinfo, attribute) -> (propinfo :> MemberInfo, attribute :?> 't))
                    |> Seq.append( (* this is bad code *)
                        objectType.GetCustomAttributes(recursiveLoad)
                        |> Seq.filter(fun(attribute) -> attribute :? 't)
                        |> Seq.map(fun(attribute) -> (objectType.GetTypeInfo() :> MemberInfo, attribute :?> 't)))
                    |> Seq.append(
                        match objectType.GetCustomAttributes(typeof<LoaderAttribute>, recursiveLoad) with
                        | null -> Seq.empty
                        | _ as x ->
                            x
                            |> Seq.collect (fun x ->
                                    (x :?> LoaderAttribute).LoaderType.GetCustomAttributes()
                                )
                            |> Seq.filter(fun(attribute) -> attribute :? 't)
                            |> Seq.map(fun(attribute) -> (objectType.GetTypeInfo() :> MemberInfo, attribute :?> 't))
                       )
                    |> Seq.iter(fun(propinfo, valatt) ->
                        match atts.ContainsKey(propinfo) with
                            | true -> atts.Item(propinfo).Add(valatt)
                            | false ->
                                atts.Add(propinfo, new List<Attribute>(seq { yield valatt :> Attribute;}))
                       )

                    //Add to cache
                    if (_cache.ContainsKey(attType) = false) then
                        let entry = new Dictionary<Type, Dictionary<MemberInfo, List<Attribute>>>();
                        entry.Add(objectType, atts)
                        _cache.Add(typeof<'t>, entry)
                    else
                        let mainEntry = _cache.Item(attType)

                        if mainEntry.ContainsKey(objectType) = false then
                            mainEntry.Item(objectType) <- atts
                        else
                            atts.Keys
                            |> Seq.iter(fun x -> mainEntry.Item(objectType).Add(x, atts.Item(x)))
                    atts

        let vals = getValidationDictionary objectType
        result.Add(objectType, vals)

        if recursiveLoad then
            //find rec
            getGroupedValidatableProperties objectType
            |> Seq.map(fun(p, t) -> (t, getValidationDictionary t))
            |> Seq.filter(fun(t, vals) -> not(result.ContainsKey(t)))
            |> Seq.iter(fun(t, vals) -> result.Add(t, vals))
            |> ignore

        match filter with
        | null -> result
        | _ -> AttributeFinder.filterResults<'t>(result, filter)

    static member Find<'t when 't :> Attribute>(objectType: Type, filter: Func<Attribute, bool>) =
        AttributeFinder.Find<'t>(objectType, true, true, filter)

    static member Find<'t when 't :> Attribute>(objectType: Type) =
        AttributeFinder.Find<'t>(objectType, true, true, null)