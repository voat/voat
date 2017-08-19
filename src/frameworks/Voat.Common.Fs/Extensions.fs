module Extensions

open System
open System.Collections.Generic

type System.String with
    member this.ConcatIf( d: string ) =
       let x =
           match String.IsNullOrEmpty(this) with
           | true -> this
           | false -> String.Concat(this, d)
       x

type Object with
    member this.ToEnumerable() = [|this|]
