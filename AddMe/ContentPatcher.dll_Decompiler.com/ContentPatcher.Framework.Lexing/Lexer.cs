using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Lexing.LexTokens;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.Lexing;

internal class Lexer
{
	private const char StartTokenChar = '{';

	private const char EndTokenChar = '}';

	private const char NamedInputChar = '|';

	private const char PositionalInputChar = ':';

	private static readonly char[] SplitPattern = new char[4] { '{', '}', ':', '|' };

	private static readonly LexBit EmptyStringBit = new LexBit(LexBitType.Literal, string.Empty);

	private static readonly LexBit StartTokenBit = new LexBit(LexBitType.StartToken, "{{");

	private static readonly LexBit EndTokenBit = new LexBit(LexBitType.EndToken, "}}");

	private static readonly LexBit NamedInputSeparatorBit = new LexBit(LexBitType.NamedInputArgSeparator, "|");

	private static readonly LexBit PositionalInputSeparatorBit = new LexBit(LexBitType.PositionalInputArgSeparator, ":");

	public static Lexer Instance { get; } = new Lexer();

	public IEnumerable<LexBit> TokenizeString(string? rawText)
	{
		if (rawText == null)
		{
			yield break;
		}
		if (rawText.Length == 0)
		{
			yield return EmptyStringBit;
			yield break;
		}
		int length = rawText.Length;
		int maxIndex = length - 1;
		int lastMatch = 0;
		int start = 0;
		while (lastMatch < length)
		{
			int index = rawText.IndexOfAny(SplitPattern, start);
			if (index == -1)
			{
				if (lastMatch < length)
				{
					int num = lastMatch;
					yield return new LexBit(LexBitType.Literal, rawText.Substring(num, rawText.Length - num));
				}
				break;
			}
			char match = rawText[index];
			switch (match)
			{
			case '{':
			case '}':
				if (index >= maxIndex || match != rawText[index + 1])
				{
					start = index + 1;
					break;
				}
				if (lastMatch != index)
				{
					int num = lastMatch;
					yield return new LexBit(LexBitType.Literal, rawText.Substring(num, index - num));
				}
				yield return (match == '{') ? StartTokenBit : EndTokenBit;
				start = index + 2;
				lastMatch = start;
				break;
			case '|':
				if (lastMatch != index)
				{
					int num = lastMatch;
					yield return new LexBit(LexBitType.Literal, rawText.Substring(num, index - num));
				}
				yield return NamedInputSeparatorBit;
				start = index + 1;
				lastMatch = start;
				break;
			case ':':
				if (lastMatch != index)
				{
					int num = lastMatch;
					yield return new LexBit(LexBitType.Literal, rawText.Substring(num, index - num));
				}
				yield return PositionalInputSeparatorBit;
				start = index + 1;
				lastMatch = start;
				break;
			default:
				throw new InvalidOperationException($"Found invalid lexical token '{match}' while parsing string '{rawText}'. This is a bug in Content Patcher's lexer.");
			}
		}
	}

	public IEnumerable<ILexToken> ParseBits(string? rawText, bool impliedBraces, bool trim = false)
	{
		IEnumerable<LexBit> bits = TokenizeString(rawText);
		return ParseBits(bits, impliedBraces, trim);
	}

	public IEnumerable<ILexToken> ParseBits(IEnumerable<LexBit> bits, bool impliedBraces, bool trim = false)
	{
		return ParseBitQueue(new Queue<LexBit>(bits), impliedBraces, trim);
	}

	public IEnumerable<string> SplitLexically(string? str, string delimiter = ",", bool ignoreEmpty = true, bool trim = true)
	{
		if (str == null)
		{
			return Array.Empty<string>();
		}
		return from p in RawSplit(this, str, delimiter)
			select trim ? p.Trim() : p into p
			where !ignoreEmpty || !string.IsNullOrEmpty(p)
			select p;
		static IEnumerable<string> RawSplit(Lexer lexer, string text, string text2)
		{
			if (!text.Contains(text2))
			{
				yield return text;
			}
			else if (!lexer.MightContainTokens(text))
			{
				string[] array = text.Split(text2);
				for (int i = 0; i < array.Length; i++)
				{
					yield return array[i];
				}
			}
			else
			{
				StringBuilder cur = new StringBuilder(text.Length);
				foreach (ILexToken bit in lexer.ParseBits(text, impliedBraces: false))
				{
					LexTokenLiteral literal = bit as LexTokenLiteral;
					if (literal?.Text.Contains(text2) ?? false)
					{
						string[] parts = literal.Text.Split(text2);
						cur.Append(parts[0]);
						yield return cur.ToString();
						cur.Clear();
						for (int j = 1; j < parts.Length - 1; j++)
						{
							yield return parts[j];
						}
						cur.Append(parts.Last());
					}
					else
					{
						cur.Append(bit.ToString());
					}
				}
				yield return cur.ToString();
			}
		}
	}

	public bool MightContainTokens(string? rawText)
	{
		return !string.IsNullOrEmpty(rawText) && rawText.Contains("{{");
	}

	private IEnumerable<ILexToken> ParseBitQueue(Queue<LexBit> input, bool impliedBraces, bool trim)
	{
		LinkedList<ILexToken> linkedList;
		try
		{
			linkedList = new LinkedList<ILexToken>(RawParse());
		}
		catch (Exception innerException)
		{
			string text = string.Join("", input.Select((LexBit p) => p.Text));
			throw new InvalidOperationException("Error parsing '" + text + "' as a tokenizable string", innerException);
		}
		HashSet<LinkedListNode<ILexToken>> hashSet = new HashSet<LinkedListNode<ILexToken>>(new ObjectReferenceComparer<LinkedListNode<ILexToken>>());
		for (LinkedListNode<ILexToken> linkedListNode = linkedList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			if (linkedListNode.Value is LexTokenLiteral lexTokenLiteral)
			{
				ILexToken lexToken = linkedListNode.Previous?.Value;
				ILexToken lexToken2 = linkedListNode.Next?.Value;
				string text2 = lexTokenLiteral.Text;
				if (lexToken is LexTokenLiteral lexTokenLiteral2)
				{
					text2 = lexTokenLiteral2.Text + text2;
					hashSet.Add(linkedListNode.Previous);
				}
				if (lexToken2 != null && lexToken2.Type == LexTokenType.TokenInput)
				{
					text2 = text2.TrimEnd();
				}
				if (lexToken != null && lexToken.Type == LexTokenType.TokenInput)
				{
					text2 = text2.TrimStart();
				}
				if (trim && (lexToken == null || lexToken2 == null))
				{
					if (lexToken == null)
					{
						text2 = text2.TrimStart();
					}
					if (lexToken2 == null)
					{
						text2 = text2.TrimEnd();
					}
					if (text2 == "")
					{
						hashSet.Add(linkedListNode);
					}
				}
				if (lexTokenLiteral.Text != text2)
				{
					lexTokenLiteral.MigrateTo(text2);
				}
			}
		}
		foreach (LinkedListNode<ILexToken> item in hashSet)
		{
			linkedList.Remove(item);
		}
		return linkedList;
		IEnumerable<ILexToken> RawParse()
		{
			if (impliedBraces)
			{
				if (input.Any())
				{
					yield return ExtractToken(input, impliedBraces: true);
					if (input.Any())
					{
						LexBit next = input.Peek();
						throw new LexFormatException($"Unexpected {next.Type}, expected {LexBitType.Literal}");
					}
				}
			}
			else
			{
				while (input.Any())
				{
					LexBit next2 = input.Peek();
					switch (next2.Type)
					{
					case LexBitType.StartToken:
						yield return ExtractToken(input, impliedBraces: false);
						break;
					case LexBitType.Literal:
					case LexBitType.PositionalInputArgSeparator:
					case LexBitType.NamedInputArgSeparator:
						input.Dequeue();
						yield return new LexTokenLiteral(next2.Text);
						break;
					default:
						throw new LexFormatException($"Unexpected {next2.Type}, expected {LexBitType.StartToken} or {LexBitType.Literal}");
					}
				}
			}
		}
	}

	public LexTokenToken ExtractToken(Queue<LexBit> input, bool impliedBraces)
	{
		if (!impliedBraces)
		{
			LexBit lexBit = GetNextAndAssert("start of token ('{{')");
			if (lexBit.Type != LexBitType.StartToken)
			{
				throw new LexFormatException($"Unexpected {lexBit.Type} at start of token.");
			}
		}
		LexBit lexBit2 = GetNextAndAssert("token name");
		if (lexBit2.Type != LexBitType.Literal)
		{
			throw new LexFormatException($"Unexpected {lexBit2.Type} where token name should be.");
		}
		LexTokenInput inputArgs = null;
		if (input.Any())
		{
			LexBitType type = input.Peek().Type;
			if ((uint)(type - 3) <= 1u)
			{
				if (type == LexBitType.PositionalInputArgSeparator)
				{
					input.Dequeue();
				}
				inputArgs = ExtractInputArguments(input);
			}
		}
		if (!impliedBraces)
		{
			LexBit lexBit3 = GetNextAndAssert("end of token ('}}')");
			if (lexBit3.Type != LexBitType.EndToken)
			{
				throw new LexFormatException($"Unexpected {lexBit3.Type} before end of token.");
			}
		}
		return new LexTokenToken(lexBit2.Text.Trim(), inputArgs, impliedBraces);
		LexBit GetNextAndAssert(string expectedPhrase)
		{
			if (!input.Any())
			{
				throw new LexFormatException("Reached end of input, expected " + expectedPhrase + ".");
			}
			return input.Dequeue();
		}
	}

	public LexTokenInput ExtractInputArguments(Queue<LexBit> input)
	{
		Queue<LexBit> queue = new Queue<LexBit>();
		int num = 0;
		bool flag = false;
		while (!flag && input.Any())
		{
			switch (input.Peek().Type)
			{
			case LexBitType.StartToken:
				num++;
				queue.Enqueue(input.Dequeue());
				break;
			case LexBitType.EndToken:
				num--;
				if (num < 0)
				{
					flag = true;
				}
				else
				{
					queue.Enqueue(input.Dequeue());
				}
				break;
			default:
				queue.Enqueue(input.Dequeue());
				break;
			}
		}
		ILexToken[] tokenParts = ParseBitQueue(queue, impliedBraces: false, trim: true).ToArray();
		return new LexTokenInput(tokenParts);
	}
}
