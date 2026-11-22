using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.ConfigModels;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens.Json;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations;

internal abstract class BaseMigration : IMigration
{
	protected InvariantSet? AddedTokens;

	public ISemanticVersion Version { get; }

	public string[] MigrationWarnings { get; protected set; } = Array.Empty<string>();

	public virtual bool TryMigrateMainContent(ContentConfig content, [NotNullWhen(false)] out string? error)
	{
		error = null;
		return true;
	}

	public virtual bool TryMigrate(ref PatchConfig[] patches, [NotNullWhen(false)] out string? error)
	{
		error = null;
		return true;
	}

	public virtual bool TryMigrate(ref ILexToken lexToken, [NotNullWhen(false)] out string? error)
	{
		if (lexToken is LexTokenToken lexTokenToken)
		{
			InvariantSet? addedTokens = AddedTokens;
			if (addedTokens != null && addedTokens.Contains(lexTokenToken.Name))
			{
				error = GetNounPhraseError("using token " + lexTokenToken.Name);
				return false;
			}
			if (lexTokenToken.HasInputArgs())
			{
				ILexToken[] parts = lexTokenToken.InputArgs.Parts;
				for (int i = 0; i < parts.Length; i++)
				{
					if (!TryMigrate(ref parts[i], out error))
					{
						return false;
					}
				}
			}
		}
		error = null;
		return true;
	}

	public virtual bool TryMigrate(IManagedTokenString tokenStr, [NotNullWhen(false)] out string? error)
	{
		error = null;
		return true;
	}

	public bool TryMigrate(TokenizableJToken tokenStructure, [NotNullWhen(false)] out string? error)
	{
		foreach (IManagedTokenString tokenString in tokenStructure.GetTokenStrings())
		{
			if (!TryMigrate(tokenString, out error))
			{
				return false;
			}
		}
		error = null;
		return true;
	}

	public virtual bool TryMigrate(Condition condition, [NotNullWhen(false)] out string? error)
	{
		error = null;
		return true;
	}

	protected BaseMigration(ISemanticVersion version)
	{
		Version = version;
	}

	protected string GetNounPhraseError(string nounPhrase)
	{
		return $"{nounPhrase} requires {"Format"} version {Version} or later";
	}

	protected PatchType? GetAction(PatchConfig patch)
	{
		return GetEnum<PatchType>(patch.Action);
	}

	protected bool HasAction(PatchConfig patch, PatchType type)
	{
		return GetAction(patch) == type;
	}

	protected TEnum? GetEnum<TEnum>(string? raw) where TEnum : struct
	{
		TEnum result;
		return Enum.TryParse<TEnum>(raw, ignoreCase: true, out result) ? new TEnum?(result) : ((TEnum?)null);
	}
}
