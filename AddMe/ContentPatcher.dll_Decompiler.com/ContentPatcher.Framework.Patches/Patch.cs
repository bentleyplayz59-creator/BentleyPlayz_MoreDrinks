using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Tokens;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace ContentPatcher.Framework.Patches;

internal abstract class Patch : IPatch, IContextual, IContextualInfo
{
	private readonly Func<string, IAssetName> ParseAssetNameImpl;

	protected readonly AggregateContextual Contextuals = new AggregateContextual();

	private readonly HashSet<IContextual> ManuallyUpdatedTokens = new HashSet<IContextual>(new ObjectReferenceComparer<IContextual>());

	protected readonly ContextualState State = new ContextualState();

	private readonly LocalContext PatchFieldTokensContext;

	private readonly LocalContext? CustomLocalTokensContext;

	private readonly LocalContext? InheritedLocalTokensContext;

	private bool FromAssetExistsImpl;

	protected IInvariantSet? TokensUsedCache;

	protected IManagedTokenString? ManagedRawFromAsset { get; }

	protected IManagedTokenString? ManagedRawTargetAsset { get; }

	protected IManagedTokenString? ManagedRawTargetLocale { get; set; }

	[MemberNotNullWhen(true, new string[] { "RawFromAsset", "ManagedRawFromAsset" })]
	protected bool HasFromAsset
	{
		[MemberNotNullWhen(true, new string[] { "RawFromAsset", "ManagedRawFromAsset" })]
		get
		{
			return RawFromAsset != null && ManagedRawFromAsset != null;
		}
	}

	[MemberNotNullWhen(true, new string[] { "RawTargetAsset", "ManagedRawTargetAsset" })]
	protected bool HasTargetAsset
	{
		[MemberNotNullWhen(true, new string[] { "RawTargetAsset", "ManagedRawTargetAsset" })]
		get
		{
			return RawTargetAsset != null && ManagedRawTargetAsset != null;
		}
	}

	protected InvariantDictionary<IManagedTokenString>? LocalTokens { get; }

	protected InvariantDictionary<IManagedTokenString>? LocalTokensIncludingInherited { get; }

	[MemberNotNullWhen(true, new string[] { "LocalTokens", "CustomLocalTokensContext", "InheritedLocalTokensContext" })]
	protected bool HasLocalTokens
	{
		[MemberNotNullWhen(true, new string[] { "LocalTokens", "CustomLocalTokensContext", "InheritedLocalTokensContext" })]
		get;
	}

	public int[] IndexPath { get; }

	public LogPathBuilder Path { get; }

	public PatchType Type { get; }

	public IContentPack ContentPack { get; }

	public IRuntimeMigration Migrator { get; }

	public IPatch? ParentPatch { get; }

	public bool IsMutable { get; } = true;

	public bool IsReady { get; protected set; }

	public string? FromAsset { get; private set; }

	public ITokenString? RawFromAsset => ManagedRawFromAsset;

	public IAssetName? TargetAsset { get; private set; }

	public string? TargetLocale { get; private set; }

	public IAssetName? TargetAssetBeforeRedirection { get; private set; }

	public ITokenString? RawTargetAsset => ManagedRawTargetAsset;

	public int Priority { get; }

	public UpdateRate UpdateRate { get; set; }

	public Condition[] Conditions { get; }

	public bool IsApplied { get; set; }

	public int LastChangedTick { get; private set; }

	public virtual bool UpdateContext(IContext context)
	{
		if (!IsMutable && Contextuals.WasEverUpdated)
		{
			return false;
		}
		bool isReady = IsReady;
		State.Reset();
		bool flag = true;
		bool flag2 = false;
		PatchFieldTokensContext.Update(context);
		InheritedLocalTokensContext?.UpdateParentContextLinkOnly(PatchFieldTokensContext);
		LocalContext context2 = InheritedLocalTokensContext ?? PatchFieldTokensContext;
		IManagedTokenString? managedRawTargetAsset = ManagedRawTargetAsset;
		flag2 = ((managedRawTargetAsset == null || !managedRawTargetAsset.UsesTokens(InternalConstants.FromFileTokens)) ? (flag2 | (UpdateTargetPath(context2) | UpdateFromFile(context2))) : (flag2 | (UpdateFromFile(context2) | UpdateTargetPath(context2))));
		bool flag3 = flag;
		ITokenString? rawTargetAsset = RawTargetAsset;
		flag = flag3 & ((rawTargetAsset == null || rawTargetAsset.IsReady) && (RawFromAsset?.IsReady ?? true));
		if (HasLocalTokens)
		{
			foreach (var (name, managedTokenString2) in LocalTokens)
			{
				flag2 |= managedTokenString2.UpdateContext(InheritedLocalTokensContext);
				CustomLocalTokensContext.SetLocalValue(name, managedTokenString2, managedTokenString2.IsReady);
			}
			CustomLocalTokensContext.UpdateParentContextLinkOnly(InheritedLocalTokensContext);
		}
		if (flag)
		{
			IContext context3 = CustomLocalTokensContext ?? PatchFieldTokensContext;
			flag2 |= Contextuals.UpdateContext(context3, (IContextual p) => !ManuallyUpdatedTokens.Contains(p), (IContextual p) => !(p is Condition));
			flag &= Contextuals.IsReady && (!Conditions.Any() || Conditions.All((Condition p) => p.IsMatch));
			FromAssetExistsImpl = false;
		}
		if (flag && HasFromAsset && FromAsset != null)
		{
			FromAssetExistsImpl = ContentPack.HasFile(FromAsset);
			if (!FromAssetExistsImpl && Conditions.All((Condition p) => p.IsMatch))
			{
				State.AddError("FromFile '" + FromAsset + "' does not exist");
			}
		}
		IsReady = flag;
		if (flag2 || IsReady != isReady)
		{
			LastChangedTick = Game1.ticks;
			return MarkUpdated();
		}
		return false;
	}

	[MemberNotNullWhen(true, new string[] { "RawFromAsset", "FromAsset", "ManagedRawFromAsset" })]
	public bool FromAssetExists()
	{
		return FromAssetExistsImpl;
	}

	public virtual T Load<T>(IAssetName assetName) where T : notnull
	{
		throw new NotSupportedException("This patch type doesn't support loading assets.");
	}

	public virtual void Edit<T>(IAssetData asset) where T : notnull
	{
		throw new NotSupportedException("This patch type doesn't support loading assets.");
	}

	public IInvariantSet GetTokensUsed()
	{
		return TokensUsedCache ?? (TokensUsedCache = Contextuals.GetTokensUsed());
	}

	public IContextualState GetDiagnosticState()
	{
		return State.Clone().MergeFrom(Contextuals.GetDiagnosticState());
	}

	public abstract IEnumerable<string> GetChangeLabels();

	protected Patch(int[] indexPath, LogPathBuilder path, PatchType type, IManagedTokenString? assetName, IManagedTokenString? assetLocale, int priority, UpdateRate updateRate, InvariantDictionary<IManagedTokenString>? inheritedLocalTokens, InvariantDictionary<IManagedTokenString>? localTokens, IEnumerable<Condition> conditions, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, Func<string, IAssetName> parseAssetName, IManagedTokenString? fromAsset = null)
	{
		IndexPath = indexPath;
		Path = path;
		Type = type;
		ManagedRawTargetAsset = assetName;
		ManagedRawTargetLocale = assetLocale;
		Priority = priority;
		UpdateRate = updateRate;
		Conditions = conditions.ToArray();
		ParseAssetNameImpl = parseAssetName;
		PatchFieldTokensContext = new LocalContext(contentPack.Manifest.UniqueID);
		ManagedRawFromAsset = fromAsset;
		ContentPack = contentPack;
		Migrator = migrator;
		ParentPatch = parentPatch;
		Contextuals.Add(Conditions).Add(assetName).Add(assetLocale)
			.Add(fromAsset);
		if (assetName != null)
		{
			ManuallyUpdatedTokens.Add(assetName);
		}
		if (assetLocale != null)
		{
			ManuallyUpdatedTokens.Add(assetLocale);
		}
		if (fromAsset != null)
		{
			ManuallyUpdatedTokens.Add(fromAsset);
		}
		if (localTokens == null && inheritedLocalTokens == null)
		{
			return;
		}
		LocalTokens = localTokens ?? new InvariantDictionary<IManagedTokenString>();
		LocalTokensIncludingInherited = new InvariantDictionary<IManagedTokenString>(LocalTokens);
		CustomLocalTokensContext = new LocalContext(contentPack.Manifest.UniqueID);
		InheritedLocalTokensContext = new LocalContext(contentPack.Manifest.UniqueID);
		HasLocalTokens = true;
		string key;
		IManagedTokenString value;
		if (localTokens != null)
		{
			foreach (KeyValuePair<string, IManagedTokenString> localToken in localTokens)
			{
				localToken.Deconstruct(out key, out value);
				string name = key;
				IManagedTokenString managedTokenString = value;
				CustomLocalTokensContext.SetLocalValue(name, managedTokenString, managedTokenString.IsReady);
				Contextuals.Add(managedTokenString);
				ManuallyUpdatedTokens.Add(managedTokenString);
			}
		}
		if (inheritedLocalTokens == null)
		{
			return;
		}
		foreach (KeyValuePair<string, IManagedTokenString> inheritedLocalToken in inheritedLocalTokens)
		{
			inheritedLocalToken.Deconstruct(out key, out value);
			string text = key;
			IManagedTokenString managedTokenString2 = value;
			LocalTokensIncludingInherited.TryAdd(text, managedTokenString2);
			InheritedLocalTokensContext.SetLocalValue(text, managedTokenString2, managedTokenString2.IsReady);
			Contextuals.Add(managedTokenString2);
			ManuallyUpdatedTokens.Add(managedTokenString2);
		}
	}

	protected bool MarkUpdated()
	{
		return true;
	}

	protected bool TryReadArea(TokenRectangle? tokenArea, int defaultX, int defaultY, int defaultWidth, int defaultHeight, out Rectangle area, [NotNullWhen(false)] out string? error)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (tokenArea != null)
		{
			return tokenArea.TryGetRectangle(out area, out error);
		}
		area = new Rectangle(defaultX, defaultY, defaultWidth, defaultHeight);
		error = null;
		return true;
	}

	protected bool Fail(string inError, out string outError)
	{
		outError = inError;
		return false;
	}

	private bool UpdateTargetPath(LocalContext context)
	{
		if (!HasTargetAsset)
		{
			return false;
		}
		bool result = ManagedRawTargetAsset.UpdateContext(context) | (ManagedRawTargetLocale?.UpdateContext(context) ?? false);
		if (RawTargetAsset.IsReady)
		{
			IManagedTokenString? managedRawTargetLocale = ManagedRawTargetLocale;
			if (managedRawTargetLocale == null || managedRawTargetLocale.IsReady)
			{
				IAssetName val = ParseAssetNameImpl(RawTargetAsset.Value);
				IAssetName val2 = Migrator.RedirectTarget(val, this);
				TargetAsset = val2 ?? val;
				TargetLocale = ManagedRawTargetLocale?.Value;
				TargetAssetBeforeRedirection = ((val2 != null) ? val : null);
				context.SetLocalValue("Target", TargetAsset.Name);
				context.SetLocalValue("TargetPathOnly", System.IO.Path.GetDirectoryName(TargetAsset.Name));
				context.SetLocalValue("TargetWithoutPath", System.IO.Path.GetFileName(TargetAsset.Name));
				goto IL_0181;
			}
		}
		TargetAsset = null;
		TargetLocale = null;
		context.SetLocalValue("Target", "", ready: false);
		context.SetLocalValue("TargetPathOnly", "", ready: false);
		context.SetLocalValue("TargetWithoutPath", "", ready: false);
		goto IL_0181;
		IL_0181:
		return result;
	}

	private bool UpdateFromFile(LocalContext context)
	{
		if (!HasFromAsset)
		{
			FromAsset = null;
			context.SetLocalValue("FromFile", "");
			return false;
		}
		bool result = ManagedRawFromAsset.UpdateContext(context);
		if (RawFromAsset.IsReady)
		{
			FromAsset = NormalizeLocalAssetPath(RawFromAsset.Value, "FromFile field");
			context.SetLocalValue("FromFile", FromAsset);
		}
		else
		{
			FromAsset = null;
			context.SetLocalValue("FromFile", "", ready: false);
		}
		return result;
	}

	private string? NormalizeLocalAssetPath(string? path, string logName)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return null;
			}
			string text = PathUtilities.NormalizePath(path);
			string fullPath = ContentPack.GetFullPath(text);
			if (!File.Exists(fullPath) && File.Exists(fullPath + ".xnb"))
			{
				text += ".xnb";
			}
			return text;
		}
		catch (Exception innerException)
		{
			throw new FormatException($"The {logName} for patch '{Path}' isn't a valid asset path (current value: '{path}').", innerException);
		}
	}
}
