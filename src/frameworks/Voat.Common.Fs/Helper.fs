namespace Voat.Common.Fs

open System
open System.Collections.Generic
open System.Linq.Expressions
open System.Text.RegularExpressions

type Helper() =

    static member ConcatIf (expr: LambdaExpression) =
        Helper.NormalizeExpressionPath(expr, "")

    static member NormalizeExpressionPath (expr: LambdaExpression) =
        Helper.NormalizeExpressionPath(expr, "")

    static member NormalizeExpressionPath (expr: LambdaExpression, rootName: string) =
        //Future People: try to get rid of this 
        let expression = expr.Body
        let args = [
                    (@"\.Item\[(?<index>(\d+|"".+"")+)\]", "[$1]"); //assuming that an indexer is either a numeric or a "string" value.
                    (@"\.get_Item\((?<index>value\([\.\w+<>_]*\).\w*)\)", "[$1]"); //matching hoisted lambda expressions
                    (@"\.get_Item\((?<index>.*)\)", "[$1]");
                    ("\.get_Chars\((?<index>[\d]*)\)", "[$1]"); //Stupid String.Chars[indexer]
                    ("\((?<path>.+),.+\)", "$1"); //Remove any cast expressions from the path
                    ("(?<lambda>^([\w]*))(?<dot>(\.{1}))?", String.Format("{0}{1}", (match String.IsNullOrEmpty(rootName) with | true -> "" | _ -> rootName), (match String.IsNullOrEmpty(rootName) with | true -> "" | _ -> "$2")));
                    ]

        let pathName =  List.fold(fun source (regex, replace) -> Regex.Replace(source, regex, (string)replace)) (expression.ToString()) args

        let rec replaceLamdbaExpressions (lambda:Expression, fullPath: String) =

            let arguments =
                match lambda with
                | :? MethodCallExpression as mcall -> Some(mcall.Arguments)
                | _ -> None

            let replaced =
                match arguments with
                | Some(x) ->
                    x
                    |> Seq.filter(fun x -> x :? MemberExpression)
                    |> Seq.map(fun x -> (x, Expression.Lambda(x).Compile().DynamicInvoke()))
                    |> Seq.fold(fun (source : String) (replaceMatch, replaceValue)  -> source.Replace(replaceMatch.ToString(), replaceValue.ToString())) fullPath
                | None -> fullPath

            match lambda with
            | :? MethodCallExpression as mcall -> replaceLamdbaExpressions(mcall.Object, replaced)
            | :? MemberExpression as mcall -> replaceLamdbaExpressions(mcall.Expression, replaced)
            | _ -> replaced

        let lambdaReplacedPathName = replaceLamdbaExpressions(expression, pathName)

        let friendlyName =
            match expression with
                //TODO: Should have property to control returning camel case output
                | :? MemberExpression as me -> me.Member.Name
                | _ -> lambdaReplacedPathName

        (friendlyName, lambdaReplacedPathName)