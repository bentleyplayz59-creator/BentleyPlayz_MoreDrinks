using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace ContentPatcher.Framework.Patches.EditData;

internal class KeyValueEditorFactory
{
	private static readonly Dictionary<Type, Func<object, IKeyValueEditor>?> CachedConstructors = new Dictionary<Type, Func<object, IKeyValueEditor>>();

	public bool TryGetEditorFor([NotNullWhen(true)] object? data, [NotNullWhen(true)] out IKeyValueEditor? editor)
	{
		Type type = data?.GetType();
		editor = null;
		if (data == null || type == null)
		{
			return false;
		}
		if (!CachedConstructors.TryGetValue(type, out Func<object, IKeyValueEditor> value))
		{
			if (!TryGetConstructorWithoutCache(type, out value))
			{
				value = null;
			}
			CachedConstructors[type] = value;
		}
		if (value != null)
		{
			editor = value(data);
			return true;
		}
		return false;
	}

	private bool TryGetConstructorWithoutCache(Type type, [NotNullWhen(true)] out Func<object, IKeyValueEditor>? getEditor)
	{
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<, >))
		{
			Type[] genericArguments = type.GetGenericArguments();
			if (genericArguments.Length != 2)
			{
				throw new InvalidOperationException("Can't parse the asset's dictionary key/value types.");
			}
			Type type2 = type.GetGenericArguments().FirstOrDefault();
			Type type3 = type.GetGenericArguments().LastOrDefault();
			if (type2 == null)
			{
				throw new InvalidOperationException("Can't parse the asset's dictionary key type.");
			}
			if (type3 == null)
			{
				throw new InvalidOperationException("Can't parse the asset's dictionary value type.");
			}
			MethodInfo method = GetType().GetMethod("GetConstructorForDictionary", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method == null)
			{
				throw new InvalidOperationException("Can't fetch the internal GetConstructorForDictionary method.");
			}
			getEditor = (Func<object, IKeyValueEditor>)method.MakeGenericMethod(type2, type3).Invoke(this, null);
			return true;
		}
		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
		{
			Type type4 = type.GetGenericArguments().FirstOrDefault();
			if (type4 == null)
			{
				throw new InvalidOperationException("Can't parse the asset's list value type.");
			}
			MethodInfo method2 = GetType().GetMethod("GetConstructorForList", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method2 == null)
			{
				throw new InvalidOperationException("Can't fetch the internal GetConstructorForList method.");
			}
			getEditor = (Func<object, IKeyValueEditor>)method2.MakeGenericMethod(type4).Invoke(this, null);
			return true;
		}
		if ((object)type != null && !type.IsValueType && !type.IsGenericType && type != typeof(string))
		{
			getEditor = (object data) => new ModelKeyValueEditor(data);
			return true;
		}
		getEditor = null;
		return false;
	}

	private Func<object, IKeyValueEditor> GetConstructorForDictionary<TKey, TValue>()
	{
		return (object data) => new DictionaryKeyValueEditor<TKey, TValue>((IDictionary<TKey, TValue>)data);
	}

	private Func<object, IKeyValueEditor> GetConstructorForList<TValue>()
	{
		return (object data) => new ListKeyValueEditor<TValue>((IList<TValue>)data);
	}
}
