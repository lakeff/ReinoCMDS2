using System;
using System.Collections.Generic;
using KindredCommands;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;

namespace UnitKiller;

internal static class MobUtility
{
	private static NativeArray<Entity> GetMobs()
	{
		var mobQuery = Core.EntityManager.CreateEntityQuery(new EntityQueryDesc()
		{
			All = new[]
			{
				ComponentType.ReadOnly<LocalToWorld>(),
				ComponentType.ReadOnly<Team>()
			},
			None = new[] { ComponentType.ReadOnly<Dead>(), ComponentType.ReadOnly<DestroyTag>() }
		});

		return mobQuery.ToEntityArray(Allocator.Temp);
	}

	internal static List<Entity> ClosestMobs(ChatCommandContext ctx, float radius, PrefabGUID? mobGUID = null)
	{
		try
		{
			var e = ctx.Event.SenderCharacterEntity;
			var mobs = GetMobs();
			var results = new List<Entity>();
			var origin = Core.EntityManager.GetComponentData<LocalToWorld>(e).Position;
			var prefabCollectionSystem = Core.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
			foreach (var mob in mobs)
			{
				var position = Core.EntityManager.GetComponentData<LocalToWorld>(mob).Position;
				var distance = UnityEngine.Vector3.Distance(origin, position); 
				var em = Core.EntityManager;
				var getGuid = mob.Read<PrefabGUID>();
				if (!mobGUID.HasValue && distance < radius)
				{
					results.Add(mob);
				}
				else if (distance < radius && getGuid == mobGUID)
				{
					results.Add(mob);
				}
			}

			return results;
		}
		catch (Exception)
		{
			return null;
		}
	}

}
