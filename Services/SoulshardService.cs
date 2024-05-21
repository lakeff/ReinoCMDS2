using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using KindredCommands.Data;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace KindredCommands.Services;
internal class SoulshardService
{
	readonly List<Entity> droppedSoulshards = [];
	readonly List<Entity> spawnedSoulshards = []; // Tracked with the ScriptSpawn tag

	public int ShardDropLimit => Core.ConfigSettings.ShardDropLimit;

	EntityQuery relicDroppedQuery;
	EntityQuery soulshardPrefabsQuery;

	public SoulshardService()
	{
		EntityQueryDesc relicDroppedQueryDesc = new()
		{
			All = new ComponentType[]
			{
				new(Il2CppType.Of<RelicDropped>(), ComponentType.AccessMode.ReadOnly),
			},
			Options = EntityQueryOptions.IncludeSystems
		};

		relicDroppedQuery = Core.EntityManager.CreateEntityQuery(relicDroppedQueryDesc);

		EntityQueryDesc soulshardPrefabsQueryDesc = new()
		{
			All = new ComponentType[]
			{
				new(Il2CppType.Of<ItemData>(), ComponentType.AccessMode.ReadOnly),
				new(Il2CppType.Of<Relic>(), ComponentType.AccessMode.ReadOnly),
				new(Il2CppType.Of<Prefab>(), ComponentType.AccessMode.ReadOnly),
			},
			Options = EntityQueryOptions.IncludePrefab
		};
		soulshardPrefabsQuery = Core.EntityManager.CreateEntityQuery(soulshardPrefabsQueryDesc);

		foreach (var entity in Helper.GetEntitiesByComponentTypes<ItemData, Relic>())
		{
			if (entity.Has<ScriptSpawn>())
				spawnedSoulshards.Add(entity);
			else
				droppedSoulshards.Add(entity);
		}
		RefreshWillDrop();
	}

	private void RefreshWillDrop()
	{
		if (Core.ServerGameSettingsSystem._Settings.RelicSpawnType == RelicSpawnType.Plentiful) return;
		var relicDropped = GetRelicDropped();
		for (var relicType = RelicType.TheMonster; relicType <= RelicType.Dracula; relicType++)
		{
			var droppedCount = droppedSoulshards.Where(e => e.Read<Relic>().RelicType == relicType).Count();
			var shouldDrop = droppedCount < ShardDropLimit;
			var isDropped = relicDropped[(int)relicType].Value;

			if (isDropped == shouldDrop)
				relicDropped[(int)relicType] = new RelicDropped() { Value = !shouldDrop };
		}
	}

	public void SetShardDropLimit(int limit)
	{
		Core.ConfigSettings.ShardDropLimit = limit;
		RefreshWillDrop();
	}

	DynamicBuffer<RelicDropped> GetRelicDropped()
	{
		var entities = relicDroppedQuery.ToEntityArray(Allocator.Temp);

		var buffer = Core.EntityManager.GetBuffer<RelicDropped>(entities[0]);
		entities.Dispose();
		return buffer;
	}

	public (bool willDrop, int droppedCount, int spawnedCount)[] GetSoulshardStatus()
	{
		var returning = new (bool willDrop, int droppedCount, int spawnedCount)[5];

		var relicDropped = GetRelicDropped();
		
		for(var relicType = RelicType.None; relicType <= RelicType.Dracula; relicType++)
		{
			var droppedCount = droppedSoulshards.Where(e => e.Read<Relic>().RelicType == relicType).Count();
			var spawnedCount = spawnedSoulshards.Where(e => e.Read<Relic>().RelicType == relicType).Count();
			var willDrop = !relicDropped[(int)relicType].Value;
			returning[(int)relicType] = (willDrop, droppedCount, spawnedCount);
		}
		return returning;
	}

	public void HandleSoulshardSpawn(Entity soulshardItemEntity)
	{
		if (!soulshardItemEntity.Has<InventoryItem>()) return;

		var invItem = soulshardItemEntity.Read<InventoryItem>();
		var isSpawned = invItem.ContainerEntity == Entity.Null || invItem.ContainerEntity.Read<PrefabGUID>() == Prefabs.External_Inventory;

		if (isSpawned)
		{
			spawnedSoulshards.Add(soulshardItemEntity);
			soulshardItemEntity.Add<ScriptSpawn>();
		}
		else
		{
			droppedSoulshards.Add(soulshardItemEntity);
			RefreshWillDrop();
		}
	}

	public void HandleSoulshardDestroy(Entity soulshardItemEntity)
	{
		if (!droppedSoulshards.Remove(soulshardItemEntity))
			spawnedSoulshards.Remove(soulshardItemEntity);

		if (Core.ServerGameSettingsSystem._Settings.RelicSpawnType == RelicSpawnType.Plentiful) return;

		var relicType = soulshardItemEntity.Read<Relic>().RelicType;
		var relicCount = droppedSoulshards.Where(e => e.Read<Relic>().RelicType == relicType).Count();

		// Let the destruction system handle this normally if we are below the limit
		if (relicCount < ShardDropLimit) return;

		soulshardItemEntity.Remove<Relic>();
	}
}
