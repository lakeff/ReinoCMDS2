using System;
using System.Text.RegularExpressions;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using KindredCommands.Commands.Converters;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VampireCommandFramework;

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
}
