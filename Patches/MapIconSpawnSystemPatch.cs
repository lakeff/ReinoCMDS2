using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;

namespace KindredCommands.Patches;

[HarmonyPatch(typeof(MapIconSpawnSystem), nameof(MapIconSpawnSystem.OnUpdate))]
internal class MapIconSpawnSystemPatch
{
	public static void Prefix(MapIconSpawnSystem __instance)
	{
		var entities = __instance.__query_1050583545_0.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (!entity.Has<Attach>()) continue;

			var attach = entity.Read<Attach>().Parent;
			if(attach == Entity.Null) continue;
			if(!attach.Has<Relic>()) continue;
			
			Core.SoulshardService.HandleSoulshardSpawn(attach);
		}
		entities.Dispose();
	}
}
