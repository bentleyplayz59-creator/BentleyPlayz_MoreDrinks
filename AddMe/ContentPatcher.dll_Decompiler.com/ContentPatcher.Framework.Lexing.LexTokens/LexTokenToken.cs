using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ContentPatcher.Framework.Lexing.LexTokens;

internal class LexTokenToken : ILexToken
{
	public LexTokenType Type => LexTokenType.Token;

	public string Name { get; private set; }

	public LexTokenInput? InputArgs { get; private set; }

	public bool ImpliedBraces { get; }

	public LexTokenToken(string name, LexTokenInput? inputArgs, bool impliedBraces)
	{
		ImpliedBraces = impliedBraces;
		MigrateTo(name, inputArgs);
	}

	[MemberNotNull("Name")]
	public void MigrateTo(string name, LexTokenInput? inputArgs)
	{
		Name = name;
		InputArgs = inputArgs;
	}

	public string? GetProviderModId()
	{
		string[] array = Name.Split("/", 2, StringSplitOptions.TrimEntries);
		return (array.Length == 2) ? array[0] : null;
	}

	[MemberNotNullWhen(true, "InputArgs")]
	public bool HasInputArgs()
	{
		LexTokenInput? inputArgs = InputArgs;
		return inputArgs != null && inputArgs.Parts.Length != 0;
	}

	public override string ToString()
	{
		return GetRawText(Name, InputArgs, ImpliedBraces);
	}

	private static string GetRawText(string name, LexTokenInput? inputArgs, bool impliedBraces)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (!impliedBraces)
		{
			stringBuilder.Append("{{");
		}
		stringBuilder.Append(name);
		if (inputArgs != null)
		{
			if (!inputArgs.ToString().StartsWith("|"))
			{
				stringBuilder.Append(":");
			}
			stringBuilder.Append(inputArgs);
		}
		if (!impliedBraces)
		{
			stringBuilder.Append("}}");
		}
		return stringBuilder.ToString();
	}
}
