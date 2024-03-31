using System.Collections.Generic;
using KindredCommands.Commands.Converters;
using KindredCommands.Models.Discord;
using KindredCommands.Services;
using ProjectM;
using VampireCommandFramework;
using static KindredCommands.Commands.BuffCommands;

namespace KindredCommands.Commands;
internal static class DurabilityCommands
{
	[CommandGroup("gear")]
	internal class GearCommands
	{
		[Command("repair", "r", description: "Repairs all gear.", adminOnly: false)]
		public static void RepairCommand(ChatCommandContext ctx, FoundPlayer player = null)

		{
			if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
			{
				var targetEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

				Helper.RepairGear(targetEntity);

				List<ContentHelper> content = new()
			{
				new ContentHelper
				{
					Title = "Comando",
					Content = "gear repair"
				},
			};

				if (player != null)
					content.Add(new ContentHelper
					{
						Title = "Player",
						Content = player?.Value.CharacterName.ToString()
					});

				DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);
				ctx.Reply($"Gear repaired for {targetEntity.Read<PlayerCharacter>().Name}.");
			}
		}

		[Command("break", "b", description: "Breaks all gear.", adminOnly: true)]
		public static void BreakGearCommand(ChatCommandContext ctx, FoundPlayer player = null)
		{
			if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
			{
				var targetEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
				Helper.RepairGear(targetEntity, false);

				List<ContentHelper> content = new()
			{
				new ContentHelper
				{
					Title = "Comando",
					Content = "gear break"
				},
			};

				if (player != null)
					content.Add(new ContentHelper
					{
						Title = "Player",
						Content = player?.Value.CharacterName.ToString()
					});

				DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);

				ctx.Reply($"Gear broken for {targetEntity.Read<PlayerCharacter>().Name}.");
			}
		}
	}
}
