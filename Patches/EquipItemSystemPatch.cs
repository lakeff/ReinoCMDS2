using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;

namespace KindredCommands.Patches;
[HarmonyPatch(typeof(EquipItemSystem), nameof(EquipItemSystem.OnUpdate))]
internal class EquipItemSystemPatch
{
	public static void Prefix(EquipItemSystem __instance)
	{
		var entities = __instance._EventQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
		foreach (var entity in entities)
		{
			var fromCharacter = entity.Read<FromCharacter>();
			var equipItemEvent = entity.Read<EquipItemEvent>();

			var inventoryEntity = Core.EntityManager.GetBuffer<InventoryInstanceElement>(fromCharacter.Character)[0].ExternalInventoryEntity.GetEntityOnServer();

			var inventory = Core.EntityManager.GetBuffer<InventoryBuffer>(inventoryEntity);
			var itemEquipped = inventory[equipItemEvent.SlotIndex];

			Core.Log.LogInfo($"EquipItemSystem: {fromCharacter.Character.Read<PlayerCharacter>().Name} equipped slot {equipItemEvent.SlotIndex} {itemEquipped.ItemType.LookupName()}");
		}
		entities.Dispose();
	}
}
