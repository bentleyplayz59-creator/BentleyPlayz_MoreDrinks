using System;
using System.Diagnostics.CodeAnalysis;
using Pathoschild.Stardew.Common.Utilities;
using xTile.Dimensions;

namespace ContentPatcher.Framework.Tokens;

internal class TokenPosition : IContextual, IContextualInfo
{
	protected readonly AggregateContextual Contextuals = new AggregateContextual();

	public ITokenString X { get; }

	public ITokenString Y { get; }

	public bool IsMutable => Contextuals.IsMutable;

	public bool IsReady => Contextuals.IsReady;

	public TokenPosition(IManagedTokenString x, IManagedTokenString y)
	{
		X = x ?? throw new ArgumentNullException("x");
		Y = y ?? throw new ArgumentNullException("y");
		Contextuals.Add(x).Add(y);
	}

	public bool TryGetLocation(out Location position, [NotNullWhen(false)] out string? error)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetNumber(X, "X", out var parsed, out error) || !TryGetNumber(Y, "Y", out var parsed2, out error))
		{
			position = Location.Origin;
			return false;
		}
		position = new Location(parsed, parsed2);
		return true;
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

	protected bool TryGetNumber(ITokenString raw, string name, out int parsed, [NotNullWhen(false)] out string? error)
	{
		if (!raw.IsReady)
		{
			parsed = -1;
			error = name + " is not ready";
			return false;
		}
		if (!int.TryParse(raw.Value, out parsed))
		{
			error = $"{name} value '{raw}' can't be parsed as an integer";
			return false;
		}
		error = null;
		return true;
	}
}
