using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework;

internal interface IContextualState
{
	bool IsValid { get; }

	bool IsInScope { get; }

	bool IsReady { get; }

	IInvariantSet InvalidTokens { get; }

	IInvariantSet UnreadyTokens { get; }

	IInvariantSet UnavailableModTokens { get; }

	IInvariantSet Errors { get; }
}
