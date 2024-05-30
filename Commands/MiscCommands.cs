using System.Collections.Generic;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredCommands.Commands
{
	internal class MiscCommands
	{
		[Command("forcerespawn", "fr", description: "Sets the chain transition time for nearby spawn chains to now to force them to respawn if they can", adminOnly: true)]
		public static void ChainTransition(ChatCommandContext ctx, float range = 10)
		{
			var charEntity = ctx.Event.SenderCharacterEntity;
			var time = Core.ServerTime;
			foreach (var chainEntity in Helper.GetAllEntitiesInRadius<AutoChainInstanceData>(charEntity.Read<Translation>().Value.xz, range))
			{
				chainEntity.Write(new AutoChainInstanceData() { NextTransitionAttempt = time });
			}
		}

		[Command("settime", "st", description: "Sets the game time to the day and hour", adminOnly: true)]
		public static void SetTime(ChatCommandContext ctx, int day, int hour)
		{
			var st = Core.EntityManager.CreateEntity(new ComponentType[1] { ComponentType.ReadOnly<SetTimeOfDayEvent>() });
			st.Write(new SetTimeOfDayEvent()
			{
				Day = day,
				Hour = hour,
				Type = SetTimeOfDayEvent.SetTimeType.Set
			});
		}

		[Command("cleancontainerlessshards", "ccs", description: "Destroys all items that are not in a container", adminOnly: true)]
		public static void CleanContainerlessShards(ChatCommandContext ctx)
		{
			var destroyedPrefabs = new Dictionary<PrefabGUID, int>();
			foreach (var item in Helper.GetEntitiesByComponentTypes<InventoryItem, Relic>())
			{
				if (!item.Read<InventoryItem>().ContainerEntity.Equals(Entity.Null)) continue;

				DestroyEntityAndAttached(item, destroyedPrefabs);
			}

			foreach (var (guid, count) in destroyedPrefabs)
			{
				ctx.Reply($"Destroyed <color=white>{count}</color>x <color=yellow>{guid.LookupName()}</color>");
				Core.Log.LogInfo($"Destroyed {count}x {guid.LookupName()}");
			}
		}

		static void DestroyEntityAndAttached(Entity entity, Dictionary<PrefabGUID, int> destroyCount)
		{
			if (entity.Has<AttachedBuffer>())
			{
				var attachedBuffer = Core.EntityManager.GetBuffer<AttachedBuffer>(entity);
				foreach (var attached in attachedBuffer)
				{
					DestroyEntityAndAttached(attached.Entity, destroyCount);
				}
			}

			if (entity.Has<PrefabGUID>())
			{
				var guid = entity.Read<PrefabGUID>();
				if (!destroyCount.TryGetValue(guid, out var count))
					count = 0;
				destroyCount[guid] = count + 1;
			}
			DestroyUtility.Destroy(Core.EntityManager, entity);
		}
	}
}
