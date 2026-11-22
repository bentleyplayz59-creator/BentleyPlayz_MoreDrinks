using System;
using System.Collections.Generic;
using ContentPatcher.Framework.Constants;

namespace ContentPatcher.Framework.TextOperations;

internal class AppendOrPrependTextOperation : BaseTextOperation
{
	public ITokenString Value { get; }

	public string Delimiter { get; }

	public AppendOrPrependTextOperation(TextOperationType operation, ICollection<IManagedTokenString> target, IManagedTokenString value, string? delimiter)
		: base(operation, target)
	{
		Value = value;
		Delimiter = delimiter ?? string.Empty;
		Contextuals.Add(value);
	}

	public override string Apply(string? text)
	{
		string value = Value.Value;
		if (value == null)
		{
			return text ?? "";
		}
		string text2 = (string.IsNullOrEmpty(text) ? "" : Delimiter);
		TextOperationType operation = Operation;
		if (1 == 0)
		{
		}
		string result = operation switch
		{
			TextOperationType.Append => text + text2 + value, 
			TextOperationType.Prepend => value + text2 + text, 
			_ => throw new InvalidOperationException($"Unknown text operation type '{Operation}'."), 
		};
		if (1 == 0)
		{
		}
		return result;
	}
}
