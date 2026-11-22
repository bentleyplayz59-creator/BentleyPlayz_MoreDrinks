using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations;

internal abstract class BaseDelimitedTextOperation : BaseTextOperation
{
	public ITokenString Search { get; }

	public string Delimiter { get; }

	public TextOperationReplaceMode ReplaceMode { get; }

	[return: NotNullIfNotNull("text")]
	public override string? Apply(string? text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		string value = Search.Value;
		if (value == null)
		{
			return text;
		}
		IReadOnlyList<string> values = text.Split(Delimiter);
		Lazy<IList<string>> lazy = new Lazy<IList<string>>(() => new List<string>(values));
		int count = values.Count;
		switch (ReplaceMode)
		{
		case TextOperationReplaceMode.First:
		{
			for (int num2 = 0; num2 < count; num2++)
			{
				if (values[num2] == value)
				{
					ApplyToValue(values, lazy, num2);
					break;
				}
			}
			break;
		}
		case TextOperationReplaceMode.Last:
		{
			for (int num3 = count - 1; num3 >= 0; num3--)
			{
				if (values[num3] == value)
				{
					ApplyToValue(values, lazy, num3);
					break;
				}
			}
			break;
		}
		case TextOperationReplaceMode.All:
		{
			for (int num = count - 1; num >= 0; num--)
			{
				if (values[num] == value)
				{
					ApplyToValue(values, lazy, num);
				}
			}
			break;
		}
		}
		return lazy.IsValueCreated ? string.Join(Delimiter, lazy.Value) : text;
	}

	protected BaseDelimitedTextOperation(TextOperationType operation, ICollection<IManagedTokenString> target, IManagedTokenString search, string delimiter, TextOperationReplaceMode replaceMode)
		: base(operation, target)
	{
		Search = search;
		Delimiter = delimiter;
		ReplaceMode = replaceMode;
		Contextuals.Add(search);
	}

	protected abstract void ApplyToValue(IReadOnlyList<string> values, Lazy<IList<string>> mutableValues, int index);
}
