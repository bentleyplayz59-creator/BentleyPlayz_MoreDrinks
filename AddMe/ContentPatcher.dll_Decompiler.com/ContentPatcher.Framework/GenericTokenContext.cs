using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework;

internal class GenericTokenContext : IContext
{
	private readonly Func<string, bool> IsModInstalledImpl;

	private readonly Func<int> GetUpdateTick;

	public InvariantDictionary<IToken> Tokens { get; } = new InvariantDictionary<IToken>();

	public int UpdateTick => GetUpdateTick();

	public GenericTokenContext(Func<string, bool> isModInstalled, Func<int> getUpdateTick)
	{
		IsModInstalledImpl = isModInstalled;
		GetUpdateTick = getUpdateTick;
	}

	public bool IsModInstalled(string id)
	{
		return IsModInstalledImpl(id);
	}

	public void Save(IToken token)
	{
		Tokens[token.Name] = token;
	}

	public void Remove(string name)
	{
		Tokens.Remove(name);
	}

	public bool Contains(string name, bool enforceContext)
	{
		return GetToken(name, enforceContext) != null;
	}

	public IToken? GetToken(string name, bool enforceContext)
	{
		IToken value;
		return (Tokens.TryGetValue(name, out value) && ShouldConsider(value, enforceContext)) ? value : null;
	}

	public IEnumerable<IToken> GetTokens(bool enforceContext)
	{
		foreach (IToken token in Tokens.Values)
		{
			if (ShouldConsider(token, enforceContext))
			{
				yield return token;
			}
		}
	}

	public IInvariantSet GetValues(string name, IInputArguments input, bool enforceContext)
	{
		return GetToken(name, enforceContext)?.GetValues(input) ?? InvariantSets.Empty;
	}

	private bool ShouldConsider(IToken token, bool enforceContext)
	{
		return !enforceContext || token.IsReady;
	}
}
