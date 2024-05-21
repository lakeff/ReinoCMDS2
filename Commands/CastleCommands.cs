using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Il2CppInterop.Runtime;
using KindredCommands.Commands.Converters;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using ProjectM.Terrain;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements.UIR;
using VampireCommandFramework;

namespace KindredCommands.Commands;

internal class CastleCommands
{
	[Command("claim", description: "Claims the Castle Heart you are standing next to for a specified player", adminOnly: true)]
	public static void CastleClaim(ChatCommandContext ctx, OnlinePlayer player = null)
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

			ctx.Reply($"Assigning castle heart to {name}");

			TeamUtility.ClaimCastle(Core.EntityManager, newOwnerUser, castleHeart);
			return;
		}
		ctx.Reply("Not close enough to a castle heart");
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
	}*/
	[Command("incomingdecay", "incd", description: "Reports which territories have the least time remaining", adminOnly: true)]
	public static void PlotsDecayingNext(ChatCommandContext ctx)
	{
        // report a list of territories with the least time remaining
        var castleTerritories = Helper.GetEntitiesByComponentType<CastleTerritory>();

        var castleTerritoryList = new List<(Entity, CastleTerritory, double)>();
        foreach (var castleTerritoryEntity in castleTerritories)
        {
            var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
            if (castleTerritory.CastleHeart.Equals(Entity.Null)) continue;
            castleTerritoryList.Add((castleTerritoryEntity, castleTerritory, GetFuelTimeRemaining(castleTerritory.CastleHeart)));
        }
        castleTerritoryList.Sort((a, b) => a.Item3.CompareTo(b.Item3));
        var sb = new StringBuilder();
        foreach (var (territoryEntity, territory, secondsRemaining) in castleTerritoryList)
        {
            if (secondsRemaining <= 1) continue;

            var time = TimeSpan.FromSeconds(secondsRemaining);
			var region = territoryEntity.Read<TerritoryWorldRegion>().Region;
			var newLine = $"Castle {territory.CastleTerritoryIndex} in {RegionName(region)} with {time:%d}d {time:%h}h {time:%m}m remaining.";
            if (sb.ToString().Length + newLine.Length >= Core.MAX_REPLY_LENGTH)
            {
                break;
            }

			sb.AppendLine(newLine);
		}

		if (sb.Length == 0)
		{
			sb.AppendLine("No territories with fuel remaining");
		}

        ctx.Reply(sb.ToString());
	}

	public static double GetFuelTimeRemaining(Entity castleHeart)
	{
		var castleHeartComponent = castleHeart.Read<CastleHeart>();

		var secondsPerFuel = (8 * 60)/ Mathf.Min(Core.ServerGameSettingsSystem.Settings.CastleBloodEssenceDrainModifier, 3);
		return (castleHeartComponent.FuelEndTime - Core.ServerTime) + secondsPerFuel * castleHeartComponent.FuelQuantity;
	}

	[Command("openplots", "op", description: "Reports all the territories with open and/or decaying plots.")]
	public static void OpenPlots(ChatCommandContext ctx)
	{
		Dictionary<WorldRegionType, int> openPlots = [];
		Dictionary<WorldRegionType, int> plotsInDecay = [];
		foreach (var castleTerritoryEntity in Helper.GetEntitiesByComponentType<CastleTerritory>())
		{
			var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
			if (!castleTerritory.CastleHeart.Equals(Entity.Null))
			{
				var castleHeart = castleTerritory.CastleHeart.Read<CastleHeart>();
				if ((castleHeart.FuelEndTime - Core.ServerTime) > 0 || castleHeart.FuelQuantity > 0) continue;
				
				var region = castleTerritoryEntity.Read<TerritoryWorldRegion>().Region;
				if(plotsInDecay.ContainsKey(region))
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
				var region = castleTerritoryEntity.Read<TerritoryWorldRegion>().Region;
				if(openPlots.ContainsKey(region))
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

		foreach(var plot in openPlots)
		{
			if(plotsInDecay.ContainsKey(plot.Key))
			{
				stringList.Add($"{RegionName(plot.Key)} has {plot.Value} open plots and {plotsInDecay[plot.Key]} plots in decay");
			}
			else
			{
				stringList.Add($"{RegionName(plot.Key)} has {plot.Value} open plots");
			}
		}
		foreach(var plot in plotsInDecay)
		{
			if(!openPlots.ContainsKey(plot.Key))
			{
				stringList.Add($"{RegionName(plot.Key)} has {plot.Value} plots in decay");
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

	public static string RegionName(WorldRegionType region)
	{
		return Regex.Replace(region.ToString().Replace("_", ""), "(?<!^)([A-Z])", " $1");
	}

	[Command("plotsowned", "po", description: "Reports the number of plots owned by each player", adminOnly: true)]
    public static void PlotsOwned(ChatCommandContext ctx, int? page = null)
    {
        var castleTerritories = Helper.GetEntitiesByComponentType<CastleTerritory>();
        var playerPlots = new Dictionary<Entity, int>();
        foreach (var castleTerritoryEntity in castleTerritories)
        {
			var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
            if (castleTerritory.CastleHeart.Equals(Entity.Null)) continue;

            var userOwner = castleTerritory.CastleHeart.Read<UserOwner>();
            if (playerPlots.ContainsKey(userOwner.Owner.GetEntityOnServer()))
            {
                playerPlots[userOwner.Owner.GetEntityOnServer()]++;
            }
            else
            {
                playerPlots[userOwner.Owner.GetEntityOnServer()] = 1;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("Players by Plots Owned");
        int count = 0;
        int startIndex = (page ?? 1) == 1 ? 0 : ((page ?? 1) - 1) * 8;
        foreach (var playerPlot in playerPlots.OrderByDescending(x => x.Value).Skip(startIndex).Take(8))
        {
            var user = playerPlot.Key.Read<User>();
            sb.AppendLine($"{user.CharacterName} owns {playerPlot.Value} plots");
            count++;
            if (count % 8 == 0)
            {
                ctx.Reply(sb.ToString());
                sb.Clear();
            }
        }

        if (sb.Length > 0)
        {
            ctx.Reply(sb.ToString());
        }
    }
}
