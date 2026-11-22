using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using StardewModdingAPI;
using StardewValley;

namespace ContentPatcher.Framework.Tokens.ValueProviders;

internal class TranslationValueProvider : BaseValueProvider
{
	private readonly ITranslationHelper TranslationHelper;

	private LanguageCode LastLocale;

	public TranslationValueProvider(ITranslationHelper translationHelper)
		: base(ConditionType.I18n, mayReturnMultipleValuesForRoot: false)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		TranslationHelper = translationHelper;
		LastLocale = translationHelper.LocaleEnum;
		EnableInputArguments(required: true, mayReturnMultipleValues: false, 1);
		AllowAnyNamedArguments = true;
		MarkReady(ready: true);
	}

	public override bool UpdateContext(IContext context)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		LanguageCode localeEnum = TranslationHelper.LocaleEnum;
		if (localeEnum == LastLocale)
		{
			return false;
		}
		LastLocale = localeEnum;
		return true;
	}

	public override IEnumerable<string> GetValues(IInputArguments input)
	{
		AssertInput(input);
		string firstPositionalArg = input.GetFirstPositionalArg();
		if (string.IsNullOrWhiteSpace(firstPositionalArg))
		{
			return InvariantSets.Empty;
		}
		object obj = (input.HasNamedArgs ? input.NamedArgs.ToDictionary((KeyValuePair<string, IInputArgumentValue> p) => p.Key, (KeyValuePair<string, IInputArgumentValue> p) => Stringify(p.Value)) : null);
		Translation val = TranslationHelper.Get(firstPositionalArg, obj).ApplyGenderSwitchBlocks(false);
		bool flag = val.HasValue();
		if (!flag && input.NamedArgs.TryGetValue("defaultKeys", out IInputArgumentValue value))
		{
			string[] parsed = value.Parsed;
			foreach (string text in parsed)
			{
				Translation val2 = TranslationHelper.Get(text, obj).ApplyGenderSwitchBlocks(false);
				if (val2.HasValue())
				{
					val = val2;
					flag = true;
					break;
				}
			}
		}
		if (!flag && input.NamedArgs.TryGetValue("default", out IInputArgumentValue value2))
		{
			val = val.Default(Stringify(value2)).UsePlaceholder(false);
		}
		return InvariantSets.FromCaseSensitiveValue(Translation.op_Implicit(val));
	}

	private string Stringify(IInputArgumentValue input)
	{
		return string.Join(", ", input.Parsed);
	}
}
