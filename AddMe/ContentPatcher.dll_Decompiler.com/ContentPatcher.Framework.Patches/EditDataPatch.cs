using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Constants;
using ContentPatcher.Framework.Migrations;
using ContentPatcher.Framework.Patches.EditData;
using ContentPatcher.Framework.TextOperations;
using ContentPatcher.Framework.Tokens;
using ContentPatcher.Framework.Tokens.Json;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Toolkit;
using xTile;

namespace ContentPatcher.Framework.Patches;

internal class EditDataPatch : Patch
{
	public delegate bool TryParseFieldsDelegate(IContext context, PatchConfig entry, out List<EditDataPatchRecord> entries, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out List<IManagedTokenString> targetField, [NotNullWhen(false)] out string? error);

	private readonly IMonitor Monitor;

	private readonly KeyValueEditorFactory EditorFactory = new KeyValueEditorFactory();

	private readonly IManagedTokenString[] TargetField;

	private EditDataPatchRecord[] Records;

	private EditDataPatchField[] Fields;

	private EditDataPatchMoveRecord[] MoveRecords;

	private readonly ITextOperation[] TextOperations;

	private readonly TryParseFieldsDelegate TryParseFields;

	private static readonly Lazy<JsonSerializer> JsonSerializer = new Lazy<JsonSerializer>((Func<JsonSerializer>)CreateJsonSerializer);

	private bool HasEverLoadedFromFile;

	public EditDataPatch(int[] indexPath, LogPathBuilder path, IManagedTokenString assetName, IManagedTokenString? assetLocale, AssetEditPriority priority, IEnumerable<Condition> conditions, IManagedTokenString? fromFile, IEnumerable<EditDataPatchRecord>? records, IEnumerable<EditDataPatchField>? fields, IEnumerable<EditDataPatchMoveRecord>? moveRecords, IEnumerable<ITextOperation>? textOperations, IEnumerable<IManagedTokenString>? targetField, UpdateRate updateRate, InvariantDictionary<IManagedTokenString>? inheritedLocalTokens, InvariantDictionary<IManagedTokenString>? localTokens, IContentPack contentPack, IRuntimeMigration migrator, IPatch? parentPatch, IMonitor monitor, Func<string, IAssetName> parseAssetName, TryParseFieldsDelegate tryParseFields)
		: base(indexPath, path, PatchType.EditData, assetName, assetLocale, (int)priority, updateRate, inheritedLocalTokens, localTokens, conditions, contentPack, migrator, parentPatch, parseAssetName, fromFile)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected I4, but got Unknown
		Records = records?.ToArray() ?? Array.Empty<EditDataPatchRecord>();
		Fields = fields?.ToArray() ?? Array.Empty<EditDataPatchField>();
		MoveRecords = moveRecords?.ToArray() ?? Array.Empty<EditDataPatchMoveRecord>();
		TextOperations = textOperations?.ToArray() ?? Array.Empty<ITextOperation>();
		TargetField = targetField?.ToArray() ?? Array.Empty<IManagedTokenString>();
		Monitor = monitor;
		TryParseFields = tryParseFields;
		Contextuals.Add(Records).Add(Fields).Add(MoveRecords)
			.Add(TextOperations)
			.Add(TargetField)
			.Add(Conditions);
	}

	public override bool UpdateContext(IContext context)
	{
		if (HasFromAsset && (ManagedRawFromAsset.UpdateContext(context) || !HasEverLoadedFromFile))
		{
			HasEverLoadedFromFile = true;
			Contextuals.Remove(Records).Remove(Fields).Remove(MoveRecords);
			base.UpdateContext(context);
			Records = Array.Empty<EditDataPatchRecord>();
			Fields = Array.Empty<EditDataPatchField>();
			MoveRecords = Array.Empty<EditDataPatchMoveRecord>();
			if (IsReady)
			{
				if (TryLoadFile(RawFromAsset, context, out List<EditDataPatchRecord> entries, out List<EditDataPatchField> fields, out List<EditDataPatchMoveRecord> moveEntries, out string error))
				{
					Records = entries.ToArray();
					Fields = fields.ToArray();
					MoveRecords = moveEntries.ToArray();
				}
				else
				{
					Monitor.Log($"Can't load \"{Path}\" fields from file '{RawFromAsset}': {error}.", (LogLevel)3);
				}
			}
			Contextuals.Add(Records).Add(Fields).Add(MoveRecords)
				.UpdateContext(context);
			IsReady = IsReady && Contextuals.IsReady;
			TokensUsedCache = null;
			return MarkUpdated();
		}
		return base.UpdateContext(context);
	}

	public override void Edit<T>(IAssetData asset)
	{
		Edit<T>(asset, null);
	}

	public void Edit<T>(IAssetData asset, Action<string, IMonitor>? onWarning)
	{
		if (!EditorFactory.TryGetEditorFor(((IAssetData<object>)(object)asset).Data, out IKeyValueEditor editor))
		{
			WarnForPatch(GetEditorNotCompatibleError("the target asset", ((IAssetData<object>)(object)asset).Data, entryExists: true), onWarning);
			return;
		}
		if (TargetField.Any())
		{
			List<string> list = new List<string>(TargetField.Length);
			IManagedTokenString[] targetField = TargetField;
			foreach (IManagedTokenString managedTokenString in targetField)
			{
				list.Add(managedTokenString.Value);
				IKeyValueEditor keyValueEditor = editor;
				object key = keyValueEditor.ParseKey(managedTokenString.Value);
				object entry = keyValueEditor.GetEntry(key);
				if (entry == null)
				{
					Type entryType = keyValueEditor.GetEntryType(key);
					if (entryType?.IsGenericType ?? false)
					{
						Type genericTypeDefinition = entryType.GetGenericTypeDefinition();
						if (genericTypeDefinition == typeof(List<>) || genericTypeDefinition == typeof(Dictionary<, >))
						{
							object value = Activator.CreateInstance(entryType);
							keyValueEditor.SetEntry(key, value);
							entry = keyValueEditor.GetEntry(key);
						}
					}
				}
				if (!EditorFactory.TryGetEditorFor(entry, out editor))
				{
					WarnForPatch(GetEditorNotCompatibleError("the field '" + string.Join("' > '", list) + "'", entry, keyValueEditor.HasEntry(key)), onWarning);
					return;
				}
			}
		}
		char stringFieldDelimiter = GetStringFieldDelimiter((IAssetInfo)(object)asset);
		ApplyEdits(editor, stringFieldDelimiter, onWarning);
	}

	public override IEnumerable<string> GetChangeLabels()
	{
		if (Records.Any((EditDataPatchRecord p) => p.Value?.Value == null))
		{
			yield return "deleted entries";
		}
		if (Fields.Any() || Records.Any((EditDataPatchRecord p) => p.Value?.Value != null))
		{
			yield return "changed entries";
		}
		if (MoveRecords.Any())
		{
			yield return "reordered entries";
		}
		if (TextOperations.Any())
		{
			yield return "applied text operations";
		}
	}

	private static JsonSerializer CreateJsonSerializer()
	{
		object? obj = typeof(SCore).GetField("Toolkit", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(SCore.Instance);
		ModToolkit val = (ModToolkit)(((obj is ModToolkit) ? obj : null) ?? throw new InvalidOperationException("Can't access SMAPI's toolkit instance."));
		return val.JsonHelper.GetSerializer();
	}

	private bool TryLoadFile(ITokenString fromFile, IContext context, [NotNullWhen(true)] out List<EditDataPatchRecord>? entries, [NotNullWhen(true)] out List<EditDataPatchField>? fields, [NotNullWhen(true)] out List<EditDataPatchMoveRecord>? moveEntries, [NotNullWhen(false)] out string? error)
	{
		//IL_00a5: Expected Obj, but got Unknown
		if (fromFile != null && fromFile.IsMutable && !fromFile.IsReady)
		{
			error = "the fromFile contains tokens which aren't available yet";
			entries = null;
			fields = null;
			moveEntries = null;
			return false;
		}
		if (!ContentPack.HasFile(fromFile.Value))
		{
			error = "that file doesn't exist in the content pack";
			entries = null;
			fields = null;
			moveEntries = null;
			return false;
		}
		PatchConfig patchConfig;
		try
		{
			patchConfig = ContentPack.ReadJsonFile<PatchConfig>(fromFile.Value);
			if (patchConfig == null)
			{
				error = "could not parse that file: file contains a null patch.";
				entries = null;
				fields = null;
				moveEntries = null;
				return false;
			}
		}
		catch (JsonException ex)
		{
			JsonException value = ex;
			error = $"could not parse that file: {value}";
			entries = null;
			fields = null;
			moveEntries = null;
			return false;
		}
		List<IManagedTokenString> targetField;
		return TryParseFields(context, patchConfig, out entries, out fields, out moveEntries, out targetField, out error);
	}

	private void ApplyEdits(IKeyValueEditor editor, char fieldDelimiter, Action<string, IMonitor>? onWarning)
	{
		ApplyRecords(editor, onWarning);
		ApplyFields(editor, fieldDelimiter, onWarning);
		ApplyTextOperations(editor, fieldDelimiter, onWarning);
		ApplyMoveEntries(editor, onWarning);
	}

	private void ApplyRecords(IKeyValueEditor editor, Action<string, IMonitor>? onWarning)
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Invalid comparison between Unknown and I4
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		EditDataPatchRecord[] records = Records;
		foreach (EditDataPatchRecord editDataPatchRecord in records)
		{
			num++;
			object obj = editor.ParseKey(editDataPatchRecord.Key.Value);
			Type entryType = editor.GetEntryType(obj);
			JToken val = editDataPatchRecord.Value?.Value;
			JTokenType val2 = (JTokenType)((val == null) ? 10 : ((int)val.Type));
			if (!editor.CanAddEntries && !editor.HasEntry(obj))
			{
				if (editor is ModelKeyValueEditor modelKeyValueEditor)
				{
					WarnForRecord(num, $"this asset is a data model, which doesn't allow adding new entries. The entry '{editDataPatchRecord.Key.Value}' isn't defined in the model, must be one of: {string.Join(", ", modelKeyValueEditor.FieldNames)}.", onWarning);
				}
				else
				{
					WarnForRecord(num, "this asset doesn't allow adding new entries, and the entry '" + editDataPatchRecord.Key.Value + "' isn't defined in the model.", onWarning);
				}
				continue;
			}
			if ((object)entryType == null)
			{
				WarnForRecord(num, "this asset doesn't have a type for entry '" + editDataPatchRecord.Key.Value + "', so new entries can't be added.", onWarning);
				continue;
			}
			if (val == null)
			{
				editor.RemoveEntry(obj);
				continue;
			}
			object value;
			try
			{
				value = val.ToObject(entryType, JsonSerializer.Value);
			}
			catch (Exception ex)
			{
				WarnForRecord(num, $"failed converting {(((int)val2 == 1) ? "entry" : $"{val2} value")} to the expected type '{entryType.FullName}': {ex.Message}.", onWarning);
				continue;
			}
			try
			{
				editor.SetEntry(obj, value);
			}
			catch (Exception ex2)
			{
				WarnForRecord(num, $"failed setting '{obj}' entry: {ex2.Message}", onWarning);
			}
		}
	}

	private void ApplyFields(IKeyValueEditor editor, char fieldDelimiter, Action<string, IMonitor>? onWarning)
	{
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Expected Obj, but got Unknown
		foreach (IGrouping<string, EditDataPatchField> item in Fields.GroupByIgnoreCase((EditDataPatchField p) => p.EntryKey.Value))
		{
			object obj = editor.ParseKey(item.Key);
			Type entryType = editor.GetEntryType(obj);
			if (!editor.HasEntry(obj))
			{
				WarnForField($"there's no record matching key '{obj}' under {"Fields"}.", onWarning);
				continue;
			}
			if (entryType == typeof(string))
			{
				string[] array = ((string)editor.GetEntry(obj)).Split(fieldDelimiter);
				foreach (EditDataPatchField item2 in item)
				{
					if (!int.TryParse(item2.FieldKey.Value, out var result))
					{
						WarnForField($"record '{obj}' under {"Fields"} is a string, so it requires a field index between 0 and {array.Length - 1} (received \"{item2.FieldKey}\" instead)).", onWarning);
					}
					else if (result < 0 || result > array.Length - 1)
					{
						WarnForField($"record '{obj}' under {"Fields"} has no field with index {result} (must be 0 to {array.Length - 1}).", onWarning);
					}
					else
					{
						int num = result;
						TokenizableJToken? value = item2.Value;
						array[num] = ((value != null) ? Extensions.Value<string>((IEnumerable<JToken>)value.Value) : null) ?? string.Empty;
					}
				}
				editor.SetEntry(obj, string.Join(fieldDelimiter.ToString(), array));
				continue;
			}
			JObject val = new JObject();
			foreach (EditDataPatchField item3 in item)
			{
				val[item3.FieldKey.Value] = item3.Value?.Value;
			}
			object entry = editor.GetEntry(obj);
			if (entry == null)
			{
				if (editor.TryInitializeEntry(obj))
				{
					entry = editor.GetEntry(obj);
				}
				if (entry == null)
				{
					WarnForField($"record '{obj}' under {"Fields"} refers to field which doesn't exist and can't be added.", onWarning);
					continue;
				}
			}
			JsonReader val2 = ((JToken)val).CreateReader();
			try
			{
				JsonSerializer.Value.Populate(val2, entry);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
	}

	private void ApplyTextOperations(IKeyValueEditor editor, char fieldDelimiter, Action<string, IMonitor>? onWarning)
	{
		for (int i = 0; i < TextOperations.Length; i++)
		{
			if (!TryApplyTextOperation(TextOperations[i], editor, fieldDelimiter, out string error))
			{
				string text = $"Can't apply data patch \"{Path} > text operation #{i}\" to {TargetAsset}: {error}";
				onWarning?.Invoke(text, Monitor);
				Monitor.Log(text, (LogLevel)3);
			}
		}
	}

	private void ApplyMoveEntries(IKeyValueEditor editor, Action<string, IMonitor>? onWarning)
	{
		if (!MoveRecords.Any())
		{
			return;
		}
		if (!editor.CanMoveEntries)
		{
			string text = $"Can't move records for \"{Path}\" > {"MoveEntries"}: target asset '{TargetAsset}' isn't an ordered list.";
			onWarning?.Invoke(text, Monitor);
			Monitor.LogOnce(text, (LogLevel)3);
			return;
		}
		EditDataPatchMoveRecord[] moveRecords = MoveRecords;
		foreach (EditDataPatchMoveRecord editDataPatchMoveRecord in moveRecords)
		{
			if (!editDataPatchMoveRecord.IsReady)
			{
				continue;
			}
			object obj = editor.ParseKey(editDataPatchMoveRecord.ID.Value);
			string text2 = $"record \"{Path}\" > {"MoveEntries"} > \"{editDataPatchMoveRecord.ID}\"";
			MoveResult moveResult = MoveResult.Success;
			object obj2 = null;
			MoveEntryPosition toPosition = editDataPatchMoveRecord.ToPosition;
			if ((uint)(toPosition - 1) <= 1u)
			{
				obj2 = editDataPatchMoveRecord.ToPosition;
				moveResult = editor.MoveEntry(obj, editDataPatchMoveRecord.ToPosition);
			}
			else if (editDataPatchMoveRecord.AfterId.IsMeaningful() || editDataPatchMoveRecord.BeforeId.IsMeaningful())
			{
				bool flag = editDataPatchMoveRecord.AfterId.IsMeaningful();
				string text3 = (flag ? editDataPatchMoveRecord.AfterId.Value : editDataPatchMoveRecord.BeforeId.Value);
				obj2 = editor.ParseKey(text3);
				text2 += $" {(flag ? "AfterId" : "BeforeId")} \"{text3}\"";
				moveResult = editor.MoveEntry(obj, obj2, flag);
			}
			if (moveResult != MoveResult.Success)
			{
				bool flag2 = true;
				string text4;
				switch (moveResult)
				{
				case MoveResult.TargetNotFound:
					text4 = $"Can't move {text2}: no entry with ID '{obj}' exists.";
					flag2 = false;
					break;
				case MoveResult.AnchorNotFound:
					text4 = $"Can't move {text2}: no entry with ID '{obj2}' exists.";
					flag2 = false;
					break;
				case MoveResult.AnchorIsMain:
					text4 = "Can't move " + text2 + ": can't move entry relative to itself.";
					break;
				default:
					text4 = "Can't move " + text2 + ": an unknown error occurred.";
					break;
				}
				onWarning?.Invoke(text4, Monitor);
				Monitor.LogOnce(text4, (LogLevel)(flag2 ? 3 : 0));
			}
		}
	}

	private bool TryApplyTextOperation(ITextOperation operation, IKeyValueEditor editor, char fieldDelimiter, [NotNullWhen(false)] out string? error)
	{
		TextOperationTargetRoot? targetRoot = operation.GetTargetRoot();
		switch (targetRoot)
		{
		case TextOperationTargetRoot.Entries:
		{
			if (operation.Target.Length != 2)
			{
				return Fail($"an '{TextOperationTargetRoot.Entries}' path must have exactly one other segment: the entry key.", out error);
			}
			object key2 = editor.ParseKey(operation.Target[1].Value);
			Type entryType3 = editor.GetEntryType(key2);
			if (entryType3 != typeof(string))
			{
				return Fail("can't apply text operation to the '" + operation.Target[1].Value + "' entry because it's not a string value.", out error);
			}
			string text2 = (string)editor.GetEntry(key2);
			string text3 = operation.Apply(text2);
			if (text3 != null)
			{
				editor.SetEntry(key2, text3);
			}
			break;
		}
		case TextOperationTargetRoot.Fields:
		{
			if (operation.Target.Length != 3)
			{
				return Fail($"a '{TextOperationTargetRoot.Fields}' path must have exactly two other segments: one for the entry key, and one for the field key or index.", out error);
			}
			string value = operation.Target[1].Value;
			string value2 = operation.Target[2].Value;
			object obj = editor.ParseKey(value);
			Type entryType = editor.GetEntryType(obj);
			object entry = editor.GetEntry(obj);
			if (entry == null)
			{
				return Fail($"record '{value}' has no value, so field '{value2}' can't be modified using text operations.", out error);
			}
			if (!EditorFactory.TryGetEditorFor(entry, out IKeyValueEditor editor2))
			{
				if (!(entryType == typeof(string)))
				{
					return Fail($"record '{value}' > field '{value2}' can't be modified using text operations because its type ({entryType?.FullName ?? "<unknown>"}) isn't supported.", out error);
				}
				editor2 = new DelimitedStringKeyValueEditor(editor, obj, fieldDelimiter);
			}
			object key = editor2.ParseKey(value2);
			Type entryType2 = editor2.GetEntryType(key);
			object entry2 = editor2.GetEntry(key);
			if (entryType2 != typeof(string))
			{
				return Fail($"field '{value}' > '{value2}' has type '{entryType2}', but you can only apply text operations to a text field.", out error);
			}
			string text = operation.Apply(entry2 as string);
			if (text != null)
			{
				editor2.SetEntry(key, text);
			}
			break;
		}
		default:
			return Fail((!targetRoot.HasValue) ? $"unknown path root '{operation.Target[0]}'." : $"path root '{targetRoot}' isn't valid for an {"EditMap"} patch", out error);
		}
		error = null;
		return true;
	}

	private char GetStringFieldDelimiter(IAssetInfo asset)
	{
		return asset.Name.IsEquivalentTo("Data/Achievements", true) ? '^' : '/';
	}

	private void WarnForPatch(string message, Action<string, IMonitor>? onWarning)
	{
		string text = $"Can't apply data patch \"{Path}\" to {TargetAsset}: {message}";
		onWarning?.Invoke(text, Monitor);
		Monitor.Log(text, (LogLevel)3);
	}

	private void WarnForField(string message, Action<string, IMonitor>? onWarning)
	{
		string text = $"Can't apply data patch \"{Path}\" to {TargetAsset}: {message}";
		onWarning?.Invoke(text, Monitor);
		Monitor.Log(text, (LogLevel)3);
	}

	private void WarnForRecord(int index, string message, Action<string, IMonitor>? onWarning)
	{
		string text = $"Can't apply data patch \"{Path} > entry #{index}\" to {TargetAsset}: {message}";
		onWarning?.Invoke(text, Monitor);
		Monitor.Log(text, (LogLevel)3);
	}

	private string GetEditorNotCompatibleError(string nounPhrase, object? data, bool entryExists)
	{
		if (!entryExists)
		{
			return nounPhrase + " doesn't match an existing target";
		}
		if (data == null)
		{
			return nounPhrase + " is null and can't be targeted for edits";
		}
		Type type = data.GetType();
		if (typeof(Texture2D).IsAssignableFrom(type))
		{
			return nounPhrase + " is an image, not data";
		}
		if (typeof(Map).IsAssignableFrom(type))
		{
			return nounPhrase + " is a map, not data";
		}
		return nounPhrase + " has type '" + type.FullName + "', which isn't recognized by Content Patcher";
	}
}
