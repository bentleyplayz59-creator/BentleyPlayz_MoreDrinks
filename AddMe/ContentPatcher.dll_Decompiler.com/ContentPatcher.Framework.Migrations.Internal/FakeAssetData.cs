using System;
using StardewModdingAPI;

namespace ContentPatcher.Framework.Migrations.Internal;

internal class FakeAssetData : IAssetData, IAssetData<object>, IAssetInfo
{
	public string? Locale { get; }

	public IAssetName Name { get; }

	public IAssetName NameWithoutLocale { get; }

	public Type DataType { get; }

	public object Data { get; }

	public FakeAssetData(IAssetData realData, IAssetName assetName, object data)
	{
		Locale = ((IAssetInfo)realData).Locale;
		Name = assetName;
		NameWithoutLocale = assetName.GetBaseAssetName();
		DataType = data.GetType();
		Data = data;
	}

	public void ReplaceWith(object value)
	{
		throw new NotImplementedException();
	}

	public IAssetDataForDictionary<TKey, TValue> AsDictionary<TKey, TValue>()
	{
		throw new NotImplementedException();
	}

	public IAssetDataForImage AsImage()
	{
		throw new NotImplementedException();
	}

	public IAssetDataForMap AsMap()
	{
		throw new NotImplementedException();
	}

	public TData GetData<TData>()
	{
		throw new NotImplementedException();
	}
}
