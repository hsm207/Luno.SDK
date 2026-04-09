using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Luno.SDK.Analyzers
{
    /// <summary>
    /// Enforces the 'Triple-Banning Protocol' by preventing sensitive types from being passed to ILogger.
    /// Blocks: RequestInformation, LunoCredentials, and ILunoCredentialProvider (including implementers).
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LunoSecurityLoggingAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "LUNO001";
        private const string Category = "Security";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            "Sensitive type passed to ILogger",
            "Sensitive type '{0}' must never be passed to ILogger directly",
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Sensitive SDK types must not be passed to the logging pipeline to prevent credential or metadata leakage.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                // Resolve and cache the 'Triple-Banned' symbols
                var symbols = new BannedSymbols(compilationContext.Compilation);
                if (!symbols.IsComplete) return;

                // Register the operation action inside the compilation context for better performance
                compilationContext.RegisterOperationAction(operationContext => 
                    AnalyzeInvocation(operationContext, symbols), OperationKind.Invocation);
            });
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context, BannedSymbols symbols)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var method = invocation.TargetMethod;

            if (!IsLoggerMethod(method, symbols)) return;

            foreach (var argument in invocation.Arguments)
            {
                // Unroll the value to find the true underlying type (handles boxing/conversions)
                var value = Unwrap(argument.Value);
                
                // If it's a params array (common in LogInformation), we must inspect its elements
                if (value is IArrayCreationOperation arrayCreation && arrayCreation.Initializer != null)
                {
                    foreach (var element in arrayCreation.Initializer.ElementValues)
                    {
                        CheckType(context, Unwrap(element).Type, element.Syntax, symbols);
                    }
                }
                else
                {
                    CheckType(context, value.Type, argument.Syntax, symbols);
                }
            }
        }

        private static IOperation Unwrap(IOperation operation)
        {
            while (operation is IConversionOperation conversion)
            {
                operation = conversion.Operand;
            }
            return operation;
        }

        private static void CheckType(OperationAnalysisContext context, ITypeSymbol type, SyntaxNode syntax, BannedSymbols symbols)
        {
            if (type == null) return;

            if (IsBannedType(type, symbols, out var matchedType))
            {
                var diagnostic = Diagnostic.Create(Rule, syntax.GetLocation(), matchedType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsLoggerMethod(IMethodSymbol method, BannedSymbols symbols)
        {
            var containingType = method.ContainingType;
            if (containingType == null) return false;

            return SymbolEqualityComparer.Default.Equals(containingType, symbols.ILogger) ||
                   SymbolEqualityComparer.Default.Equals(containingType, symbols.ILoggerOfT) ||
                   SymbolEqualityComparer.Default.Equals(containingType, symbols.LoggerExtensions);
        }

        private static bool IsBannedType(ITypeSymbol type, BannedSymbols symbols, out ITypeSymbol matchedType)
        {
            // 1. Direct match or Identity match
            if (symbols.IsBanned(type))
            {
                matchedType = type;
                return true;
            }

            // 2. Verification of the AllInterfaces collection (Interface enforcement)
            foreach (var @interface in type.AllInterfaces)
            {
                if (symbols.IsBanned(@interface))
                {
                    matchedType = @interface;
                    return true;
                }
            }

            // 3. Recursive check of the BaseType hierarchy (Inheritance enforcement)
            var current = type.BaseType;
            while (current != null)
            {
                if (symbols.IsBanned(current))
                {
                    matchedType = current;
                    return true;
                }
                current = current.BaseType;
            }

            matchedType = null;
            return false;
        }

        private class BannedSymbols
        {
            public INamedTypeSymbol ILogger { get; }
            public INamedTypeSymbol ILoggerOfT { get; }
            public INamedTypeSymbol LoggerExtensions { get; }
            public INamedTypeSymbol RequestInformation { get; }
            public INamedTypeSymbol LunoCredentials { get; }
            public INamedTypeSymbol ILunoCredentialProvider { get; }

            public bool IsComplete => ILogger != null && RequestInformation != null;

            public BannedSymbols(Compilation compilation)
            {
                ILogger = GetType(compilation, "Microsoft.Extensions.Logging.ILogger");
                ILoggerOfT = GetType(compilation, "Microsoft.Extensions.Logging.ILogger`1");
                LoggerExtensions = GetType(compilation, "Microsoft.Extensions.Logging.LoggerExtensions");
                
                RequestInformation = GetType(compilation, "Microsoft.Kiota.Abstractions.RequestInformation");
                LunoCredentials = GetType(compilation, "Luno.SDK.LunoCredentials");
                ILunoCredentialProvider = GetType(compilation, "Luno.SDK.ILunoCredentialProvider");
            }

            private static INamedTypeSymbol GetType(Compilation compilation, string metadataName)
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

            public bool IsBanned(ITypeSymbol type)
            {
                if (type == null) return false;
                
                return SymbolEqualityComparer.Default.Equals(type, RequestInformation) ||
                       SymbolEqualityComparer.Default.Equals(type, LunoCredentials) ||
                       SymbolEqualityComparer.Default.Equals(type, ILunoCredentialProvider);
            }
        }
    }
}
