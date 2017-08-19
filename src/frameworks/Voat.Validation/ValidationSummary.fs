namespace Voat.Validation

open System.Collections.Generic

open System.ComponentModel.DataAnnotations
open Newtonsoft.Json

type ValidationSummary() =

    let mutable _violations = new Dictionary<string, List<ValidationViolation>>()

    let getViolations severity =

        let d =
            _violations
            |> Seq.map(fun x ->
                let v = x.Value
                        |> Seq.filter(fun v -> v.Severity = severity)

                (x.Key, new List<ValidationViolation>(v))
            )
            |> dict

        let nd = new Dictionary<string, List<ValidationViolation>>()

        for e in d do
            if e.Value.Count > 0 then
                nd.Add(e.Key, e.Value)

        nd

    member this.Violations with get() = _violations and set(value) = _violations <- value

    [<JsonIgnore>]
    member this.Errors with get() = getViolations ValidationSeverity.Error

    [<JsonIgnore>]
    member this.Warnings with get() = getViolations ValidationSeverity.Warning

    [<JsonIgnore>]
    member this.IsValid with get() = _violations.Count = 0

    [<JsonIgnore>]
    member this.HasErrors with get() = this.Errors.Count > 0

    [<JsonIgnore>]
    member this.HasWarnings with get() = this.Warnings.Count > 0

    static member Map(validationResults: IEnumerable<ValidationResult>) =

        let result = new ValidationSummary()

        let addToError (key, validationResult: ValidationResult) =

            if not (result.Errors.ContainsKey(key)) then
                result.Violations.Add(key, new List<ValidationViolation>())

            let errorList = result.Violations.Item(key)

            let meta =
                match validationResult with
                | :? ValidationPathResult as x -> (x.Severity, x.Type)
                | _ -> (ValidationSeverity.Error, "")

            let s, t = meta;

            errorList.Add(new ValidationViolation(s, validationResult.ErrorMessage, t))

        match validationResults with
        | null -> ()
        | _ as valResult ->
            valResult
            |> Seq.iter(fun(x) ->
                x.MemberNames
                |> Seq.iter(fun name -> addToError(name, x))
            )

        result

//    //for use in when accessing validation of domain models
//    static member Map(validationResults: IEnumerable<ModelValidationResult>) =
//
//        let result = new ValidationSummary()
//
//        let addToError (key, modelValidation: ModelValidationResult) =
//
//            if not (result.Errors.ContainsKey(key)) then
//                result.Errors.Add(key, new List<ValidationViolation>())
//
//            let errorList = result.Errors.Item(key)
//
//            errorList.Add(new ValidationViolation(ValidationSeverity.Error, modelValidation.Message))
//
//        validationResults
//        |> Seq.iter(fun(x) -> addToError(x.MemberName, x))
//
//        result
//
//    //for use in MVC/API when validating incoming models using ModelState object
//    static member Map(dict: ModelStateDictionary) =
//
//        let result = new ValidationSummary()
//
//        let addToError (key, modelError: ModelState) =
//
//            if not (result.Errors.ContainsKey(key)) then
//                result.Errors.Add(key, new List<ValidationViolation>())
//
//            let errorList = result.Errors.Item(key)
//
//            modelError.Errors
//            |> Seq.iter(fun (x) -> errorList.Add(new ValidationViolation(ValidationSeverity.Error, x.ErrorMessage)))
//
//
//        Seq.iter(fun(x) -> addToError(x, dict.Item(x))) dict.Keys
//
//        result