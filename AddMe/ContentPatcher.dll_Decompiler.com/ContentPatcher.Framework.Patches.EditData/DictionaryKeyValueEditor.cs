using System;
using System.Collections.Generic;

namespace ContentPatcher.Framework.Patches.EditData;

internal class DictionaryKeyValueEditor<TKey, TValue> : BaseDataEditor
{
	private readonly IDictionary<TKey, TValue> Data;

	public DictionaryKeyValueEditor(IDictionary<TKey, TValue> data)
	{
		Data = data;
	}

	public override object ParseKey(string key)
	{
		return ParseKey<TKey>(key);
	}

	public override bool HasEntry(object key)
	{
		TKey key2 = (TKey)key;
		return Data.ContainsKey(key2);
	}

	public override object? GetEntry(object key)
	{
		TKey key2 = (TKey)key;
		TValue value;
		return Data.TryGetValue(key2, out value) ? value : default(TValue);
	}

	public override Type GetEntryType(object key)
	{
		return typeof(TValue);
	}

	public override void RemoveEntry(object key)
	{
		TKey key2 = (TKey)key;
		Data.Remove(key2);
	}

	public override void SetEntry(object key, object value)
	{
		TKey key2 = (TKey)key;
		Data[key2] = (TValue)value;
	}
}
