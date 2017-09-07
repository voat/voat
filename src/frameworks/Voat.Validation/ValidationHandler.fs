namespace Voat.Validation

open System
open System.ComponentModel.DataAnnotations
open System.Reflection
open System.Collections.Generic
open System.Linq
open System.Linq.Expressions
open System.Collections
open Voat.Common.Fs

type ValidationHandler() =

    static member Validate<'t when 't: equality and 't: null>(model: 't, contextDictionary: Dictionary<Object, Object>, throw: bool, filter: Func<ValidationAttribute, bool>) =

        let validationResults = new List<ValidationResult>()

        if not (model = null) then

            let root = Expression.Parameter(model.GetType(), "Model")

            let convertIfNotType (obj: Object, typeDef: Type, expression: Expression) = 
                let runtimeType = obj.GetType();
                if runtimeType = typeDef then
                    expression
                else
                    Expression.Convert(expression, runtimeType) :> Expression
                
            let runValidate (valAtt: ValidationAttribute, model, modelRelative, validationResults: List<ValidationResult>, contextDictionary, resultFun) =
                match valAtt.GetValidationResult(modelRelative, new ValidationContext(model, contextDictionary)) with
                | null -> ()
                | vr ->
                    match vr with
                    | :? ValidationPathResultComposite as c ->
                        validationResults.AddRange(Seq.map(fun x ->
                                x :> ValidationResult
                                ) (c.Results))
                    | :? ValidationPathResult as x ->
                        validationResults.Add(x)
                    | _ -> resultFun (valAtt :> Object, vr)

            let rec evaluateType(model, expression: Expression) =
                match model with
                | null -> ()
                | _ ->
                    let t = model.GetType()

                    let handleValidationResult (validator: Object, valResponse: ValidationResult) =
                        match valResponse with
                        | :? ValidationPathResult as pathResult -> 
                            match pathResult.PathExpression with 
                                | _ -> 
                                    match pathResult.PathExpression.Body with 
                                        | :? System.Linq.Expressions.MemberExpression as m ->
                                            let newPath = convertIfNotType (model, t, Expression.MakeMemberAccess(expression, m.Member))
                                            let lambda = Expression.Lambda(newPath)
                                            validationResults.Add(new ValidationPathResult(pathResult.ErrorMessage, lambda, pathResult.Type))
                                        | _ -> validationResults.Add(pathResult)
                                | null ->
                                    validationResults.Add(pathResult)
                                    
                                   
                        | _ ->
                            //this only finds the first member name in a regular ValidationResult object, will need to update this maybe
                            let del = typedefof<Func<_,_>>.MakeGenericType(root.Type, t)
                            let name = match validator with 
                                | null -> t.Name
                                | _ -> validator.GetType().Name
                            let lambda = Expression.Lambda(del, expression, root)
                            let f, p = Helper.NormalizeExpressionPath lambda
                            let memberName = valResponse.MemberNames.FirstOrDefault()
                            let fullPath = match String.IsNullOrEmpty(memberName) with 
                                | true -> p
                                | false -> String.Concat(p, ".", memberName)
                            validationResults.Add(new ValidationPathResult(valResponse.ErrorMessage, fullPath, name))
                            //validationResults.Add(ValidationPathResult.Create(model, valResponse.ErrorMessage, name, Expression.Lambda(del, expression, root)))
                        
                    //type check
                    match box model with
                    | :? IValidatableObject as x ->
                        match x.Validate(new ValidationContext(model, contextDictionary)) with
                        | null -> ()
                        | vals ->
                            vals
                            |> Seq.iter(fun v -> handleValidationResult(x, v))
                    | _ -> ()

                    let vals = AttributeFinder.Find<ValidationAttribute>(t, true, true, fun x -> filter.Invoke(x :?> ValidationAttribute))

                    if vals.ContainsKey(t) then
                        let currentObject = vals.[t]

                        //type atts
                        if currentObject.ContainsKey(model.GetType()) then
                            currentObject.Item(model.GetType())
                            |> Seq.cast<ValidationAttribute>
                            |> Seq.iter(fun v -> runValidate(v, model, model, validationResults, contextDictionary, handleValidationResult))

                        //property atts
                        currentObject.Keys
                        |> Seq.filter(fun x -> match x with
                                        | :? PropertyInfo -> true
                                        | _ -> false)
                        |> Seq.map(fun p -> p :?> PropertyInfo)
                        |> Seq.iter(fun p -> currentObject.Item(p)
                                                |> Seq.cast<ValidationAttribute>
                                                |> Seq.iter(fun v ->
                                                    let m = p.GetValue(model)
                                                    let handleValidationResultForProperty (validator: Object, valResponse: ValidationResult) =
                                                        let del = typedefof<Func<_,_>>.MakeGenericType(root.Type, p.PropertyType)
                                                        let property = Expression.Property(convertIfNotType(model, p.ReflectedType, expression), p.Name);
                                                        //let property = Expression.Property(Expression.Convert(expression, p.ReflectedType), p.Name);
                                                        validationResults.Add(ValidationPathResult.Create(model, v.FormatErrorMessage(p.Name), validator.GetType().Name, Expression.Lambda(del, property, [|root|])))

                                                    runValidate (v, model, m, validationResults, contextDictionary, handleValidationResultForProperty)
                                                    )
                                    )
                        |> ignore

                    //loop
                    let validatableProperties = AttributeFinder.FindValidatableProperties(t)
                    validatableProperties
                    |> Seq.filter(fun (p, t) -> 
                        //we don't care about the overhead here because each type will be cached
                        let atts = AttributeFinder.Find<ValidationAttribute>(t, true, true, fun x -> filter.Invoke(x :?> ValidationAttribute))
                        match atts.ContainsKey(t) with
                        | true -> true
                        | false -> 
                            if typedefof<IValidatableObject>.IsAssignableFrom(t) || typedefof<IPerformValidation>.IsAssignableFrom(t) then
                                true
                            else 
                                p.GetCustomAttributes(typedefof<PerformValidation>).Any() 
                       )
                    |> Seq.iter(fun (p, t) ->
                        match p.PropertyType.GetInterface(typeof<IEnumerable>.Name) with
                        | null ->
                            let newRoot = Expression.MakeMemberAccess(expression, p);
                            evaluateType (p.GetValue(model), newRoot) 
                        | _ ->
                            match p.GetValue(model) with
                            | null -> ()
                            | collection ->
                                let property = Expression.Property(expression, p)
                                match collection with
                                | :? IEnumerable<_> as col ->
                                    Seq.iteri(fun i x ->
                                        //let newRoot = Expression.Convert(Expression.MakeIndex(property, p.PropertyType.GetProperty("Item"), [|Expression.Constant(i)|]), x.GetType())
                                        //evaluateType(x, newRoot)
                                        let newRoot = Expression.MakeIndex(property, p.PropertyType.GetProperty("Item"), [|Expression.Constant(i)|])
                                        evaluateType(x, convertIfNotType (x, t, newRoot))
                                    ) (col :?> IEnumerable<_>)
                                | _ ->
                                    for entry in (collection :?> IEnumerable) do
                                        let etype = entry.GetType()
                                        let key = etype.GetProperty("Key").GetValue(entry)
                                        let value = etype.GetProperty("Value").GetValue(entry)
                                        let newRoot = Expression.MakeIndex(property, p.PropertyType.GetProperty("Item"), [|Expression.Constant(key)|])
                                        evaluateType (value, convertIfNotType (value, t, newRoot))
                    )

            evaluateType (model :> Object, root)

            let handleValidationResults(model, results: List<ValidationResult>, throw) =
                match results with
                | null -> null
                | _ when results.Count = 0 -> null
                | _ ->
                    match throw with
                    | true -> raise (new ValidationErrorException(results, model))
                    | false -> results

            handleValidationResults(model, validationResults, throw)
        else
            validationResults

    static member Validate<'t when 't: equality and 't: null>(model: 't, contextDictionary: Dictionary<Object, Object>, throw: bool) =
        ValidationHandler.Validate(model, contextDictionary, throw, fun x -> true)

    static member Validate<'t when 't: equality and 't: null>(model: 't, contextDictionary: Dictionary<Object, Object>) =
        ValidationHandler.Validate(model, contextDictionary, false)

    static member Validate<'t when 't: equality and 't: null>(model: 't) =
        ValidationHandler.Validate(model, new Dictionary<Object, Object>(), false)