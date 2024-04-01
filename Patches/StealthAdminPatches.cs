
using System;
using System.Reflection;
using Bloodstone.API;
using HarmonyLib;
using KindredCommands;
using ProjectM;
using ProjectM.Network;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;
using VampireCommandFramework;
using VampireCommandFramework.Breadstone;

[HarmonyBefore("gg.deca.VampireCommandFramework")]
[HarmonyPatch(typeof(ChatMessageSystem), nameof(ChatMessageSystem.OnUpdate))]
public static class StealthAdminChatPatch
{
	public static bool Prefix(ChatMessageSystem __instance)
	{
		NativeArray<Entity> entities = __instance.__ChatMessageJob_entityQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromData = entity.Read<FromCharacter>();
			var userData = fromData.User.Read<User>();
			var chatEventData = entity.Read<ChatMessageEvent>();

			var messageText = chatEventData.MessageText.ToString();

			var addedAdmin = userData.IsAdmin;
			var stealthAdmin = Core.StealthAdminService.IsStealthAdmin(fromData.User);

			if (!addedAdmin && stealthAdmin && chatEventData.MessageText.ToString().StartsWith("."))
			{
				addedAdmin = true;
				userData.IsAdmin = true;
				fromData.User.Write(userData);
			}

			// Access private VChatEvent constructor
			VChatEvent ev = (VChatEvent)typeof(VChatEvent).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
				new Type[] { typeof(Entity), typeof(Entity), typeof(string), typeof(ChatMessageType), typeof(User) }, null).
				Invoke(new object[] { fromData.User, fromData.Character, messageText, chatEventData.MessageType, userData });

			var ctx = new ChatCommandContext(ev);

			CommandResult result;
			try
			{
				result = CommandRegistry.Handle(ctx, messageText);
			}
			catch (Exception e)
			{
				Core.Log.LogError($"Error while handling chat message {e}");
				continue;
			}

			var messageParts = messageText.Split(" ");
			var firstPart = messageParts[0];
			var secondPart = messageParts.Length > 1 ? messageParts[1] : "";
			if (firstPart.ToLowerInvariant().Contains("nolog") || firstPart.ToLowerInvariant().Contains("password"))
			{
				messageText = firstPart + " <Not Logging For Security>";
			}
			else if(secondPart.ToLowerInvariant().Contains("nolog") || secondPart.ToLowerInvariant().Contains("password"))
			{
				messageText = firstPart + " " + secondPart + " <Not Logging For Security>";
			}

			// Legacy .help pass through support
			if (result == CommandResult.Success && messageText.StartsWith(".help-legacy", System.StringComparison.InvariantCulture))
			{
				chatEventData.MessageText = messageText.Replace("-legacy", string.Empty);
				__instance.EntityManager.SetComponentData(entity, chatEventData);
			}
			else if (result != CommandResult.Unmatched)
			{
				switch(result)
				{
					case CommandResult.Denied:
						Core.Log.LogInfo($"{ctx.Name} was denied trying to use command: {messageText}");
						break;
					case CommandResult.Success:
						Core.Log.LogInfo($"{ctx.Name} used command: {messageText}");
						break;
					case CommandResult.InternalError:
						Core.Log.LogInfo($"{ctx.Name} had an internal error trying to use command: {messageText}");
						break;
					case CommandResult.UsageError:
						Core.Log.LogInfo($"{ctx.Name} had a usage error trying to use command: {messageText}");
						break;
				}
				//__instance.EntityManager.AddComponent<DestroyTag>(entity);
				VWorld.Server.EntityManager.DestroyEntity(entity);
			}

			if (addedAdmin && stealthAdmin)
			{
				userData.IsAdmin = false;
				fromData.User.Write(userData);
			}
		}
		entities.Dispose();
		return true;
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
		if (!__instance._NetEndPointToApprovedUserIndex.TryGetValue(netConnectionId, out var userIndex)) return;
		var serverClient = __instance._ApprovedUsersLookup[userIndex];
		var userEntity = serverClient.UserEntity;
		Core.StealthAdminService.HandleUserDisconnecting(userEntity);
	}
}

