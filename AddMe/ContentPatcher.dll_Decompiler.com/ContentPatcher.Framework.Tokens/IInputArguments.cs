using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ContentPatcher.Framework.Tokens;

internal interface IInputArguments
{
	ITokenString? TokenString { get; }

	string[] PositionalArgs { get; }

	IDictionary<string, IInputArgumentValue> NamedArgs { get; }

	IDictionary<string, IInputArgumentValue> ReservedArgs { get; }

	KeyValuePair<string, IInputArgumentValue>[] ReservedArgsList { get; }

	[MemberNotNullWhen(true, "TokenString")]
	bool HasNamedArgs
	{
		[MemberNotNullWhen(true, "TokenString")]
		get;
	}

	[MemberNotNullWhen(true, "TokenString")]
	bool HasPositionalArgs
	{
		[MemberNotNullWhen(true, "TokenString")]
		get;
	}

	[MemberNotNullWhen(true, "TokenString")]
	bool IsMutable
	{
		[MemberNotNullWhen(true, "TokenString")]
		get;
	}

	[MemberNotNullWhen(true, "TokenString")]
	bool IsReady
	{
		[MemberNotNullWhen(true, "TokenString")]
		get;
	}

	string? GetFirstPositionalArg();

	string? GetRawArgumentValue(string key);

	string? GetPositionalSegment();
}
