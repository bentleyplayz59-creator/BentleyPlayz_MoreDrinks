using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod;

internal class ProducerFrameworkModIntegration : BaseIntegration<IProducerFrameworkModApi>
{
	private bool LoggedInvalidRecipeError;

	public ProducerFrameworkModIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Producer Framework Mod", "Digus.ProducerFrameworkMod", "1.9.3", modRegistry, monitor)
	{
	}

	public IEnumerable<ProducerFrameworkRecipe> GetRecipes()
	{
		AssertLoaded();
		List<Dictionary<string, object>> raw = SafelyCallApi((IProducerFrameworkModApi api) => api.GetRecipes(), "Failed getting recipes from {0}.") ?? new List<Dictionary<string, object>>();
		return ReadRecipes(raw);
	}

	public IEnumerable<ProducerFrameworkRecipe> GetRecipes(Object machine)
	{
		AssertLoaded();
		List<Dictionary<string, object>> raw = SafelyCallApi((IProducerFrameworkModApi api) => api.GetRecipes(machine), "Failed getting recipes from {0}.") ?? new List<Dictionary<string, object>>();
		return ReadRecipes(raw);
	}

	private IEnumerable<ProducerFrameworkRecipe> ReadRecipes(IEnumerable<IDictionary<string, object?>> raw)
	{
		return raw.Select(ReadRecipe).WhereNotNull();
	}

	private ProducerFrameworkRecipe? ReadRecipe(IDictionary<string, object?> raw)
	{
		try
		{
			string inputId = (string)raw["InputKey"];
			string machineId = (string)raw["MachineID"];
			ProducerFrameworkIngredient[] ingredients = ((List<Dictionary<string, object>>)raw["Ingredients"]).Select(ReadIngredient).ToArray();
			string[] exceptIngredients = (from p in ((List<Dictionary<string, object>>)raw["ExceptIngredients"]).Select(ReadIngredient)
				select p.InputId).ToArray();
			string outputId = (string)raw["Output"];
			int minOutput = (int)raw["MinOutput"];
			int maxOutput = (int)raw["MaxOutput"];
			Nullable val = (Nullable)raw["PreserveType"];
			double outputChance = (double)raw["OutputChance"];
			Nullable preserveType = val;
			return new ProducerFrameworkRecipe(inputId, machineId, ingredients, exceptIngredients, outputId, minOutput, maxOutput, outputChance, preserveType);
		}
		catch (Exception ex)
		{
			if (!LoggedInvalidRecipeError)
			{
				LoggedInvalidRecipeError = true;
				Monitor.Log("Failed to load some recipes from Producer Framework Mod. Some custom machines may not appear in lookups.", (LogLevel)3);
				Monitor.Log(ex.ToString(), (LogLevel)0);
			}
			return null;
		}
	}

	private ProducerFrameworkIngredient ReadIngredient(IDictionary<string, object?> raw)
	{
		string inputId = (string)raw["ID"];
		int count = ((!raw.TryGetValue("Count", out object value) || value == null) ? 1 : ((int)value));
		return new ProducerFrameworkIngredient
		{
			InputId = inputId,
			Count = count
		};
	}
}
