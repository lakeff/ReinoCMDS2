using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using KindredCommands.Models;
using KindredCommands.Models.Discord;
using KindredCommands.Services;
using ProjectM;
using Unity.Transforms;
using VampireCommandFramework;
using static RootMotion.FinalIK.Grounding;

namespace KindredCommands.Commands;
internal class InfoCommands
{
	[Command("whereami", "wai", description: "Gives your current position", adminOnly: false)]
	public static void WhereAmI(ChatCommandContext ctx)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var pos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;

			List<ContentHelper> content = new()
				{
					new ContentHelper
					{
							Title = "Comando",
							Content = "whereami"
					},
				};

			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);

			ctx.Reply($"You are at {pos.x}, {pos.y}, {pos.z}");
		}
	}

	[Command("discord", "disc", description: "Envia o link do discord no chat", adminOnly: false)]

	public static void DiscordInvite(ChatCommandContext ctx)
	{
		string discord = Database.GetDiscord() ?? string.Empty;
		if (!string.IsNullOrEmpty(discord))
		{
			string splitString = discord.Split('/')[3];
			ctx.Reply($"{discord} | {splitString}");
		}
		else
		{
			ctx.Reply($"Informação não encontrada, contate a administração");
		}
	}

	[Command("informacoes", "info", description: "Descreve as informações do servidor.", adminOnly: false)]
	public static void GetInfo(ChatCommandContext ctx)
	{
		Dictionary<string, string> infos = Database.GetInfo();

		if (infos.Any())
		{
			foreach (var info in infos)
			{
				ctx.Reply($"{info.Key}: {info.Value}");
			}
		}
		else
		{
			ctx.Reply($"Informação não encontrada, contate a administração");
		}
	}

	[Command("setinfo", "sti", "<Nome da Informãção> [Valor da informação]", description: "Descreve as informações do servidor.", adminOnly: true)]
	public static void SetInfo(ChatCommandContext ctx, string name, string value)
	{

		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
		{
			ctx.Reply($"Digite o nome e o valor da informação");

		}
		else
		{
			Database.SetInfo(name, value);
			ctx.Reply($"Informação adicionada {name}: {value}");

		}
	}

	[Command("removeinfo", "rmvi", "<Nome da Informãção>", description: "Descreve as informações do servidor.", adminOnly: true)]
	public static void RemoveInfo(ChatCommandContext ctx, string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			ctx.Reply($"Informação não encontrada.");

		}
		else
		{
			Database.RemoveInfo(name);
			ctx.Reply($"Informação {name} removida!");

		}
	}
}
