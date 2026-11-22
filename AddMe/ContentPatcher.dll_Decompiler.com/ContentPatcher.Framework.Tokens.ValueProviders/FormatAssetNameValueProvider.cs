using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI.Utilities;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class FormatAssetNameValueProvider : BaseValueProvider
{
	public FormatAssetNameValueProvider()
		: base(ConditionType.FormatAssetName, mayReturnMultipleValuesForRoot: false, isDeterministicForInput: true)
	{
		EnableInputArguments(required: true, mayReturnMultipleValues: false, null);
		ValidNamedArguments = InvariantSets.FromValue("separator");
	}

	public override bool UpdateContext(IContext context)
	{
		bool result = !IsReady;
		MarkReady(ready: true);
		return result;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		string positionalSegment = input.GetPositionalSegment();
		if (!string.IsNullOrWhiteSpace(positionalSegment))
		{
			positionalSegment = PathUtilities.NormalizeAssetName(positionalSegment);
			if (input.NamedArgs.TryGetValue("separator", out IInputArgumentValue value))
			{
				positionalSegment = positionalSegment.Replace(PathUtilities.PreferredAssetSeparator.ToString(), value.Parsed.FirstOrDefault());
			}
			return InvariantSets.FromValue(positionalSegment);
		}
		return InvariantSets.Empty;
	}
}
