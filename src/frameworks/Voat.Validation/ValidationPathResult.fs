namespace Voat.Validation

open System
open System.Collections.Generic
open System.Linq.Expressions
open System.Text.RegularExpressions
open System.ComponentModel.DataAnnotations
open Voat.Common.Fs

type ValidationPathResult(errorMessage: string, memberPath: string, validationType: string) =
    inherit ValidationResult(errorMessage, [memberPath])

    let mutable _type = validationType
    do
        if _type.EndsWith("Attribute") then
            _type <- _type.Replace("Attribute","")

    static let getResult (path, errorMessage, validationType) =
        let f, p = Helper.NormalizeExpressionPath path

        let err =
            match errorMessage with
            | null -> "{0} is invalid"
            | _ -> errorMessage

        new ValidationPathResult(String.Format(err, f), p, validationType)

    member val Severity = ValidationSeverity.Error with get, set

    member this.Type with get() = _type and set(value) = _type <- value

    member this.ToEnumerable() = [|this|] :> IEnumerable<ValidationPathResult>

    static member Create<'m, 'prop>(model: 'm, errorMessage: string, path: Expression<Func<'m, 'prop>>) =
        getResult (path, errorMessage, "")

    static member Create<'m, 'prop>(model: 'm, errorMessage: string, validationType: string, path: Expression<Func<'m, 'prop>>) =
        getResult (path, errorMessage, validationType)

    static member Create<'m>(model: 'm, errorMessage: string, validationType: string, path: LambdaExpression) =
        getResult (path, errorMessage, validationType)