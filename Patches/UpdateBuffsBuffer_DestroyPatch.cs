using HarmonyLib;
using KindredCommands.Data;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace KindredCommands.Patches;
[HarmonyPatch(typeof(UpdateBuffsBuffer_Destroy), nameof(UpdateBuffsBuffer_Destroy.OnUpdate))]
internal class UpdateBuffsBuffer_DestroyPatch
{
	public static void Prefix(UpdateBuffsBuffer_Destroy __instance)
	{
		var entities = __instance.__query_401358720_0.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var prefabGUID = entity.Read<PrefabGUID>();
			if (prefabGUID != Prefabs.EquipBuff_ShroudOfTheForest)
				continue;

			var attach = entity.Read<Attach>();
			if(attach.Parent != Entity.Null)
			{
				Core.BoostedPlayerService.HandleShroudRemoval(attach.Parent);
			}
		}
		entities.Dispose();
	}
}
