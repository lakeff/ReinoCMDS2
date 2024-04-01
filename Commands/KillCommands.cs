using System.Collections.Generic;
using KindredCommands.Commands.Converters;
using KindredCommands.Models.Discord;
using KindredCommands.Services;
using ProjectM;
using VampireCommandFramework;
namespace KindredCommands.Commands;
public class KillCommands
{
	[Command("kill", adminOnly: false)]
	public static void KillCommand(ChatCommandContext ctx, FoundPlayer player = null)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			if (player is not null)
			{
				if (Helper.VerifyAdminLevel(AdminLevel.SuperAdmin, player.Value.CharEntity) || Helper.VerifyAdminLevel(AdminLevel.Admin, player.Value.CharEntity))
				{
					StatChangeUtility.KillEntity(Core.EntityManager, ctx.Event.SenderCharacterEntity,ctx.Event.SenderCharacterEntity, 0, true);
					ctx.Reply($"Otário");
					return;
				}
			}

			StatChangeUtility.KillEntity(Core.EntityManager, player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity,
			ctx.Event.SenderCharacterEntity, 0, true);

			List<ContentHelper> content = new()
			{
				new ContentHelper
				{
						Title = "Comando",
						Content = "kill"
				},
			};

			if (player != null)
				content.Add(new ContentHelper
				{
					Title = "Player",
					Content = player?.Value.CharacterName.ToString()
				});

			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);

			ctx.Reply($"Killed {player?.Value.CharacterName ?? "you"}.");
		}
	}
}

