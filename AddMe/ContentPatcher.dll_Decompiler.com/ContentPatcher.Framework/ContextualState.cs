using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework;

internal class ContextualState : IContextualState
{
	private readonly MutableInvariantSet InvalidTokensImpl = new MutableInvariantSet();

	private readonly MutableInvariantSet UnreadyTokensImpl = new MutableInvariantSet();

	private readonly MutableInvariantSet UnavailableModTokensImpl = new MutableInvariantSet();

	private readonly MutableInvariantSet ErrorsImpl = new MutableInvariantSet();

	public bool IsValid => !InvalidTokens.Any() && !UnavailableModTokens.Any();

	public bool IsInScope => IsValid && !UnreadyTokens.Any();

	public bool IsReady => IsInScope && !Errors.Any();

	public IInvariantSet InvalidTokens => InvalidTokensImpl.GetImmutable();

	public IInvariantSet UnreadyTokens => UnreadyTokensImpl.GetImmutable();

	public IInvariantSet UnavailableModTokens => UnavailableModTokensImpl.GetImmutable();

	public IInvariantSet Errors => ErrorsImpl.GetImmutable();

	public ContextualState Reset()
	{
		InvalidTokensImpl.Clear();
		UnreadyTokensImpl.Clear();
		ErrorsImpl.Clear();
		return this;
	}

	public ContextualState Clone()
	{
		return new ContextualState().MergeFrom(this);
	}

	public ContextualState MergeFrom(IContextualState? other)
	{
		if (other is ContextualState contextualState)
		{
			AddRange(InvalidTokensImpl, contextualState.InvalidTokensImpl);
			AddRange(UnreadyTokensImpl, contextualState.UnreadyTokensImpl);
			AddRange(ErrorsImpl, contextualState.ErrorsImpl);
		}
		else if (other != null)
		{
			AddRange(InvalidTokensImpl, other.InvalidTokens);
			AddRange(UnreadyTokensImpl, other.UnreadyTokens);
			AddRange(ErrorsImpl, other.Errors);
		}
		return this;
	}

	public ContextualState AddInvalidToken(string token)
	{
		InvalidTokensImpl.Add(token);
		return this;
	}

	public ContextualState AddUnreadyToken(string token)
	{
		UnreadyTokensImpl.Add(token);
		return this;
	}

	public ContextualState AddUnavailableModToken(string token)
	{
		UnavailableModTokensImpl.Add(token);
		return this;
	}

	public ContextualState AddError(string error)
	{
		ErrorsImpl.Add(error);
		return this;
	}

	private void AddRange(MutableInvariantSet target, IEnumerable<string>? source)
	{
		if (source != null)
		{
			CollectionExtensions.AddRange<string>((ISet<string>)target, source);
		}
	}
}
