using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework.ConfigModels;

internal class PatchConfig
{
	public string? LogName { get; set; }

	public string? Action { get; set; }

	public string? Target { get; set; }

	public string? TargetLocale { get; set; }

	public string? Update { get; set; }

	public string? FromFile { get; set; }

	public string? Enabled { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public InvariantDictionary<string?> When { get; } = new InvariantDictionary<string>();

	public string? Priority { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public InvariantDictionary<string?>? LocalTokens { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<TextOperationConfig?> TextOperations { get; } = new List<TextOperationConfig>();

	public PatchRectangleConfig? FromArea { get; set; }

	public PatchRectangleConfig? ToArea { get; set; }

	public string? PatchMode { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public InvariantDictionary<JToken?> Entries { get; } = new InvariantDictionary<JToken>();

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public InvariantDictionary<InvariantDictionary<JToken?>?> Fields { get; } = new InvariantDictionary<InvariantDictionary<JToken>>();

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<PatchMoveEntryConfig?> MoveEntries { get; } = new List<PatchMoveEntryConfig>();

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<string> TargetField { get; } = new List<string>();

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public InvariantDictionary<string?> MapProperties { get; } = new InvariantDictionary<string>();

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<string?> AddNpcWarps { get; } = new List<string>();

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<string?> AddWarps { get; } = new List<string>();

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public List<PatchMapTileConfig?> MapTiles { get; } = new List<PatchMapTileConfig>();

	[JsonConstructor]
	public PatchConfig()
	{
	}

	public PatchConfig(PatchConfig other)
	{
		LogName = other.LogName;
		Action = other.Action;
		Target = other.Target;
		Update = other.Update;
		FromFile = other.FromFile;
		Enabled = other.Enabled;
		When = other.When.Clone();
		Priority = other.Priority;
		LocalTokens = other.LocalTokens?.Clone();
		TextOperations = other.TextOperations.Select((TextOperationConfig p) => (p != null) ? new TextOperationConfig(p) : null).ToList();
		FromArea = ((other.FromArea != null) ? new PatchRectangleConfig(other.FromArea) : null);
		ToArea = ((other.ToArea != null) ? new PatchRectangleConfig(other.ToArea) : null);
		PatchMode = other.PatchMode;
		Entries = other.Entries.Clone((JToken? value) => (value != null) ? value.DeepClone() : null);
		Fields = other.Fields.Clone((InvariantDictionary<JToken?>? entryFields) => entryFields?.Clone((JToken? value) => (value != null) ? value.DeepClone() : null));
		MoveEntries = other.MoveEntries.Select((PatchMoveEntryConfig p) => (p != null) ? new PatchMoveEntryConfig(p) : null).ToList();
		TargetField = other.TargetField.ToList();
		MapProperties = other.MapProperties.Clone();
		AddNpcWarps = other.AddNpcWarps.ToList();
		AddWarps = other.AddWarps.ToList();
		MapTiles = other.MapTiles.Select((PatchMapTileConfig p) => (p != null) ? new PatchMapTileConfig(p) : null).ToList();
	}
}
