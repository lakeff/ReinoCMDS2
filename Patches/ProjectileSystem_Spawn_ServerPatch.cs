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
				Entity charEntity = entity.Read<EntityOwner>().Owner;
				if (!charEntity.Has<PlayerCharacter>()) continue;

				if (Core.BoostedPlayerService.GetProjectileSpeedMultiplier(charEntity, out var speed))
				{
					var projectile = entity.Read<Projectile>();
					projectile.Speed *= speed;
					entity.Write(projectile);
				}
				if (Core.BoostedPlayerService.GetProjectileRangeMultiplier(charEntity, out var range))
				{
					var projectile = entity.Read<Projectile>();
					projectile.Range *= range;
					entity.Write(projectile);
				}
			}
		}
		catch
		{

		}
	}
}
