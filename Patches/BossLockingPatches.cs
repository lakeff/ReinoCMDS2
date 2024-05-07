using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;

namespace KindredCommands.Patches;


[HarmonyPatch(typeof(BloodAltarSystem_StartTrackVBloodUnit_System_V2), nameof(BloodAltarSystem_StartTrackVBloodUnit_System_V2.OnUpdate))]
public static class BloodAltarSystem_StartTrackVBloodUnit_System_V2_HandleEventPatch
{
	public static void Prefix(BloodAltarSystem_StartTrackVBloodUnit_System_V2 __instance)
	{
		foreach(var entity in __instance._EventQuery.ToEntityArray(Allocator.Temp))
		{
			var huntTarget = entity.Read<StartTrackVBloodUnitEventV2>().HuntTarget;
			var fromCharacter = entity.Read<FromCharacter>();
			if(Core.Boss.IsBossLocked(huntTarget))
			{
				ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, fromCharacter.User.Read<User>(), "This boss is locked.");
				Core.EntityManager.DestroyEntity(entity);
			}
		}
	}
}

