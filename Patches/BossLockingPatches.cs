using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace KindredCommands.Patches;

[HarmonyPatch(typeof(BloodAltarSystem_StartTrackVBloodUnit_System_V2), "HandleEvent")]
public static class BloodAltarSystem_StartTrackVBloodUnit_System_V2_HandleEventPatch
{
	public static bool Prefix(BloodAltarSystem_StartTrackVBloodUnit_System_V2 __instance,
		StartTrackVBloodUnitEventV2 trackVBloodUnitEvent, FromCharacter fromCharacter, NativeHashMap<NetworkId, Entity> networkIdToEntityMap,
		EntityCommandBuffer spawnCommandBuffer, EntityCommandBuffer destroyCommandBuffer)
	{
		if(Core.Boss.IsBossLocked(trackVBloodUnitEvent.HuntTarget))
		{
			ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, fromCharacter.User.Read<User>(), "This boss is locked and cannot be hunted.");
			return false;
		}
		return true;
	}
}
