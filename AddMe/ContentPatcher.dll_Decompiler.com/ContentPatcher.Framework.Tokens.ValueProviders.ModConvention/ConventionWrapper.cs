using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Tokens.ValueProviders.ModConvention;

internal class ConventionWrapper
{
	private ConventionDelegates.IsMutable? IsMutableImpl;

	private ConventionDelegates.IsDeterministicForInput? IsDeterministicForInputImpl;

	private ConventionDelegates.AllowsInput? AllowsInputImpl;

	private ConventionDelegates.RequiresInput? RequiresInputImpl;

	private ConventionDelegates.CanHaveMultipleValues? CanHaveMultipleValuesImpl;

	private ConventionDelegates.GetValidInputs? GetValidInputsImpl;

	private ConventionDelegates.HasBoundedValues? HasBoundedValuesImpl;

	private ConventionDelegates.HasBoundedRangeValues? HasBoundedRangeValuesImpl;

	private ConventionDelegates.TryValidateInput? TryValidateInputImpl;

	private ConventionDelegates.TryValidateValues? TryValidateValuesImpl;

	private ConventionDelegates.NormalizeValue? NormalizeValueImpl;

	private ConventionDelegates.IsReady? IsReadyImpl;

	private ConventionDelegates.GetValues? GetValuesImpl;

	private ConventionDelegates.UpdateContext? UpdateContextImpl;

	public Func<string, string>? NormalizeValue { get; protected set; }

	public static bool TryCreate(object instance, IReflectionHelper reflection, [NotNullWhen(true)] out ConventionWrapper? wrapper, [NotNullWhen(false)] out string? error)
	{
		error = null;
		ConventionWrapper result = new ConventionWrapper();
		bool flag = true;
		string[] array = (from MethodInfo method in instance.GetType().GetMethods()
			where typeof(ConventionWrapper).GetMethod(method.Name) == null
			select method.Name).ToArray();
		if (array.Any())
		{
			flag = false;
			error = ((array.Length == 1) ? ("found unsupported '" + array[0] + "' method") : ("found unsupported methods: " + string.Join(", ", array)));
		}
		if (flag)
		{
			flag = TryMap<ConventionDelegates.IsMutable>(out error) && TryMap<ConventionDelegates.IsDeterministicForInput>(out error) && TryMap<ConventionDelegates.AllowsInput>(out error) && TryMap<ConventionDelegates.RequiresInput>(out error) && TryMap<ConventionDelegates.CanHaveMultipleValues>(out error) && TryMap<ConventionDelegates.GetValidInputs>(out error) && TryMap<ConventionDelegates.HasBoundedValues>(out error) && TryMap<ConventionDelegates.HasBoundedRangeValues>(out error) && TryMap<ConventionDelegates.TryValidateInput>(out error) && TryMap<ConventionDelegates.TryValidateValues>(out error) && TryMap<ConventionDelegates.NormalizeValue>(out error) && TryMap<ConventionDelegates.IsReady>(out error) && TryMap<ConventionDelegates.GetValues>(out error) && TryMap<ConventionDelegates.UpdateContext>(out error);
		}
		if (flag && result.NormalizeValueImpl != null)
		{
			result.NormalizeValue = (string value) => result.NormalizeValueImpl(value);
		}
		wrapper = (flag ? result : null);
		return flag;
		bool TryMap<TDelegate>([NotNullWhen(false)] out string? mapError) where TDelegate : Delegate
		{
			string name = typeof(TDelegate).Name;
			string text = name + "Impl";
			if (TryWrapMethod<TDelegate>(instance, name, out TDelegate methodDelegate, out mapError))
			{
				reflection.GetField<TDelegate>((object)result, text, true).SetValue(methodDelegate);
			}
			return mapError == null;
		}
	}

	public bool IsMutable()
	{
		return IsMutableImpl?.Invoke() ?? true;
	}

	public bool IsDeterministicForInput()
	{
		return IsDeterministicForInputImpl?.Invoke() ?? false;
	}

	public bool AllowsInput()
	{
		return AllowsInputImpl?.Invoke() ?? false;
	}

	public bool RequiresInput()
	{
		return RequiresInputImpl?.Invoke() ?? false;
	}

	public bool CanHaveMultipleValues(string? input = null)
	{
		return CanHaveMultipleValuesImpl?.Invoke() ?? true;
	}

	public IEnumerable<string> GetValidInputs()
	{
		IEnumerable<string> result;
		if (GetValidInputsImpl == null)
		{
			IEnumerable<string> enumerable = Array.Empty<string>();
			result = enumerable;
		}
		else
		{
			result = GetValidInputsImpl();
		}
		return result;
	}

	public bool HasBoundedValues(string? input, [NotNullWhen(true)] out IEnumerable<string>? allowedValues)
	{
		allowedValues = null;
		return HasBoundedValuesImpl?.Invoke(input, out allowedValues) ?? false;
	}

	public bool HasBoundedRangeValues(string? input, out int min, out int max)
	{
		min = 0;
		max = 0;
		return HasBoundedRangeValuesImpl?.Invoke(input, out min, out max) ?? false;
	}

	public bool TryValidateInput(string? input, [NotNullWhen(false)] out string? error)
	{
		error = null;
		return TryValidateInputImpl?.Invoke(input, out error) ?? true;
	}

	public bool TryValidateValues(string? input, IEnumerable<string> values, [NotNullWhen(false)] out string? error)
	{
		error = null;
		return TryValidateValuesImpl?.Invoke(input, values, out error) ?? true;
	}

	public bool IsReady()
	{
		return IsReadyImpl?.Invoke() ?? true;
	}

	public IEnumerable<string> GetValues(string? input)
	{
		IEnumerable<string> result;
		if (GetValuesImpl == null)
		{
			IEnumerable<string> enumerable = Array.Empty<string>();
			result = enumerable;
		}
		else
		{
			result = GetValuesImpl(input);
		}
		return result;
	}

	public bool UpdateContext()
	{
		return UpdateContextImpl?.Invoke() ?? false;
	}

	private static bool TryWrapMethod<TDelegate>(object instance, string name, [NotNullWhen(true)] out TDelegate? methodDelegate, out string? error) where TDelegate : Delegate
	{
		methodDelegate = null;
		error = null;
		MethodInfo[] array = (from p in instance.GetType().GetMethods()
			where p.Name == name
			select p).ToArray();
		if (array.Length == 0)
		{
			return false;
		}
		if (array.Length > 1)
		{
			return Fail("method " + name + " has multiple implementations.", out error);
		}
		try
		{
			methodDelegate = (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), instance, name);
			error = null;
			return true;
		}
		catch (ArgumentException ex)
		{
			methodDelegate = null;
			return Fail(GetMethodMismatchError(instance, name, ex), out error);
		}
	}

	private static string GetMethodMismatchError(object instance, string name, Exception ex)
	{
		MethodInfo method = typeof(ConventionWrapper).GetMethod(name);
		if (method == null)
		{
			throw new InvalidOperationException($"The {"ConventionWrapper"} class has no '{name}' method.");
		}
		MethodInfo method2 = instance.GetType().GetMethod(name);
		if (method2 == null)
		{
			throw new InvalidOperationException("The token has no '" + name + "' method.");
		}
		if (method.ReturnType != method2.ReturnType)
		{
			return $"method {name} has return type {GetTypeName(method2.ReturnType)}, but expected {GetTypeName(method.ReturnType)}.";
		}
		if (method.ContainsGenericParameters)
		{
			return "method " + name + " is a generic method, which isn't supported.";
		}
		ParameterInfo[] parameters = method.GetParameters();
		ParameterInfo[] parameters2 = method2.GetParameters();
		if (parameters.Length == parameters2.Length)
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameterInfo = parameters[i];
				ParameterInfo parameterInfo2 = parameters2[i];
				if (parameterInfo.ParameterType != parameterInfo2.ParameterType)
				{
					return $"method {name} > parameter {i + 1} ({parameterInfo2.Name}) has type {GetTypeName(parameterInfo2.ParameterType)}, but expected {GetTypeName(parameterInfo.ParameterType)}.";
				}
				if (parameterInfo.Attributes != parameterInfo2.Attributes)
				{
					return $"method {name} > parameter {i + 1} ({parameterInfo2.Name}) has attributes {parameterInfo2.Attributes}, but expected {parameterInfo.Attributes}.";
				}
			}
			return $"method {name} could not be mapped:\n{ex}";
		}
		return $"method {name} has {parameters2.Length} arguments, but expected {parameters.Length}.";
		static string GetTypeName(Type type)
		{
			if (type == typeof(bool))
			{
				return "bool";
			}
			if (type == typeof(int))
			{
				return "int";
			}
			if (type == typeof(string))
			{
				return "string";
			}
			if (type.IsPrimitive)
			{
				return type.Name;
			}
			return type.FullName;
		}
	}

	private static bool Fail(string inError, out string outError)
	{
		outError = inError;
		return false;
	}
}
