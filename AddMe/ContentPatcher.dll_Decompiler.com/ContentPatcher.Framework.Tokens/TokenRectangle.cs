using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;

namespace ContentPatcher.Framework.Tokens;

internal class TokenRectangle : TokenPosition
{
	public ITokenString Width { get; }

	public ITokenString Height { get; }

	public TokenRectangle(IManagedTokenString x, IManagedTokenString y, IManagedTokenString width, IManagedTokenString height)
		: base(x, y)
	{
		Width = width ?? throw new ArgumentNullException("width");
		Height = height ?? throw new ArgumentNullException("height");
		Contextuals.Add(width).Add(height);
	}

	public bool TryGetRectangle(out Rectangle rectangle, [NotNullWhen(false)] out string? error)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetNumber(X, "X", out var parsed, out error) || !TryGetNumber(Y, "Y", out var parsed2, out error) || !TryGetNumber(Width, "Width", out var parsed3, out error) || !TryGetNumber(Height, "Height", out var parsed4, out error))
		{
			rectangle = Rectangle.Empty;
			return false;
		}
		rectangle = new Rectangle(parsed, parsed2, parsed3, parsed4);
		return true;
	}
}
