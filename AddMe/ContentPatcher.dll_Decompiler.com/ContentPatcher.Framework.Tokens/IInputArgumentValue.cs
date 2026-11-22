namespace ContentPatcher.Framework.Tokens;

internal interface IInputArgumentValue
{
	string Raw { get; }

	string[] Parsed { get; }
}
