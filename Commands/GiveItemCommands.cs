using Epic.OnlineServices;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements.UIR;
using VampireCommandFramework;

namespace KindredCommands.Commands;
internal class GiveItemCommands
{
	public record class GivenItem(PrefabGUID Value);

	internal class GiveItemConverter : CommandArgumentConverter<GivenItem>
	{
		public override GivenItem Parse(ICommandContext ctx, string input)
		{

			if (int.TryParse(input, out var integral))
			{
				return new GivenItem(new(integral));
			}

			if (TryGet(input, out var result)) return result;

			var inputIngredientAdded = "Item_Ingredient_" + input;
			if (TryGet(inputIngredientAdded, out result)) return result;

			// Standard postfix
			var standardPostfix = inputIngredientAdded + "_Standard";
			if (TryGet(standardPostfix, out result)) return result;

			List<(string Name, PrefabGUID Prefab)> searchResults = [];
			foreach (var kvp in Core.Prefabs.SpawnableNameToGuid)
			{
				if (kvp.Value.Name.StartsWith("Item_") && kvp.Key.Contains(input, StringComparison.OrdinalIgnoreCase))
				{
					searchResults.Add((kvp.Value.Name["Item_".Length..], kvp.Value.Prefab));
				}
			}

			if (searchResults.Count == 1)
			{
				return new GivenItem(searchResults[0].Prefab);
			}

			if (searchResults.Count > 1)
			{
				var sb = new StringBuilder();
				sb.AppendLine("Multiple results be more specific");
				foreach (var kvp in searchResults)
				{
					sb.AppendLine(kvp.Name);
				}
				throw ctx.Error(sb.ToString());
			}

			// Try a double search splitting the input
			for (var i = 3; i < input.Length; ++i)
			{
				var inputOne = input[..i];
				var inputTwo = input[i..];
				foreach (var kvp in Core.Prefabs.SpawnableNameToGuid)
				{
					if (kvp.Value.Name.StartsWith("Item_") &&
						kvp.Key.Contains(inputOne, StringComparison.OrdinalIgnoreCase) &&
						kvp.Key.Contains(inputTwo, StringComparison.OrdinalIgnoreCase))
					{
						searchResults.Add((kvp.Value.Name["Item_".Length..], kvp.Value.Prefab));
					}
				}

				if (searchResults.Count == 1)
				{
					return new GivenItem(searchResults[0].Prefab);
				}
			}

			var resultsFromFirstSplit = searchResults;
			searchResults = [];

			// Try a double search splitting the input with _ prepended
			for (var i = 3; i < input.Length; ++i)
			{
				var inputOne = "_" + input[..i];
				var inputTwo = input[i..];
				foreach (var kvp in Core.Prefabs.SpawnableNameToGuid)
				{
					if (kvp.Value.Name.StartsWith("Item_") &&
						kvp.Key.Contains(inputOne, StringComparison.OrdinalIgnoreCase) &&
						kvp.Key.Contains(inputTwo, StringComparison.OrdinalIgnoreCase))
					{
						searchResults.Add((kvp.Value.Name["Item_".Length..], kvp.Value.Prefab));
					}
				}

				if (searchResults.Count == 1)
				{
					return new GivenItem(searchResults[0].Prefab);
				}

				if (searchResults.Count > 1)
				{
					var sb = new StringBuilder();
					sb.AppendLine("Multiple results be more specific");
					foreach (var kvp in searchResults)
					{
						sb.AppendLine(kvp.Name);
					}
					throw ctx.Error(sb.ToString());
				}
			}

			if (resultsFromFirstSplit.Count > 1)
			{
				var sb = new StringBuilder();
				sb.AppendLine("Multiple results be more specific");
				foreach (var kvp in resultsFromFirstSplit)
				{
					sb.AppendLine(kvp.Name);
				}
				throw ctx.Error(sb.ToString());
			}

			throw ctx.Error($"Invalid item id: {input}");
		}

		private static bool TryGet(string input, out GivenItem item)
		{
			if (Core.Prefabs.TryGetItem(input, out var prefab))
			{
				item = new GivenItem(prefab);
				return true;
			}

			item = new GivenItem(new(0));
			return false;
		}
	}

	[Command("give", "g", "<Prefab GUID or name> [quantity=1]", "Gives the specified item to the player", adminOnly: true)]
	public static void GiveItem(ChatCommandContext ctx, GivenItem item, int quantity = 1)
	{
		Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, item.Value, quantity);
		var prefabSys = Core.Server.GetExistingSystem<PrefabCollectionSystem>();
		prefabSys.PrefabGuidToNameDictionary.TryGetValue(item.Value, out var name); // seems excessive
		ctx.Reply($"Gave {quantity} {name}");
	}
}
