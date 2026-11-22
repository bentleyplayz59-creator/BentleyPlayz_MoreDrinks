using System.Linq;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Conditions;

internal class Condition : IContextual, IContextualInfo
{
	private readonly AggregateContextual Contextuals;

	private readonly ContextualState State = new ContextualState();

	public string Name { get; }

	public IInputArguments Input { get; }

	public ITokenString Values { get; }

	public IInvariantSet CurrentValues { get; private set; }

	public bool IsMutable => field || Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady && State.IsReady;

	public bool IsMatch { get; private set; }

	public Condition(string name, IManagedTokenString? input, IManagedTokenString values, bool isTokenMutable)
	{
		Name = name;
		IInputArguments inputArguments;
		if (string.IsNullOrWhiteSpace(input?.Raw))
		{
			inputArguments = InputArguments.Empty;
		}
		else
		{
			IInputArguments inputArguments2 = new InputArguments(input);
			inputArguments = inputArguments2;
		}
		Input = inputArguments;
		Values = values;
		IsMutable = isTokenMutable;
		Contextuals = new AggregateContextual().Add(input).Add(values);
		CurrentValues = (Values.IsReady ? Values.SplitValuesUnique() : InvariantSets.Empty);
	}

	public bool Is(ConditionType type)
	{
		return StringExtensions.EqualsIgnoreCase(Name, type.ToString());
	}

	public bool UpdateContext(IContext context)
	{
		if (!IsMutable && Contextuals.WasEverUpdated)
		{
			return false;
		}
		bool isReady = IsReady;
		bool isMatch = IsMatch;
		State.Reset();
		bool flag = Contextuals.UpdateContext(context);
		IToken token = context.GetToken(Name, enforceContext: true);
		if (token == null)
		{
			State.AddUnreadyToken(Name);
		}
		if (IsReady && token != null)
		{
			CurrentValues = Values.SplitValuesUnique(token.NormalizeValue);
			IsMatch = token.GetValues(Input).Any((string value) => CurrentValues.Contains(value));
		}
		else
		{
			CurrentValues = InvariantSets.Empty;
			IsMatch = false;
		}
		return flag || isReady != IsReady || isMatch != IsMatch;
	}

	public IInvariantSet GetTokensUsed()
	{
		return Contextuals.GetTokensUsed().GetWith(Name);
	}

	public IContextualState GetDiagnosticState()
	{
		return State.Clone().MergeFrom(Contextuals.GetDiagnosticState());
	}
}
