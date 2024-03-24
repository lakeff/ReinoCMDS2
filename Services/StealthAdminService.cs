using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;

namespace KindredCommands.Services;
public class StealthAdminService
{
	private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
	private static readonly string STEALTH_ADMINS_PATH = Path.Combine(CONFIG_PATH, "stealthAdmins.json");

	readonly List<Entity> stealthUsers = [];

	public AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystem<AdminAuthSystem>();

	public StealthAdminService()
	{
		LoadStealthAdmins();
	}

	void LoadStealthAdmins()
	{
		if (!File.Exists(STEALTH_ADMINS_PATH))
		{
			return;
		}

		stealthUsers.Clear();

		var json = File.ReadAllText(STEALTH_ADMINS_PATH);
		var characterNames = JsonSerializer.Deserialize<List<string>>(json);

		var wereAnyAdminsRemoved = false;
		var userEntities = Helper.GetEntitiesByComponentType<User>(includeDisabled: true);
		foreach (var userEntity in userEntities)
		{
			var user = userEntity.Read<User>();
			if(characterNames.Contains(user.CharacterName.ToString()))
			{
				if(!adminAuthSystem._LocalAdminList.Contains(user.PlatformId))
				{
					wereAnyAdminsRemoved = true;
					continue;
				}

				stealthUsers.Add(userEntity);
				user.IsAdmin = false;
				userEntity.Write(user);
				if (userEntity.Has<AdminUser>())
				{
					userEntity.Remove<AdminUser>();
				}
			}
		}

		if(wereAnyAdminsRemoved)
		{
			SaveStealthAdmins();
		}
	}

	void SaveStealthAdmins()
	{
		// Verify the path exists
		if (!Directory.Exists(CONFIG_PATH))
		{
			Directory.CreateDirectory(CONFIG_PATH);
		}

		var characterNames = stealthUsers.Select(e => e.Read<User>().CharacterName.ToString()).ToList();
		characterNames.Sort();
		var json = JsonSerializer.Serialize(characterNames, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(STEALTH_ADMINS_PATH, json);
	}

	public void HandleUserConnecting(Entity userEntity)
	{
		if (stealthUsers.Contains(userEntity))
		{
			var user = userEntity.Read<User>();
			ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, $"You are in stealth admin mode.");
		}
	}

	public void HandleUserDisconnecting(Entity userEntity)
	{
		if (stealthUsers.Contains(userEntity))
		{
			var user = userEntity.Read<User>();
			if (user.IsAdmin)
			{
				user.IsAdmin = false;
				userEntity.Write(user);
			}
		}
	}

	static void AuthAdmin(Entity userEntity)
	{
		var user = userEntity.Read<User>();
		var entity = Core.EntityManager.CreateEntity(
						ComponentType.ReadWrite<FromCharacter>(),
						ComponentType.ReadWrite<AdminAuthEvent>()
					);
		entity.Write(new FromCharacter()
		{
			Character = user.LocalCharacter.GetEntityOnServer(),
			User = userEntity
		});
	}

	static void DeauthAdmin(Entity userEntity)
	{
		var user = userEntity.Read<User>();
		var entity = Core.EntityManager.CreateEntity(
						ComponentType.ReadWrite<FromCharacter>(),
						ComponentType.ReadWrite<DeauthAdminEvent>()
					);
		entity.Write(new FromCharacter()
		{
			Character = user.LocalCharacter.GetEntityOnServer(),
			User = userEntity
		});
	}


	public bool ToggleStealthUser(Entity userEntity)
	{
		var user = userEntity.Read<User>();
		if (stealthUsers.Contains(userEntity))
		{
			DeauthAdmin(userEntity);
			stealthUsers.Remove(userEntity);
			SaveStealthAdmins();
			return false;
		}

		if (user.IsAdmin)
		{
			user.IsAdmin = false;
			userEntity.Write(user);
		}

		if (userEntity.Has<AdminUser>())
		{
			userEntity.Remove<AdminUser>();
		}

		stealthUsers.Add(userEntity);
		SaveStealthAdmins();

		Helper.KickPlayer(userEntity);
		return true;
	}

	public void RemoveStealthAdmin(Entity userEntity)
	{
		if (stealthUsers.Contains(userEntity))
		{
			stealthUsers.Remove(userEntity);
			SaveStealthAdmins();
		}
	}

	public bool IsStealthAdmin(Entity userEntity)
	{
		return stealthUsers.Contains(userEntity);
	}

	public void HandleRename(Entity userEntity)
	{
		if (stealthUsers.Contains(userEntity))
		{
			SaveStealthAdmins();
		}
	}
}
