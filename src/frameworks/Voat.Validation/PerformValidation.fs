namespace Voat.Validation

open System.ComponentModel.DataAnnotations
open System
open System.Reflection
open System.Collections.Generic

[<AttributeUsage(AttributeTargets.Property ||| AttributeTargets.Class, AllowMultiple=true)>]
type PerformValidation() =
    inherit ValidationAttribute()

     override this.IsValid(model, context) =
        null