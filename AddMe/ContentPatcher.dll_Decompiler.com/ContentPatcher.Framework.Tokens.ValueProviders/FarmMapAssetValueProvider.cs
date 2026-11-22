using System.Collections.Generic;
using ContentPatcher.Framework.Conditions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class FarmMapAssetValueProvider : BaseValueProvider
{
	private readonly TokenSaveReader SaveReader;

	private string? LastFarmType;

	private string? FarmMapAsset;

	public FarmMapAssetValueProvider(TokenSaveReader saveReader)
		: base(ConditionType.FarmMapAsset, mayReturnMultipleValuesForRoot: false)
	{
		SaveReader = saveReader;
	}

	public override bool UpdateContext(IContext context)
	{
		string farmType = SaveReader.GetFarmType();
		if (LastFarmType == farmType)
		{
			return false;
		}
		FarmMapAsset = SaveReader.GetFarmMapAssetName();
		LastFarmType = farmType;
		return true;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		return InvariantSets.FromValue(FarmMapAsset ?? string.Empty);
	}
}
