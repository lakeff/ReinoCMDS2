
using System.Collections.Generic;
using HarmonyLib;
using KindredCommands;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;

[HarmonyBefore("gg.deca.VampireCommandFramework")]
[HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
public static class StealthAdminChatPatch
{
	static readonly List<Entity> entitiesStealthAdmin = [];

	public static bool Prefix(ChatMessageSystem __instance)
	{
		entitiesStealthAdmin.Clear();
		NativeArray<Entity> entities = __instance.__ChatMessageJob_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromData = entity.Read<FromCharacter>();
			var chatData = entity.Read<ChatMessageEvent>();
			if(Core.StealthAdminService.IsStealthAdmin(fromData.User) && chatData.MessageText.ToString().StartsWith("."))
			{
				entitiesStealthAdmin.Add(fromData.User);
				var userData = fromData.User.Read<User>();
				userData.IsAdmin = true;
				Core.Log.LogInfo("Enabled Admin");
				fromData.User.Write(userData);
			}
		}
		entities.Dispose();
		return true;
	}

	public static void Postfix(ChatMessageSystem __instance)
	{
		foreach (var userEntity in entitiesStealthAdmin)
		{
			var userData = userEntity.Read<User>();
			userData.IsAdmin = false;
			Core.Log.LogInfo("Disabled Admin");
			userEntity.Write(userData);
		}
	}
}


[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
public static class StealthAdminOnUserConnected_Patch
{
	public static void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
	{
		if (Core.Players == null) Core.InitializeAfterLoaded();
		var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
		var serverClient = __instance._ApprovedUsersLookup[userIndex];
		var userEntity = serverClient.UserEntity;
		Core.StealthAdminService.HandleUserConnecting(userEntity);
	}
}

[HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserDisconnected))]
public static class StealthAdminOnUserDisconnected_Patch
{
	public static void Prefix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId, ConnectionStatusChangeReason connectionStatusReason, string extraData)
	{
		if (Core.Players == null) Core.InitializeAfterLoaded();
		var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
		var serverClient = __instance._ApprovedUsersLookup[userIndex];
		var userEntity = serverClient.UserEntity;
		Core.StealthAdminService.HandleUserDisconnecting(userEntity);
	}
}
