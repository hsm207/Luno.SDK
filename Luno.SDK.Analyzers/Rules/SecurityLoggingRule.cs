using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Luno.SDK.Analyzers.Core;
using System.Linq;

namespace Luno.SDK.Analyzers.Rules
{
    /// <summary>
    /// Enforces restrictions on logging sensitive types within ILogger invocations.
    /// Performs hierarchy traversal to identify prohibited types.
    /// </summary>
    public static class SecurityLoggingRule
    {
        public static void Enforce(IOperation operation, SecurityPolicyMetadata metadata, System.Action<ITypeSymbol, SyntaxNode> reportViolation)
        {
            var invocation = (IInvocationOperation)operation;
            if (!metadata.IsLoggingMethod(invocation.TargetMethod)) return;

            foreach (var argument in invocation.Arguments)
            {
                var value = argument.Value.Unwrap();

                if (value is IArrayCreationOperation array && array.Initializer != null)
                {
                    foreach (var element in array.Initializer.ElementValues)
                    {
                        ValidateType(element.Unwrap().Type, element.Syntax, metadata, reportViolation);
                    }
                }
                else
                {
                    ValidateType(value.Type, argument.Syntax, metadata, reportViolation);
                }
            }
        }

        private static void ValidateType(ITypeSymbol? type, SyntaxNode syntax, SecurityPolicyMetadata metadata, System.Action<ITypeSymbol, SyntaxNode> reportViolation)
        {
            if (CheckLineage(type, metadata, out var matchedType))
            {
                reportViolation(matchedType, syntax);
            }
        }

        private static bool CheckLineage(ITypeSymbol? type, SecurityPolicyMetadata metadata, out ITypeSymbol matchedType)
        {
            if (type == null) { matchedType = null!; return false; }
            if (metadata.IsProhibited(type)) { matchedType = type; return true; }

            foreach (var @interface in type.AllInterfaces)
            {
                if (metadata.IsProhibited(@interface)) { matchedType = @interface; return true; }
            }

            var current = type.BaseType;
            while (current != null)
            {
                if (metadata.IsProhibited(current)) { matchedType = current; return true; }
                current = current.BaseType;
            }

            matchedType = null!;
            return false;
        }
    }
}
