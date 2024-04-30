using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using KindredCommands.Commands.Converters;
using KindredCommands.Models;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;
using static KindredCommands.Commands.PlayerCommands;

namespace KindredCommands.Commands;

public static class PlayerCommands
{
	[Command("rename", description: "Rename another player.", adminOnly: true)]
	public static void RenameOther(ChatCommandContext ctx, FoundPlayer player, NewName newName)
	{
		Core.Players.RenamePlayer(player.Value.UserEntity, player.Value.CharEntity, newName.Name);
		ctx.Reply($"Renamed {Format.B(player.Value.CharacterName.ToString())} -> {Format.B(newName.Name.ToString())}");
	}

	[Command("rename", description: "Rename yourself.", adminOnly: true)]
	public static void RenameMe(ChatCommandContext ctx, NewName newName)
	{
		Core.Players.RenamePlayer(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, newName.Name);
		ctx.Reply($"Your name has been updated to: {Format.B(newName.Name.ToString())}");
	}

	public record struct NewName(FixedString64 Name);

	public class NewNameConverter : CommandArgumentConverter<NewName>
	{
		public override NewName Parse(ICommandContext ctx, string input)
		{
			if (!IsAlphaNumeric(input))
			{
				throw ctx.Error("Name must be alphanumeric.");
			}
			var newName = new NewName(input);
			if (newName.Name.utf8LengthInBytes > 20)
			{
				throw ctx.Error("Name too long.");
			}

			return newName;
		}
		public static bool IsAlphaNumeric(string input)
		{
			return Regex.IsMatch(input, @"^[a-zA-Z0-9\[\]]+$");
		}
	}

	[Command("unbindplayer", description: "Unbinds a SteamID from a player's save.", adminOnly: true)]
	public static void UnbindPlayer(ChatCommandContext ctx, FoundPlayer player)
	{
		var userEntity = player.Value.UserEntity;
		var user = userEntity.Read<User>();
		ctx.Reply($"Unbound the player {user.CharacterName}");

		Helper.KickPlayer(userEntity);

		user = userEntity.Read<User>();
		user.PlatformId = 0;
		userEntity.Write(user);

		Core.StealthAdminService.RemoveStealthAdmin(userEntity);
	}

	[Command("swapplayers", description: "Switches the steamIDs of two players.", adminOnly: true)]
	public static void SwapPlayers(ChatCommandContext ctx, FoundPlayer player1, FoundPlayer player2)
	{
		var userEntity1 = player1.Value.UserEntity;
		var userEntity2 = player2.Value.UserEntity;
		var user1 = userEntity1.Read<User>();
		var user2 = userEntity2.Read<User>();

		Helper.KickPlayer(userEntity1);
		Helper.KickPlayer(userEntity2);

		ctx.Reply($"Swapped {user1.CharacterName} with {user2.CharacterName}");

		user1 = userEntity1.Read<User>();
		user2 = userEntity2.Read<User>();
		(user1.PlatformId, user2.PlatformId) = (user2.PlatformId, user1.PlatformId);
		userEntity1.Write(user1);
		userEntity2.Write(user2);

		Core.StealthAdminService.RemoveStealthAdmin(userEntity1);
		Core.StealthAdminService.RemoveStealthAdmin(userEntity2);
	}

	[Command("unlock", description: "Unlocks a player's skills, jouirnal, etc.", adminOnly: true)]
	public static void UnlockPlayer(ChatCommandContext ctx, FoundPlayer player)
	{
		var User = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;
		var Character = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

		try
		{
			var debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();
			var fromCharacter = new FromCharacter()
			{
				User = User,
				Character = Character
			};

			UnlockPlayer(fromCharacter);
			ctx.Reply($"Unlocked everything for {player?.Value.CharacterName ?? "you"}.");
		}
		catch (Exception e)
		{
			throw ctx.Error(e.ToString());
		}
	}

	public static DebugEventsSystem debugEventsSystem = VWorld.Server.GetExistingSystem<DebugEventsSystem>();

	public static void UnlockPlayer(FromCharacter fromCharacter)
	{
		debugEventsSystem.UnlockAllResearch(fromCharacter);
		debugEventsSystem.UnlockAllVBloods(fromCharacter);
		debugEventsSystem.CompleteAllAchievements(fromCharacter);
		Helper.UnlockWaypoints(fromCharacter.User);
		Helper.RevealMapForPlayer(fromCharacter.User);
	}

	[Command("revealmap", description: "Reveal the map for a player.", adminOnly: true)]
	public static void RevealMap(ChatCommandContext ctx, FoundPlayer player = null)
	{
		var userEntity = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;
		Helper.RevealMapForPlayer(userEntity);
		if(player != null)
			ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, userEntity.Read<User>(), "Your map has been revealed, you must relog to see.");
		ctx.Reply($"Map has been revealed, {player?.Value.CharacterName ?? "you"} must relog to see.");
	}

	[Command("revealmapforallplayers", description: "Reveal the map for all players.", adminOnly: true)]
	public static void RevealMapForAllPlayers(ChatCommandContext ctx)
	{
		if(Core.ConfigSettings.RevealMapToAll)
		{
			ctx.Reply("Map is already revealed for all players.");
			return;
		}

		ctx.Reply("Revealing map to all players. Current logged in players will require a relog to see it.");
		Core.ConfigSettings.RevealMapToAll = true;
		var userEntities = Helper.GetEntitiesByComponentType<User>();
		foreach (var userEntity in userEntities)
		{
			Helper.RevealMapForPlayer(userEntity);
		}
	}

	[Command ("fly" , description: "Toggle fly mode for a player.", adminOnly: true)]
	public static void Fly(ChatCommandContext ctx, FoundPlayer player = null)
	{
		var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
		var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;

		if (Core.BoostedPlayerService.ToggleFlying(charEntity))
		{
			ctx.Reply($"Flying added to {name}");
		}
		else
		{
			ctx.Reply($"Flying removed from {name}");
		}
		Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
	}

	[Command("flyup", "f^", description: "Set fly height up a floor", adminOnly: true)]
    public static void Up(ChatCommandContext ctx, FoundPlayer player = null)
    {
        var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
        var canFly = charEntity.Read<CanFly>();
        var currentHeight = canFly.FlyingHeight.Value;
        var newHeight = currentHeight + 5;
        canFly.FlyingHeight.Value = newHeight;
		canFly.HeightAboveObstacle.Value = 0;
		charEntity.Write(canFly);

        var floorLevel = newHeight / 5;
        var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;

        ctx.Reply($"Moved {name} up to floor level {floorLevel}");
    }

	[Command("flydown", "fv", description: "Set fly height down a floor", adminOnly: true)]
	public static void Down(ChatCommandContext ctx, FoundPlayer player = null)
	{
		var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
		var canFly = charEntity.Read<CanFly>();
		var currentHeight = canFly.FlyingHeight.Value;
		var newHeight = currentHeight - 5;
		canFly.FlyingHeight.Value = newHeight;
		canFly.HeightAboveObstacle.Value = 0;
		charEntity.Write(canFly);

		var floorLevel = newHeight / 5;
		var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;

		ctx.Reply($"Moved {name} down to floor level {floorLevel}");
	}

	[Command("level", description: "Set fly height to a specific level", adminOnly: true)]
	public static void Floor(ChatCommandContext ctx, int floor, FoundPlayer player = null)
	{
		var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
		var canFly = charEntity.Read<CanFly>();
		var newHeight = floor * 5;
		canFly.FlyingHeight.Value = newHeight;
		canFly.HeightAboveObstacle.Value = 0;
		charEntity.Write(canFly);

		var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;

		ctx.Reply($"Adjusted {name}'s fly height to level {floor}");
	}

	[Command("flyheight", description: "Sets the fly height for the user", adminOnly: true)]
	public static void SetFlyHeight(ChatCommandContext ctx, float height = 30)
	{
		var charEntity = ctx.Event.SenderCharacterEntity;
		var canFly = charEntity.Read<CanFly>();
		canFly.FlyingHeight.Value = height;
		charEntity.Write(canFly);
		ctx.Reply($"Set fly height to {height}");
	}

	[Command("flyobstacleheight", description: "Set the height to fly above any obstacles", adminOnly: true)]
	public static void SetFlyObstacleHeight(ChatCommandContext ctx, float height = 7)
	{
		var charEntity = ctx.Event.SenderCharacterEntity;
		var canFly = charEntity.Read<CanFly>();
		canFly.HeightAboveObstacle.Value = height;
		charEntity.Write(canFly);
		ctx.Reply($"Set fly obstacle height to {height}");
	}
	/* // This was made for a specific server who wanted to wipe players and castles but keep certain ones to clone the map.
	static Entity UserDoingWipe;
	static Entity[] castleHeartsToNotWipe;
	static Entity[] usersToNotWipe;

	[Command("wipe", description: "Wipe's a server except excluded territoryIds and their owners.", adminOnly: true)]
	public static void WipeServer(ChatCommandContext ctx, IntArray territoryIds=null)
	{
		// Check if they are allowed to wipe
		if (!Database.CanWipe(ctx.Event.SenderUserEntity))
		{
			ctx.Reply("You are not allowed to wipe the server.");
			return;
		}

		if(UserDoingWipe != Entity.Null)
		{
			ctx.Reply($"A wipe is already in progress by {UserDoingWipe.Read<User>().CharacterName}");
			return;
		}

		var castleHeartList = new List<Entity>();
		var userList = new List<Entity>();
		var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
		foreach (var heartEntity in castleHearts)
		{
			var castleHeart = heartEntity.Read<CastleHeart>();
			if (castleHeart.CastleTerritoryEntity.Equals(Entity.Null))
				continue;

			var castleTerritoryIndex = castleHeart.CastleTerritoryEntity.Read<CastleTerritory>().CastleTerritoryIndex;
			if (territoryIds!=null && territoryIds.Value.Contains(castleTerritoryIndex))
			{
				castleHeartList.Add(heartEntity);

				var userOwner = heartEntity.Read<UserOwner>();
				var userEntity = userOwner.Owner.GetEntityOnServer();
				userList.Add(userEntity);
				ctx.Reply($"{userEntity.Read<User>().CharacterName} is excluded from the wipe via territoryId {castleTerritoryIndex}");
			}
		}
		castleHearts.Dispose();

		UserDoingWipe = ctx.Event.SenderUserEntity;
		castleHeartsToNotWipe = castleHeartList.ToArray();
		usersToNotWipe = userList.ToArray();
		ctx.Reply("Use the command .commencewipe to actually perform the wipe if you wish to continue or .cancelwipe to disengage the wipe.");
		ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, $"{ctx.User.CharacterName} has started to initiate a wipe");
	}

	[Command("commencewipe", description: "Actually performs the wipe", adminOnly: true)]
	public static void CommenceWipe(ChatCommandContext ctx)
	{
		if(ctx.Event.SenderUserEntity != UserDoingWipe)
		{
			if(UserDoingWipe == Entity.Null)
			{
				ctx.Reply("There is no wipe in progress.");
				return;
			}
			ctx.Reply($"You are not the user who initiated the wipe. It was initiated by {UserDoingWipe.Read<User>().CharacterName}");
			return;
		}

		/*var heartConnections = Helper.GetEntitiesByComponentType<CastleHeartConnection>();
		foreach (var connectionEntity in heartConnections)
		{
			if(connectionEntity.Has<CastleHeart>())
				continue;

			var heartConnection = connectionEntity.Read<CastleHeartConnection>();
			var heart = heartConnection.CastleHeartEntity.GetEntityOnServer();
			if (heart.Equals(Entity.Null))
				continue;
			if(castleHeartsToNotWipe.Where(x => x.Equals(heart)).Any())
				continue;

			if (connectionEntity.Has<SpawnChainChild>())
				connectionEntity.Remove<SpawnChainChild>();

			if (connectionEntity.Has<DropTableBuffer>())
				connectionEntity.Remove<DropTableBuffer>();

			if (connectionEntity.Has<InventoryBuffer>())
				Core.EntityManager.GetBuffer<InventoryBuffer>(connectionEntity).Clear();

			DestroyUtility.Destroy(Core.EntityManager, connectionEntity);
		}
		heartConnections.Dispose();*/
	/*
		var pss = Core.Server.GetExistingSystem<PylonstationSystem>();
		var serverTime = Core.CastleBuffsTickSystem._ServerTime.GetSingleton();
		var bufferSystem = Core.Server.GetExistingSystem<EntityCommandBufferSystem>();
		var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
		foreach (var heartEntity in castleHearts)
		{
			if(castleHeartsToNotWipe.Where(x => x.Equals(heartEntity)).Any())
				continue;

			var userEntity = heartEntity.Read<UserOwner>().Owner.GetEntityOnServer();
			var fromCharacter = new FromCharacter()
			{
				User = userEntity,
				Character = userEntity.Read<User>().LocalCharacter.GetEntityOnServer()
			};

			var pylonstation = heartEntity.Read<Pylonstation>();
			var buffer = bufferSystem.CreateCommandBuffer();
			pss.DestroyCastle(heartEntity, ref pylonstation, ref fromCharacter, ref serverTime, ref buffer);
		}
		castleHearts.Dispose();

		List<Entity> clansToIgnore = new();
		var userEntities = Helper.GetEntitiesByComponentType<User>();
		foreach(var userEntity in userEntities)
		{
			if (usersToNotWipe.Where(x => x.Equals(userEntity)).Any())
			{
				clansToIgnore.Add(userEntity.Read<User>().ClanEntity.GetEntityOnServer());
				continue;
			}
			Helper.KickPlayer(userEntity);
			var user = userEntity.Read<User>();
			user.PlatformId = 0;
			userEntity.Write(user);

			var charEntity = user.LocalCharacter.GetEntityOnServer();
			if(charEntity.Equals(Entity.Null))
				continue;

			Core.Players.RenamePlayer(userEntity, charEntity, "");

			charEntity.Write(new Translation() { Value = new float3(-818f, 10f, -1989f) });
			charEntity.Write(new LastTranslation() { Value = new float3(-818f, 10f, -1989f) });

			StatChangeUtility.KillEntity(Core.EntityManager, charEntity, ctx.Event.SenderCharacterEntity, 0, true);
		}
		userEntities.Dispose();

		var playerDeathContainers = Helper.GetEntitiesByComponentType<PlayerDeathContainer>();
		foreach (var deathContainer in playerDeathContainers)
		{
			Core.EntityManager.GetBuffer<InventoryBuffer>(deathContainer).Clear();
			DestroyUtility.Destroy(Core.EntityManager, deathContainer);
		}
		playerDeathContainers.Dispose();

		var clanTeams = Helper.GetEntitiesByComponentType<ClanTeam>();
		foreach (var clanEntity in clanTeams)
		{
			if (clansToIgnore.Where(x => x.Equals(clanEntity)).Any())
				continue;

			var clanTeam = clanEntity.Read<ClanTeam>();
			clanTeam.Name = "";
			clanTeam.Motto = "";
			clanEntity.Write(clanTeam);
		}
		clanTeams.Dispose();

		var st = Core.EntityManager.CreateEntity(new ComponentType[1] { ComponentType.ReadOnly<SetTimeOfDayEvent>() });
		st.Write(new SetTimeOfDayEvent()
		{
			Day = 10,
			Hour = 0,
			Type = SetTimeOfDayEvent.SetTimeType.Add
		});
		

		UserDoingWipe = Entity.Null;
		ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, $"{ctx.User.CharacterName} has wiped the server");
		Core.Log.LogInfo("Server has been wiped.");
	}

	[Command("cancelwipe", description: "Cancels the wipe", adminOnly: true)]
	public static void CancelWipe(ChatCommandContext ctx)
	{
		if (UserDoingWipe == Entity.Null)
		{
			ctx.Reply("There is no wipe in progress.");
			return;
		}

		UserDoingWipe = Entity.Null;
		castleHeartsToNotWipe = null;
		usersToNotWipe = null;
		ctx.Reply("Wipe has been cancelled.");
		ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, $"{ctx.User.CharacterName} has canceled the wipe");
	}
	*/
}
