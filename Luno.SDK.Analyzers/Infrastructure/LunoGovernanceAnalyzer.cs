using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Luno.SDK.Analyzers.Rules;

namespace Luno.SDK.Analyzers.Infrastructure
{
    /// <summary>
    /// The primary orchestration entry point for Luno SDK Governance rules.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class LunoGovernanceAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics 
            => ImmutableArray.CreateRange(RuleDefinitions.AllRules);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(ctx =>
            {
                var metadata = SymbolResolver.Resolve(ctx.Compilation);
                if (!metadata.IsActive) return;

                // Subscribe to invocation operations
                ctx.RegisterOperationAction(opCtx =>
                {
                    SecurityLoggingRule.Enforce(
                        opCtx.Operation, 
                        metadata, 
                        (type, syntax) => opCtx.ReportDiagnostic(
                            Diagnostic.Create(RuleDefinitions.ProhibitedLoggingRule, syntax.GetLocation(), type.Name)));
                }, OperationKind.Invocation);
            });
        }
    }
}
