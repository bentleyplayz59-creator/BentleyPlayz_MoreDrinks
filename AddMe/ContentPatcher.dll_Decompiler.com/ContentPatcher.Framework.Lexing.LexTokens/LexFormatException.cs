using System;

namespace ContentPatcher.Framework.Lexing.LexTokens;

internal class LexFormatException : FormatException
{
	public LexFormatException(string message, FormatException? innerException = null)
		: base(message, innerException)
	{
	}
}
