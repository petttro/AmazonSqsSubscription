using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;
using Package.Demo.Logging.LayoutRenderers.RedactSensitiveInformation;

namespace Package.Demo.Logging.LayoutRenderers
{
    [LayoutRenderer("redact")]
    [ThreadAgnostic]
    public class RedactSensitiveInformationLayoutRenderer : WrapperLayoutRendererBase
    {
        private static IEnumerable<string> RedactedFields { get; } = new List<string>
            {
                "Password"
            }.Distinct()
            .ToList();

        private static IEnumerable<string> HashedFields { get; } = new List<string>
            {
                "Password"
            }.Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        private static IReadOnlyList<RedactingStatement> Statements { get; set; } =
            RedactedFields.Select(x => new RedactingStatement(x))
                .Concat(HashedFields.Select(x => new HashingStatement(x)))
                .ToList();

        protected override void Append(StringBuilder builder, LogEventInfo logEvent) => builder.Append(Transform(RenderInner(logEvent)));

        protected override string Transform(string text) =>
            Statements.Where(s => s.ContainedIn(text)).Aggregate(text, (currentText, statement) => statement.GetTextWithFieldReplaced(currentText));
    }
}
