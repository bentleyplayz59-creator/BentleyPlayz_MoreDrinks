using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework;

internal class LocalContext : IContext
{
	private readonly string Scope;

	private IContext? LastParentContext;

	private readonly InvariantDictionary<ManagedManualToken> LocalTokens = new InvariantDictionary<ManagedManualToken>();

	public int UpdateTick => LastParentContext?.UpdateTick ?? 0;

	public LocalContext(string scope, IContext? parentContext = null)
	{
		Scope = scope;
		LastParentContext = parentContext;
	}

	public void Update(IContext parentContext)
	{
		UpdateParentContextLinkOnly(parentContext);
		foreach (ManagedManualToken value in LocalTokens.Values)
		{
			value.ValueProvider.SetReady(ready: false);
		}
	}

	public void UpdateParentContextLinkOnly(IContext parentContext)
	{
		LastParentContext = parentContext;
	}

	public bool IsModInstalled(string id)
	{
		return LastParentContext?.IsModInstalled(id) ?? false;
	}

	public bool Contains(string name, bool enforceContext)
	{
		return GetToken(name, enforceContext) != null;
	}

	public IToken? GetToken(string name, bool enforceContext)
	{
		ManagedManualToken value;
		return LocalTokens.TryGetValue(name, out value) ? value.Token : LastParentContext?.GetToken(name, enforceContext);
	}

	public IEnumerable<IToken> GetTokens(bool enforceContext)
	{
		foreach (ManagedManualToken managed in LocalTokens.Values)
		{
			yield return managed.Token;
		}
		if (LastParentContext == null)
		{
			yield break;
		}
		foreach (IToken token in LastParentContext.GetTokens(enforceContext))
		{
			yield return token;
		}
	}

	public IInvariantSet GetValues(string name, IInputArguments input, bool enforceContext)
	{
		return GetToken(name, enforceContext)?.GetValues(input) ?? InvariantSets.Empty;
	}

	public void SetLocalValue(string name, string? value, bool ready = true)
	{
		ITokenString value2 = new LiteralString(value, new LogPathBuilder("LocalContext", Scope, name));
		SetLocalValue(name, value2, ready);
	}

	public void SetLocalValue(string name, ITokenString value, bool ready = true)
	{
		if (!LocalTokens.TryGetValue(name, out ManagedManualToken value2))
		{
			value2 = (LocalTokens[name] = new ManagedManualToken(name, isBounded: false, Scope));
		}
		value2.ValueProvider.SetValue(value);
		value2.ValueProvider.SetReady(ready);
	}
}
