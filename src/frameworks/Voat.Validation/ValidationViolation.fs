namespace Voat.Validation

open System
open System.Collections.Generic
open System.Linq.Expressions
open System.Text.RegularExpressions

type ValidationViolation(validationSeverity: ValidationSeverity, message: string, validationType: string) =

    let mutable _severity = validationSeverity

    let mutable _type = match validationType with | null -> String.Empty | _ -> match validationType.Contains("Attribute") with | true -> validationType.Replace("Attribute", "") | false -> validationType

    member this.Type with get() = _type and set value = _type <- value

    member this.Severity with get() = _severity and set value = _severity <- value

    member this.Message with get() = message

    override this.ToString() =
        String.Format("{0}: {1}", this.Severity, this.Message)