using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class InternalAssetKeyValueProvider : BaseValueProvider
{
	private readonly Func<string, IAssetName> GetInternalAssetKey;

	public InternalAssetKeyValueProvider(Func<string, IAssetName> getInternalAssetKey)
		: base(ConditionType.InternalAssetKey, mayReturnMultipleValuesForRoot: false)
	{
		GetInternalAssetKey = getInternalAssetKey;
		EnableInputArguments(required: true, mayReturnMultipleValues: false, null);
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
		return (!string.IsNullOrWhiteSpace(positionalSegment)) ? InvariantSets.FromValue(GetInternalAssetKey(positionalSegment).Name) : InvariantSets.Empty;
	}
}
