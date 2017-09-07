namespace Voat.Validation

open System
open System.Collections.Generic
open System.Linq.Expressions
open System.Text.RegularExpressions
open System.ComponentModel.DataAnnotations
open Voat.Common.Fs
open System.IO
open System.Xml.XPath
open System.Xml


type ValidationPathResult(errorMessage: string, memberPath: string, validationType: string) as this =
    inherit ValidationResult(errorMessage, [memberPath])
    
    let mutable _type: string = ValidationPathResult.CleanType(validationType)
    let mutable _pathExpression: LambdaExpression = null
   
    member this.PathExpression with get() = _pathExpression and set(value) = _pathExpression <- value

    member val Severity = ValidationSeverity.Error with get, set

    member this.Type
        with get() : string = 
            _type
        and set(value : string)  = 
           _type <- ValidationPathResult.CleanType(value)

    member this.ToEnumerable() = [|this|] :> IEnumerable<ValidationPathResult>

    new (errorMessage: string, pathExpression: LambdaExpression, validationType: string) as x = 
        let f, p = Helper.NormalizeExpressionPath pathExpression

        let err =
            match errorMessage with
            | null -> String.Format("{0} is invalid", f)
            | _ -> 
                if errorMessage.Contains("{0}") then 
                    String.Format(errorMessage, f)
                else
                    errorMessage
        
        new ValidationPathResult (err, p, validationType) then 
        x.PathExpression <- pathExpression

    static member CleanType (x) : string = 
        if x.EndsWith("Attribute") then 
            x.Replace("Attribute","")
        else
            x

    static member Create<'m, 'prop>(model: 'm, errorMessage: string, path: Expression<Func<'m, 'prop>>) =
        ValidationPathResult (errorMessage, path, "")

    static member Create<'m, 'prop>(model: 'm, errorMessage: string, validationType: string, path: Expression<Func<'m, 'prop>>) =
        ValidationPathResult (errorMessage, path, validationType)

    static member Create<'m>(model: 'm, errorMessage: string, validationType: string, path: LambdaExpression) =
        ValidationPathResult (errorMessage, path, validationType)