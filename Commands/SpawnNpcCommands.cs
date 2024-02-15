using System.Linq;
using KindredCommands.Data;
using KindredCommands.Models;
using ProjectM;
using UnitKiller;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;

namespace KindredCommands.Commands;

internal static class SpawnCommands
{
	public record struct CharacterUnit(string Name, PrefabGUID Prefab);

	public class CharacterUnitConverter : CommandArgumentConverter<CharacterUnit>
	{
		public override CharacterUnit Parse(ICommandContext ctx, string input)
		{
			if (Character.Named.TryGetValue(input, out var unit) || Character.Named.TryGetValue("CHAR_" + input, out unit))
			{
				return new(Character.NameFromPrefab[unit.GuidHash], unit);
			}
			// "CHAR_Bandit_Bomber": -1128238456,
			if (int.TryParse(input, out var id) && Character.NameFromPrefab.TryGetValue(id, out var name))
			{
				return new(name, new(id));
			}

			throw ctx.Error($"Can't find unit {input.Bold()}");
		}
	}

	[Command("spawnnpc", "spwn", description: "Spawns CHAR_ npcs", adminOnly: true)]
	public static void SpawnNpc(ChatCommandContext ctx, CharacterUnit character, int count = 1)
	{
		if (Database.IsSpawnBanned(character.Name, out var reason))
		{
			throw ctx.Error($"Cannot spawn {character.Name.Bold()} because it is banned. Reason: {reason}");
		}

		var pos = Core.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;

		Services.UnitSpawnerService.Spawn(ctx.Event.SenderUserEntity, character.Prefab, count, new(pos.x, pos.z), 1, 2, -1);
		ctx.Reply($"Spawning {count} {character.Name.Bold()} at your position");
	}

	[Command("customspawn", "cspwn", "customspawn <Prefab Name> [<BloodType> <BloodQuality> <Consumable(\"true/false\")> <Duration> <level>]", "Spawns a modified NPC at your current position.", adminOnly: true)]
	public static void CustomSpawnNpc(ChatCommandContext ctx, CharacterUnit unit, BloodType type = BloodType.Frailed, int quality = 0, bool consumable = true, int duration = -1, int level = 0)
	{
		if (Database.IsSpawnBanned(unit.Name, out var reason))
		{
			throw ctx.Error($"Cannot spawn {unit.Name.Bold()} because it is banned. Reason: {reason}");
		}

		var pos = Core.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;

		if (quality > 100 || quality < 0)
		{
			throw ctx.Error($"Blood Quality must be between 0 and 100");
		}

		Core.UnitSpawner.SpawnWithCallback(ctx.Event.SenderUserEntity, unit.Prefab, pos.xz, duration, (Entity e) =>
		{
			var blood = Core.EntityManager.GetComponentData<BloodConsumeSource>(e);
			blood.UnitBloodType = new PrefabGUID((int)type);
			blood.BloodQuality = quality;
			blood.CanBeConsumed = consumable;

			var unitLevel = Core.EntityManager.GetComponentData<UnitLevel>(e);
			unitLevel.Level = level;

			Core.EntityManager.SetComponentData(e, unitLevel);
			Core.EntityManager.SetComponentData(e, blood);
		});
		ctx.Reply($"Spawning {unit.Name.Bold()} with {quality}% {type} blood at your position. It is Lvl{level} and will live {(duration<0?"until killed":$"{duration} seconds")}.");
	}


	[Command("despawnnpc", "dspwn", description: "Despawns CHAR_ npcs", adminOnly: true)]
	public static void DespawnNpc(ChatCommandContext ctx, CharacterUnit character, float radius = 25f)
	{
		var mobs = MobUtility.ClosestMobs(ctx, radius, character.Prefab);
		mobs.ForEach(e => StatChangeUtility.KillEntity(Core.EntityManager, e,
						ctx.Event.SenderCharacterEntity, Time.time, true));
		ctx.Reply($"You've killed {mobs.Count} {character.Name.Bold()} at your position. You murderer!");
	}


	[Command("spawnhorse", "sh", description: "Spawns a horse", adminOnly: true)] 
	public static void SpawnHorse(ChatCommandContext ctx, float speed, float acceleration, float rotation, bool spectral=false, int num=1)
	{
		var pos = Core.EntityManager.GetComponentData<LocalToWorld>(ctx.Event.SenderCharacterEntity).Position;
		var horsePrefab = spectral ? Prefabs.CHAR_Mount_Horse_Spectral : Prefabs.CHAR_Mount_Horse;

		for (int i = 0; i < num; i++)
		{
			Core.UnitSpawner.SpawnWithCallback(ctx.Event.SenderUserEntity, horsePrefab, pos.xz, -1, (Entity horse) =>
			{
				var mount = horse.Read<Mountable>();
				mount.MaxSpeed = speed;
				mount.Acceleration = acceleration;
				mount.RotationSpeed = rotation * 10f;
				horse.Write<Mountable>(mount);
			});
		}

		ctx.Reply($"Spawned {num}{(spectral == false ? "" : " spectral")} horse{(num > 1 ? "s" : "")} (with speed:{speed}, accel:{acceleration}, and rotate:{rotation}) near you.");
	}


	[Command("spawnban", description: "Shows which GUIDs are banned and why.", adminOnly: true)]
	public static void SpawnBan(ChatCommandContext ctx, CharacterUnit character, string reason)
	{
		Database.SetNoSpawn(character.Name, reason);
		ctx.Reply($"Banned '{character.Name}' from spawning with reason '{reason}'");
	}

	readonly static float3 banishLocation = new(-218.4665f, 15, -556.69354f);

    [Command("banishhorse", "bh", description: "Banishes dominated ghost horses on the server out of bounds", adminOnly: true)]
	public static void BanishGhost(ChatCommandContext ctx)
	{
		var horses = Helper.GetEntitiesByComponentTypes<Immortal, Mountable>(true).ToArray()
                        .Where(x => x.Read<PrefabGUID>().GuidHash == Prefabs.CHAR_Mount_Horse_Vampire.GuidHash)
                        .Where(x => BuffUtility.HasBuff(Core.EntityManager, x, Prefabs.Buff_General_VampireMount_Dead));

		var horsesToBanish = horses.Where(x => Vector3.Distance(banishLocation, x.Read<LocalToWorld>().Position) > 30f);

        if (horsesToBanish.Any())
		{
			foreach (var horse in horsesToBanish)
			{
				Core.EntityManager.SetComponentData(horse, new LastTranslation { Value = banishLocation });
				Core.EntityManager.SetComponentData(horse, new Translation { Value = banishLocation });
			}
			ctx.Reply($"Banished {horsesToBanish.Count()} ghost horse{(horsesToBanish.Count() > 1 ? "s" : "")}");
		}
		else
		{
			ctx.Reply($"No valid ghost horses found to banish but {horses.Count()} already banished");
		}
    }
}
