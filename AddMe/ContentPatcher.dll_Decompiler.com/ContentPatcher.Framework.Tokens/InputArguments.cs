using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ContentPatcher.Framework.Lexing;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Tokens;

internal class InputArguments : IInputArguments
{
	internal const string ContainsKey = "contains";

	internal const string ValueAtKey = "valueAt";

	internal const string InputSeparatorKey = "inputSeparator";

	private static readonly IInvariantSet ReservedArgKeys = InvariantSets.From(new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "contains", "valueAt", "inputSeparator" }));

	private string? LastRawValue;

	private string? LastParsedValue;

	private string? PositionalSegment;

	private string[] PositionalArgsImpl = Array.Empty<string>();

	private InvariantDictionary<IInputArgumentValue> NamedArgsImpl = new InvariantDictionary<IInputArgumentValue>();

	private InvariantDictionary<IInputArgumentValue> ReservedArgsImpl = new InvariantDictionary<IInputArgumentValue>();

	private KeyValuePair<string, IInputArgumentValue>[] ReservedArgsListImpl = Array.Empty<KeyValuePair<string, IInputArgumentValue>>();

	public static IInputArguments Empty { get; } = new EmptyInputArguments();

	public ITokenString? TokenString { get; }

	public string[] PositionalArgs => ParseIfNeeded().PositionalArgsImpl;

	public IDictionary<string, IInputArgumentValue> NamedArgs => ParseIfNeeded().NamedArgsImpl;

	public IDictionary<string, IInputArgumentValue> ReservedArgs => ParseIfNeeded().ReservedArgsImpl;

	public KeyValuePair<string, IInputArgumentValue>[] ReservedArgsList => ParseIfNeeded().ReservedArgsListImpl;

	[MemberNotNullWhen(true, "TokenString")]
	public bool HasNamedArgs
	{
		[MemberNotNullWhen(true, "TokenString")]
		get
		{
			return NamedArgs.Any();
		}
	}

	[MemberNotNullWhen(true, "TokenString")]
	public bool HasPositionalArgs
	{
		[MemberNotNullWhen(true, "TokenString")]
		get
		{
			return PositionalArgs.Any();
		}
	}

	[MemberNotNullWhen(true, "TokenString")]
	public bool IsMutable
	{
		[MemberNotNullWhen(true, "TokenString")]
		get
		{
			return TokenString?.IsMutable ?? false;
		}
	}

	[MemberNotNullWhen(true, "TokenString")]
	public bool IsReady
	{
		[MemberNotNullWhen(true, "TokenString")]
		get
		{
			return TokenString?.IsReady ?? false;
		}
	}

	public InputArguments(ITokenString tokenString)
	{
		TokenString = tokenString;
	}

	public string? GetFirstPositionalArg()
	{
		return PositionalArgs.FirstOrDefault();
	}

	public string? GetRawArgumentValue(string key)
	{
		IInputArgumentValue value;
		return NamedArgs.TryGetValue(key, out value) ? value.Raw : null;
	}

	public string? GetPositionalSegment()
	{
		return PositionalSegment;
	}

	private InputArguments ParseIfNeeded()
	{
		if (LastParsedValue != TokenString?.Value || LastRawValue != TokenString?.Raw)
		{
			Parse(TokenString, out PositionalSegment, out PositionalArgsImpl, out NamedArgsImpl, out ReservedArgsImpl, out List<KeyValuePair<string, IInputArgumentValue>> reservedArgsList);
			ReservedArgsListImpl = reservedArgsList.ToArray();
			LastParsedValue = TokenString?.Value;
			LastRawValue = TokenString?.Raw;
		}
		return this;
	}

	private static void Parse(ITokenString? input, out string positionalSegment, out string[] positionalArgs, out InvariantDictionary<IInputArgumentValue> namedArgs, out InvariantDictionary<IInputArgumentValue> reservedArgs, out List<KeyValuePair<string, IInputArgumentValue>> reservedArgsList)
	{
		Lexer instance = Lexer.Instance;
		GetRawArguments(input, instance, out positionalSegment, out InvariantDictionary<string> rawNamed);
		if (!rawNamed.TryGetValue("inputSeparator", out var value) || string.IsNullOrWhiteSpace(value))
		{
			value = ",";
		}
		positionalArgs = instance.SplitLexically(positionalSegment, value).ToArray();
		namedArgs = new InvariantDictionary<IInputArgumentValue>();
		reservedArgs = new InvariantDictionary<IInputArgumentValue>();
		reservedArgsList = new List<KeyValuePair<string, IInputArgumentValue>>();
		foreach (var (text3, text4) in rawNamed)
		{
			InputArgumentValue value2 = new InputArgumentValue(text4, instance.SplitLexically(text4, value).ToArray());
			if (ReservedArgKeys.Contains(text3))
			{
				reservedArgs[text3] = value2;
				reservedArgsList.Add(new KeyValuePair<string, IInputArgumentValue>(text3, value2));
			}
			else
			{
				namedArgs[text3] = value2;
			}
		}
	}

	private static void GetRawArguments(ITokenString? input, Lexer lexer, out string rawPositional, out InvariantDictionary<string> rawNamed)
	{
		string str = ((input != null && input.IsReady) ? input.Value : input?.Raw)?.Trim() ?? string.Empty;
		string[] array = lexer.SplitLexically(str, "|", ignoreEmpty: false, trim: false).ToArray();
		string text = array[0].Trim();
		string[] array2 = (from p in array.Skip(1)
			select p.Trim() into p
			where !string.IsNullOrWhiteSpace(p)
			select p).ToArray();
		rawPositional = text;
		rawNamed = new InvariantDictionary<string>();
		string[] array3 = array2;
		foreach (string text2 in array3)
		{
			string[] array4 = lexer.SplitLexically(text2, "=", ignoreEmpty: true, trim: false).ToArray();
			if (array4.Length == 1)
			{
				rawNamed[text2] = string.Empty;
				continue;
			}
			string key = array4[0].Trim();
			string value = string.Join("=", array4.Skip(1)).Trim();
			rawNamed[key] = value;
		}
	}
}
