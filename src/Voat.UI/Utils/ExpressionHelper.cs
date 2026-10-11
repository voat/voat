using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Voat
{
    // Modernization: replacement for Microsoft.AspNetCore.Mvc.ViewFeatures.Internal.ExpressionHelper and
    // ExpressionMetadataProvider.FromLambdaExpression, which became internal in ASP.NET Core 3.0+.
    // GetExpressionText is a faithful port of the ASP.NET Core 2.0 implementation
    // (aspnet/Mvc rel/2.0.0, Apache-2.0, .NET Foundation) so generated html field names stay identical.
    internal static class ExpressionHelper
    {
        public static string GetExpressionText(LambdaExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            // Walk the expression body towards the lambda parameter, collecting segments right-to-left.
            // Member access contributes "Name"; single-argument indexers and array indexing contribute
            // "[value]" (index evaluated against captured state); a trailing "model" segment (the view's
            // Model property) is stripped; identifiers containing "__" are treated as compiler-generated
            // and stop the walk.
            var parts = new List<KeyValuePair<string, bool>>(); // segment text, isIndexer
            var lastIsModel = false;

            var part = expression.Body;
            while (part != null)
            {
                switch (part.NodeType)
                {
                    case ExpressionType.Call:
                        lastIsModel = false;
                        var methodExpression = (MethodCallExpression)part;
                        if (IsSingleArgumentIndexer(methodExpression))
                        {
                            parts.Insert(0, new KeyValuePair<string, bool>(IndexerInvocationText(methodExpression.Arguments.Single(), expression), true));
                            part = methodExpression.Object;
                        }
                        else
                        {
                            // Unsupported.
                            part = null;
                        }
                        break;

                    case ExpressionType.ArrayIndex:
                        lastIsModel = false;
                        var binaryExpression = (BinaryExpression)part;
                        parts.Insert(0, new KeyValuePair<string, bool>(IndexerInvocationText(binaryExpression.Right, expression), true));
                        part = binaryExpression.Left;
                        break;

                    case ExpressionType.MemberAccess:
                        var memberExpression = (MemberExpression)part;
                        var name = memberExpression.Member.Name;

                        // If identifier contains "__", it is "reserved for use by the implementation" and likely
                        // compiler- or Razor-generated e.g. the name of a field in a delegate's generated class.
                        if (name.Contains("__"))
                        {
                            part = null;
                        }
                        else
                        {
                            lastIsModel = string.Equals("model", name, StringComparison.OrdinalIgnoreCase);
                            parts.Insert(0, new KeyValuePair<string, bool>(name, false));
                            part = memberExpression.Expression;
                        }
                        break;

                    case ExpressionType.Parameter:
                        // Reached the lambda parameter: no more segments.
                        lastIsModel = false;
                        part = null;
                        break;

                    default:
                        // Unsupported.
                        part = null;
                        break;
                }
            }

            // If the name would start with "model.", then strip that part away.
            if (lastIsModel && parts.Count > 0)
            {
                parts.RemoveAt(0);
            }

            // Segments join with '.' unless the segment on the right is an indexer: "Foo[0]", "Foo[0].Bar".
            var builder = new StringBuilder();
            for (var i = 0; i < parts.Count; i++)
            {
                if (i > 0 && !parts[i].Value)
                {
                    builder.Append('.');
                }
                builder.Append(parts[i].Key);
            }
            return builder.ToString();
        }

        // Replacement for ExpressionMetadataProvider.FromLambdaExpression(expression, viewData, metadataProvider).Model:
        // evaluates the expression against the given model instance.
        public static object Evaluate<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression, object model)
        {
            return expression.Compile().DynamicInvoke(model);
        }

        private static string IndexerInvocationText(Expression indexExpression, LambdaExpression parentExpression)
        {
            if (indexExpression == null)
            {
                throw new ArgumentNullException(nameof(indexExpression));
            }

            // Compile the index expression on its own so captured variables/constants are evaluated,
            // as the original implementation did.
            var converted = Expression.Convert(indexExpression, typeof(object));
            var fakeParameter = Expression.Parameter(typeof(object), null);
            var lambda = Expression.Lambda<Func<object, object>>(converted, fakeParameter);
            Func<object, object> func;

            try
            {
                func = lambda.Compile();
            }
            catch (InvalidOperationException ex)
            {
                var parameters = parentExpression.Parameters.ToArray();
                throw new InvalidOperationException(
                    $"The index expression '{indexExpression}' is invalid. Ensure it does not reference the lambda parameter '{parameters[0].Name}'.",
                    ex);
            }

            return "[" + Convert.ToString(func(null), CultureInfo.InvariantCulture) + "]";
        }

        private static bool IsSingleArgumentIndexer(Expression expression)
        {
            var methodExpression = expression as MethodCallExpression;
            if (methodExpression == null || methodExpression.Arguments.Count != 1)
            {
                return false;
            }

            // Check whether GetDefaultMembers() (if present in CoreCLR) would return a member of this type. Compiler
            // names the indexer property, if any, in a generated [DefaultMember] attribute for the containing type.
            var declaringType = methodExpression.Method.DeclaringType;
            var defaultMember = declaringType == null ? null : declaringType.GetTypeInfo().GetCustomAttribute<DefaultMemberAttribute>(inherit: true);
            if (defaultMember == null)
            {
                return false;
            }

            // Find default property (the indexer) and confirm its getter is the method in this expression.
            foreach (var property in declaringType.GetRuntimeProperties())
            {
                if (string.Equals(defaultMember.MemberName, property.Name, StringComparison.Ordinal) &&
                    property.GetMethod == methodExpression.Method)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
