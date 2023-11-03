namespace Package.Demo.Logging.LayoutRenderers.RedactSensitiveInformation
{
    internal class HashingStatement : RedactingStatement
    {
        public HashingStatement(string field)
            : base(field)
        {
        }
    }
}
