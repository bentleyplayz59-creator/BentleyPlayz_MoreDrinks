using System;
using System.Collections.Generic;
using System.Reflection;
using Sickhead.Engine.Util;

namespace ContentPatcher.Framework.Patches.EditData;

internal class ModelKeyValueEditor : BaseDataEditor
{
	private readonly object Data;

	private readonly Lazy<Dictionary<string, MemberInfo>> FieldMap;

	public IEnumerable<string> FieldNames => FieldMap.Value.Keys.OrderByHuman();

	public ModelKeyValueEditor(object data)
	{
		CanAddEntries = false;
		Data = data;
		FieldMap = new Lazy<Dictionary<string, MemberInfo>>(GetFieldMap);
	}

	public override object ParseKey(string key)
	{
		return key;
	}

	public override bool HasEntry(object key)
	{
		string key2 = (string)key;
		return FieldMap.Value.ContainsKey(key2);
	}

	public override object? GetEntry(object key)
	{
		string name = (string)key;
		MemberInfo? member = GetMember(name);
		return ((object)member != null) ? MemberInfoExtensions.GetValue(member, Data) : null;
	}

	public override Type? GetEntryType(object key)
	{
		string name = (string)key;
		MemberInfo? member = GetMember(name);
		return ((object)member != null) ? MemberInfoExtensions.GetDataType(member) : null;
	}

	public override void RemoveEntry(object key)
	{
		SetEntry(key, null);
	}

	public override bool TryInitializeEntry(object key)
	{
		Type entryType = GetEntryType(key);
		if ((object)entryType == null)
		{
			return false;
		}
		object obj;
		try
		{
			obj = Activator.CreateInstance(entryType);
			if (obj == null)
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		SetEntry(key, obj);
		return true;
	}

	public override void SetEntry(object key, object? value)
	{
		string name = (string)key;
		MemberInfo? member = GetMember(name);
		if ((object)member != null)
		{
			MemberInfoExtensions.SetValue(member, Data, value);
		}
	}

	private Dictionary<string, MemberInfo> GetFieldMap()
	{
		Type type = Data.GetType();
		Dictionary<string, MemberInfo> dictionary = new Dictionary<string, MemberInfo>(StringComparer.OrdinalIgnoreCase);
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsPrivate && !fieldInfo.IsFamily)
			{
				dictionary[fieldInfo.Name] = fieldInfo;
			}
		}
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanRead && propertyInfo.CanWrite)
			{
				dictionary[propertyInfo.Name] = propertyInfo;
			}
		}
		return dictionary;
	}

	private MemberInfo? GetMember(string name)
	{
		return FieldMap.Value.GetValueOrDefault(name);
	}
}
