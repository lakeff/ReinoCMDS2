using Bloodstone.API;
using KindredCommands.Data;
using Il2CppInterop.Runtime;
using Il2CppSystem;
using ProjectM;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;
using System.Collections.Generic;
using KindredCommands.Models;
using System.Linq;
using ProjectM.Network;
using KindredCommands.Services;
using KindredCommands.Models.Enums;
using ProjectM.Gameplay.Clan;

namespace KindredCommands;

// This is an anti-pattern, move stuff away from Helper not into it
internal static partial class Helper
{
	public static AdminAuthSystem adminAuthSystem = VWorld.Server.GetExistingSystem<AdminAuthSystem>();
	public static ClanSystem_Server clanSystem = VWorld.Server.GetExistingSystem<ClanSystem_Server>();
	public static EntityCommandBufferSystem entityCommandBufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();

	public static PrefabGUID GetPrefabGUID(Entity entity)
	{
		var entityManager = Core.EntityManager;
		PrefabGUID guid;
		try
		{
			guid = entityManager.GetComponentData<PrefabGUID>(entity);
		}
		catch
		{
			guid.GuidHash = 0;
		}
		return guid;
	}

	public static System.DateTime GetVipDate()
	{
		System.DateTime dateTime = System.DateTime.UtcNow;
		dateTime.AddHours(3);
		return dateTime;
	}

	public static Entity AddItemToInventory(Entity recipient, PrefabGUID guid, int amount)
	{
		try
		{
			var gameData = Core.Server.GetExistingSystem<GameDataSystem>();
			var itemSettings = AddItemSettings.Create(Core.EntityManager, gameData.ItemHashLookupMap);
			var inventoryResponse = InventoryUtilitiesServer.TryAddItem(itemSettings, recipient, guid, amount);

			return inventoryResponse.NewEntity;
		}
		catch (System.Exception e)
		{
			Core.LogException(e);
		}
		return new Entity();
	}

	public static NativeArray<Entity> GetEntitiesByComponentType<T1>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		EntityQueryDesc queryDesc = new()
		{
			All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = Core.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static NativeArray<Entity> GetEntitiesByComponentTypes<T1, T2>(bool includeAll = false, bool includeDisabled = false, bool includeSpawn = false, bool includePrefab = false, bool includeDestroyed = false)
	{
		EntityQueryOptions options = EntityQueryOptions.Default;
		if (includeAll) options |= EntityQueryOptions.IncludeAll;
		if (includeDisabled) options |= EntityQueryOptions.IncludeDisabled;
		if (includeSpawn) options |= EntityQueryOptions.IncludeSpawnTag;
		if (includePrefab) options |= EntityQueryOptions.IncludePrefab;
		if (includeDestroyed) options |= EntityQueryOptions.IncludeDestroyTag;

		EntityQueryDesc queryDesc = new()
		{
			All = new ComponentType[] { new(Il2CppType.Of<T1>(), ComponentType.AccessMode.ReadWrite), new(Il2CppType.Of<T2>(), ComponentType.AccessMode.ReadWrite) },
			Options = options
		};

		var query = Core.EntityManager.CreateEntityQuery(queryDesc);

		var entities = query.ToEntityArray(Allocator.Temp);
		return entities;
	}

	public static void RepairGear(Entity Character, bool repair = true)
	{
		Equipment equipment = Character.Read<Equipment>();
		NativeList<Entity> equippedItems = new(Allocator.Temp);
		equipment.GetAllEquipmentEntities(equippedItems);
		foreach (var equippedItem in equippedItems)
		{
			if (equippedItem.Has<Durability>())
			{
				var durability = equippedItem.Read<Durability>();
				if (repair)
				{
					durability.Value = durability.MaxDurability;
				}
				else
				{
					durability.Value = 0;
				}

				equippedItem.Write(durability);
			}
		}
		equippedItems.Dispose();

		for (int i = 0; i < 36; i++)
		{
			if (InventoryUtilities.TryGetItemAtSlot(Core.EntityManager, Character, i, out InventoryBuffer item))
			{
				var itemEntity = item.ItemEntity._Entity;
				if (itemEntity.Has<Durability>())
				{
					var durability = itemEntity.Read<Durability>();
					if (repair)
					{
						durability.Value = durability.MaxDurability;
					}
					else
					{
						durability.Value = 0;
					}

					itemEntity.Write(durability);
				}
			}
		}
	}

	public static void ReviveCharacter(Entity Character, Entity User, ChatCommandContext ctx = null)
	{
		var health = Character.Read<Health>();
		ctx?.Reply("TryGetbuff");
		if (BuffUtility.TryGetBuff(Core.EntityManager, Character, Prefabs.Buff_General_Vampire_Wounded_Buff, out var buffData))
		{
			ctx?.Reply("Destroy");
			DestroyUtility.Destroy(Core.EntityManager, buffData, DestroyDebugReason.TryRemoveBuff);

			ctx?.Reply("Health");
			health.Value = health.MaxHealth;
			health.MaxRecoveryHealth = health.MaxHealth;
			Character.Write(health);
		}
		if (health.IsDead)
		{
			ctx?.Reply("Respawn");
			var pos = Character.Read<LocalToWorld>().Position;

			Nullable_Unboxed<float3> spawnLoc = new()
			{
				value = pos,
				has_value = true
			};

			ctx?.Reply("Respawn2");
			var sbs = VWorld.Server.GetExistingSystem<ServerBootstrapSystem>();
			var bufferSystem = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>();
			var buffer = bufferSystem.CreateCommandBuffer();
			ctx?.Reply("Respawn3");
			sbs.RespawnCharacter(buffer, User,
				customSpawnLocation: spawnLoc,
				previousCharacter: Character);
		}
	}
	public static VipEnum GetVipEnum(string vip)
	{
		if(string.IsNullOrEmpty(vip) || string.IsNullOrWhiteSpace(vip))
		{
			return VipEnum.None;
		} 

		if(System.Enum.TryParse(vip, out VipEnum result))
		{
			return result;
		} else
		{
			return VipEnum.None;
		}
	}

	public static AdminLevel GetStaffEnum(Player player)
	{
		Dictionary<string, string> dict = Database.GetStaff() ?? new Dictionary<string, string>();
		var pStaff = dict.FirstOrDefault(x => x.Key == player.SteamID.ToString());
		string rank = pStaff.Value.Replace("[", "").Replace("]", "");

		if (System.Enum.TryParse(rank, out AdminLevel result))
		{
			return result;
		}
		else
		{
			return AdminLevel.None;
		}
	}

	public static bool VerifyAdminLevel(AdminLevel requiredLevel, Entity entity)
	{
		Player player = new(entity);
		AdminLevel level = GetStaffEnum(player);
		return level.Equals(requiredLevel) || level.Equals(AdminLevel.SuperAdmin) || level.Equals(AdminLevel.Admin) ? true : false;
	}
	public static void ClearExtraBuffs(Entity player)
	{
		var buffs = Core.EntityManager.GetBuffer<BuffBuffer>(player);
		var stringsToIgnore = new List<string>
		{
			"BloodBuff",
			"SetBonus",
			"EquipBuff",
			"Combat",
			"VBlood_Ability_Replace",
			"Shapeshift",
			"Interact",
			"AB_Consumable",
		};

		foreach (var buff in buffs)
		{
			bool shouldRemove = true;
			foreach (string word in stringsToIgnore)
			{
				if (buff.PrefabGuid.LookupName().Contains(word))
				{
					shouldRemove = false;
					break;
				}
			}
			if (shouldRemove)
			{
				DestroyUtility.Destroy(Core.EntityManager, buff.Entity, DestroyDebugReason.TryRemoveBuff);
			}
		}
		var equipment = player.Read<Equipment>();
		if (!equipment.IsEquipped(Prefabs.Item_Cloak_Main_ShroudOfTheForest, out var _) && BuffUtility.HasBuff(Core.EntityManager, player, Prefabs.EquipBuff_ShroudOfTheForest))
		{
			Buffs.RemoveBuff(player, Prefabs.EquipBuff_ShroudOfTheForest);
		}
	}

	public static float3 GetPlayerPosition(Entity player)
	{
		var pos = Core.EntityManager.GetComponentData<LocalToWorld>(player).Position;
		
		return pos;
	}
	public static void KickPlayer(Entity userEntity)
	{
		EntityManager entityManager = Core.Server.EntityManager;
		User user = userEntity.Read<User>();

		if (!user.IsConnected || user.PlatformId == 0) return;

		Entity entity = entityManager.CreateEntity(new ComponentType[3]
		{
		ComponentType.ReadOnly<NetworkEventType>(),
		ComponentType.ReadOnly<SendEventToUser>(),
		ComponentType.ReadOnly<KickEvent>()
		});

		entity.Write(new KickEvent()
		{
			PlatformId = user.PlatformId
		});
		entity.Write(new SendEventToUser()
		{
			UserIndex = user.Index
		});
		entity.Write(new NetworkEventType()
		{
			EventId = NetworkEvents.EventId_KickEvent,
			IsAdminEvent = false,
			IsDebugEvent = false
		});
	}

	public static void UnlockWaypoints(Entity userEntity)
	{
		DynamicBuffer<UnlockedWaypointElement> dynamicBuffer = Core.EntityManager.AddBuffer<UnlockedWaypointElement>(userEntity);
		dynamicBuffer.Clear();
		foreach (Entity waypoint in Helper.GetEntitiesByComponentType<ChunkWaypoint>())
			dynamicBuffer.Add(new UnlockedWaypointElement()
			{
				Waypoint = waypoint.Read<NetworkId>()
			});
	}
	public static void RevealMapForPlayer(Entity userEntity)
	{
		var mapZoneElements = Core.EntityManager.GetBuffer<UserMapZoneElement>(userEntity);
		foreach (var mapZone in mapZoneElements)
		{
			var userZoneEntity = mapZone.UserZoneEntity.GetEntityOnServer();
			var revealElements = Core.EntityManager.GetBuffer<UserMapZonePackedRevealElement>(userZoneEntity);
			revealElements.Clear();
			var revealElement = new UserMapZonePackedRevealElement
			{
				PackedPixel = 255
			};
			for (var i = 0; i < 8192; i++)
			{
				revealElements.Add(revealElement);
			}
		}
	}
}
