using System;
using StardewValley;

namespace Pathoschild.Stardew.Common.Items;

internal class SearchableItem
{
	public string Type { get; }

	public Item Item { get; }

	public Func<Item> CreateItem { get; }

	public string Id { get; }

	public string QualifiedItemId { get; }

	public string Name => Item.Name;

	public string DisplayName => Item.DisplayName;

	public SearchableItem(string type, string id, Func<SearchableItem, Item> createItem)
	{
		SearchableItem arg = this;
		Type = type;
		Id = id;
		QualifiedItemId = Type + Id;
		CreateItem = () => createItem(arg);
		Item = createItem(this);
	}

	public bool NameContains(string substring)
	{
		return Name.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1 || DisplayName.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1;
	}

	public bool NameEquivalentTo(string name)
	{
		return Name.Equals(name, StringComparison.OrdinalIgnoreCase) || DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase);
	}
}
