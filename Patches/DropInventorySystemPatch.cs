using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2CppSystem;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using Unity.Transforms;

namespace KindredCommands.Patches;

[HarmonyPatch(typeof(DropInventorySystem), nameof(DropInventorySystem.DropItem))]
internal class DropInventorySystemPatch
{
	public static void Prefix(DropInventorySystem __instance,
		EntityCommandBuffer commandBuffer,
		[In] ref Translation translation,
		PrefabGUID itemHash,
		int amount,
		Entity itemEntity,
		Nullable_Unboxed<Entity> customDropArc,
		Nullable_Unboxed<float> minRange,
	    Nullable_Unboxed<float> maxRange)
	{
		if(itemEntity != Entity.Null)
		{
			Core.Log.LogInfo($"Dropping item {itemHash.LookupName()} at {translation.Value}");
		}
	}
}
