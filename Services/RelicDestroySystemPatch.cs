using HarmonyLib;
using ProjectM;
using Unity.Collections;

namespace KindredCommands.Services;

[HarmonyPatch(typeof(RelicDestroySystem), nameof(RelicDestroySystem.OnUpdate))]
internal class RelicDestroySystemPatch
{
	public static void Prefix(RelicDestroySystem __instance)
	{
		var entities = __instance._Query.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			Core.SoulshardService.HandleSoulshardDestroy(entity);
		}
		entities.Dispose();
	}
}
