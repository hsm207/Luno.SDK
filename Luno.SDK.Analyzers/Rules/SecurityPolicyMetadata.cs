using Microsoft.CodeAnalysis;

namespace Luno.SDK.Analyzers.Rules
{
    /// <summary>
    /// Metadata container representing the prohibited symbols and logging infrastructure.
    /// </summary>
    public sealed class SecurityPolicyMetadata
    {
        public INamedTypeSymbol? ILogger { get; set; }
        public INamedTypeSymbol? ILoggerOfT { get; set; }
        public INamedTypeSymbol? LoggerExtensions { get; set; }
        
        public INamedTypeSymbol? RequestInformation { get; set; }
        public INamedTypeSymbol? LunoCredentials { get; set; }
        public INamedTypeSymbol? ILunoCredentialProvider { get; set; }

        public bool IsActive => ILogger != null && RequestInformation != null;

        /// <summary>
        /// Orchestrates the 'Triple-Banning' identity check against the policy metadata.
        /// </summary>
        public bool IsProhibited(ITypeSymbol? type)
        {
            if (type == null) return false;
            
            return SymbolEqualityComparer.Default.Equals(type, RequestInformation) ||
                   SymbolEqualityComparer.Default.Equals(type, LunoCredentials) ||
                   SymbolEqualityComparer.Default.Equals(type, ILunoCredentialProvider);
        }

        /// <summary>
        /// Validates if the target method belongs to the supported logging infrastructure.
        /// </summary>
        public bool IsLoggingMethod(IMethodSymbol method)
        {
            var containingType = method.ContainingType;
            if (containingType == null) return false;

            return SymbolEqualityComparer.Default.Equals(containingType, ILogger) ||
                   SymbolEqualityComparer.Default.Equals(containingType, ILoggerOfT) ||
                   SymbolEqualityComparer.Default.Equals(containingType, LoggerExtensions);
        }
    }
}
