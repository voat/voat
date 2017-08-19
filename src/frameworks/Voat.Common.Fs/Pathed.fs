namespace Voat.Common.Fs

open System
open System.Linq.Expressions
open System.Diagnostics
open Newtonsoft.Json

type Pathed() =

    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _path = ""

    [<DebuggerBrowsable(DebuggerBrowsableState.Never)>]
    let mutable _pathExpression: LambdaExpression = null

    [<JsonIgnore>]
    member this.PathExpression
        with get() : LambdaExpression =
            _pathExpression
        and set(value) =
            _pathExpression <- value
            if _pathExpression <> null then
                let (name, path) = Helper.NormalizeExpressionPath(_pathExpression)
                _path <- path

    member this.Path
        with get() : string =
            _path
        and set(value) =
            _path <- value