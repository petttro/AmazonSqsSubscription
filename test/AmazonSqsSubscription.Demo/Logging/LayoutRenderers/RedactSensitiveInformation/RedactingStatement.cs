using System.Text.RegularExpressions;

namespace Package.Demo.Logging.LayoutRenderers.RedactSensitiveInformation
{
    internal class RedactingStatement
    {
        private readonly string _field;
        private readonly Regex _regex;

        public RedactingStatement(string field)
        {
            _field = field;

            // "(Username)":s*?"(.*?)"|\b(Username)=(\S*)\b
            _regex = new Regex($"\"({field})\":\\s*?\"(.*?)\"|\\b({field})=(\\S*)\\b", RegexOptions.IgnoreCase);
        }

        // using string.indexOf() method because it's more effective than regex.IsMatch()
        public bool ContainedIn(string text) => text.IndexOf(_field, StringComparison.OrdinalIgnoreCase) > 0;

        // using regex.Replace() because string.Replace() is not suitable when there are a lot of different variants for the fields with sensitive information
        public string GetTextWithFieldReplaced(string text)
        {
            var match = _regex.Match(text);

            if (!string.IsNullOrWhiteSpace(match?.Groups[1]?.Value))
            {
                return _regex.Replace(text, $"\"$1\":\"{GetReplacedValue(match)}\"");
            }
            else if (!string.IsNullOrEmpty(match?.Groups[3]?.Value))
            {
                return _regex.Replace(text, $"$3={GetReplacedValue(match)}");
            }

            return text;
        }

        protected virtual string GetReplacedValue(Match match) => "*REDACTED*";
    }
}
