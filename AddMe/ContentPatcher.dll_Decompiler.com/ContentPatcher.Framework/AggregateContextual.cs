using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework;

internal class AggregateContextual : IContextual, IContextualInfo
{
	private readonly HashSet<IContextual> ValuesImpl = new HashSet<IContextual>();

	public IEnumerable<IContextual> Values => ValuesImpl;

	public bool IsMutable { get; private set; }

	public bool IsReady { get; private set; } = true;

	public bool WasEverUpdated { get; private set; }

	public AggregateContextual Add(IContextual? value)
	{
		if (value != null)
		{
			ValuesImpl.Add(value);
			IsMutable = IsMutable || value.IsMutable;
			IsReady = IsReady && value.IsReady;
		}
		return this;
	}

	public AggregateContextual Add(IEnumerable<IContextual?>? values)
	{
		if (values != null)
		{
			foreach (IContextual value in values)
			{
				Add(value);
			}
		}
		return this;
	}

	public AggregateContextual Remove(IContextual? value)
	{
		if (value != null && ValuesImpl.Remove(value))
		{
			IsMutable = ValuesImpl.Any((IContextual p) => p.IsMutable);
			IsReady = ValuesImpl.All((IContextual p) => p.IsReady);
		}
		return this;
	}

	public AggregateContextual Remove(IEnumerable<IContextual?>? values)
	{
		if (values != null)
		{
			foreach (IContextual value in values)
			{
				Remove(value);
			}
		}
		return this;
	}

	bool IContextual.UpdateContext(IContext context)
	{
		return UpdateContext(context);
	}

	public bool UpdateContext(IContext context, Func<IContextual, bool>? update = null, Func<IContextual, bool>? countChange = null)
	{
		bool isReady = IsReady;
		IsReady = true;
		bool flag = false;
		foreach (IContextual item in ValuesImpl)
		{
			if (update == null || update(item))
			{
				if (item.UpdateContext(context) && (countChange == null || countChange(item)))
				{
					flag = true;
				}
				if (!item.IsReady)
				{
					IsReady = false;
					break;
				}
			}
		}
		WasEverUpdated = true;
		return flag || IsReady != isReady;
	}

	public IInvariantSet GetTokensUsed()
	{
		return InvariantSets.From(ValuesImpl.SelectMany((IContextual p) => p.GetTokensUsed()));
	}

	public IContextualState GetDiagnosticState()
	{
		ContextualState contextualState = new ContextualState();
		foreach (IContextual item in ValuesImpl)
		{
			contextualState.MergeFrom(item.GetDiagnosticState());
		}
		return contextualState;
	}
}
