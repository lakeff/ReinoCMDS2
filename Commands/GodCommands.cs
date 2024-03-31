using System.Collections.Generic;
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
	public static void GodCommand(ChatCommandContext ctx, OnlinePlayer player = null, int invisible = 0)
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
			Buffs.AddBuff(userEntity, charEntity, Prefabs.CustomBuff, true);
			Buffs.AddBuff(userEntity, charEntity, Prefabs.EquipBuff_ShroudOfTheForest, true);

			if(invisible >=  1)
				Buffs.AddBuff(userEntity, charEntity, Prefabs.Admin_Observe_Invisible_Buff, true);

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

	[Command("heal", adminOnly: false)]
	public static void HealPlayer(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		if (Helper.VerifyAdminLevel(AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
			if (!GodPlayers.Contains(charEntity) && !BuffUtility.HasBuff(Core.EntityManager, charEntity, Prefabs.CustomBuff)) return;

			Health health = charEntity.Read<Health>();
			health.Value = health.MaxHealth;
			health.MaxRecoveryHealth = health.MaxHealth;

			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;

			List<ContentHelper> content = new()
			{
				new ContentHelper
				{
					Title = "Comando",
					Content = "heal"
				},
			};

			var reply = $"Jogador curado!";

			if (player != null)
			{
				reply = $"Jogador {name} curado!";
				content.Add(new ContentHelper
				{
					Title = "Player",
					Content = name.ToString()
				});
			}

			DiscordService.SendWebhook(ctx.Event.User.CharacterName, content);
			ctx.Reply(reply);
		}
		else
		{
			ctx.Reply($"Você não tem permissão para usar esse comando!");
		}
	}




	private static void MakePlayerImmaterial(Entity User, Entity Character)
	{
		Buffs.AddBuff(User, Character, Prefabs.AB_Blood_BloodRite_Immaterial, true);
		if (BuffUtility.TryGetBuff(Core.EntityManager, Character, Prefabs.AB_Blood_BloodRite_Immaterial, out Entity buffEntity))
		{
			var modifyMovementSpeedBuff = buffEntity.Read<ModifyMovementSpeedBuff>();
			modifyMovementSpeedBuff.MoveSpeed = 1; //bloodrite makes you accelerate forever, disable this
			buffEntity.Write(modifyMovementSpeedBuff);
		}
	}
}
