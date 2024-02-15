using KindredCommands.Commands;
using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;

namespace KindredCommands.Patches;

[HarmonyPatch(typeof(ProjectileSystem_Spawn_Server), nameof(ProjectileSystem_Spawn_Server.OnUpdate))]
public static class ProjectileSystem_Spawn_ServerPatch
{
	public static void Prefix(ProjectileSystem_Spawn_Server __instance)
	{
		try
		{
			EntityManager entityManager = __instance.EntityManager;
			NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

			foreach (var entity in entities)
			{
				PrefabGUID GUID = entity.Read<PrefabGUID>();
				Entity Character = entity.Read<EntityOwner>().Owner;
				if (!Character.Has<PlayerCharacter>()) continue;

				if (GodCommands.PlayerProjectileSpeeds.ContainsKey(Character) && GodCommands.PlayerProjectileSpeeds[Character] != 1f)
				{
					var projectile = entity.Read<Projectile>();
					projectile.Speed *= GodCommands.PlayerProjectileSpeeds[Character];
					entity.Write(projectile);
				}
				if (GodCommands.PlayerProjectileRanges.ContainsKey(Character) && GodCommands.PlayerProjectileRanges[Character] != 1f)
				{
					var projectile = entity.Read<Projectile>();
					projectile.Range *= GodCommands.PlayerProjectileRanges[Character];
					entity.Write(projectile);
				}
			}
		}
		catch
		{

		}
	}
}
