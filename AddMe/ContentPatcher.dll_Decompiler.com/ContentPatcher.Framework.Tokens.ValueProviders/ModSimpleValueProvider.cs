using System;
using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class ModSimpleValueProvider : BaseValueProvider
{
	private readonly Func<IEnumerable<string>?> GetValueImpl;

	private IInvariantSet Values = InvariantSets.Empty;

	public ModSimpleValueProvider(string name, Func<IEnumerable<string>?> getValue)
		: base(name, mayReturnMultipleValuesForRoot: true)
	{
		GetValueImpl = getValue;
		MarkReady(ready: false);
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return IsReady ? Values : InvariantSets.Empty;
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(Values, () =>
		{
			IEnumerable<string> enumerable = GetValueImpl();
			Values = ((enumerable != null) ? InvariantSets.From(enumerable) : InvariantSets.Empty);
			MarkReady(Values.Any());
			return Values;
		});
	}
}
