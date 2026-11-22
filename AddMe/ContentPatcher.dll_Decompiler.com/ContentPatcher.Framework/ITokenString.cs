namespace ContentPatcher.Framework;

internal interface ITokenString : IContextualInfo
{
	string? Raw { get; }

	bool HasAnyTokens { get; }

	string? Value { get; }

	bool IsSingleTokenOnly { get; }

	string Path { get; }
}
