using System;
using KindredCommands.Commands.Converters;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;
using System.Text;

namespace KindredCommands.Commands;
internal class InfoCommands
{
	[Command("whereami", "wai", description: "Gives your current position", adminOnly: true)]
	public static void WhereAmI(ChatCommandContext ctx)
	{
		var pos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
		ctx.Reply($"You are at {pos.x}, {pos.y}, {pos.z}");
	}

	[Command("playerinfo", "pinfo", description: "Displays information about a player.", adminOnly: true)]
	public static void PlayerInfo(ChatCommandContext ctx, FoundPlayer player)
	{
		var user = player.Value.UserEntity.Read<User>();
		var steamID = user.PlatformId;
		var name = user.CharacterName;
		var online = user.IsConnected;
        var clanName = "Clan: No clan found\n";
        var clanEntity = user.ClanEntity.GetEntityOnServer();

		if (clanEntity != Entity.Null && clanEntity.Has<ClanTeam>())
		{
			var clanTeam = clanEntity.Read<ClanTeam>();
			clanName = $"Clan: {clanTeam.Name}\n";
		}
		
		var pos = Core.EntityManager.GetComponentData<LocalToWorld>(player.Value.CharEntity).Position;
		var posStr = $"{pos.x}, {pos.y}, {pos.z}";


		var castleFound = true;
		var castleInfo = new StringBuilder();
		foreach (var castleTerritoryEntity in Helper.GetEntitiesByComponentType<CastleTerritory>())
		{
			var castleTerritory = castleTerritoryEntity.Read<CastleTerritory>();
			if (castleTerritory.CastleHeart.Equals(Entity.Null)) continue;

			var userOwner = castleTerritory.CastleHeart.Read<UserOwner>();
			if (!userOwner.Owner.GetEntityOnServer().Equals(player.Value.UserEntity)) continue;

			var region = CastleCommands.TerritoryRegions(castleTerritory);
			var pylonstation = castleTerritory.CastleHeart.Read<Pylonstation>();
			var time = TimeSpan.FromMinutes(pylonstation.MinutesRemaining);
			castleInfo.AppendLine($"Castle {castleTerritory.CastleTerritoryIndex} in {region} with {time:%d}d {time:%h}h {time:%m}m remaining.");
		}
		if(!castleFound)
			castleInfo.AppendLine("No castle found");

		ctx.Reply($"Player Info for {name}\n" +
				  $"SteamID: {steamID}\n" +
				  $"Online: {online}\n" +
				  clanName +
				  $"Position: {posStr}\n"+
				  castleInfo.ToString());
	}

	[Command("idcheck", description: "searches for a player by steamid", adminOnly: true)]
	public static void SteamIdCheck(ChatCommandContext ctx, ulong steamid)
	{
		foreach(var userEntity in Helper.GetEntitiesByComponentType<ProjectM.Network.User>())
		{
			var user = userEntity.Read<User>();
			if(user.PlatformId == steamid)
			{
				ctx.Reply($"User found: {user.CharacterName}");
				return;
			}
		}
		
		ctx.Reply("No user found with that steamid");
	}


}
