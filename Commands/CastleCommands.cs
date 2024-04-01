using System;
using System.Collections.Generic;
using System.Text;
using KindredCommands.Commands.Converters;
using KindredCommands.Models.Discord;
using KindredCommands.Services;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;

namespace KindredCommands.Commands;

internal class CastleCommands
{
	[Command("claim", description: "Claims the Castle Heart you are standing next to for a specified player", adminOnly: true)]
	public static void CastleClaim(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			Entity newOwnerUser = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;

			var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
			var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
			foreach (var castleHeart in castleHearts)
			{
				var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

				if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
				{
					continue;
				}

				var name = player?.Value.CharacterName.ToString() ?? ctx.Name;
				List<ContentHelper> content = new()
			{
				new ContentHelper
				{
					Title = "Comando",
					Content = "claim"
				},
				new ContentHelper
				{
					Title = "Player",
					Content = player.Value.UserEntity.Read<User>().CharacterName.ToString()
				},
				new ContentHelper
				{
					Title = "Posi��o do Castelo",
					Content = castleHeartPos.ToString()
				}
			};


				DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);

				ctx.Reply($"Assigning castle heart to {name}");

				TeamUtility.ClaimCastle(Core.EntityManager, newOwnerUser, castleHeart);
				return;
			}
			ctx.Reply("Not close enough to a castle heart");
		}
	}

	//folded this into playerinfo
	/*[Command("castleinfo", "cinfo", description: "Reports information about a player's territories.", adminOnly: true)]
	public static void CastleInfo(ChatCommandContext ctx, OnlinePlayer player)
	{
		var foundCastle = false;
		ctx.Reply($"Castle Report for {player.Value.CharacterName}");
		foreach(var castleTerritoryEntity in Helper.GetEntitiesByComponentType<CastleTerritory>())
		{
			var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
			if (castleTerritory.CastleHeart.Equals(Entity.Null)) continue;

			var userOwner = castleTerritory.CastleHeart.Read<UserOwner>();
			if (!userOwner.Owner.GetEntityOnServer().Equals(player.Value.UserEntity)) continue;

			var region = TerritoryRegions(castleTerritory);
			var pylonstation = castleTerritory.CastleHeart.Read<Pylonstation>();
			var time = TimeSpan.FromMinutes(pylonstation.MinutesRemaining);
			ctx.Reply($"Castle {castleTerritory.CastleTerritoryIndex} in {region} with {time:%d}d {time:%h}h {time:%m} remaining.");
			foundCastle = true;
		}

		if(!foundCastle)
		{
			ctx.Reply("No owned territories found.");
		}

		}
	*/

	[Command("incomingdecay", "incd", description: "Reports which territories have the least time remaining", adminOnly: true)]

	public static void PlotsDecayingNext(ChatCommandContext ctx)
	{
		// report a list of territories with the least time remaining
		var castleTerritories = Helper.GetEntitiesByComponentType<CastleTerritory>();

		var castleTerritoryList = new List<CastleTerritory>();
		foreach (var castleTerritoryEntity in castleTerritories)
		{
			var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
			if (castleTerritory.CastleHeart.Equals(Entity.Null)) continue;
			castleTerritoryList.Add(castleTerritory);
		}
		castleTerritoryList.Sort((a, b) => a.CastleHeart.Read<Pylonstation>().MinutesRemaining.CompareTo(b.CastleHeart.Read<Pylonstation>().MinutesRemaining));
		var sb = new StringBuilder();
		foreach (var territory in castleTerritoryList)
		{
			var minutesRemaining = territory.CastleHeart.Read<Pylonstation>().MinutesRemaining;
			if (minutesRemaining <= 1) continue;

			var time = TimeSpan.FromMinutes(minutesRemaining);
			sb.AppendLine($"Castle {territory.CastleTerritoryIndex} in {TerritoryRegions(territory)} with {time:%d}d {time:%h}h {time:%m}m remaining.");

			if (sb.ToString().Split('\n').Length >= 7)
			{
				break;
			}
		}

		ctx.Reply(sb.ToString());

	}

	[Command("openplots", "op", description: "Reports all the territories with open and/or decaying plots.")]
	public static void OpenPlots(ChatCommandContext ctx)
	{
		Dictionary<string, int> openPlots = [];
		Dictionary<string, int> plotsInDecay = [];
		foreach (var castleTerritoryEntity in Helper.GetEntitiesByComponentType<CastleTerritory>())
		{
			var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
			if (!castleTerritory.CastleHeart.Equals(Entity.Null))
			{
				var pylonstation = castleTerritory.CastleHeart.Read<Pylonstation>();
				if (pylonstation.MinutesRemaining > 0 || pylonstation.FuelPercentage > 0) continue;

				var region = TerritoryRegions(castleTerritory);
				if (plotsInDecay.ContainsKey(region))
				{
					plotsInDecay[region]++;
				}
				else
				{
					plotsInDecay[region] = 1;
				}
				continue;
			}
			else
			{
				var region = TerritoryRegions(castleTerritory);
				if (openPlots.ContainsKey(region))
				{
					openPlots[region]++;
				}
				else
				{
					openPlots[region] = 1;
				}
			}
		}
		var stringList = new List<string>();

		foreach (var plot in openPlots)
		{
			if (plotsInDecay.ContainsKey(plot.Key))
			{
				stringList.Add($"{plot.Key} has {plot.Value} open plots and {plotsInDecay[plot.Key]} plots in decay");
			}
			else
			{
				stringList.Add($"{plot.Key} has {plot.Value} open plots");
			}
		}
		foreach (var plot in plotsInDecay)
		{
			if (!openPlots.ContainsKey(plot.Key))
			{
				stringList.Add($"{plot.Key} has {plot.Value} plots in decay");
			}
		}
		stringList.Sort();

		var sb = new StringBuilder();
		foreach (var appendString in stringList)
		{
			if (sb.Length + appendString.Length > Core.MAX_REPLY_LENGTH)
			{
				ctx.Reply(sb.ToString());
				sb.Clear();
			}
			sb.AppendLine(appendString);
		}

		if (stringList.Count == 0)
			sb.AppendLine("No open or decaying plots");

		ctx.Reply(sb.ToString());
	}

	public static string TerritoryRegions(CastleTerritory castleTerritory)
	{
		if (castleTerritory.CastleTerritoryIndex == 71 || castleTerritory.CastleTerritoryIndex == 72 || castleTerritory.CastleTerritoryIndex == 73 || castleTerritory.CastleTerritoryIndex == 85 || castleTerritory.CastleTerritoryIndex == 86)
		{
			return "Hallowed Mountains";
		}
		else if (castleTerritory.CastleTerritoryIndex >= 1 && castleTerritory.CastleTerritoryIndex <= 70)
		{
			return "Farbane Woods";
		}
		else if (castleTerritory.CastleTerritoryIndex >= 77 && castleTerritory.CastleTerritoryIndex <= 84 || castleTerritory.CastleTerritoryIndex >= 87 && castleTerritory.CastleTerritoryIndex <= 92 || castleTerritory.CastleTerritoryIndex >= 94 && castleTerritory.CastleTerritoryIndex <= 99 || castleTerritory.CastleTerritoryIndex >= 102 && castleTerritory.CastleTerritoryIndex <= 107 || castleTerritory.CastleTerritoryIndex == 116 || castleTerritory.CastleTerritoryIndex == 117)
		{
			return "Dunley Farmlands";
		}
		else if (castleTerritory.CastleTerritoryIndex >= 74 && castleTerritory.CastleTerritoryIndex <= 76 || castleTerritory.CastleTerritoryIndex == 93 || castleTerritory.CastleTerritoryIndex == 100 || castleTerritory.CastleTerritoryIndex == 101)
		{
			return "Silverlight Hills";
		}
		else if (castleTerritory.CastleTerritoryIndex >= 111 && castleTerritory.CastleTerritoryIndex <= 115 || castleTerritory.CastleTerritoryIndex >= 118 && castleTerritory.CastleTerritoryIndex <= 122 || castleTerritory.CastleTerritoryIndex == 127 || castleTerritory.CastleTerritoryIndex >= 134 && castleTerritory.CastleTerritoryIndex <= 138)
		{
			return "Gloomrot South";
		}
		else if (castleTerritory.CastleTerritoryIndex >= 108 && castleTerritory.CastleTerritoryIndex <= 110 || castleTerritory.CastleTerritoryIndex >= 123 && castleTerritory.CastleTerritoryIndex <= 126 || castleTerritory.CastleTerritoryIndex >= 131 && castleTerritory.CastleTerritoryIndex <= 133)
		{
			return "Gloomrot North";
		}
		else if (castleTerritory.CastleTerritoryIndex >= 128 && castleTerritory.CastleTerritoryIndex <= 130 || castleTerritory.CastleTerritoryIndex == 139)
		{
			return "Cursed Forest";
		}
		else if (castleTerritory.CastleTerritoryIndex == 0)
		{
			return "Developer Island";
		}
		else
		{
			return "Unknown";
		}
	}


}


