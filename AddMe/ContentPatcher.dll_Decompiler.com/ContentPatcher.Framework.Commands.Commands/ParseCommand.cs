using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Lexing.LexTokens;
using ContentPatcher.Framework.Tokens;
using Pathoschild.Stardew.Common.Commands;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Commands.Commands;

internal class ParseCommand : BaseCommand
{
	private readonly Func<string?, IContext> GetContext;

	public ParseCommand(IMonitor monitor, Func<string?, IContext> getContext)
		: base(monitor, "parse")
	{
		GetContext = getContext;
	}

	public override string GetDescription()
	{
		return "patch parse\n   usage: patch parse \"value\" [compact]\n   Parses the given token string and shows the result. For example, `patch parse \"assets/{{Season}}.png\" will show a value like \"assets/Spring.png\". If present, 'compact' returns less details.\n\npatch parse \"value\" \"content -pack.id\" [compact]\n   Parses the given token string and shows the result, using tokens available to the specified content pack (using the ID from the content pack's manifest.json). For example, `patch parse \"assets/{{CustomToken}}.png\" \"Pathoschild.ExampleContentPack\". If present, 'compact' returns less details.";
	}

	public override void Handle(string[] args)
	{
		int num = args.Length;
		if ((num < 1 || num > 3) ? true : false)
		{
			Monitor.Log("The 'patch parse' command expects one to three arguments. See 'patch help parse' for more info.", (LogLevel)4);
			return;
		}
		string text = args[0];
		string arg = ((args.Length >= 2 && args[1] != "compact") ? args[1] : null);
		bool flag = args.Skip(1).Contains("compact");
		IContext context;
		try
		{
			context = GetContext(arg);
		}
		catch (KeyNotFoundException ex)
		{
			Monitor.Log(ex.Message, (LogLevel)4);
			return;
		}
		catch (Exception ex2)
		{
			Monitor.Log(ex2.ToString(), (LogLevel)4);
			return;
		}
		TokenString tokenString;
		LexFormatException ex5 = default;
		try
		{
			tokenString = new TokenString(text, context, new LogPathBuilder("console command"));
		}
		catch (LexFormatException ex3)
		{
			Monitor.Log("Can't parse that token value: " + ex3.Message, (LogLevel)4);
			return;
		}
		catch (InvalidOperationException ex4) when (((Func<bool>)delegate
		{
			// Could not convert BlockContainer to single expression
			ex5 = ex4.InnerException as LexFormatException;
			return ex5 != null;
		}).Invoke())
		{
			Monitor.Log("Can't parse that token value: " + ex5.Message, (LogLevel)4);
			return;
		}
		catch (Exception value)
		{
			Monitor.Log($"Can't parse that token value: {value}", (LogLevel)4);
			return;
		}
		IContextualState diagnosticState = tokenString.GetDiagnosticState();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine();
		if (!flag)
		{
			stringBuilder.AppendLine("Metadata");
			stringBuilder.AppendLine("----------------");
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("   raw value:   ");
			handler.AppendFormatted(text);
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("   ready:       ");
			handler.AppendFormatted(tokenString.IsReady);
			stringBuilder4.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder5 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("   mutable:     ");
			handler.AppendFormatted(tokenString.IsMutable);
			stringBuilder5.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder6 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
			handler.AppendLiteral("   has tokens:  ");
			handler.AppendFormatted(tokenString.HasAnyTokens);
			stringBuilder6.AppendLine(ref handler);
			if (tokenString.HasAnyTokens)
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder7 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(16, 1, stringBuilder2);
				handler.AppendLiteral("   tokens used: ");
				handler.AppendFormatted(string.Join(", ", tokenString.GetTokensUsed().Distinct().OrderByHuman()));
				stringBuilder7.AppendLine(ref handler);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Diagnostic state");
			stringBuilder.AppendLine("----------------");
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder8 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
			handler.AppendLiteral("   valid:    ");
			handler.AppendFormatted(diagnosticState.IsValid);
			stringBuilder8.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder9 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
			handler.AppendLiteral("   in scope: ");
			handler.AppendFormatted(diagnosticState.IsValid);
			stringBuilder9.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder10 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(13, 1, stringBuilder2);
			handler.AppendLiteral("   ready:    ");
			handler.AppendFormatted(diagnosticState.IsReady);
			stringBuilder10.AppendLine(ref handler);
			if (diagnosticState.Errors.Any())
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder11 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
				handler.AppendLiteral("   errors:         ");
				handler.AppendFormatted(string.Join(", ", diagnosticState.Errors));
				stringBuilder11.AppendLine(ref handler);
			}
			if (diagnosticState.InvalidTokens.Any())
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder12 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
				handler.AppendLiteral("   invalid tokens: ");
				handler.AppendFormatted(string.Join(", ", diagnosticState.InvalidTokens));
				stringBuilder12.AppendLine(ref handler);
			}
			if (diagnosticState.UnreadyTokens.Any())
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder13 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(19, 1, stringBuilder2);
				handler.AppendLiteral("   unready tokens: ");
				handler.AppendFormatted(string.Join(", ", diagnosticState.UnreadyTokens));
				stringBuilder13.AppendLine(ref handler);
			}
			if (diagnosticState.UnavailableModTokens.Any())
			{
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder14 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(27, 1, stringBuilder2);
				handler.AppendLiteral("   unavailable mod tokens: ");
				handler.AppendFormatted(string.Join(", ", diagnosticState.UnavailableModTokens));
				stringBuilder14.AppendLine(ref handler);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Result");
			stringBuilder.AppendLine("----------------");
		}
		stringBuilder.AppendLine((!tokenString.IsReady) ? "The token string is invalid or unready." : $"   The token string is valid and ready. Parsed value: \"{tokenString}\"");
		Monitor.Log(stringBuilder.ToString(), (LogLevel)1);
	}
}
