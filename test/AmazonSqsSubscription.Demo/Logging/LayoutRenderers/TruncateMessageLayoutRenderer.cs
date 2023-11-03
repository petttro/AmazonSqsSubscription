using NLog.Config;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;

namespace Package.Demo.Logging.LayoutRenderers
{
    [LayoutRenderer("truncate")]
    [ThreadAgnostic]
    public class TruncateMessageLayoutRenderer : WrapperLayoutRendererBase
    {
        public bool Truncate { get; set; } = true;

        public int Limit { get; set; } = 10000;

        /// <summary>
        /// Post-processes the rendered message. This implementation truncates the log message to be below
        /// the set limit (includes the length of a WarningMessage)
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>Trimmed string.</returns>
        protected override string Transform(string text)
        {
            if (!Truncate || Limit <= 0 || text.Length < Limit)
            {
                return text;
            }

            var warningMessage = GenerateWarningMessage(text.Length);
            var truncated = text.Substring(0,  Limit - warningMessage.Length);
            return $"{truncated}{warningMessage}";
        }

        private string GenerateWarningMessage(int actualCharacterLength)
        {
            return $"... MESSAGE Truncated=True DUE TO EXCEEDING MaxAllowedCharacterLength={Limit}, ActualCharacterLength={actualCharacterLength}";
        }
    }
}
