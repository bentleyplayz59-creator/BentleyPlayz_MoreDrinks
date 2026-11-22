using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.ValueProviders;
using ContentPatcher.Framework.Tokens.ValueProviders.ModConvention;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework;

public class ContentPatcherAPI : IContentPatcherAPI
{
	internal delegate IManagedConditions ParseConditionsDelegate(IManifest manifest, InvariantDictionary<string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null);

	internal delegate ContentPatcher.IManagedTokenString ParseTokenStringDelegate(IManifest manifest, string rawTokenString, ISemanticVersion formatVersion, string[]? assumeModIds = null);

	private static readonly Regex ValidNamePattern = new Regex("^[a-z]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private readonly string ContentPatcherId;

	private readonly IMonitor Monitor;

	private readonly IReflectionHelper Reflection;

	private readonly Action<ModProvidedToken> AddModToken;

	private readonly Func<bool> IsConditionsApiReadyImpl;

	private readonly ParseConditionsDelegate ParseConditionsImpl;

	private readonly ParseTokenStringDelegate ParseTokenStringImpl;

	public bool IsConditionsApiReady => IsConditionsApiReadyImpl();

	internal ContentPatcherAPI(string contentPatcherId, IMonitor monitor, IReflectionHelper reflection, Action<ModProvidedToken> addModToken, Func<bool> isConditionsApiReady, ParseConditionsDelegate parseConditions, ParseTokenStringDelegate parseTokenString)
	{
		ContentPatcherId = contentPatcherId;
		Monitor = monitor;
		Reflection = reflection;
		AddModToken = addModToken;
		IsConditionsApiReadyImpl = isConditionsApiReady;
		ParseConditionsImpl = parseConditions;
		ParseTokenStringImpl = parseTokenString;
	}

	public IManagedConditions ParseConditions(IManifest manifest, IDictionary<string, string?>? rawConditions, ISemanticVersion formatVersion, string[]? assumeModIds = null)
	{
		if (!IsConditionsApiReady)
		{
			throw new InvalidOperationException($"'{manifest.Name}' accessed Content Patcher's conditions API before it was ready to use. (For mod authors: see the documentation on {"IContentPatcherAPI"}.{"IsConditionsApiReady"} for details.)");
		}
		if (!manifest.HasDependency(ContentPatcherId, out ISemanticVersion minVersion, canBeOptional: false))
		{
			throw new InvalidOperationException("'" + manifest.Name + "' must list Content Patcher as a required dependency in its manifest.json to access the conditions API.");
		}
		if (minVersion == null || minVersion.IsOlderThan("1.22.0"))
		{
			throw new InvalidOperationException("'" + manifest.Name + "' must specify Content Patcher 1.22.0 as the minimum required version in its manifest.json to access the conditions API.");
		}
		InvariantDictionary<string> rawConditions2 = ((rawConditions != null && rawConditions.Any()) ? new InvariantDictionary<string>(rawConditions) : null);
		return ParseConditionsImpl(manifest, rawConditions2, formatVersion, assumeModIds);
	}

	public ContentPatcher.IManagedTokenString ParseTokenString(IManifest manifest, string rawValue, ISemanticVersion formatVersion, string[]? assumeModIds = null)
	{
		if (!IsConditionsApiReady)
		{
			throw new InvalidOperationException($"'{manifest.Name}' accessed Content Patcher's token string API before it was ready to use. (For mod authors: see the documentation on {"IContentPatcherAPI"}.{"IsConditionsApiReady"} for details.)");
		}
		if (!manifest.HasDependency(ContentPatcherId, out ISemanticVersion minVersion, canBeOptional: false))
		{
			throw new InvalidOperationException("'" + manifest.Name + "' must list Content Patcher as a required dependency in its manifest.json to access the token string API.");
		}
		if (minVersion == null || minVersion.IsOlderThan("2.1.0"))
		{
			throw new InvalidOperationException("'" + manifest.Name + "' must specify Content Patcher 2.1.0 as the minimum required version in its manifest.json to access the token string API.");
		}
		return ParseTokenStringImpl(manifest, rawValue, formatVersion, assumeModIds);
	}

	public void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>?> getValue)
	{
		RegisterValueProvider(mod, new ModSimpleValueProvider(name, getValue));
	}

	public void RegisterToken(IManifest mod, string name, object token)
	{
		if (token is Func<IEnumerable<string>> getValue)
		{
			RegisterValueProvider(mod, new ModSimpleValueProvider(name, getValue));
		}
		else
		{
			RegisterValueProviderByConvention(mod, name, token);
		}
	}

	private void RegisterValueProvider(IManifest mod, IValueProvider valueProvider)
	{
		if (valueProvider == null)
		{
			Monitor.Log("Rejected token added by " + mod.Name + " because the token is null.", (LogLevel)4);
			return;
		}
		if (!mod.HasDependency(ContentPatcherId))
		{
			Monitor.Log("Rejected token added by " + mod.Name + " because that mod doesn't list Content Patcher as a dependency.", (LogLevel)4);
			return;
		}
		string text = valueProvider.Name?.Trim();
		if (string.IsNullOrWhiteSpace(text))
		{
			Monitor.Log("Rejected token added by " + mod.Name + " because the token has no name.", (LogLevel)4);
			return;
		}
		if (!ValidNamePattern.IsMatch(text))
		{
			Monitor.Log("Rejected token added by " + mod.Name + " because the token name is invalid (it can only contain alphabetical characters).", (LogLevel)4);
			return;
		}
		ModProvidedToken obj = new ModProvidedToken(text, mod, valueProvider, Monitor);
		AddModToken(obj);
	}

	private void RegisterValueProviderByConvention(IManifest mod, string name, object? provider)
	{
		ConventionWrapper wrapper;
		string error;
		if (provider == null)
		{
			Monitor.Log($"Rejected token '{name}' added by {mod.Name} because the token is null.", (LogLevel)4);
		}
		else if (!ConventionWrapper.TryCreate(provider, Reflection, out wrapper, out error))
		{
			Monitor.Log($"Rejected token '{name}' added by {mod.Name} because it could not be mapped: {error}", (LogLevel)4);
		}
		else
		{
			RegisterValueProvider(mod, new ConventionValueProvider(name, wrapper));
		}
	}
}
