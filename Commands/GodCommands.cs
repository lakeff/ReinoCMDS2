using System;
using System.Collections.Generic;
using System.Xml.Schema;
using KindredCommands.Commands.Converters;
using KindredCommands.Data;
using KindredCommands.Models.Discord;
using KindredCommands.Services;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using VampireCommandFramework;
using static RootMotion.FinalIK.Grounding;

namespace KindredCommands.Commands;
internal class GodCommands
{
	public static Dictionary<Entity, float> PlayerSpeeds = [];
	public static Dictionary<Entity, int> PlayerHps = [];
	public static Dictionary<Entity, float> PlayerProjectileSpeeds = [];
	public static Dictionary<Entity, float> PlayerProjectileRanges = [];
	public static HashSet<Entity> GodPlayers = [];
	const int DEFAULT_FAST_SPEED = 15;


	[Command("god", adminOnly: true)]
	public static void GodCommand(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		var userEntity = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			PlayerSpeeds[charEntity] = DEFAULT_FAST_SPEED;
			PlayerProjectileSpeeds[charEntity] = 10f;
			PlayerProjectileRanges[charEntity] = 10f;
			MakePlayerImmaterial(userEntity, charEntity);
			GodPlayers.Add(charEntity);
			Buffs.AddBuff(userEntity, charEntity, Prefabs.CustomBuff, -1, true);
			Buffs.AddBuff(userEntity, charEntity, Prefabs.EquipBuff_ShroudOfTheForest, -1, true);


			// Heal back to full
			Health health = charEntity.Read<Health>();
			health.Value = health.MaxHealth;
			health.MaxRecoveryHealth = health.MaxHealth;
			charEntity.Write(health);

			// Remove PVP Buff
			Buffs.RemoveBuff(charEntity, Prefabs.Buff_InCombat_PvPVampire);

			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			List<ContentHelper> content = new()
				{
					new ContentHelper
					{
						Title = "Comando",
						Content = "god"
					}

				};

			DiscordService.SendWebhook(name, content);
			ctx.Reply($"God mode added to {name}");
		}
	}

	[Command("mortal", adminOnly: true)]
	public static void MortalCommand(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var charEntity = (player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity);

			if (!GodPlayers.Contains(charEntity) && !BuffUtility.HasBuff(Core.EntityManager, charEntity, Prefabs.CustomBuff)) return;

			PlayerSpeeds.Remove(charEntity);
			PlayerProjectileSpeeds.Remove(charEntity);
			PlayerProjectileRanges.Remove(charEntity);
			GodPlayers.Remove(charEntity);

			Helper.ClearExtraBuffs(charEntity);

			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;



			List<ContentHelper> content = new()
			{
				new ContentHelper
				{
					Title = "Comando",
					Content = "mortal"
				},
			};

			if (player != null)
				content.Add(new ContentHelper
				{
					Title = "Player",
					Content = player.Value.UserEntity.Read<User>().CharacterName.ToString()
				});

			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);
			ctx.Reply($"God mode removed from {name}");
		}
	}

	[Command("invisible","inv", description: "Torna o jogador alvo. invisível", adminOnly: false)]

	public static void TurnInvisible(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		if(Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
			var userEntity = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;

			if (!BuffUtility.HasBuff(Core.EntityManager, charEntity, Prefabs.Admin_Observe_Invisible_Buff))
			{
				Buffs.AddBuff(userEntity, charEntity, Prefabs.Admin_Observe_Invisible_Buff, -1, true);
				ctx.Reply($"{name} ficou invisível");

			} else
			{
				Buffs.RemoveBuff(charEntity, Prefabs.Admin_Observe_Invisible_Buff);
				ctx.Reply($"{name} não está mais invisível");
			}

		} else
		{
			ctx.Reply($"Você não tem acesso à esse comando");
		}
	}

	[Command("heal", description: "Cura a si mesmo ou um player alvo", adminOnly: false)]
	public static void HealPlayer(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Health health = charEntity.Read<Health>();
			health.Value = health.MaxHealth;
			health.MaxRecoveryHealth = health.MaxHealth;
			charEntity.Write(health);

			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;

			List<ContentHelper> content = new()
			{
				new ContentHelper
				{
					Title = "Comando",
					Content = "heal"
				},
			};

			if (player != null)
			{
				content.Add(new ContentHelper
				{
					Title = "Player",
					Content = name.ToString()
				});
			}

			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);
			ctx.Reply($"Jogador {name} curado!");
		}
		else
		{
			ctx.Reply($"Você não tem permiss�o para usar esse comando!");
		}
	}

	private static void MakePlayerImmaterial(Entity User, Entity Character)
	{
		Buffs.AddBuff(User, Character, Prefabs.AB_Blood_BloodRite_Immaterial, -1, true);
		if (BuffUtility.TryGetBuff(Core.EntityManager, Character, Prefabs.AB_Blood_BloodRite_Immaterial, out Entity buffEntity))
		{
			var modifyMovementSpeedBuff = buffEntity.Read<ModifyMovementSpeedBuff>();
			modifyMovementSpeedBuff.MoveSpeed = 1; //bloodrite makes you accelerate forever, disable this
			buffEntity.Write(modifyMovementSpeedBuff);
		}
	}
}
