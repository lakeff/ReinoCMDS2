using KindredCommands.Commands.Converters;
using ProjectM;
using VampireCommandFramework;

namespace KindredCommands.Commands;
internal static class DurabilityCommands
{
	[CommandGroup("gear")]
	internal class GearCommands
	{
		[Command("repair", "r", description: "Repairs all gear.", adminOnly: true)]
		public static void RepairCommand(ChatCommandContext ctx, FoundPlayer player = null)
		{
			var targetEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;

			Helper.RepairGear(targetEntity);
			ctx.Reply($"Gear repaired for {targetEntity.Read<PlayerCharacter>().Name}.");
		}

		[Command("break", "b", description: "Breaks all gear.", adminOnly: true)]
		public static void BreakGearCommand(ChatCommandContext ctx, FoundPlayer player = null)
		{
			var targetEntity = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
			Helper.RepairGear(targetEntity, false);
			ctx.Reply($"Gear broken for {targetEntity.Read<PlayerCharacter>().Name}.");
		}

		[Command("headgear", "hg", description: "Toggles headgear loss on death.", adminOnly: true)]
		public static void HeadgearBloodBoundCommand(ChatCommandContext ctx)
		{
			if(Core.GearService.ToggleHeadgearBloodbound())
			{
				ctx.Reply("Headgear will not be lost on death.");
			}
			else
			{
				ctx.Reply("Headgear will be lost on death.");
			}
		}

/*
		[Command("showhair", "sh", description: "Toggles hair visibility.")]
		public static void ShowHairCommand(ChatCommandContext ctx)
		{
			var charEntity = ctx.Event.SenderCharacterEntity;
			var equipment = charEntity.Read<Equipment>();

			if (equipment.ArmorHeadgearSlotEntity.Equals(Entity.Null))
			{
				ctx.Reply("No headgear equipped.");
				return;
			}

			var headgear = equipment.ArmorHeadgearSlotEntity.GetEntityOnServer();
			var headgearToggleData = headgear.Read<HeadgearToggleData>();
			headgearToggleData.HideCharacterHairOnEquip = !headgearToggleData.HideCharacterHairOnEquip;
			headgear.Write(headgearToggleData);

			ctx.Reply("Hair is " + (headgearToggleData.HideCharacterHairOnEquip ? " hidden" : "visible") + " with current headgear");
		}

	*/

	}
}



