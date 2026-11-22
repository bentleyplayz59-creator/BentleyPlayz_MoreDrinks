using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Commands;

internal class PatchBaseInfo
{
	public Condition[] ParsedConditions { get; }

	public bool MatchesContext { get; }

	public IContextualState State { get; }

	public PatchBaseInfo(Condition[]? parsedConditions, bool matchesContext, IContextualState state)
	{
		ParsedConditions = parsedConditions ?? Array.Empty<Condition>();
		MatchesContext = matchesContext;
		State = state;
	}

	public virtual string? GetReasonNotLoaded()
	{
		IContextualState state = State;
		if (state.InvalidTokens.Count > 0 || state.UnreadyTokens.Count > 0 || state.Errors.Count > 0)
		{
			List<string> list = new List<string>();
			if (state.InvalidTokens.Any())
			{
				list.Add("invalid tokens: " + string.Join(", ", state.InvalidTokens.OrderByHuman()));
			}
			if (state.UnreadyTokens.Any())
			{
				list.Add("tokens not ready: " + string.Join(", ", state.UnreadyTokens.OrderByHuman()));
			}
			if (state.Errors.Any())
			{
				list.Add(string.Join("; ", state.Errors));
			}
			if (list.Any())
			{
				return string.Join("; ", list);
			}
		}
		if (!MatchesContext && ParsedConditions.Any())
		{
			string[] array = (from condition in ParsedConditions
				let displayText = (!condition.Is(ConditionType.HasFile) && !string.IsNullOrWhiteSpace(condition.Input.TokenString?.Raw)) ? (condition.Name + ":" + condition.Input.TokenString.Raw) : condition.Name
				orderby displayText
				where !condition.IsMatch
				select displayText ?? "").ToArray();
			if (array.Any())
			{
				return "conditions don't match: " + string.Join(", ", array);
			}
		}
		if (state.UnavailableModTokens.Any())
		{
			return "tokens provided by an unavailable mod: " + string.Join(", ", state.UnavailableModTokens.OrderByHuman());
		}
		if (!MatchesContext)
		{
			return "doesn't match context (unknown reason)";
		}
		return null;
	}
}
