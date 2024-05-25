using KindredCommands.Models;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using UnityEngine;
using VampireCommandFramework;

namespace KindredCommands.Commands;

internal static class BloodCommands
{

	[Command("bloodpotion", "bp", description: "Creates a Potion with specified Blood Type, Quality and Value, and amount", adminOnly: true)]
	public static void GiveBloodPotionCommand(ChatCommandContext ctx, BloodType type = BloodType.Frailed, float quality = 100f, int quantity = 1)
	{
		quality = Mathf.Clamp(quality, 0, 100);
		for (var i = 0; i < quantity; i++)
		{
			Entity entity = Helper.AddItemToInventory(ctx.Event.SenderCharacterEntity, new PrefabGUID(1223264867), 1);

			if(entity == Entity.Null)
			{
				ctx.Reply($"Received <color=#ff0>{i}</color> Blood Merlot of <color=#ff0>{type}</color> type of <color=#ff0>{quality}</color>% quality");
				ctx.Reply($"Inventory is full, could not add the last <color=#ff0>{quantity - i}</color> Blood Merlot");
				return;
			}

			var blood = new StoredBlood()
			{
				BloodQuality = quality,
				BloodType = new PrefabGUID((int)type)
			};

			Core.EntityManager.SetComponentData(entity, blood);
		}
		ctx.Reply($"Received <color=#ff0>{quantity}</color> Blood Merlot of <color=#ff0>{type}</color> type of <color=#ff0>{quality}</color>% quality");
	}
}
