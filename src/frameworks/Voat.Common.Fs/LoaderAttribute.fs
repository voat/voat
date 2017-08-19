namespace Voat.Common.Fs

open System
open System.Reflection
open System.Collections.Generic

// indirect loading
[<AttributeUsage(AttributeTargets.Class, AllowMultiple=true)>]
type LoaderAttribute(loaderType:Type) =
    inherit Attribute()

    let _loaderType = loaderType

    member this.LoaderType with get() = _loaderType

    new(loaderType:String) =

        let foundType = match Type.GetType(loaderType) with
                | null -> raise(new ArgumentException(String.Format("Type '{0}' can not be loaded.", loaderType)))
                | _ as x -> x

        new LoaderAttribute(foundType)