using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework;

internal class ModTokenContext : IContext
{
	private readonly string Scope;

	private readonly IContext ParentContext;

	private readonly GenericTokenContext LocalContext;

	private readonly GenericTokenContext DynamicContext;

	private readonly InvariantDictionary<ManagedManualToken> DynamicTokens = new InvariantDictionary<ManagedManualToken>();

	private readonly List<DynamicTokenValue> DynamicTokenValues = new List<DynamicTokenValue>();

	private readonly InvariantDictionary<string> AliasTokenNames = new InvariantDictionary<string>();

	private bool HasNewTokens;

	private InvariantDictionary<MutableInvariantSet> DynamicTokenDependencies { get; } = new InvariantDictionary<MutableInvariantSet>();

	private InvariantDictionary<MutableInvariantSet> DynamicTokenDependents { get; } = new InvariantDictionary<MutableInvariantSet>();

	private MutableInvariantSet InterdependentTokens { get; } = new MutableInvariantSet();

	public int UpdateTick => ParentContext.UpdateTick;

	public ModTokenContext(string scope, IContext parentContext)
	{
		Scope = scope;
		ParentContext = parentContext;
		LocalContext = new GenericTokenContext(IsModInstalled, () => UpdateTick);
		DynamicContext = new GenericTokenContext(IsModInstalled, () => UpdateTick);
	}

	public void AddLocalToken(IToken token)
	{
		if (token.Scope != Scope)
		{
			throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because its scope '{token.Scope}' doesn't match this mod scope '{Scope}.");
		}
		if (token.Name.Contains(":"))
		{
			throw new InvalidOperationException($"Can't register the '{token.Name}' mod token because positional input arguments aren't supported ({":"} character).");
		}
		if (ParentContext.Contains(token.Name, enforceContext: false))
		{
			throw new InvalidOperationException("Can't register the '" + token.Name + "' mod token because there's a global token with that name.");
		}
		if (LocalContext.Contains(token.Name, enforceContext: false))
		{
			throw new InvalidOperationException("The '" + token.Name + "' token is already registered.");
		}
		LocalContext.Save(token);
		HasNewTokens = true;
	}

	public void RemoveLocalToken(string name)
	{
		LocalContext.Remove(name);
		HasNewTokens = true;
	}

	public void AddDynamicToken(string name, IManagedTokenString rawValue, Condition[] conditions)
	{
		if (ParentContext.Contains(name, enforceContext: false))
		{
			throw new InvalidOperationException("Can't register a '" + name + "' token because there's a global token with that name.");
		}
		if (LocalContext.Contains(name, enforceContext: false))
		{
			throw new InvalidOperationException("Can't register a '" + name + "' dynamic token because there's a config token or alias with that name.");
		}
		if (!DynamicTokens.TryGetValue(name, out ManagedManualToken value))
		{
			value = new ManagedManualToken(name, isBounded: true, Scope);
			DynamicTokens[name] = value;
			DynamicContext.Save(value.Token);
		}
		DynamicTokenValue dynamicTokenValue = new DynamicTokenValue(value, rawValue, conditions);
		IInvariantSet without = dynamicTokenValue.GetTokensUsed().GetWithout(name);
		value.ValueProvider.AddTokensUsed(without);
		value.ValueProvider.AddAllowedValues(rawValue);
		if (dynamicTokenValue.IsReady)
		{
			value.ValueProvider.SetValue(dynamicTokenValue.Value);
			value.ValueProvider.SetReady(ready: true);
		}
		DynamicTokenValues.Add(dynamicTokenValue);
		if (without.Any())
		{
			if (!DynamicTokenDependencies.TryGetValue(name, out MutableInvariantSet value2))
			{
				value2 = (DynamicTokenDependencies[name] = new MutableInvariantSet());
			}
			Queue<string> queue = new Queue<string>(without);
			while (queue.Any())
			{
				string text = queue.Dequeue();
				if (!value2.Add(text))
				{
					continue;
				}
				if (!DynamicTokenDependents.TryGetValue(text, out MutableInvariantSet value3))
				{
					value3 = (DynamicTokenDependents[text] = new MutableInvariantSet());
				}
				value3.Add(name);
				if (DynamicTokens.ContainsKey(text))
				{
					InterdependentTokens.Add(text);
					InterdependentTokens.Add(name);
				}
				IInvariantSet invariantSet = GetToken(text, enforceContext: false)?.GetTokensUsed();
				if (invariantSet == null || invariantSet.Count <= 0)
				{
					continue;
				}
				foreach (string item in invariantSet)
				{
					queue.Enqueue(item);
				}
			}
		}
		HasNewTokens = true;
	}

	public void AddAliasTokenName(string alias, string actual)
	{
		AliasTokenNames.Add(alias, actual);
		HasNewTokens = true;
	}

	public string ResolveAlias(string tokenName)
	{
		return AliasTokenNames.GetValueOrDefault(tokenName, tokenName);
	}

	public void UpdateContext(IInvariantSet globalChangedTokens)
	{
		bool flag = HasNewTokens;
		MutableInvariantSet mutableInvariantSet = null;
		foreach (IToken token in LocalContext.GetTokens(enforceContext: false))
		{
			if (token.IsMutable && token.UpdateContext(this) && !flag && DynamicTokenDependents.TryGetValue(token.Name, out MutableInvariantSet value))
			{
				if (mutableInvariantSet == null)
				{
					mutableInvariantSet = new MutableInvariantSet();
				}
				CollectionExtensions.AddRange<string>((ISet<string>)mutableInvariantSet, (IEnumerable<string>)value);
			}
		}
		if (DynamicTokens.Any())
		{
			if (!flag)
			{
				foreach (string globalChangedToken in globalChangedTokens)
				{
					if (DynamicTokenDependents.TryGetValue(globalChangedToken, out MutableInvariantSet value2))
					{
						if (mutableInvariantSet == null)
						{
							mutableInvariantSet = new MutableInvariantSet();
						}
						CollectionExtensions.AddRange<string>((ISet<string>)mutableInvariantSet, (IEnumerable<string>)value2);
					}
				}
			}
			if (!flag && mutableInvariantSet != null && InterdependentTokens.Count > 0)
			{
				foreach (string item2 in mutableInvariantSet)
				{
					flag = InterdependentTokens.Contains(item2);
					if (flag)
					{
						break;
					}
				}
			}
			if (flag || mutableInvariantSet != null)
			{
				foreach (var (item, managedManualToken2) in DynamicTokens)
				{
					if (flag || mutableInvariantSet.Contains(item))
					{
						managedManualToken2.ValueProvider.SetValue(InvariantSet.Empty);
						managedManualToken2.ValueProvider.SetReady(ready: false);
					}
				}
				foreach (DynamicTokenValue dynamicTokenValue in DynamicTokenValues)
				{
					if (flag || mutableInvariantSet.Contains(dynamicTokenValue.Name))
					{
						dynamicTokenValue.UpdateContext(this);
						if (dynamicTokenValue.IsReady && dynamicTokenValue.Conditions.All((Condition p) => p.IsMatch))
						{
							ManualValueProvider valueProvider = dynamicTokenValue.ParentToken.ValueProvider;
							valueProvider.SetValue(dynamicTokenValue.Value);
							valueProvider.SetReady(ready: true);
						}
					}
				}
			}
		}
		HasNewTokens = false;
	}

	public IInvariantSet GetTokensWhichAffect(string token)
	{
		MutableInvariantSet value;
		return DynamicTokenDependencies.TryGetValue(token, out value) ? value.GetImmutable() : InvariantSet.Empty;
	}

	public bool IsModInstalled(string id)
	{
		return ParentContext.IsModInstalled(id);
	}

	public bool Contains(string name, bool enforceContext)
	{
		return GetContexts().Any((IContext p) => p.Contains(name, enforceContext));
	}

	public IToken? GetToken(string name, bool enforceContext)
	{
		string name2 = ResolveAlias(name);
		foreach (IContext context in GetContexts())
		{
			IToken token = context.GetToken(name2, enforceContext);
			if (token != null)
			{
				return token;
			}
		}
		return null;
	}

	public IEnumerable<IToken> GetTokens(bool enforceContext)
	{
		foreach (IContext context in GetContexts())
		{
			foreach (IToken token in context.GetTokens(enforceContext))
			{
				yield return token;
			}
		}
	}

	public IInvariantSet GetValues(string name, IInputArguments input, bool enforceContext)
	{
		return GetToken(name, enforceContext)?.GetValues(input) ?? InvariantSets.Empty;
	}

	private IEnumerable<IContext> GetContexts()
	{
		return new _003C_003Ez__ReadOnlyArray<IContext>(new IContext[3] { ParentContext, LocalContext, DynamicContext });
	}
}
