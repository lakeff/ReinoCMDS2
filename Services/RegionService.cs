using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ProjectM;
using ProjectM.Network;
using ProjectM.Physics;
using ProjectM.Terrain;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace KindredCommands.Services;
internal class RegionService
{
	static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
	static readonly string REGIONS_PATH = Path.Combine(CONFIG_PATH, "regions.json");

	GameObject regionGameObject;
	IgnorePhysicsDebugSystem regionMonoBehaviour;

	List<WorldRegionType> lockedRegions = [];
	Dictionary<string, int> gatedRegions = [];
	Dictionary<Entity, (WorldRegionType, Vector3)> lastValidPos = [];
	Dictionary<Entity, float> lastSentMessage = [];
	Dictionary<string, float> maxPlayerLevels = [];
	List<string> allowPlayers = [];

	public IEnumerable<WorldRegionType> LockedRegions => lockedRegions;
	public IEnumerable<KeyValuePair<string, int>> GatedRegions => gatedRegions;
	public IEnumerable<string> AllowedPlayers => allowPlayers;

	struct RegionFile
	{
		public WorldRegionType[] LockedRegions { get; set; }
		public Dictionary<string, int> GatedRegions { get; set; }
		public Dictionary<string, float> MaxPlayerLevels { get; set; }
		public string[] AllowPlayers { get; set; }
	}

	public RegionService()
	{
		regionGameObject = new GameObject("RegionService");
		regionMonoBehaviour = regionGameObject.AddComponent<IgnorePhysicsDebugSystem>();
		regionMonoBehaviour.StartCoroutine(CheckPlayerRegions().WrapToIl2Cpp());

		LoadRegions();
	}

	void LoadRegions()
	{
		if (!File.Exists(REGIONS_PATH))
		{
			return;
		}

		var options = new JsonSerializerOptions
		{
			Converters = { new RegionConverter() },
			WriteIndented = true,
		};

		var json = File.ReadAllText(REGIONS_PATH);
		var regionFile = JsonSerializer.Deserialize<RegionFile>(json, options);

		lockedRegions.Clear();
		if(regionFile.LockedRegions != null)
			lockedRegions.AddRange(regionFile.LockedRegions);
		gatedRegions = regionFile.GatedRegions ?? gatedRegions;
		maxPlayerLevels = regionFile.MaxPlayerLevels ?? maxPlayerLevels;
		allowPlayers.Clear();
		if(regionFile.AllowPlayers != null)
			allowPlayers.AddRange(regionFile.AllowPlayers);
	}

	void SaveRegions()
	{
		if (!Directory.Exists(CONFIG_PATH)) Directory.CreateDirectory(CONFIG_PATH);

		var regionFile = new RegionFile
		{
			LockedRegions = lockedRegions.ToArray(),
			GatedRegions = gatedRegions,
			MaxPlayerLevels = maxPlayerLevels,
			AllowPlayers = allowPlayers.ToArray(),
		};

		var options = new JsonSerializerOptions
		{
			Converters = { new RegionConverter() },
			WriteIndented = true,
		};

		var json = JsonSerializer.Serialize(regionFile, options);
		File.WriteAllText(REGIONS_PATH, json);
	}

	public bool LockRegion(WorldRegionType region)
	{
		if (lockedRegions.Contains(region))
		{
			return false;
		}

		lockedRegions.Add(region);
		SaveRegions();
		return true;
	}

	public bool UnlockRegion(WorldRegionType region)
	{
		var result = lockedRegions.Remove(region);
		SaveRegions();
		return result;
	}

	public void GateRegion(WorldRegionType region, int level)
	{
		gatedRegions[region.ToString()] = level;
		SaveRegions();
	}

	public bool UngateRegion(WorldRegionType region)
	{
		var result = gatedRegions.Remove(region.ToString());
		SaveRegions();
		return result;
	}

	public void AllowPlayer(string playerName)
	{
		if(allowPlayers.Contains(playerName))
			return;
		allowPlayers.Add(playerName);
		SaveRegions();
	}

	public void RemovePlayer(string playerName)
	{
		allowPlayers.Remove(playerName);
		SaveRegions();
	}

	IEnumerator CheckPlayerRegions()
	{
		while(true)
		{
			foreach(var userEntity in Core.Players.GetCachedUsersOnline())
			{
				if(!userEntity.Has<User>() || !userEntity.Has<CurrentWorldRegion>()) continue;

				var charName = userEntity.Read<User>().CharacterName.ToString();

				if(String.IsNullOrEmpty(charName)) continue;

				var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();
				if(!charEntity.Has<Equipment>()) continue;

				var currentWorldRegion = userEntity.Read<CurrentWorldRegion>();
				var equipment = charEntity.Read<Equipment>();
				var maxLevel = Mathf.Max(equipment.ArmorLevel+equipment.SpellLevel+equipment.WeaponLevel,
										 maxPlayerLevels.TryGetValue(charName, out var cachedLevel) ? cachedLevel : 0);
				maxPlayerLevels[charName] = maxLevel;

				var returnReason = DisallowedFromRegion(userEntity, currentWorldRegion.CurrentRegion);
				if (returnReason != null)
				{
					ReturnPlayer(userEntity, returnReason);
				}
				else
				{
					lastValidPos[userEntity] = (currentWorldRegion.CurrentRegion, charEntity.Read<Translation>().Value);
				}
				yield return null;
			}
			yield return null;
		}
	}

	string DisallowedFromRegion(Entity userEntity, WorldRegionType region)
	{
		var charName = userEntity.Read<User>().CharacterName.ToString();
		if (allowPlayers.Contains(charName))
			return null;

		var maxLevel = maxPlayerLevels[charName];
		if (lockedRegions.Contains(region))
		{
			return $"Can't enter region {region.ToString()} as it's locked";
		}
		else if(gatedRegions.TryGetValue(region.ToString(), out var level) && maxLevel < level)
		{
			return $"Can't enter region {region.ToString()} as it's gated to level {level} while your max reached level is only {Mathf.FloorToInt(maxLevel)}";
		}

		return null;
	}

	void ReturnPlayer(Entity userEntity, string returnReason)
	{
		if(lastValidPos.TryGetValue(userEntity, out var lastValid))
		{
			WorldRegionType region;
			Vector3 returnPos;
			(region, returnPos) = lastValid;

			// Don't send them back if the last good is a disallowed region for them
			if (DisallowedFromRegion(userEntity, region) != null) return;

			if (!lastSentMessage.TryGetValue(userEntity, out var lastSent) ||
				lastSent + 10 < Time.time)
			{
				ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, userEntity.Read<User>(), returnReason);
				lastSentMessage[userEntity] = Time.time;
			}

			var charEntity = userEntity.Read<User>().LocalCharacter.GetEntityOnServer();
			charEntity.Write(new Translation { Value = returnPos });
			charEntity.Write(new LastTranslation { Value = returnPos });
		}
	}


	internal class RegionConverter : JsonConverter<WorldRegionType>
	{
		public override WorldRegionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
			{
				throw new JsonException();
			}

			reader.GetString();

			foreach(var value in Enum.GetValues<WorldRegionType>())
			{
				if (value.ToString() == reader.GetString())
				{
					return value;
				}
			}

			return WorldRegionType.None;
		}

		public override void Write(Utf8JsonWriter writer, WorldRegionType value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString());
		}
	}
}
