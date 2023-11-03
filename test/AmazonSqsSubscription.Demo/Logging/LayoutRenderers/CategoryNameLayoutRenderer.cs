using System.ComponentModel;
using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Package.Demo.Logging.LayoutRenderers
{
    [LayoutRenderer("category")]
    [ThreadAgnostic]
    public class CategoryNameLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets a value indicating whether to render short category name.
        /// </summary>
        [DefaultValue(true)]
        public bool CompressedName { get; set; } = true;

        /// <summary>
        /// Renders the category name and appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            AppendConvertedFullTypeName(builder, logEvent.LoggerName, CompressedName);
        }

        private void AppendConvertedFullTypeName(StringBuilder builder, string name, bool compressedName)
        {
            if (compressedName && !string.IsNullOrEmpty(name))
            {
                var firstPointIndex = name.LastIndexOf('.');
                if (firstPointIndex >= 0)
                {
                    var secondPointIndex = name.LastIndexOf('.', firstPointIndex - 1);
                    if (secondPointIndex >= 0)
                    {
                        var endName = name.Substring(secondPointIndex + 1, name.Length - secondPointIndex - 1);
                        var startPoint = 0;
                        while (startPoint >= 0 && startPoint < secondPointIndex)
                        {
                            builder.Append(name.Substring(startPoint, 1) + '.');
                            startPoint = name.IndexOf('.', startPoint + 1) + 1;
                        }

                        builder.Append(endName);
                        return;
                    }
                }
            }

            builder.Append(name);
        }
    }
}
