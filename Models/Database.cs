using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Il2CppSystem;
using Il2CppSystem.IO;
using ProjectM.Network;
using Unity.Entities;

namespace KindredCommands.Models;
public readonly struct Database
{
	private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, "KindredCommands");
	private static readonly string STAFF_PATH = Path.Combine(CONFIG_PATH, "staff.json");
	private static readonly string NOSPAWN_PATH = Path.Combine(CONFIG_PATH, "nospawn.json");
	private static readonly string INFO_PATH = Path.Combine(CONFIG_PATH, "info.json");
	private static readonly string VIP_PATH = Path.Combine(CONFIG_PATH, "vip.json");

	public static void InitConfig()
	{
		string json;
		Dictionary<string, string> dict;

		Dictionary<string, Dictionary<string, string>> dictMap;

		STAFF.Clear();
		NOSPAWN.Clear();
		INFO.Clear();
		VIP.Clear();

		if (File.Exists(STAFF_PATH))
		{
			json = File.ReadAllText(STAFF_PATH);
			dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

			foreach (var kvp in dict)
			{
				STAFF[kvp.Key] = kvp.Value;
			}
		}
		else
		{
			SaveStaff();
		}

		if (File.Exists(INFO_PATH))
		{
			json = File.ReadAllText(INFO_PATH);
			dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

			foreach (var kvp in dict)
			{
				INFO[kvp.Key] = kvp.Value;
			}
		}
		else
		{
			SaveInfo();
		}

		if (File.Exists(VIP_PATH))
		{
			json = File.ReadAllText(VIP_PATH);
			dictMap = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

			foreach (var kvp in dictMap)
			{
				VIP[kvp.Key] = kvp.Value;
			}
		}
		else
		{
			SaveVip();
		}

		if (File.Exists(NOSPAWN_PATH))
		{
			json = File.ReadAllText(NOSPAWN_PATH);
			dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

			foreach (var kvp in dict)
			{
				NOSPAWN[kvp.Key] = kvp.Value;
			}
		}
		else
		{
			NOSPAWN["CHAR_VampireMale"] = "it causes corruption to the save file.";
			NOSPAWN["CHAR_Mount_Horse_Gloomrot"] = "it causes an instant server crash.";
			NOSPAWN["CHAR_Mount_Horse_Vampire"] = "it causes an instant server crash.";
			SaveNoSpawn();
		}
	}



	static void WriteConfig<T>(string path, Dictionary<string, T> dict)
	{
		if (!Directory.Exists(CONFIG_PATH)) Directory.CreateDirectory(CONFIG_PATH);
		var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(path, json);
	}

	static public void SaveStaff()
	{
		WriteConfig(STAFF_PATH, STAFF);
	}

	static public void SaveNoSpawn()
	{
		WriteConfig(NOSPAWN_PATH, NOSPAWN);
	}
	static public void SaveInfo()
	{
		WriteConfig(INFO_PATH, INFO);
	}

	static public void SaveVip()
	{
		WriteConfig(VIP_PATH, VIP);
	}

	static public void SetInfo(string name, string value)
	{
		INFO[name] = value;
		SaveInfo();
		Core.Log.LogWarning($"Informação adicionada - Nome: {name}, Valor: {value}");
	}

	static public void SetVip(Entity userEntity, string level, System.DateTime expireDate)
	{
		Player player = new(userEntity);

		var date = expireDate.AddDays(30);

		VIP[player.SteamID.ToString()] = new Dictionary<string, string>()
		{
			{ "Level", level },
			{ "ExpireDate",  date.ToString()},
			{ "Nome", player.Name },
		};
		SaveVip();
		Core.Log.LogWarning($"Vip adicionado ao {player.Name}, expira em {expireDate.ToShortDateString}");
	}
	static public void SetStaff(Entity userEntity, string rank)
	{
		var user = userEntity.Read<User>();
		STAFF[user.PlatformId.ToString()] = rank;
		SaveStaff();
		Core.Log.LogWarning($"User {user.CharacterName} added to staff config as {rank}.");
	}

	static public void SetNoSpawn(string prefabName, string reason)
	{
		NOSPAWN[prefabName] = reason;
		SaveNoSpawn();
		Core.Log.LogWarning($"NPC {prefabName} is banned from spawning because {reason}.");
	}

	static public bool IsSpawnBanned(string prefabName, out string reason)
	{
		return NOSPAWN.TryGetValue(prefabName, out reason);
	}

	private static readonly Dictionary<string, string> STAFF = new()
	{
		{ "SteamID1", "[Rank]" },
		{ "SteamID2", "[Rank]" }
	};

	private static readonly Dictionary<string, Dictionary<string, string>> VIP = new()
	{
		{ "SteamID1", new Dictionary<string, string> { { "Nivel", "Valor" }, { "ExpireDate", "Date" }, { "Nome", "Valor" } } },
		{ "SteamID2", new Dictionary<string, string> { { "Nivel", "Valor" }, { "ExpireDate", "Date" }, { "Nome", "Valor" } } }
	};

	private static readonly Dictionary<string, string> NOSPAWN = new()
	{
		{ "PrefabGUID", "Reason" }
	};
	private static readonly Dictionary<string, string> INFO = new()
	{
		{ "Name", "Value" },
		{ "Name2", "Value" },

	};
	public static Dictionary<string, string> GetStaff()
	{
		return STAFF;
	}
	public static Dictionary<string, string> GetInfo()
	{
		return INFO;
	}
	public static Dictionary<string, Dictionary<string, string>> GetVip()
	{
		return VIP;
	}

	public static string GetDiscord()
	{
		string discord = INFO.FirstOrDefault(x => x.Key.ToLower() == "Discord".ToLower()).Value;
		return discord;
	}

	public static bool RemoveStaff(Entity userEntity)
	{
		var removed = STAFF.Remove(userEntity.Read<User>().PlatformId.ToString());
		if (removed)
		{
			SaveStaff();
			Core.Log.LogWarning($"User {userEntity.Read<User>().CharacterName} removed from staff config.");
		}
		else
		{
			Core.Log.LogInfo($"User {userEntity.Read<User>().CharacterName} attempted to be removed from staff config but wasn't there.");
		}
		return removed;
	}

	public static bool RemoveVip(string steamId)
	{
		var removed = VIP.Remove(steamId);
		if (removed)
		{
			SaveVip();
		}
		else
		{
			Core.Log.LogInfo($"Player não foi encontrado nas configurações de VIP");
		}
		return removed;
	}

	public static bool RemoveInfo(string name)
	{
		var remove = INFO.Remove(name);

		if (remove)
		{
			SaveInfo();
			Core.Log.LogWarning($"{name} foi removido");
		}
		else
		{
			Core.Log.LogWarning($"Informação não encontrada");
		}
		return remove;
	}
}
