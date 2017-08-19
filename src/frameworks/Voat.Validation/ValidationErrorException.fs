namespace Voat.Validation

open System
open System.Collections.Generic
open System.Reflection
open System.ComponentModel.DataAnnotations
open Voat.Validation

type ValidationErrorException(validationResult: ValidationResult, validationAttribute: ValidationAttribute, model: Object) =
    inherit ValidationException(validationResult, validationAttribute, model)
    let allValResults = new List<ValidationResult>()

    member this.Results with get() = allValResults :> IEnumerable<ValidationResult>

    new(validationResults: IEnumerable<ValidationResult>, model: Object) =
        let first = Seq.find(fun x -> true) (validationResults)
        ValidationErrorException(first, null, model)