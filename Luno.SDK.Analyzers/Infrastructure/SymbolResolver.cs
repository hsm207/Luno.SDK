using Microsoft.CodeAnalysis;
using System.Linq;
using Luno.SDK.Analyzers.Rules;

namespace Luno.SDK.Analyzers.Infrastructure
{
    /// <summary>
    /// Infrastructure factory responsible for populating Domain Metadata from the compilation context.
    /// Supports resilient lookup in both metadata references and source code.
    /// </summary>
    public static class SymbolResolver
    {
        public static SecurityPolicyMetadata Resolve(Compilation compilation)
        {
            return new SecurityPolicyMetadata
            {
                ILogger = GetType(compilation, "Microsoft.Extensions.Logging.ILogger"),
                ILoggerOfT = GetType(compilation, "Microsoft.Extensions.Logging.ILogger`1"),
                LoggerExtensions = GetType(compilation, "Microsoft.Extensions.Logging.LoggerExtensions"),

                RequestInformation = GetType(compilation, "Microsoft.Kiota.Abstractions.RequestInformation"),
                LunoCredentials = GetType(compilation, "Luno.SDK.LunoCredentials"),
                ILunoCredentialProvider = GetType(compilation, "Luno.SDK.ILunoCredentialProvider")
            };
        }

        private static INamedTypeSymbol? GetType(Compilation compilation, string metadataName)
        {
            var type = compilation.GetTypeByMetadataName(metadataName);
            if (type != null) return type;

            // Fallback for types defined in the current source compilation (common in tests)
            var parts = metadataName.Split('.');
            INamespaceOrTypeSymbol current = compilation.GlobalNamespace;

            foreach (var part in parts)
            {
                var next = current.GetMembers(part).FirstOrDefault();
                if (next is INamespaceOrTypeSymbol nsOrType)
                {
                    current = nsOrType;
                }
                else
                {
                    return null;
                }
            }

            return current as INamedTypeSymbol;
        }
    }
}
