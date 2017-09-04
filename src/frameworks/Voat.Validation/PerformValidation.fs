namespace Voat.Validation

open System.ComponentModel.DataAnnotations
open System
open System.Reflection
open System.Collections.Generic

/// <summary>Marker Attribute for forcing validator to validate this type</summary>
[<AttributeUsage(AttributeTargets.Property ||| AttributeTargets.Class, AllowMultiple = false)>]
type PerformValidation() =
    inherit Attribute()

/// <summary>Marker Interface for forcing validator to validate this type</summary>
type IPerformValidation = interface end

