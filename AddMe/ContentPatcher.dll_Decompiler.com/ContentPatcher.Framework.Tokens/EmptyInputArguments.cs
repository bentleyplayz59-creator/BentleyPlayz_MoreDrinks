using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ContentPatcher.Framework.Tokens;

internal class EmptyInputArguments : IInputArguments
{
	private static readonly IDictionary<string, IInputArgumentValue> EmptyArgValues = new ReadOnlyDictionary<string, IInputArgumentValue>(new Dictionary<string, IInputArgumentValue>());

	public ITokenString? TokenString => null;

	public string[] PositionalArgs => Array.Empty<string>();

	public IDictionary<string, IInputArgumentValue> NamedArgs => EmptyArgValues;

	public IDictionary<string, IInputArgumentValue> ReservedArgs => EmptyArgValues;

	public KeyValuePair<string, IInputArgumentValue>[] ReservedArgsList => Array.Empty<KeyValuePair<string, IInputArgumentValue>>();

	public bool HasNamedArgs => false;

	public bool HasPositionalArgs => false;

	public bool IsMutable => false;

	public bool IsReady => true;

	public string? GetFirstPositionalArg()
	{
		return null;
	}

	public string? GetRawArgumentValue(string key)
	{
		return null;
	}

	public string? GetPositionalSegment()
	{
		return null;
	}
}
