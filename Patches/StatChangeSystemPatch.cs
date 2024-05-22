using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using ProjectM;
using ProjectM.Behaviours;
using ProjectM.Gameplay.Systems;

namespace KindredCommands.Patches;

/*[HarmonyPatch(typeof(StatChangeSystem), nameof(StatChangeSystem.OnUpdate))]
internal class StatChangeSystemPatch
{
	public static void Postfix(StatChangeSystem __instance)
	{
		var entities = __instance._DamageTakenEventQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
		foreach (var entity in entities)
		{
			var damageTakenEvent = entity.Read<DamageTakenEvent>();
			if (!Core.BoostedPlayerService.IsPlayerNoDurability(damageTakenEvent.Entity))
				continue;

			var player = damageTakenEvent.Entity;

			var inventories = Core.EntityManager.GetBuffer<InventoryInstanceElement>(damageTakenEvent.Entity);
			foreach (var inventory in inventories)
			{
				if(inventory.ExternalInventoryEntity.GetEntityOnServer() == Entity.Null)
					continue;



			}
		}
		entities.Dispose();
	}
}*/
