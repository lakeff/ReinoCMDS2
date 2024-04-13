
using ProjectM;
using Unity.Entities;

namespace KindredCommands.Services;

internal class GearService
{
	public GearService()
	{
		if(Core.ConfigSettings.HeadgearBloodbound)
			SetHeadgearBloodbound(true);
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
		var allHeadgear = Helper.GetEntitiesByComponentTypes<HeadgearToggleData, Prefab>(includePrefab:true);
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
}
