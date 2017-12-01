namespace Voat.Validation

open System.ComponentModel.DataAnnotations
open System
open System.Reflection
open System.Collections.Generic
open System.Linq

// Try to use this for types in the future
type IDataValidator =
    abstract member Validate: Object * context: ValidationContext -> IEnumerable<ValidationPathResult>

// WIP, trying to standardize base type
[<AbstractClass>]
type DataValidator<'m>(ruleID) =
    let ruleID = match String.IsNullOrEmpty(ruleID) with | true -> String.Empty | _ -> ruleID

    abstract member Validate: 'm * context: ValidationContext -> IEnumerable<ValidationPathResult>

    ///<summary>
    /// Identifier
    ///</summary>
    member this.RuleID with get() = ruleID

    ///<summary>
    /// Distinct ID
    ///</summary>
    member this.UniqueID
        with get() =
            let format =
                if String.IsNullOrEmpty(this.RuleID) then
                    "{0}<{1}>"
                else
                    "{0}<{1}>:{2}"
            String.Format(format, this.GetType().Name, typeof<'m>.Name, this.RuleID)

    //enc violation
    interface IDataValidator with
        member this.Validate(model, context) =
            match this.Validate(model :?> 'm, context) with
            | null -> Seq.empty
            | _ as x -> x

    new() =
        DataValidator("")

///<summary>
/// Ref DataValidator<'m> objects -> validatable objects
///</summary>
[<AttributeUsage(AttributeTargets.Class ||| AttributeTargets.Property, AllowMultiple=true)>]
type DataValidationAttribute(validator:Type, pipeline: String, stage: String, order: Int32) =
    inherit ValidationAttribute()

    let _validatorType = validator
    let _pipeline = pipeline
    let _order = order
    let _stage = stage

    member this.ValidationProvider
        with get() : IDataValidator =
            match _validatorType with
            | null ->
                raise (new ArgumentException("validator Type is required"))
            | _ ->
                match _validatorType.GetInterface(typeof<IDataValidator>.Name) with
                | null -> raise (new ArgumentException("validator Type doesn't implement IDataValidator"))
                | _ -> Activator.CreateInstance(_validatorType) :?> IDataValidator

    member this.Pipeline with get() = match _pipeline with | null -> String.Empty | _ -> _pipeline
    member this.Order with get() = _order
    member this.Stage with get() = match _stage with | null -> String.Empty | _ -> _stage

    override this.IsValid(model, context) =
        let validations = this.ValidationProvider.Validate(model, context)
        let validation = 
            match Seq.toList(validations) with
                | [] -> null
                //| [ _ ] -> Seq.find(fun x -> true) (validations) :> ValidationResult
                //| null -> null
                | _ as x ->
                    let r = x
                            |> Seq.map(fun v ->
                                if String.IsNullOrEmpty(v.Type) then
                                    v.Type <- validator.Name
                                v
                            )
                    new ValidationPathResultComposite(r) :> ValidationResult
        validation

    new(validator:Type) =
        //default run
        new DataValidationAttribute(validator,"","", -1)