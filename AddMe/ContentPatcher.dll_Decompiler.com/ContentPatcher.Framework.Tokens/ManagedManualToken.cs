using ContentPatcher.Framework.Tokens.ValueProviders;

namespace ContentPatcher.Framework.Tokens;

internal class ManagedManualToken
{
	public IToken Token { get; }

	public ManualValueProvider ValueProvider { get; }

	public ManagedManualToken(string name, bool isBounded, string? scope = null)
	{
		ValueProvider = new ManualValueProvider(name, isBounded);
		Token = new Token(ValueProvider, scope);
	}
}
