using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Luno.SDK.Analyzers.Rules;

namespace Luno.SDK.Analyzers.Infrastructure
{
    /// <summary>
    /// The primary orchestration entry point for Luno SDK Governance rules.
    /// Acts as a humble bridge between the Roslyn compiler and the Rule Engine.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LunoGovernanceAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
            => ImmutableArray.Create(RuleDefinitions.ProhibitedLoggingRule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startContext =>
            {
                // Resolve metadata once per compilation for maximum performance
                var metadata = SymbolResolver.Resolve(startContext.Compilation);
                if (!metadata.IsActive) return;

                // Subscribe to invocation operations to enforce the governance policy
                startContext.RegisterOperationAction(operationContext =>
                {
                    SecurityLoggingRule.Enforce(
                        operationContext.Operation, 
                        metadata, 
                        (matchedType, syntax) => ReportViolation(operationContext, matchedType, syntax));
                }, OperationKind.Invocation);
            });
        }

        /// <summary>
        /// Humble helper to bridge domain violations to Roslyn's diagnostic reporting infrastructure.
        /// </summary>
        private static void ReportViolation(OperationAnalysisContext context, ITypeSymbol matchedType, SyntaxNode syntax)
        {
            var diagnostic = Diagnostic.Create(
                RuleDefinitions.ProhibitedLoggingRule, 
                syntax.GetLocation(), 
                matchedType.Name);
                
            context.ReportDiagnostic(diagnostic);
        }
    }
}
