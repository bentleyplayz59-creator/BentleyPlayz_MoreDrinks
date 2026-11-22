using System;
using System.Linq;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod;

internal class ProducerFrameworkRecipe
{
	public string? InputId { get; }

	public string MachineId { get; }

	public ProducerFrameworkIngredient[] Ingredients { get; }

	public string?[] ExceptIngredients { get; }

	public string OutputId { get; }

	public int MinOutput { get; }

	public int MaxOutput { get; }

	public double OutputChance { get; }

	public Nullable PreserveType { get; }

	public ProducerFrameworkRecipe(string? inputId, string machineId, ProducerFrameworkIngredient[] ingredients, string?[] exceptIngredients, string outputId, int minOutput, int maxOutput, double outputChance, Nullable preserveType)
	{
		InputId = inputId;
		MachineId = machineId;
		Ingredients = ingredients;
		ExceptIngredients = exceptIngredients;
		OutputId = outputId;
		MinOutput = minOutput;
		MaxOutput = maxOutput;
		OutputChance = outputChance;
		PreserveType = preserveType;
	}

	public bool HasContextTags()
	{
		return InputId == null || Ingredients.Any((ProducerFrameworkIngredient p) => p.InputId == null) || ExceptIngredients.Any((string p) => p == null);
	}
}
