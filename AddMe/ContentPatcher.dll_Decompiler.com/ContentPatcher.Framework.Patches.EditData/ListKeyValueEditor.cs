using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ContentPatcher.Framework.Patches.EditData;

internal class ListKeyValueEditor<TValue> : BaseDataEditor
{
	private readonly IList<TValue> Data;

	private readonly Lazy<Func<TValue, string?>?> GetAssetKeyImpl;

	public ListKeyValueEditor(IList<TValue> data)
	{
		Data = data;
		CanMoveEntries = true;
		GetAssetKeyImpl = new Lazy<Func<TValue, string>>(GetAssetKeyFunc);
	}

	public override object ParseKey(string key)
	{
		string[] array = key.Split('#', 2);
		if (array.Length == 2 && string.IsNullOrWhiteSpace(array[0]) && int.TryParse(array[1], out var result))
		{
			return result;
		}
		return key;
	}

	public override object? GetEntry(object key)
	{
		TValue value;
		int index;
		return TryGetEntry(key, out value, out index) ? value : default(TValue);
	}

	public override Type GetEntryType(object key)
	{
		return typeof(TValue);
	}

	public override void RemoveEntry(object key)
	{
		if (TryGetEntry(key, out TValue _, out int index))
		{
			Data.RemoveAt(index);
		}
	}

	public override void SetEntry(object key, object value)
	{
		if (TryGetEntry(key, out TValue _, out int index))
		{
			Data[index] = (TValue)value;
		}
		else
		{
			Data.Add((TValue)value);
		}
	}

	public override MoveResult MoveEntry(object key, MoveEntryPosition toPosition)
	{
		if (!TryGetEntry(key, out TValue value, out int index))
		{
			return MoveResult.TargetNotFound;
		}
		switch (toPosition)
		{
		case MoveEntryPosition.Top:
			Data.RemoveAt(index);
			Data.Insert(0, value);
			break;
		case MoveEntryPosition.Bottom:
			Data.RemoveAt(index);
			Data.Add(value);
			break;
		default:
			throw new InvalidOperationException($"Unknown move position '{toPosition}'.");
		case MoveEntryPosition.None:
			break;
		}
		return MoveResult.Success;
	}

	public override MoveResult MoveEntry(object key, object anchorKey, bool afterAnchor)
	{
		if (!TryGetEntry(key, out TValue value, out int index))
		{
			return MoveResult.TargetNotFound;
		}
		if (!TryGetEntry(anchorKey, out TValue _, out int index2))
		{
			return MoveResult.AnchorNotFound;
		}
		if (index == index2)
		{
			return MoveResult.AnchorIsMain;
		}
		int num = (afterAnchor ? (index2 + 1) : index2);
		if (index < index2)
		{
			num--;
		}
		Data.RemoveAt(index);
		Data.Insert(num, value);
		return MoveResult.Success;
	}

	private string? GetKey(TValue entry)
	{
		Func<TValue, string> value = GetAssetKeyImpl.Value;
		if (value != null)
		{
			return value(entry);
		}
		throw new NotSupportedException("No ID implementation for list value type " + typeof(TValue).FullName + ".");
	}

	private bool TryGetEntry(object keyOrIndex, [NotNullWhen(true)] out TValue? value, out int index)
	{
		if (!(keyOrIndex is int num))
		{
			if (keyOrIndex is string text)
			{
				for (int i = 0; i < Data.Count; i++)
				{
					value = Data[i];
					if (GetKey(value) == text)
					{
						index = i;
						return true;
					}
				}
			}
		}
		else if (num >= 0 && num < Data.Count)
		{
			value = Data[num];
			index = num;
			return true;
		}
		value = default;
		index = -1;
		return false;
	}

	public Func<TValue, string?>? GetAssetKeyFunc()
	{
		Type typeFromHandle = typeof(TValue);
		if (typeFromHandle == typeof(string))
		{
			return (TValue entry) => entry as string;
		}
		if (typeFromHandle.IsValueType && (typeFromHandle.IsPrimitive || typeFromHandle.IsEnum))
		{
			return (TValue entry) => entry?.ToString();
		}
		PropertyInfo property = typeFromHandle.GetProperty("Id") ?? typeFromHandle.GetProperty("ID");
		if (property?.GetMethod != null)
		{
			return (TValue entry) => property.GetValue(entry)?.ToString();
		}
		FieldInfo field = typeFromHandle.GetField("Id") ?? typeFromHandle.GetField("ID");
		if (field != null)
		{
			return (TValue entry) => field.GetValue(entry)?.ToString();
		}
		return null;
	}
}
