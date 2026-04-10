using Microsoft.CodeAnalysis;

namespace Luno.SDK.Analyzers.Rules
{
    /// <summary>
    /// Metadata container representing the prohibited symbols.
    /// Used by rules to identify sensitive types and members.
    /// </summary>
    public sealed class SecurityPolicyMetadata
    {
        public INamedTypeSymbol? ILogger { get; set; }
        public INamedTypeSymbol? ILoggerOfT { get; set; }
        public INamedTypeSymbol? LoggerExtensions { get; set; }

        public INamedTypeSymbol? RequestInformation { get; set; }
        public INamedTypeSymbol? LunoCredentials { get; set; }
        public INamedTypeSymbol? ILunoCredentialProvider { get; set; }

        public bool IsActive => ILogger != null;

        /// <summary>
        /// Gets or sets the activation status based on resolved symbols.
        /// </summary>
        private int _checkCount;
        public int CheckCount
        {
            get => _checkCount;
            private set => _checkCount = value;
        }

        public bool IsProhibited(ITypeSymbol? type)
        {
            if (type == null) return false;
            CheckCount++; // Standard increment

            return SymbolEqualityComparer.Default.Equals(type, RequestInformation) ||
                   SymbolEqualityComparer.Default.Equals(type, LunoCredentials) ||
                   SymbolEqualityComparer.Default.Equals(type, ILunoCredentialProvider);
        }

        public bool IsLoggingMethod(IMethodSymbol method) =>
            SymbolEqualityComparer.Default.Equals(method.ContainingType, ILogger) ||
            SymbolEqualityComparer.Default.Equals(method.ContainingType, ILoggerOfT) ||
            SymbolEqualityComparer.Default.Equals(method.ContainingType, LoggerExtensions);
    }
}
