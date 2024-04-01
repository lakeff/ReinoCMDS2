using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;

namespace KindredCommands.Services;

[HarmonyBefore("gg.deca.VampireCommandFramework")]
[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.SendRevealedMapData))]
public static class RevealedMapDataPatch
{
	public static void Prefix(ServerBootstrapSystem __instance, Entity userEntity, User user)
	{
		if(Core.ConfigSettings == null) Core.InitializeAfterLoaded();
		if (!Core.ConfigSettings.RevealMapToAll) return;

		var userData = userEntity.Read<User>();
		bool isNewVampire = userData.CharacterName.IsEmpty;
		if (isNewVampire)
		{
			Helper.RevealMapForPlayer(userEntity);
		}
	}
}
