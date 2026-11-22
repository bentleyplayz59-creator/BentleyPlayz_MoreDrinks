using System.Collections.Generic;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens;

internal interface IContext
{
	int UpdateTick { get; }

	bool IsModInstalled(string id);

	bool Contains(string name, bool enforceContext);

	IToken? GetToken(string name, bool enforceContext);

	IEnumerable<IToken> GetTokens(bool enforceContext);

	IInvariantSet GetValues(string name, IInputArguments input, bool enforceContext);
}
