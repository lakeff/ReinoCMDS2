
using System.Linq;
using Il2CppInterop.Runtime;
using KindredCommands.Data;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace KindredCommands.Services;

internal class GearService
{
	EntityQuery itemQuery;

	readonly static PrefabGUID[] shardPrefabs = [
		Prefabs.Item_MagicSource_SoulShard_Dracula,
		Prefabs.Item_MagicSource_SoulShard_Manticore,
		Prefabs.Item_MagicSource_SoulShard_Monster,
		Prefabs.Item_MagicSource_SoulShard_Solarus
	];

	public GearService()
	{
		EntityQueryDesc itemQueryDesc = new()
		{
			All = new ComponentType[] {
				new(Il2CppType.Of<InventoryItem>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<PrefabGUID>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<ItemData>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<EquippableData>(), ComponentType.AccessMode.ReadWrite),

			},
			Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludePrefab
		};
		itemQuery = Core.EntityManager.CreateEntityQuery(itemQueryDesc);

		SetHeadgearBloodbound(Core.ConfigSettings.HeadgearBloodbound);
	}

	public bool ToggleHeadgearBloodbound()
	{
		Core.ConfigSettings.HeadgearBloodbound = !Core.ConfigSettings.HeadgearBloodbound;
		SetHeadgearBloodbound(Core.ConfigSettings.HeadgearBloodbound);
		return Core.ConfigSettings.HeadgearBloodbound;
	}

	void SetHeadgearBloodbound(bool bloodBound)
	{
		var itemMap = Core.GameDataSystem.ItemHashLookupMap;
		var allHeadgear = Helper.GetEntitiesByComponentTypes<EquipmentToggleData, Prefab>(includePrefab:true);
		foreach (var headgear in allHeadgear)
		{
			var itemData = headgear.Read<ItemData>();
			if(bloodBound)
				itemData.ItemCategory |= ItemCategory.BloodBound;
			else
				itemData.ItemCategory &= ~ItemCategory.BloodBound;
			headgear.Write(itemData);

			itemMap[headgear.Read<PrefabGUID>()] = itemData;
		}
	}

	public bool ToggleShardsFlightRestricted()
	{
		Core.ConfigSettings.SoulshardsFlightRestricted = !Core.ConfigSettings.SoulshardsFlightRestricted;
		return Core.ConfigSettings.SoulshardsFlightRestricted;
	}

	public void SetShardsRestricted(bool shardsRestricted)
	{
		var newCategory = shardsRestricted ? ItemCategory.Soulshard | ItemCategory.BloodBound : ItemCategory.Magic;
		var itemMap = Core.GameDataSystem.ItemHashLookupMap;
		foreach (var prefabGUID in shardPrefabs)
		{
			if (!itemMap.TryGetValue(prefabGUID, out var itemData)) continue;
			
			itemData.ItemCategory = newCategory;
			itemMap[prefabGUID] = itemData;
		}

		var entities = itemQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			if (!shardPrefabs.Contains(entity.Read<PrefabGUID>())) continue;

			var itemData = entity.Read<ItemData>();
			itemData.ItemCategory = newCategory;
			entity.Write(itemData);
		}
		entities.Dispose();
	}
}
