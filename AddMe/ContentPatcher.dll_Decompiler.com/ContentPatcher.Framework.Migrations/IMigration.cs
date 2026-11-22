using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens.Json;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal interface IMigration
{
	ISemanticVersion Version { get; }

	string[] MigrationWarnings { get; }

	bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error);

	bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error);

	bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error);

	bool TryMigrate(IManagedTokenString tokenStr, [NotNullWhen(false)] out string? error);

	bool TryMigrate(TokenizableJToken tokenStructure, [NotNullWhen(false)] out string? error);

	bool TryMigrate(Condition condition, [NotNullWhen(false)] out string? error);
}
