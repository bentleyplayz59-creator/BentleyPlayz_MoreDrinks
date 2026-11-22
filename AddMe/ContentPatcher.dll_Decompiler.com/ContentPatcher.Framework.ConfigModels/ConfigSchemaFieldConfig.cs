namespace ContentPatcher.Framework.ConfigModels;

internal class ConfigSchemaFieldConfig
{
	public string? AllowValues { get; }

	public string? Default { get; }

	public bool AllowBlank { get; }

	public bool AllowMultiple { get; }

	public string? Description { get; }

	public string? Section { get; }

	public ConfigSchemaFieldConfig(string allowValues, string @default, bool allowBlank, bool allowMultiple, string description, string section)
	{
		AllowValues = allowValues;
		Default = @default;
		AllowBlank = allowBlank;
		AllowMultiple = allowMultiple;
		Description = description;
		Section = section;
	}
}
