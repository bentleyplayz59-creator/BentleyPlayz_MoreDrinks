using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

internal class TokenParser
{
	private readonly Lexer Lexer = ContentPatcher.Framework.Lexing.Lexer.Instance;

	private static readonly FieldInfo PropertyNameField;

	public IManifest ForMod { get; }

	public IContext Context { get; }

	public IMigration Migrator { get; }

	public IInvariantSet InstalledMods { get; }

	static TokenParser()
	{
		PropertyNameField = typeof(JProperty).GetField("_name", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Failed to initialize token parser: required field 'JProperty._name' not found");
	}

	public TokenParser(IContext tokenContext, IManifest forMod, IMigration migrator, IInvariantSet installedMods)
	{
		Context = tokenContext;
		ForMod = forMod;
		Migrator = migrator;
		InstalledMods = installedMods;
	}

	public IManagedTokenString CreateTokenString(ILexToken[] lexTokens, IContext context, LogPathBuilder path)
	{
		return CreateTokenStringOrNull(lexTokens, context, path) ?? new LiteralString(string.Empty, path);
	}

	public IManagedTokenString? CreateTokenStringOrNull(ILexToken[]? lexTokens, IContext context, LogPathBuilder path)
	{
		int? num = lexTokens?.Length;
		if ((!num.HasValue || num.GetValueOrDefault() == 0) ? true : false)
		{
			return null;
		}
		if (lexTokens != null && lexTokens.Length == 1 && lexTokens[0] is LexTokenLiteral lexTokenLiteral)
		{
			return string.IsNullOrWhiteSpace(lexTokenLiteral.Text) ? null : new LiteralString(lexTokenLiteral.Text, path);
		}
		return new TokenString(lexTokens, context, path);
	}

	public IInputArguments CreateInputArgs(ITokenString? inputStr)
	{
		IInputArguments result;
		if (inputStr == null)
		{
			result = InputArguments.Empty;
		}
		else
		{
			IInputArguments inputArguments = new InputArguments(inputStr);
			result = inputArguments;
		}
		return result;
	}

	public bool TryParseNullableString(string? rawValue, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, out IManagedTokenString? parsed)
	{
		if (rawValue == null)
		{
			error = null;
			parsed = null;
			return true;
		}
		return TryParseString(rawValue, assumeModIds, path, out error, out parsed);
	}

	public bool TryParseString(string? rawValue, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out IManagedTokenString? parsed, Func<LexTokenToken, string?>? preValidate = null)
	{
		if (!string.IsNullOrEmpty(rawValue))
		{
			ILexToken[] array = Lexer.ParseBits(rawValue, impliedBraces: false).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (!Migrator.TryMigrate(ref array[i], out error))
				{
					parsed = null;
					return false;
				}
			}
			parsed = CreateTokenString(array, Context, path);
			if (!Migrator.TryMigrate(parsed, out error))
			{
				return false;
			}
			foreach (LexTokenToken tokenPlaceholder in parsed.GetTokenPlaceholders(recursive: false))
			{
				error = preValidate?.Invoke(tokenPlaceholder);
				if (error != null)
				{
					return false;
				}
				if (!TryValidateToken(tokenPlaceholder, assumeModIds, out error))
				{
					return false;
				}
			}
		}
		else
		{
			parsed = new LiteralString(string.Empty, path);
		}
		error = null;
		return true;
	}

	public bool TryParseJson(JToken? rawJson, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, out TokenizableJToken? parsed)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Invalid comparison between Unknown and I4
		if (rawJson == null || (int)rawJson.Type == 10 || (int)rawJson.Type == 5)
		{
			error = null;
			parsed = null;
			return true;
		}
		if (!TryInjectJsonProxyFields(rawJson, assumeModIds, path, out error, out TokenizableProxy[] proxyFields))
		{
			parsed = null;
			return false;
		}
		parsed = new TokenizableJToken(rawJson, proxyFields);
		if (!Migrator.TryMigrate(parsed, out error))
		{
			return false;
		}
		foreach (LexTokenToken item in parsed.GetTokenStrings().SelectMany((IManagedTokenString p) => p.GetTokenPlaceholders(recursive: false)))
		{
			if (!TryValidateToken(item, assumeModIds, out error))
			{
				return false;
			}
		}
		if ((int)parsed.Value.Type == 10)
		{
			parsed = null;
		}
		error = null;
		return true;
	}

	public bool TryValidateToken(LexTokenToken lexToken, IInvariantSet assumeModIds, [NotNullWhen(false)] out string? error)
	{
		IToken token = Context.GetToken(lexToken.Name, enforceContext: false);
		if (token == null)
		{
			string providerModId = lexToken.GetProviderModId();
			if (providerModId != null && assumeModIds.Contains(providerModId) && !InstalledMods.Contains(providerModId))
			{
				error = null;
				return true;
			}
			error = $"'{lexToken}' can't be used as a token because that token could not be found.";
			return false;
		}
		if (token is ModProvidedToken modProvidedToken && !ForMod.HasDependency(modProvidedToken.Mod.UniqueID, canBeOptional: false) && !assumeModIds.Contains(modProvidedToken.Mod.UniqueID))
		{
			error = $"'{lexToken}' can't be used because it's provided by {modProvidedToken.Mod.Name} (ID: {modProvidedToken.Mod.UniqueID}), but {ForMod.Name} doesn't list it as a dependency and the patch doesn't have an immutable {ConditionType.HasMod} condition for that mod.";
			return false;
		}
		error = null;
		return true;
	}

	private bool TryInjectJsonProxyFields(JToken token, IInvariantSet assumeModIds, LogPathBuilder path, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out TokenizableProxy[]? proxyFields)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Invalid comparison between Unknown and I4
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		proxyFields = null;
		List<TokenizableProxy> list = new List<TokenizableProxy>();
		JValue val = (JValue)(object)((token is JValue) ? token : null);
		if (val == null)
		{
			JObject val2 = (JObject)(object)((token is JObject) ? token : null);
			if (val2 == null)
			{
				JArray val3 = (JArray)(object)((token is JArray) ? token : null);
				if (val3 == null)
				{
					throw new InvalidOperationException($"Unknown JSON token: {((object)token).GetType().FullName} ({token.Type})");
				}
				int num = 0;
				foreach (JToken item in val3)
				{
					if (!TryInjectJsonProxyFields(item, assumeModIds, path.With(num++.ToString()), out error, out TokenizableProxy[] proxyFields2))
					{
						return false;
					}
					list.AddRange(proxyFields2);
				}
			}
			else
			{
				JProperty[] array = val2.Properties().ToArray();
				foreach (JProperty property in array)
				{
					LogPathBuilder logPathBuilder = path.With(property.Name);
					if (!TryInjectJsonProxyField(property.Name, assumeModIds, (string value) =>
					{
						PropertyNameField.SetValue(property, value);
					}, logPathBuilder.With("key"), out error, out TokenizableProxy parsed))
					{
						return false;
					}
					list.Add(parsed);
					if (!TryInjectJsonProxyFields(property.Value, assumeModIds, logPathBuilder.With("value"), out error, out TokenizableProxy[] proxyFields3))
					{
						return false;
					}
					list.AddRange(proxyFields3);
				}
			}
		}
		else if ((int)((JToken)val).Type != 5)
		{
			string str = Extensions.Value<string>((IEnumerable<JToken>)val);
			if (!TryInjectJsonProxyField(str, assumeModIds, (string value) =>
			{
				val.Value = value;
			}, path, out error, out TokenizableProxy parsed2))
			{
				return false;
			}
			list.Add(parsed2);
		}
		proxyFields = list.WhereNotNull().ToArray();
		error = null;
		return true;
	}

	private bool TryInjectJsonProxyField(string? str, IInvariantSet assumeModIds, Action<string> setValue, LogPathBuilder path, [NotNullWhen(false)] out string? error, out TokenizableProxy? parsed)
	{
		if (string.IsNullOrWhiteSpace(str))
		{
			parsed = null;
			error = null;
			return true;
		}
		if (!TryParseString(str, assumeModIds, path, out error, out IManagedTokenString parsed2))
		{
			parsed = null;
			return false;
		}
		if (parsed2.IsMutable)
		{
			parsed = new TokenizableProxy(parsed2, setValue);
			return true;
		}
		if (parsed2.Value != str)
		{
			setValue(parsed2.Value);
		}
		parsed = null;
		return true;
	}
}
