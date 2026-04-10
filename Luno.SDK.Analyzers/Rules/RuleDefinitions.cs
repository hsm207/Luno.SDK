using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Luno.SDK.Analyzers.Rules
{
    public static class RuleDefinitions
    {
        public const string SecurityCategory = "Security";

        public static readonly DiagnosticDescriptor ProhibitedLoggingRule = new(
            "LUNO001",
            "Sensitive type passed to ILogger",
            "Sensitive type '{0}' must never be passed to ILogger directly",
            SecurityCategory,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );

        /// <summary>
        /// A collection expression representing the full set of governance rules.
        /// </summary>
        public static readonly DiagnosticDescriptor[] AllRules = [ProhibitedLoggingRule];
    }
}
