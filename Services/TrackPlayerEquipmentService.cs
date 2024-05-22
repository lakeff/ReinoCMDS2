using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;

namespace KindredCommands.Services;
internal class TrackPlayerEquipmentService
{
	struct TrackedItem
	{
		public Entity Item;
		public float Durability;
	}

	struct TrackPlayer
	{
		public Entity Player;
		public TrackedItem ArmorHeadgearSlot;
		public TrackedItem ArmorChestSlot;
		public TrackedItem ArmorGlovesSlot;
		public TrackedItem ArmorLegsSlot;
		public TrackedItem ArmorFootgearSlot;
		public TrackedItem CloakSlot;
		public TrackedItem WeaponSlot;
		public TrackedItem GrimoireSlot;
		public TrackedItem BagSlot;
	}

	readonly List<TrackPlayer> noDurabilityPlayers = [];

	public TrackPlayerEquipmentService()
	{
		Core.StartCoroutine(MonitorForEquipmentChanges());
	}

	public void StartTrackingPlayerForNoDurability(Entity player)
	{
		if(noDurabilityPlayers.Where(x => x.Player == player).Any()) return;

		var equipment = player.Read<Equipment>();

		var trackPlayer = new TrackPlayer
		{
			Player = player,
			ArmorHeadgearSlot = new TrackedItem { Item = equipment.ArmorHeadgearSlot.SlotEntity.GetEntityOnServer() },
			ArmorChestSlot = new TrackedItem { Item = equipment.ArmorChestSlot.SlotEntity.GetEntityOnServer() },
			ArmorGlovesSlot = new TrackedItem { Item = equipment.ArmorGlovesSlot.SlotEntity.GetEntityOnServer() },
			ArmorLegsSlot = new TrackedItem { Item = equipment.ArmorLegsSlot.SlotEntity.GetEntityOnServer() },
			ArmorFootgearSlot = new TrackedItem { Item = equipment.ArmorFootgearSlot.SlotEntity.GetEntityOnServer() },
			CloakSlot = new TrackedItem { Item = equipment.CloakSlot.SlotEntity.GetEntityOnServer() },
			WeaponSlot = new TrackedItem { Item = equipment.WeaponSlot.SlotEntity.GetEntityOnServer() },
			GrimoireSlot = new TrackedItem { Item = equipment.GrimoireSlot.SlotEntity.GetEntityOnServer() },
			BagSlot = new TrackedItem { Item = equipment.BagSlot.SlotEntity.GetEntityOnServer() }
		};

		SetItemNoDurability(ref trackPlayer.ArmorHeadgearSlot);
		SetItemNoDurability(ref trackPlayer.ArmorChestSlot);
		SetItemNoDurability(ref trackPlayer.ArmorGlovesSlot);
		SetItemNoDurability(ref trackPlayer.ArmorLegsSlot);
		SetItemNoDurability(ref trackPlayer.ArmorFootgearSlot);
		SetItemNoDurability(ref trackPlayer.CloakSlot);
		SetItemNoDurability(ref trackPlayer.WeaponSlot);
		SetItemNoDurability(ref trackPlayer.GrimoireSlot);
		SetItemNoDurability(ref trackPlayer.BagSlot);

		noDurabilityPlayers.Add(trackPlayer);
	}

	public void StopTrackingPlayerForNoDurability(Entity player)
	{
		var index = noDurabilityPlayers.FindIndex(x => x.Player == player);
		if(index == -1) return;

		var trackPlayer = noDurabilityPlayers[index];
		noDurabilityPlayers.RemoveAt(index);

		RestoreItemDurability(trackPlayer.ArmorHeadgearSlot);
		RestoreItemDurability(trackPlayer.ArmorChestSlot);
		RestoreItemDurability(trackPlayer.ArmorGlovesSlot);
		RestoreItemDurability(trackPlayer.ArmorLegsSlot);
		RestoreItemDurability(trackPlayer.ArmorFootgearSlot);
		RestoreItemDurability(trackPlayer.CloakSlot);
		RestoreItemDurability(trackPlayer.WeaponSlot);
		RestoreItemDurability(trackPlayer.GrimoireSlot);
		RestoreItemDurability(trackPlayer.BagSlot);
	}

	IEnumerator MonitorForEquipmentChanges()
	{
		while(true)
		{
			for(var i = 0; i < noDurabilityPlayers.Count; i++)
			{
				var trackPlayer = noDurabilityPlayers[i];
				var equipment = trackPlayer.Player.Read<Equipment>();

				if(trackPlayer.ArmorHeadgearSlot.Item != equipment.ArmorHeadgearSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.ArmorHeadgearSlot);
					trackPlayer.ArmorHeadgearSlot.Item = equipment.ArmorHeadgearSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.ArmorHeadgearSlot);
				}

				if(trackPlayer.ArmorChestSlot.Item != equipment.ArmorChestSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.ArmorChestSlot);
					trackPlayer.ArmorChestSlot.Item = equipment.ArmorChestSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.ArmorChestSlot);
				}

				if(trackPlayer.ArmorGlovesSlot.Item != equipment.ArmorGlovesSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.ArmorGlovesSlot);
					trackPlayer.ArmorGlovesSlot.Item = equipment.ArmorGlovesSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.ArmorGlovesSlot);
				}

				if(trackPlayer.ArmorLegsSlot.Item != equipment.ArmorLegsSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.ArmorLegsSlot);
					trackPlayer.ArmorLegsSlot.Item = equipment.ArmorLegsSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.ArmorLegsSlot);
				}

				if(trackPlayer.ArmorFootgearSlot.Item != equipment.ArmorFootgearSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.ArmorFootgearSlot);
					trackPlayer.ArmorFootgearSlot.Item = equipment.ArmorFootgearSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.ArmorFootgearSlot);
				}

				if(trackPlayer.CloakSlot.Item != equipment.CloakSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.CloakSlot);
					trackPlayer.CloakSlot.Item = equipment.CloakSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.CloakSlot);
				}

				if(trackPlayer.WeaponSlot.Item != equipment.WeaponSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.WeaponSlot);
					trackPlayer.WeaponSlot.Item = equipment.WeaponSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.WeaponSlot);
				}

				if(trackPlayer.GrimoireSlot.Item != equipment.GrimoireSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.GrimoireSlot);
					trackPlayer.GrimoireSlot.Item = equipment.GrimoireSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.GrimoireSlot);
				}

				if(trackPlayer.BagSlot.Item != equipment.BagSlot.SlotEntity.GetEntityOnServer())
				{
					RestoreItemDurability(trackPlayer.BagSlot);
					trackPlayer.BagSlot.Item = equipment.BagSlot.SlotEntity.GetEntityOnServer();
					SetItemNoDurability(ref trackPlayer.BagSlot);
				}

				noDurabilityPlayers[i] = trackPlayer;
			}

			yield return null;
		}
	}

	void SetItemNoDurability(ref TrackedItem trackedItem)
	{
		if (trackedItem.Item == Entity.Null) return;
		if (!trackedItem.Item.Has<Durability>()) return;

		var durability = trackedItem.Item.Read<Durability>();
		trackedItem.Durability = durability.Value;
		durability.LossType = DurabilityLossType.None;
		durability.TakeDamageDurabilityLossFactor = 0;
		trackedItem.Item.Write(durability);
	}

	void RestoreItemDurability(TrackedItem trackedItem)
	{
		if(trackedItem.Item == Entity.Null) return;
		if (!trackedItem.Item.Has<Durability>()) return;

		var durability = trackedItem.Item.Read<Durability>();
		durability.Value = trackedItem.Durability;

		var prefabGUID = trackedItem.Item.Read<PrefabGUID>();
		var prefab = Core.PrefabCollectionSystem._PrefabLookupMap[prefabGUID];
		var prefabDurability = prefab.Read<Durability>();
		durability.LossType = prefabDurability.LossType;
		durability.TakeDamageDurabilityLossFactor = prefabDurability.TakeDamageDurabilityLossFactor;

		trackedItem.Item.Write(durability);
	}
}
