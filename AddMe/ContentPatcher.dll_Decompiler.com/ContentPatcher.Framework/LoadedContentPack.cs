using ContentPatcher.Framework.ConfigModels;
using Pathoschild.Stardew.Common.Utilities;

namespace ContentPatcher.Framework;

internal class LoadedContentPack : RawContentPack
{
	public ConfigFileHandler ConfigFileHandler { get; }

	public InvariantDictionary<ConfigField> Config { get; }

	public LoadedContentPack(RawContentPack contentPack, ConfigFileHandler configFileHandler, InvariantDictionary<ConfigField> config)
		: base(contentPack)
	{
		ConfigFileHandler = configFileHandler;
		Config = config;
	}
}
