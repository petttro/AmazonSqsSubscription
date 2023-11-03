using NLog.Config;
using NLog.LayoutRenderers;
using NLog.LayoutRenderers.Wrappers;

namespace Package.Demo.Logging.LayoutRenderers
{
    [LayoutRenderer("replace-returns")]
    [ThreadAgnostic]
    public class ReplaceReturnsLayoutRenderer : WrapperLayoutRendererBase
    {
        // Defined target characters this way to avoid any potential inconsistency between Windows deployed implementation vs Unix deployed
        private readonly string Return = char.ConvertFromUtf32(13);
        private readonly string Space = char.ConvertFromUtf32(32);

        /// <summary>
        /// Post-processes the rendered message. This implementation removes any return characters from the log message.
        /// </summary>
        /// <param name="text">The text to be post-processed.</param>
        /// <returns>Cleaned string.</returns>
        protected override string Transform(string text)
        {
            if (text.IndexOf(Return, StringComparison.Ordinal) >= 0)
            {
                return text.Replace(Return, Space);
            }
            else
            {
                return text;
            }
        }
    }
}
