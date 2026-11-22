using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class RandomValueProvider : BaseValueProvider
{
	private int BaseSeed;

	public RandomValueProvider()
		: base(ConditionType.Random, mayReturnMultipleValuesForRoot: false)
	{
		EnableInputArguments(required: true, mayReturnMultipleValues: false, null);
		ValidNamedArguments = InvariantSets.FromValue("key");
		BaseSeed = GenerateBaseSeed();
	}

	public override bool UpdateContext(IContext context)
	{
		int baseSeed = BaseSeed;
		BaseSeed = GenerateBaseSeed();
		return base.UpdateContext(context) || BaseSeed != baseSeed;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		if (!input.HasPositionalArgs)
		{
			return InvariantSets.Empty;
		}
		string text = input.GetRawArgumentValue("key") ?? input.TokenString.Path;
		int seed = Utility.CreateRandomSeed((double)BaseSeed, (double)Game1.hash.GetDeterministicHashCode(text), 0.0, 0.0, 0.0);
		int num = new Random(seed).Next();
		return InvariantSets.FromValue(input.PositionalArgs[num % input.PositionalArgs.Length]);
	}

	public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		allowedValues = ((!input.IsMutable) ? InvariantSets.From(input.PositionalArgs) : null);
		return allowedValues != null;
	}

	private int GenerateBaseSeed()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		int daysSinceStart = SDate.Now().DaysSinceStart;
		ulong uniqueIDForThisGame = Game1.uniqueIDForThisGame;
		if (!Context.IsWorldReady && SaveGame.loaded != null)
		{
			SaveGame loaded = SaveGame.loaded;
			daysSinceStart = new SDate(loaded.dayOfMonth, loaded.currentSeason, loaded.year).DaysSinceStart;
			uniqueIDForThisGame = loaded.uniqueIDForThisGame;
		}
		return Game1.hash.GetDeterministicHashCode(new int[2]
		{
			daysSinceStart,
			(int)(uniqueIDForThisGame % 2147483647)
		});
	}
}
