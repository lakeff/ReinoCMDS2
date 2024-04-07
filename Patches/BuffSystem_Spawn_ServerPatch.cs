using System.Collections.Generic;
using KindredCommands.Commands;
using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity;

namespace KindredCommands.Patches;

[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
public static class BuffSystem_Spawn_ServerPatch
{

	public static void Prefix(BuffSystem_Spawn_Server __instance)
	{
		EntityManager entityManager = __instance.EntityManager;
		NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

		foreach (var buffEntity in entities)
		{
			PrefabGUID GUID = buffEntity.Read<PrefabGUID>();
			Entity owner = buffEntity.Read<EntityOwner>().Owner;
			if (!owner.Has<PlayerCharacter>()) continue;
			if (!Core.BoostedPlayerService.IsBoostedPlayer(owner)) continue;
			if (GUID == Data.Prefabs.CustomBuff)
			{
				Core.BoostedPlayerService.UpdateCustomBuff(buffEntity);
			}
			else if (GUID == Data.Prefabs.Buff_InCombat_PvPVampire && Core.BoostedPlayerService.IsPlayerInvincible(owner))
			{
				if (BuffUtility.TryGetBuff(Core.EntityManager, owner, Data.Prefabs.Buff_InCombat_PvPVampire, out var combatBuffEntity))
				{
					DestroyUtility.Destroy(Core.EntityManager, combatBuffEntity, DestroyDebugReason.TryRemoveBuff);
				}
			}
		}
	}
}
