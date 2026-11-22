using System;
using System.Collections.Generic;
using System.Linq;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Patches;

internal class EditMapPatchTile : IContextual, IContextualInfo
{
	private readonly AggregateContextual Contextuals;

	public TokenPosition Position { get; }

	public ITokenString? Layer { get; }

	public ITokenString? SetIndex { get; }

	public ITokenString? SetTilesheet { get; }

	public IDictionary<ITokenString, ITokenString?> SetProperties { get; }

	public ITokenString? Remove { get; }

	public bool IsMutable => Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady;

	public EditMapPatchTile(TokenPosition position, IManagedTokenString layer, IManagedTokenString? setIndex, IManagedTokenString? setTilesheet, IDictionary<IManagedTokenString, IManagedTokenString?>? setProperties, IManagedTokenString? remove)
	{
		Position = position;
		Layer = layer;
		SetIndex = setIndex;
		SetTilesheet = setTilesheet;
		SetProperties = ((setProperties != null) ? ((IEnumerable<KeyValuePair<IManagedTokenString, IManagedTokenString>>)setProperties).ToDictionary((Func<KeyValuePair<IManagedTokenString, IManagedTokenString>, ITokenString>)((KeyValuePair<IManagedTokenString, IManagedTokenString> p) => p.Key), (Func<KeyValuePair<IManagedTokenString, IManagedTokenString>, ITokenString>)((KeyValuePair<IManagedTokenString, IManagedTokenString> p) => p.Value)) : null) ?? new Dictionary<ITokenString, ITokenString>();
		Remove = remove;
		Contextuals = new AggregateContextual().Add(position).Add(layer).Add(setIndex)
			.Add(setTilesheet)
			.Add(remove);
		if (setProperties == null)
		{
			return;
		}
		foreach (var (value, value2) in setProperties)
		{
			Contextuals.Add(value).Add(value2);
		}
	}

	public bool UpdateContext(IContext context)
	{
		return Contextuals.UpdateContext(context);
	}

	public IInvariantSet GetTokensUsed()
	{
		return Contextuals.GetTokensUsed();
	}

	public IContextualState GetDiagnosticState()
	{
		return Contextuals.GetDiagnosticState();
	}
}
