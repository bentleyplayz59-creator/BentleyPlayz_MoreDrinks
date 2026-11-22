using System;
using System.Diagnostics.CodeAnalysis;
using ContentPatcher.Framework.Tokens.ValueProviders;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens;

internal class ModProvidedToken : Token
{
	private readonly IMonitor Monitor;

	public IManifest Mod { get; }

	public string NamePrefix { get; }

	public string NameWithoutPrefix { get; }

	public ModProvidedToken(string nameWithoutPrefix, IManifest mod, IValueProvider provider, IMonitor monitor)
		: base(mod.UniqueID + "/" + nameWithoutPrefix, provider)
	{
		NamePrefix = mod.UniqueID + "/";
		NameWithoutPrefix = nameWithoutPrefix;
		Mod = mod;
		Monitor = monitor;
	}

	public override bool UpdateContext(IContext context)
	{
		try
		{
			return base.UpdateContext(context);
		}
		catch (Exception ex)
		{
			Log(ex);
			return false;
		}
	}

	public override IInvariantSet GetTokensUsed()
	{
		try
		{
			return base.GetTokensUsed();
		}
		catch (Exception ex)
		{
			Log(ex);
			return InvariantSets.Empty;
		}
	}

	public override IContextualState GetDiagnosticState()
	{
		try
		{
			return base.GetDiagnosticState();
		}
		catch (Exception ex)
		{
			Log(ex);
			return new ContextualState();
		}
	}

	public override bool CanHaveMultipleValues(IInputArguments input)
	{
		try
		{
			return base.CanHaveMultipleValues(input);
		}
		catch (Exception ex)
		{
			Log(ex);
			return false;
		}
	}

	public override bool TryValidateInput(IInputArguments input, [NotNullWhen(false)] out string? error)
	{
		try
		{
			return base.TryValidateInput(input, out error);
		}
		catch (Exception ex)
		{
			Log(ex);
			error = null;
			return true;
		}
	}

	public override bool TryValidateValues(IInputArguments input, IInvariantSet values, IContext context, [NotNullWhen(false)] out string? error)
	{
		try
		{
			return base.TryValidateValues(input, values, context, out error);
		}
		catch (Exception ex)
		{
			Log(ex);
			error = null;
			return true;
		}
	}

	public override IInvariantSet? GetAllowedInputArguments()
	{
		try
		{
			return base.GetAllowedInputArguments();
		}
		catch (Exception ex)
		{
			Log(ex);
			return null;
		}
	}

	public override bool HasBoundedValues(IInputArguments input, [NotNullWhen(true)] out IInvariantSet? allowedValues)
	{
		try
		{
			return base.HasBoundedValues(input, out allowedValues);
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			Log(ex2);
			allowedValues = null;
			return false;
		}
	}

	public override bool HasBoundedRangeValues(IInputArguments input, out int min, out int max)
	{
		try
		{
			return base.HasBoundedRangeValues(input, out min, out max);
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			Log(ex2);
			min = 0;
			max = 0;
			return false;
		}
	}

	public override IInvariantSet GetValues(IInputArguments input)
	{
		try
		{
			return base.GetValues(input);
		}
		catch (InvalidOperationException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			Log(ex2);
			return InvariantSets.Empty;
		}
	}

	private void Log(Exception ex)
	{
		Monitor.LogOnce($"The mod '{Mod.Name}' added custom token '{Name}', which failed and may not work correctly. See the log for details.", (LogLevel)3);
		Monitor.Log($"Custom token '{Name}' failed:\n{ex}", (LogLevel)0);
	}
}
