using ContentPatcher.Framework.Tokens;

namespace ContentPatcher.Framework;

internal interface IContextual : IContextualInfo
{
	bool UpdateContext(IContext context);

	IContextualState GetDiagnosticState();
}
