using System;
using System.Collections.Generic;
using System.Linq;

namespace ContentPatcher.Framework;

internal class LogPathBuilder : IComparable<LogPathBuilder?>
{
	private readonly Lazy<string> PathString;

	public string[] Segments { get; }

	public LogPathBuilder(params string[] segments)
	{
		Segments = segments;
		PathString = new Lazy<string>(() => string.Join(" > ", Segments));
	}

	public LogPathBuilder(IEnumerable<string> segments)
		: this(segments.ToArray())
	{
	}

	public LogPathBuilder With(params string[] segments)
	{
		return new LogPathBuilder(Segments.Concat(segments));
	}

	public override string ToString()
	{
		return PathString.Value;
	}

	public int CompareTo(LogPathBuilder? other)
	{
		return string.Compare(PathString.Value, other?.PathString.Value, StringComparison.Ordinal);
	}
}
