using System;
using System.Linq;
using Newtonsoft.Json;

namespace ContentPatcher.Framework.ConfigModels;

internal class TextOperationConfig
{
	public string? Operation { get; }

	public string?[] Target { get; }

	public string? Delimiter { get; }

	public string? Value { get; }

	public string? Search { get; }

	public string? ReplaceMode { get; }

	[JsonConstructor]
	public TextOperationConfig(string? operation, string?[]? target, string? value, string? delimiter, string? search, string? replaceMode)
	{
		Operation = operation;
		Target = target ?? Array.Empty<string>();
		Value = value;
		Delimiter = delimiter;
		Search = search;
		ReplaceMode = replaceMode;
	}

	public TextOperationConfig(TextOperationConfig other)
		: this(other.Operation, other.Target.ToArray(), other.Value, other.Delimiter, other.Search, other.ReplaceMode)
	{
	}
}
