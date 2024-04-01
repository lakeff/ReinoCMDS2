using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KindredCommands.Commands.Converters;
using KindredCommands.Models;
using KindredCommands.Models.Discord;
using KindredCommands.Models.Enums;
using KindredCommands.Models.Vip;
using KindredCommands.Services;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using VampireCommandFramework;
using static KindredCommands.Commands.GiveItemCommands;
using static RootMotion.FinalIK.Grounding;

namespace KindredCommands.Commands;
internal class VipCommands
{
	[Command("kitvip", description: "Entrega o kit vip à um player.", adminOnly: true)]
	public static void DeliveryKitVip(ChatCommandContext ctx)
	{
		var userEntity = ctx.Event.SenderUserEntity;
		Player player = new(userEntity);
		var verifyVip = VipService.VerifyVipPermission(player.SteamID.ToString());

		if (verifyVip)
		{
			var vip = Database.GetVip().FirstOrDefault(x => x.Key.ToString() == player.SteamID.ToString());
			VipEnum vipLevel = Helper.GetVipEnum(vip.Value["Level"]);
			KitVip kit = new(vipLevel);

			foreach (var item in kit.ItemList)
			{
				Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, item.GUID, item.Quantity);
			};

			List<ContentHelper> content = new()
			{
				new ContentHelper
				{
						Title = "Comando",
						Content = "kitvip"
				},
				new ContentHelper
				{
					Title = "Vip",
					Content = vipLevel.ToString()
				},
			};

			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);

			ctx.Reply($"Itens vips entregues");
		}
		else
		{
			ctx.Reply("Você não tem nenhum cargo vip.");
		}

	}

	[Command("addvip", description: "Adiciona jogador ao vip", adminOnly: true)]

	public static void AddVipPlayer(ChatCommandContext ctx, FoundPlayer player = null, string vipLevel = "")
	{
		var userEntity = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;
		Player playerChar = new(userEntity);

		if (playerChar is Player)
		{
			VipEnum vipEnum = Helper.GetVipEnum(vipLevel);
			if (vipEnum == VipEnum.None)
			{
				ctx.Reply("Nível vip inválido");
			}
			else
			{

				Database.SetVip(userEntity, vipLevel, Helper.GetVipDate());
				var name = playerChar.Name;
				List<ContentHelper> content = new()
				{
					new ContentHelper
					{
						Title = "Comando",
						Content = "addvip"
					},
					new ContentHelper
					{
						Title = "Nível",
						Content = $"{vipLevel}"
					}

				};

				if (player != null)
				{
					content.Add(new ContentHelper
					{
						Title = "Nome",
						Content = $"{name}"
					});
				}

				DiscordService.SendWebhook(ctx.Event.SenderUserEntity.Read<User>().CharacterName, content);
				ctx.Reply($"Vip ${vipLevel} adicionado ao player {playerChar.Name}");
			}

		}
		else
		{
			ctx.Reply("Erro ao tentar adicionad Vip ao player");
		}


	}

	[Command("removevip", description: "Remove jogador ao vip", adminOnly: true)]

	public static void RemoveVipPlayer(ChatCommandContext ctx, FoundPlayer player = null)
	{
		var userEntity = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;
		Player playerChar = new(userEntity);

		if (playerChar is Player)
		{
			var name = playerChar.Name;
			var removed = Database.RemoveVip(playerChar.SteamID.ToString());
			if (removed)
			{
				List<ContentHelper> content = new()
				{
					new ContentHelper
					{
						Title = "Comando",
						Content = "removevip"
					},

				};

				if (player != null)
				{
					content.Add(new ContentHelper
					{
						Title = "Nome",
						Content = $"{name}"
					});
				}

				DiscordService.SendWebhook(ctx.Event.SenderUserEntity.Read<User>().CharacterName, content);
				ctx.Reply($"Vip removido do player {playerChar.Name}");
			}
			else
			{
				ctx.Reply($"Erro ao remover vip do player.");
			}

		}
		else
		{
			ctx.Reply("Jogador não encontrado");
		}
	}

	[Command("vipexpire", "vipexp", description:"Remove todos os vips expirados da lista de vip", adminOnly: true)]
	public static void VerifyVIPExpirationDate(ChatCommandContext ctx)
	{
		Dictionary<string, Dictionary<string, string>> vipList = Database.GetVip();
		var removed = VipService.VerifyVipExpireDate(vipList);
		if (removed)
		{
			ctx.Reply($"Removido os vips expirados");
		}
		else
		{
			ctx.Reply($"Não foi encontrado vips expirados.");
		}
	}
}
