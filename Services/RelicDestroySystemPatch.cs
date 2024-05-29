using HarmonyLib;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;

namespace KindredCommands.Services;

[HarmonyPatch(typeof(RelicDestroySystem), nameof(RelicDestroySystem.OnUpdate))]
internal class RelicDestroySystemPatch
{
	static EntityQueryDesc relicDestroyQueryDesc = new EntityQueryDesc
	{
		All = new ComponentType[] { 
			ComponentType.ReadOnly(Il2CppType.Of<ItemData>()),
			ComponentType.ReadOnly(Il2CppType.Of<Relic>()),
			ComponentType.ReadOnly(Il2CppType.Of<DestroyTag>())
		},
	};
	static EntityQuery relicDestroyQuery;

	static bool queryInitialized = false;

	public static void Prefix(RelicDestroySystem __instance)
	{
		if (!queryInitialized)
		{
			relicDestroyQuery = Core.EntityManager.CreateEntityQuery(relicDestroyQueryDesc);
			queryInitialized = true;
		}

		var entities = relicDestroyQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			Core.SoulshardService.HandleSoulshardDestroy(entity);
		}
		entities.Dispose();
	}
}
