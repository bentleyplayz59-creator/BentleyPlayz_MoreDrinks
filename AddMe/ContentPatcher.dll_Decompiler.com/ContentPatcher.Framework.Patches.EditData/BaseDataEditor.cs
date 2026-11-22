using System;

namespace ContentPatcher.Framework.Patches.EditData;

internal abstract class BaseDataEditor : IKeyValueEditor
{
	public bool CanMoveEntries { get; protected set; }

	public bool CanAddEntries { get; protected set; } = true;

	public abstract object ParseKey(string key);

	public virtual bool HasEntry(object key)
	{
		return GetEntry(key) != null;
	}

	public abstract object? GetEntry(object key);

	public abstract Type? GetEntryType(object key);

	public abstract void RemoveEntry(object key);

	public virtual bool TryInitializeEntry(object key)
	{
		Type entryType = GetEntryType(key);
		if ((object)entryType == null)
		{
			return false;
		}
		object obj;
		try
		{
			obj = Activator.CreateInstance(entryType);
			if (obj == null)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		SetEntry(key, obj);
		return true;
	}

	public abstract void SetEntry(object key, object value);

	public virtual MoveResult MoveEntry(object key, MoveEntryPosition toPosition)
	{
		throw new NotImplementedException();
	}

	public virtual MoveResult MoveEntry(object key, object anchorKey, bool afterAnchor)
	{
		throw new NotImplementedException();
	}

	protected object ParseKey<TKey>(string key)
	{
		return (typeof(TKey) == typeof(string)) ? key : ((object)(TKey)Convert.ChangeType(key, typeof(TKey)));
	}
}
