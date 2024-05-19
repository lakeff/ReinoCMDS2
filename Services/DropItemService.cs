using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using KindredCommands.Data;
using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace KindredCommands.Services;
internal class DropItemService
{
	EntityQuery dropItemQuery;
	EntityQuery dropItemWithPrefabQuery;
	EntityQuery dropShardQuery;
	EntityQuery shardMapIconQuery;

	public DropItemService()
	{
		EntityQueryDesc dropItemQueryDesc = new()
		{
			All = new ComponentType[] {
				new(Il2CppType.Of<ItemPickup>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<PrefabGUID>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<Translation>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<DestroyWhenNoCharacterNearbyAfterDuration>(), ComponentType.AccessMode.ReadWrite),

			},
			None = new ComponentType[]
			{
				new(Il2CppType.Of<AttachedBuffer>(), ComponentType.AccessMode.ReadOnly),
			},
			Options = EntityQueryOptions.IncludeDisabled
		};

		dropItemQuery = Core.EntityManager.CreateEntityQuery(dropItemQueryDesc);

		dropItemQueryDesc.Options |= EntityQueryOptions.IncludePrefab;
		dropItemWithPrefabQuery = Core.EntityManager.CreateEntityQuery(dropItemQueryDesc);

		EntityQueryDesc dropShardQueryDesc = new()
		{
			All = new ComponentType[]
			{
				new(Il2CppType.Of<ItemPickup>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<PrefabGUID>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<Translation>(), ComponentType.AccessMode.ReadOnly),
			},
			None = new ComponentType[]
			{
				new(Il2CppType.Of<AttachedBuffer>(), ComponentType.AccessMode.ReadOnly),
				new(Il2CppType.Of<DestroyWhenNoCharacterNearbyAfterDuration>(), ComponentType.AccessMode.ReadWrite),
			},
			Options = EntityQueryOptions.IncludeDisabled
		};
		dropShardQuery = Core.EntityManager.CreateEntityQuery(dropShardQueryDesc);

		EntityQueryDesc shardMapIconQueryDesc = new()
		{
			All = new ComponentType[]
			{
				new(Il2CppType.Of<Attach>(), ComponentType.AccessMode.ReadWrite),
				new(Il2CppType.Of<RelicMapIcon>(), ComponentType.AccessMode.ReadWrite),
			},
			None = new ComponentType[]
			{
				new(Il2CppType.Of<AttachedBuffer>(), ComponentType.AccessMode.ReadOnly),
			},
			Options = EntityQueryOptions.IncludeDisabled
		};
		shardMapIconQuery = Core.EntityManager.CreateEntityQuery(shardMapIconQueryDesc);

		if (Core.ConfigSettings.ItemDropLifetime > 0)
		{
			SetDroppedItemLifetime(Core.ConfigSettings.ItemDropLifetime);
		}

		if(Core.ConfigSettings.ItemDropLifetimeWhenDisabled > 0)
		{
			SetDroppedItemLifetimeWhenDisabled(Core.ConfigSettings.ItemDropLifetimeWhenDisabled);
		}
	}

	IEnumerable<Entity> GetDropItems()
	{
		var entities = dropItemQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			yield return entity;
		}
		entities.Dispose();
	}

	IEnumerable<Entity> GetDropItemsWithPrefabs()
	{
		var entities = dropItemWithPrefabQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			yield return entity;
		}
		entities.Dispose();
	}

	IEnumerable<Entity> GetDropShards()
	{
		var entities = dropShardQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			var prefabGuid = entity.Read<PrefabGUID>();
			if (prefabGuid != Prefabs.Resource_Drop_SoulShard) continue;
			yield return entity;
		}
		entities.Dispose();
	}

	IEnumerable<Entity> GetShardMapIcons()
	{
		var entities = shardMapIconQuery.ToEntityArray(Allocator.Temp);
		foreach (var entity in entities)
		{
			yield return entity;
		}
		entities.Dispose();
	}

	public int ClearDropItems(Func<Entity, bool> shouldClear = null)
	{
		var count = 0;
		foreach(var entity in GetDropItems())
		{
			if (shouldClear != null && !shouldClear(entity)) continue;
			if (Core.EntityManager.TryGetBuffer<InventoryBuffer>(entity, out var inventory))
			{
				foreach (var item in inventory)
				{
					if (item.ItemEntity.GetEntityOnServer() == Entity.Null) continue;
					DestroyUtility.Destroy(Core.EntityManager, item.ItemEntity.GetEntityOnServer());
				}
			}

			DestroyUtility.Destroy(Core.EntityManager, entity);
			count++;
		}
		return count;
	}

	public int ClearDropItemsInRadius(float3 pos, float radius)
	{
		var posXZ = pos.xz;
		return ClearDropItems(entity =>
		{
			var translation = entity.Read<Translation>();
			return math.distance(posXZ, translation.Value.xz) <= radius;
		});
	}

	public int ClearDropShards(Func<Entity, bool> shouldClear=null)
	{
		var shardsItemsDestroyed = new List<Entity>();
		var count = 0;
		foreach (var entity in GetDropShards())
		{
			if (shouldClear != null && !shouldClear(entity)) continue;
			if(Core.EntityManager.TryGetBuffer<InventoryBuffer>(entity, out var inventory))
			{
				foreach (var item in inventory)
				{
					if (item.ItemEntity.GetEntityOnServer() == Entity.Null) continue;
					shardsItemsDestroyed.Add(item.ItemEntity.GetEntityOnServer());
					DestroyUtility.Destroy(Core.EntityManager, item.ItemEntity.GetEntityOnServer());
				}
			}

			DestroyUtility.Destroy(Core.EntityManager, entity);
			count++;
		}

		foreach (var mapIcon in GetShardMapIcons())
		{
			var attached = mapIcon.Read<Attach>().Parent;
			if (shardsItemsDestroyed.Contains(attached))
			{
				DestroyUtility.Destroy(Core.EntityManager, mapIcon);
			}
		}

		return count;
	}

	public int ClearDropShardsInRadius(float3 pos, float radius)
	{
		var posXZ = pos.xz;
		return ClearDropShards(entity =>
		{
			var translation = entity.Read<Translation>();
			return math.distance(posXZ, translation.Value.xz) <= radius;
		});
	}

	public void SetDroppedItemLifetime(int seconds)
	{
		if (seconds <= 0) return;

		Core.ConfigSettings.ItemDropLifetime = seconds;

		foreach (var entity in GetDropItemsWithPrefabs())
		{
			if(!entity.Has<Age>())
				entity.Add<Age>();

			if(!entity.Has<LifeTime>())
			{
				entity.Add<LifeTime>();
			}

			entity.Write(new LifeTime()
			{
				Duration = seconds,
				EndAction = LifeTimeEndAction.Destroy
			});
		}
	}

	public void RemoveDroppedItemLifetime()
	{
		Core.ConfigSettings.ItemDropLifetime = 0;

		foreach (var entity in GetDropItemsWithPrefabs())
		{
			if (entity.Has<Age>())
				entity.Remove<Age>();
			if (entity.Has<LifeTime>())
				entity.Remove<LifeTime>();
		}
	}

	public void SetDroppedItemLifetimeWhenDisabled(int seconds)
	{
		if (seconds <= 0) return;

		Core.ConfigSettings.ItemDropLifetime = seconds;
		var maxRemoveAtTime = Core.ServerTime + seconds;

		foreach (var entity in GetDropItemsWithPrefabs())
		{
			var dwncnad = entity.Read<DestroyWhenNoCharacterNearbyAfterDuration>();
			if(dwncnad.RemoveAtTime > maxRemoveAtTime)
				dwncnad.RemoveAtTime = maxRemoveAtTime;
			dwncnad.MinimumRemoveDurationIfNearby = seconds;
			entity.Write(dwncnad);
		}
	}

}
