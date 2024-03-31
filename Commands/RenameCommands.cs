using System.Text.RegularExpressions;
using KindredCommands.Commands.Converters;
using KindredCommands.Models.Discord;
using KindredCommands.Models;
using KindredCommands.Services;
using Unity.Collections;
using VampireCommandFramework;
using System.Collections.Generic;
using ProjectM;

namespace KindredCommands.Commands;

public static class RenameCommands
{
	[Command("rename", description: "Rename another player.", adminOnly: false)]
	public static void RenameOther(ChatCommandContext ctx, FoundPlayer player, NewName newName)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			List<ContentHelper> content = new()
		{
			new ContentHelper
			{
					Title = "Comando",
					Content = "rename"
			},
		};

			if (player != null)
			{
				content.Add(new ContentHelper
				{
					Title = "Player",
					Content = player?.Value.CharacterName.ToString()
				});
				content.Add(new ContentHelper
				{
					Title = "Novo nome",
					Content = newName.ToString()
				});

			}

			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);

			Core.Players.RenamePlayer(player.Value.UserEntity, player.Value.CharEntity, newName.Name);
			ctx.Reply($"Renamed {Format.B(player.Value.CharacterName.ToString())} -> {Format.B(newName.Name.ToString())}");
		}
	}

	[Command("rename", description: "Rename yourself.", adminOnly: false)]
	public static void RenameMe(ChatCommandContext ctx, NewName newName)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			Core.Players.RenamePlayer(ctx.Event.SenderUserEntity, ctx.Event.SenderCharacterEntity, newName.Name);

			List<ContentHelper> content = new()
		{
			new ContentHelper
			{
					Title = "Comando",
					Content = "rename"
			},
			new ContentHelper
			{
				Title = "Novo nome",
				Content = newName.ToString()
			}
		};

			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);
			ctx.Reply($"Your name has been updated to: {Format.B(newName.Name.ToString())}");
		}
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
}
