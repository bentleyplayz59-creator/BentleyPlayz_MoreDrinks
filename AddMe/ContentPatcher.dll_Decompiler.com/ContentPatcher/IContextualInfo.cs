using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher;

internal interface IContextualInfo
{
	bool IsMutable { get; }

	bool IsReady { get; }

	IInvariantSet GetTokensUsed();
}
