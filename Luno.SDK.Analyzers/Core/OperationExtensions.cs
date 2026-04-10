using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Luno.SDK.Analyzers.Core
{
    public static class OperationExtensions
    {
        /// <summary>
        /// Recursively unwraps implicit conversions to find the underlying operation and type.
        /// </summary>
        public static IOperation Unwrap(this IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
            }
            return operation;
        }
    }
}
