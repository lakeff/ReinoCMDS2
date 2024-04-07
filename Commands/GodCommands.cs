using System.Collections.Generic;
using KindredCommands.Commands.Converters;
using KindredCommands.Data;
using ProjectM;
using ProjectM.Network;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;

namespace KindredCommands.Commands;
internal class GodCommands
{
	const int DEFAULT_FAST_SPEED = 15;

	[Command("god", adminOnly: true)]
	public static void GodCommand(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		var userEntity = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;
		var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

		Core.BoostedPlayerService.SetAttackSpeedMultiplier(charEntity, 10f);
		Core.BoostedPlayerService.SetDamageBoost(charEntity, 1000f);
		Core.BoostedPlayerService.SetHealthBoost(charEntity, 100000);
		Core.BoostedPlayerService.SetProjectileSpeedMultiplier(charEntity, 10f);
		Core.BoostedPlayerService.SetProjectileRangeMultiplier(charEntity, 10f);
		Core.BoostedPlayerService.SetSpeedBoost(charEntity, DEFAULT_FAST_SPEED);
		Core.BoostedPlayerService.SetYieldMultiplier(charEntity, 10f);
		Core.BoostedPlayerService.AddNoAggro(charEntity);
		Core.BoostedPlayerService.AddNoBlooddrain(charEntity);
		Core.BoostedPlayerService.AddNoCooldown(charEntity);
		Core.BoostedPlayerService.AddNoDurability(charEntity);
		Core.BoostedPlayerService.AddPlayerImmaterial(charEntity);
		Core.BoostedPlayerService.AddPlayerInvincible(charEntity);
		Core.BoostedPlayerService.AddPlayerShrouded(charEntity);
		Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);

		var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
		ctx.Reply($"God mode added to {name}");
	}

	[Command("mortal", adminOnly: true)]
	public static void MortalCommand(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		var charEntity = (player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity);

		if (!Core.BoostedPlayerService.IsBoostedPlayer(charEntity) && !BuffUtility.HasBuff(Core.EntityManager, charEntity, Prefabs.CustomBuff)) return;

		Core.BoostedPlayerService.RemoveBoostedPlayer(charEntity);

		var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
		ctx.Reply($"God mode removed from {name}");
	}

	static Dictionary<string, Vector3> positionBeforeSpectate = [];

	[Command("spectate", adminOnly: true, description:"Toggles spectate on the target player")]
	public static void SpectateCommand(ChatCommandContext ctx, OnlinePlayer player = null, bool returnToStart=true)
	{
		var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
		var userEntity = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;
		var name = userEntity.Read<User>().CharacterName;

		if (BuffUtility.HasBuff(Core.EntityManager, charEntity, Prefabs.Admin_Observe_Invisible_Buff))
		{
			if (returnToStart)
			{
				if (!positionBeforeSpectate.TryGetValue(name.ToString(), out var returnPos))
					returnPos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value;
				charEntity.Write<Translation>(new Translation { Value = returnPos });
				charEntity.Write<LastTranslation>(new LastTranslation { Value = returnPos });
			}
			positionBeforeSpectate.Remove(name.ToString());
			Buffs.RemoveBuff(charEntity, Prefabs.Admin_Observe_Invisible_Buff);
			ctx.Reply($"Spectate removed from {name}");
		}
		else
		{

			Buffs.AddBuff(userEntity, charEntity, Prefabs.Admin_Observe_Invisible_Buff, -1);
			positionBeforeSpectate.Add(name.ToString(), charEntity.Read<Translation>().Value);
			ctx.Reply($"Spectate added to {name}");
		}
	}


	[CommandGroup("boost", "bst")]
	internal class BoostedCommands
	{

		[Command("attackspeed", "as", adminOnly: true)]
		public static void AttackSpeed(ChatCommandContext ctx, float speed = 10, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.SetAttackSpeedMultiplier(charEntity, speed);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Attack speed on {name} set to {speed}");
		}

		[Command("damage", "d", adminOnly: true)]
		public static void Damage(ChatCommandContext ctx, float damage = 1000, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.SetDamageBoost(charEntity, damage);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Damage boost on {name} set to {damage}");
		}

		[Command("health", "h", adminOnly: true)]
		public static void Health(ChatCommandContext ctx, int health = 100000, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.SetHealthBoost(charEntity, health);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Health boost on {name} set to {health}");
		}

		[Command("projectilespeed", "ps", adminOnly: true)]
		public static void ProjectileSpeed(ChatCommandContext ctx, float speed = 10, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.SetProjectileSpeedMultiplier(charEntity, speed);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Projectile speed on {name} set to {speed}");
		}

		[Command("projectilerange", "pr", adminOnly: true)]
		public static void ProjectileRange(ChatCommandContext ctx, float range = 10, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.SetProjectileRangeMultiplier(charEntity, range);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Projectile range on {name} set to {range}");
		}

		[Command("speed", "s", adminOnly: true)]
		public static void Speed(ChatCommandContext ctx, int speed = DEFAULT_FAST_SPEED, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.SetSpeedBoost(charEntity, speed);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Speed on {name} set to {speed}");
		}

		[Command("yield", "y", adminOnly: true)]
		public static void Yield(ChatCommandContext ctx, float yield = 10, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.SetYieldMultiplier(charEntity, yield);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Yield on {name} set to {yield}");
		}

		[Command("noaggro", "na", adminOnly: true)]
		public static void NoAggro(ChatCommandContext ctx, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.AddNoAggro(charEntity);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"No aggro added to {name}");
		}

		[Command("noblooddrain", "nb", adminOnly: true)]
		public static void NoBlooddrain(ChatCommandContext ctx, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.AddNoBlooddrain(charEntity);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"No blooddrain added to {name}");
		}

		[Command("nocooldown", "nc", adminOnly: true)]
		public static void NoCooldown(ChatCommandContext ctx, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.AddNoCooldown(charEntity);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"No cooldown added to {name}");
		}

		[Command("nodurability", "nd", adminOnly: true)]
		public static void NoDurability(ChatCommandContext ctx, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.AddNoDurability(charEntity);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"No durability loss added to {name}");
		}

		[Command("immaterial", "i", adminOnly: true)]
		public static void Immaterial(ChatCommandContext ctx, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.AddPlayerImmaterial(charEntity);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Immaterial added to {name}");
		}

		[Command("invincible", "inv", adminOnly: true)]
		public static void Invincible(ChatCommandContext ctx, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.AddPlayerInvincible(charEntity);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Invincibility added to {name}");
		}

		[Command("shrouded", "sh", adminOnly: true)]
		public static void Shrouded(ChatCommandContext ctx, OnlinePlayer player = null)
		{
			var name = player?.Value.UserEntity.Read<User>().CharacterName ?? ctx.Event.User.CharacterName;
			var charEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Core.BoostedPlayerService.AddPlayerShrouded(charEntity);
			Core.BoostedPlayerService.UpdateBoostedPlayer(charEntity);
			ctx.Reply($"Shrouded added to {name}");
		}
	}
}
