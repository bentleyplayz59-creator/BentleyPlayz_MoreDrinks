using System;

namespace ContentPatcher.Framework.Patches.EditData;

internal interface IKeyValueEditor
{
	bool CanMoveEntries { get; }

	bool CanAddEntries { get; }

	object ParseKey(string key);

	bool HasEntry(object key);

	object? GetEntry(object key);

	Type? GetEntryType(object key);

	void RemoveEntry(object key);

	bool TryInitializeEntry(object key);

	void SetEntry(object key, object value);

	MoveResult MoveEntry(object key, MoveEntryPosition toPosition);

	MoveResult MoveEntry(object key, object anchorKey, bool afterAnchor);
}
