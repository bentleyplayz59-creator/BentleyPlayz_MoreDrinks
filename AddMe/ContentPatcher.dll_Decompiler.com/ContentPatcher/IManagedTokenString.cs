using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ContentPatcher;

public interface IManagedTokenString
{
	[MemberNotNullWhen(false, "ValidationError")]
	bool IsValid
	{
		[MemberNotNullWhen(false, "ValidationError")]
		get;
	}

	string? ValidationError { get; }

	bool IsReady { get; }

	string? Value { get; }

	IEnumerable<int> UpdateContext();
}
