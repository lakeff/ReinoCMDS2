using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectM;
using VampireCommandFramework;

namespace KindredCommands.Commands;
[CommandGroup("search")]
public class SearchCommands
{
	[Command("item", "i", adminOnly: true)]
	public static void SearchItem(ChatCommandContext ctx, string search, int page = 1)
	{
		var prefabSys = Core.Server.GetExistingSystem<PrefabCollectionSystem>();

		List<(string Name, PrefabGUID Prefab)> searchResults = [];
		try
		{
			foreach (var kvp in Core.Prefabs.SpawnableNameToGuid)
			{
				if (kvp.Value.Name.StartsWith("Item_") && kvp.Key.Contains(search, StringComparison.OrdinalIgnoreCase))
				{
					searchResults.Add((kvp.Value.Name["Item_".Length..], kvp.Value.Prefab));
				}
			}

			if (!searchResults.Any())
			{
				ctx.Reply("Could not find any matching prefabs.");
			}

			searchResults = searchResults.OrderBy(kvp => kvp.Name).ToList();

			var sb = new StringBuilder();
			var totalCount = searchResults.Count;
			var pageSize = 8;
			var pageLabel = totalCount > pageSize ? $" (Page {page}/{Math.Ceiling(totalCount / (float)pageSize)})" : "";

			if (totalCount > pageSize)
			{
				searchResults = searchResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
			}

			sb.AppendLine($"Found {totalCount} matches {pageLabel}:");
			foreach (var (Name, Prefab) in searchResults)
			{
				sb.AppendLine(
					$"({Prefab.GuidHash}) {Name.Replace(search, $"<b>{search}</b>", StringComparison.OrdinalIgnoreCase)}");
			}

			ctx.Reply(sb.ToString());
		}
		catch (Exception e)
		{
			Core.LogException(e);
		}
	}

	[Command("npc", "n", adminOnly: false)]
	public static void SearchNPC(ChatCommandContext ctx, string search, int page = 1)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var prefabSys = Core.Server.GetExistingSystem<PrefabCollectionSystem>();

			List<(string Name, PrefabGUID Prefab)> searchResults = [];
			try
			{
				foreach (var kvp in Core.Prefabs.SpawnableNameToGuid)
				{
					if (kvp.Value.Name.StartsWith("CHAR_") && kvp.Key.Contains(search, StringComparison.OrdinalIgnoreCase))
					{
						searchResults.Add((kvp.Value.Name["CHAR_".Length..], kvp.Value.Prefab));
					}
				}

				if (!searchResults.Any())
				{
					ctx.Reply("Could not find any matching prefabs.");
				}

				searchResults = searchResults.OrderBy(kvp => kvp.Name).ToList();

				var sb = new StringBuilder();
				var totalCount = searchResults.Count;
				var pageSize = 8;
				var pageLabel = totalCount > pageSize ? $" (Page {page}/{Math.Ceiling(totalCount / (float)pageSize)})" : "";

				if (totalCount > pageSize)
				{
					searchResults = searchResults.Skip((page - 1) * pageSize).Take(pageSize).ToList();
				}

				sb.AppendLine($"Found {totalCount} matches {pageLabel}:");
				foreach (var (Name, Prefab) in searchResults)
				{
					sb.AppendLine(
						$"({Prefab.GuidHash}) {Name.Replace(search, $"<b>{search}</b>", StringComparison.OrdinalIgnoreCase)}");
				}

				ctx.Reply(sb.ToString());
			}
			catch (Exception e)
			{
				Core.LogException(e);
			}
		}
	}
}
