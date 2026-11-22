using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using Pathoschild.Stardew.Common;
using StardewValley.Extensions;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class WeatherValueProvider : BaseValueProvider
{
	private readonly TokenSaveReader SaveReader;

	private readonly Dictionary<string, string?> Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private string? CurrentLocationContextName;

	public WeatherValueProvider(TokenSaveReader saveReader)
		: base(ConditionType.Weather, mayReturnMultipleValuesForRoot: false)
	{
		SaveReader = saveReader;
		EnableInputArguments(required: false, mayReturnMultipleValues: true, null);
	}

	public override bool UpdateContext(IContext context)
	{
		return IsChanged(() =>
		{
			bool flag = false;
			if (MarkReady(SaveReader.IsReady))
			{
				string currentLocationContext = SaveReader.GetCurrentLocationContext(SaveReader.GetCurrentPlayer());
				flag |= currentLocationContext != CurrentLocationContextName;
				CurrentLocationContextName = currentLocationContext;
				HashSet<string> currentContexts = SaveReader.GetContexts();
				foreach (string item in currentContexts)
				{
					string weather = SaveReader.GetWeather(item);
					flag |= !Values.TryGetValue(item, out string value) || value != weather;
					Values[item] = weather;
				}
				string[] array = Values.Keys.Where((string p) => !currentContexts.Contains(p)).ToArray();
				foreach (string key in array)
				{
					Values.Remove(key);
					flag = true;
				}
			}
			return flag;
		});
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		IEnumerable<string> values = (from context in GetContextsFor(input)
			select (context != null && Values.TryGetValue(context, out string value)) ? value : "Sun").WhereNotNull();
		return InvariantSets.From(values);
	}

	private IEnumerable<string?> GetContextsFor(IInputArguments input)
	{
		if (!input.HasPositionalArgs)
		{
			yield return CurrentLocationContextName;
			yield break;
		}
		string[] positionalArgs = input.PositionalArgs;
		foreach (string context in positionalArgs)
		{
			if (StringExtensions.EqualsIgnoreCase(context, "current"))
			{
				yield return CurrentLocationContextName;
			}
			else
			{
				yield return context;
			}
		}
	}
}
