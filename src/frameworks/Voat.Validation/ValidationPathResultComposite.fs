namespace Voat.Validation

open System
open System.Collections.Generic
open System.Linq.Expressions
open System.Text.RegularExpressions
open System.ComponentModel.DataAnnotations

type ValidationPathResultComposite(results: IEnumerable<ValidationPathResult>) =
    inherit ValidationResult("Validation exceptions were found", results |> Seq.map(fun x -> x.MemberNames) |> Seq.collect(fun x -> x))

    member this.Results with get() = results