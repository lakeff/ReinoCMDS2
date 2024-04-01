using System;
using System.Collections.Generic;
using KindredCommands.Commands.Converters;
using KindredCommands.Models;
using KindredCommands.Models.Discord;
using KindredCommands.Services;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using VampireCommandFramework;

namespace KindredCommands.Commands;
internal class BuffCommands
{
	public record struct BuffInput(string Name, PrefabGUID Prefab);

	public class BuffConverter : CommandArgumentConverter<BuffInput>
	{
		public override BuffInput Parse(ICommandContext ctx, string input)
		{
			if (Core.Prefabs.TryGetBuff(input, out PrefabGUID buffPrefab))
			{
				return new(buffPrefab.LookupName(), buffPrefab);
			}
			// "CHAR_Bandit_Bomber": -1128238456,
			if (int.TryParse(input, out var id) && Core.Prefabs.CollectionSystem.PrefabGuidToNameDictionary.TryGetValue(new PrefabGUID(id), out var name) &&
				name.ToLowerInvariant().Contains("buff"))
			{
				return new(name, new(id));
			}

			throw ctx.Error($"Can't find buff {input.Bold()}");
		}
	}

	[Command("buff", adminOnly: true)]
	public static void BuffCommand(ChatCommandContext ctx, BuffInput buff,OnlinePlayer player = null, int duration = 0, bool immortal = false)
	{
		var userEntity = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;

	
		if(Helper.VerifyAdminLevel(AdminLevel.Moderator, userEntity))
		{
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
			Buffs.AddBuff(userEntity, charEntity, buff.Prefab);

			List<ContentHelper> content = new()
			{
				new ContentHelper
				{
					Title = "Comando",
					Content = "modify"
				},
				new ContentHelper
				{
					Title = "Buff",
					Content = buff.Name.ToString()
				},
			};


			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);
			ctx.Reply($"Applied the buff {buff.Name} to {userEntity.Read<User>().CharacterName}");
		}
	}

	[Command("debuff", adminOnly: false)]
	public static void DebuffCommand(ChatCommandContext ctx, BuffInput buff, OnlinePlayer player = null)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var targetEntity = (Entity)(player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity);
			Buffs.RemoveBuff(targetEntity, buff.Prefab);
			ctx.Reply($"Removed the buff {buff.Name} from {targetEntity.Read<PlayerCharacter>().Name}");

				List<ContentHelper> content = new()
			{
				new ContentHelper
				{
					Title = "Comando",
					Content = "modify"
				},
				new ContentHelper
				{
					Title = "debuff",
					Content = buff.Name.ToString()
				},
			};


			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);
		}
	}

	[Command("listbuffs", description: "Lists the buffs a player has", adminOnly: false)]
	public static void ListBuffsCommand(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var Character = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
			var buffEntities = Helper.GetEntitiesByComponentTypes<Buff, PrefabGUID>();
			foreach (var buffEntity in buffEntities)
			{
				if (buffEntity.Read<EntityOwner>().Owner == Character)
				{
					ctx.Reply(buffEntity.Read<PrefabGUID>().LookupName());
				}
			}
		}
	}

	internal static void DebuffCommand(Entity character, PrefabGUID buff_InCombat_PvPVampire)
	{
		throw new NotImplementedException();
	}
}
