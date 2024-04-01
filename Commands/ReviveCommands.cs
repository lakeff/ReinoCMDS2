using System.Collections.Generic;
using KindredCommands.Commands.Converters;
using KindredCommands.Models.Discord;
using KindredCommands.Services;
using ProjectM;
using ProjectM.Network;
using VampireCommandFramework;

namespace KindredCommands.Commands;

internal class ReviveCommands
{
	[Command("revive", adminOnly: false)]
	public static void ReviveCommand(ChatCommandContext ctx, FoundPlayer player = null)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var character = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
			var user = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;

			Helper.ReviveCharacter(character, user);

			List<ContentHelper> content = new()
		{
			new ContentHelper
			{
					Title = "Comando",
					Content = "revive"
			}
		};

			if (player != null)
			{
				content.Add(new ContentHelper
				{
					Title = "Novo nome",
					Content = player.Value.UserEntity.Read<User>().CharacterName.ToString()
				});
			}


			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);

			ctx.Reply($"Revived {user.Read<User>().CharacterName}");
		}
	}
}
