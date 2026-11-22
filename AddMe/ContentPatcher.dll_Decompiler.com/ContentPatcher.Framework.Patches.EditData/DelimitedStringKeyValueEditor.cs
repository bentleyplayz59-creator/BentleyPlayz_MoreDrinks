using System;
using System.Diagnostics.CodeAnalysis;

namespace ContentPatcher.Framework.Patches.EditData;

internal class DelimitedStringKeyValueEditor : BaseDataEditor
{
	private readonly IKeyValueEditor EntryEditor;

	private readonly object EntryKey;

	private readonly char FieldDelimiter;

	public DelimitedStringKeyValueEditor(IKeyValueEditor entryEditor, object entryKey, char fieldDelimiter)
	{
		EntryEditor = entryEditor;
		EntryKey = entryKey;
		FieldDelimiter = fieldDelimiter;
	}

	public override object ParseKey(string key)
	{
		return int.Parse(key);
	}

	public override object? GetEntry(object key)
	{
		string value;
		return TryGetField(key, out value) ? value : null;
	}

	public override Type GetEntryType(object key)
	{
		return typeof(string);
	}

	public override void RemoveEntry(object key)
	{
		SetEntry(key, null);
	}

	public override bool TryInitializeEntry(object key)
	{
		SetEntry(key, "");
		return true;
	}

	public override void SetEntry(object key, object? value)
	{
		int num = (int)key;
		if (num < 0)
		{
			return;
		}
		string[] array = GetFields();
		if (array == null)
		{
			return;
		}
		if (num > array.Length - 1)
		{
			int num2 = array.Length;
			Array.Resize(ref array, num + 1);
			for (int i = num2; i < array.Length; i++)
			{
				array[i] = string.Empty;
			}
		}
		array[num] = ((string)value) ?? string.Empty;
		EntryEditor.SetEntry(EntryKey, string.Join(FieldDelimiter, array));
	}

	private string[]? GetFields()
	{
		if (!(EntryEditor.GetEntry(EntryKey) is string text))
		{
			return null;
		}
		return text.Split(FieldDelimiter);
	}

	private bool TryGetField(object key, [NotNullWhen(true)] out string? value)
	{
		int num = (int)key;
		if (num < 0)
		{
			value = null;
			return false;
		}
		string[] fields = GetFields();
		if (fields == null || num >= fields.Length)
		{
			value = null;
			return false;
		}
		value = fields[num];
		return true;
	}
}
