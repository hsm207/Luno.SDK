using Microsoft.CodeAnalysis;

namespace Luno.SDK.Analyzers.Rules
{
    public static class RuleDefinitions
    {
        public const string SecurityCategory = "Security";
        public const string GovernanceCategory = "Governance";

        /// <summary>
        /// LUNO001: Prohibition of sensitive SDK internal types in logging sinks.
        /// </summary>
        public static readonly DiagnosticDescriptor ProhibitedLoggingRule = new DiagnosticDescriptor(
            "LUNO001",
            "Sensitive type passed to ILogger",
            "Sensitive type '{0}' must never be passed to ILogger directly",
            SecurityCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Sensitive SDK types must not be passed to the logging pipeline to prevent credential or metadata leakage."
        );
    }
}
