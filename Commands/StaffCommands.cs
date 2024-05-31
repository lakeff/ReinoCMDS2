using System.Collections.Generic;
using System.Text;
using KindredCommands.Commands.Converters;
using KindredCommands.Models;
using KindredCommands.Services;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Unity.Entities;
using VampireCommandFramework;

namespace KindredCommands.Commands;

internal class StaffCommands
{
	[Command("admin", description: "Mostra os administradores online.", adminOnly: false)]
	public static void WhoIsOnline(ChatCommandContext ctx)
	{
		var users = PlayerService.GetUsersOnline();
		var staff = Database.GetStaff();

		StringBuilder builder = new();
		foreach (var user in users)
		{
			Player player = new(user);
			foreach (KeyValuePair<string, string> _kvp in staff)
			{
				if (player.SteamID.ToString() == _kvp.Key)
				{
					string _role = _kvp.Value.Replace("</color>", "");
					builder
						.Append(_role)
						.Append(player.Name)
						.Append("</color>");
					builder.Append(' ');
				}
			}
		}
		if (builder.Length == 0)
		{
			ctx.Reply("There are no staff members online.");
			return;
		}
		ctx.Reply($"Online Staff: {builder}");
	}
	[Command("reloadstaff", description: "Reloads the staff config.", adminOnly: true)]
	public static void ReloadStaff(ChatCommandContext ctx)
	{
		Database.InitConfig();
		ctx.Reply("Staff config reloaded!");
	}

	[Command("setstaff", description: "Sets someones staff rank.", adminOnly: true)]
	public static void AddStaff(ChatCommandContext ctx, FoundPlayer player, string rank)
	{
		var userEntity = player.Value.UserEntity;
		var rankname = "[" + rank + "]";

		Database.SetStaff(userEntity, rankname);
		ctx.Reply("Staff member set!");
	}

	[Command("removestaff", description: "Removes someones staff rank.", adminOnly: true)]
	public static void RemoveStaff(ChatCommandContext ctx, FoundPlayer player)
	{
		var userEntity = player.Value.UserEntity;

		if (Database.RemoveStaff(userEntity))
			ctx.Reply("Staff member removed!");
		else
			ctx.Reply("Staff member not found!");
	}

	public static AdminAuthSystem adminAuthSystem = Core.Server.GetExistingSystemManaged<AdminAuthSystem>();
	[Command("reloadadmin", description: "Reloads the admin list.", adminOnly: true)]
	public static void ReloadCommand(ChatCommandContext ctx)
	{
		adminAuthSystem._LocalAdminList.Save();
		adminAuthSystem._LocalAdminList.Refresh();
		ctx.Reply("Admin list reloaded!");
	}

	[Command("toggleadmin", description: "Adds/Removes a player to the admin list, authing and deauthing.", adminOnly: true)]
	public static void ToggleAdminCommand(ChatCommandContext ctx, FoundPlayer player)
	{
		var userEntity = player.Value.UserEntity;
		var user = userEntity.Read<User>();
		var platformId = user.PlatformId;

		if (adminAuthSystem._LocalAdminList.Contains(platformId))
		{
			ctx.Reply($"Admin deauthed for {player.Value.CharacterName}");
			adminAuthSystem._LocalAdminList.Remove(platformId);
			Core.StealthAdminService.RemoveStealthAdmin(userEntity);

			if (userEntity.Has<AdminUser>())
			{
				userEntity.Remove<AdminUser>();
			}

			user.IsAdmin = false;
			userEntity.Write(user);

			var entity = Core.EntityManager.CreateEntity(
				ComponentType.ReadWrite<FromCharacter>(),
				ComponentType.ReadWrite<DeauthAdminEvent>()
			);
			entity.Write(new FromCharacter()
			{
				Character = player.Value.CharEntity,
				User = userEntity
			});

			ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, "You were removed as admin and deauthed");

		}
		else
		{
			ctx.Reply($"Admin authed for {player.Value.CharacterName}");
			adminAuthSystem._LocalAdminList.Add(platformId);

			userEntity.Add<AdminUser>();
			userEntity.Write(new AdminUser()
			{
				AuthMethod = AdminAuthMethod.Authenticated,
				Level = AdminLevel.SuperAdmin
			});

			user.IsAdmin = true;
			userEntity.Write(user);


			var entity = Core.EntityManager.CreateEntity(
				ComponentType.ReadWrite<FromCharacter>(),
				ComponentType.ReadWrite<AdminAuthEvent>()
			);
			entity.Write(new FromCharacter()
			{
				Character = player.Value.CharEntity,
				User = userEntity
			});

			ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, "You were added as admin and authed");
		}

		adminAuthSystem._LocalAdminList.Save();
	}

	[Command("stealthadmin", description: "Toggles stealth admin", adminOnly: true)]
	public static void StealthAdmin(ChatCommandContext ctx)
	{
		var user = ctx.Event.SenderUserEntity;
		if(Core.StealthAdminService.ToggleStealthUser(user))
		{
			ctx.Reply("Stealth admin enabled!");
		}
		else
		{
			ctx.Reply("Stealth admin disabled!");
		}
	}
}
