namespace ContentPatcher.Framework.Tokens;

internal class InputArgumentValue : IInputArgumentValue
{
	public string Raw { get; }

	public string[] Parsed { get; }

	public InputArgumentValue(string raw, string[] parsed)
	{
		Raw = raw;
		Parsed = parsed;
	}
}
