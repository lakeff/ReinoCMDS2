using System.Text;
using KindredCommands.Commands.Converters;
using ProjectM;
using ProjectM.Shared;
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

		[Command("soulshardflight", "ssf", description: "Toggles soulshard flight restrictions.", adminOnly: true)]
		public static void SoulshardsFlightRestrictedCommand(ChatCommandContext ctx)
		{
			if (Core.GearService.ToggleShardsFlightRestricted())
			{
				ctx.Reply("Soulshards will not allowing flying.");
			}
			else
			{
				ctx.Reply("Soulshards will allow flying.");
			}
		}

		[Command("soulshardlimit", "ssl", description: "How many soulshards can be dropped before a boss won't drop a new one if the relic Unique setting is active.", adminOnly: true)]
		public static void SoulshardLimitCommand(ChatCommandContext ctx, int limit)
		{
			if (limit < 0)
			{
				throw ctx.Error("Limit must be zero or greater.");
			}
			Core.SoulshardService.SetShardDropLimit(limit);
			ctx.Reply($"Soulshard limit set to {limit}.");
		}

		[Command("soulshardstatus", "sss", description: "Reports the current status of soulshards.", adminOnly: true)]
		public static void SoulshardStatusCommand(ChatCommandContext ctx)
		{
			var sb = new StringBuilder();
			var soulshardStatus = Core.SoulshardService.GetSoulshardStatus();
			sb.AppendLine($"Soulshards flight restricted: {Core.ConfigSettings.SoulshardsFlightRestricted}");
			sb.AppendLine($"Soulshard drop limit: {Core.SoulshardService.ShardDropLimit}");
			var theMonster = soulshardStatus[(int)RelicType.TheMonster];
			var solarus = soulshardStatus[(int)RelicType.Solarus];
			var wingedHorror = soulshardStatus[(int)RelicType.WingedHorror];
			var dracula = soulshardStatus[(int)RelicType.Dracula];
			sb.AppendLine($"Soulshard The Monster: {theMonster.droppedCount}x dropped {theMonster.spawnedCount}x spawned {(theMonster.willDrop ? "and will drop" : "and won't drop")}");
			sb.AppendLine($"Soulshard Solarus: {solarus.droppedCount}x dropped {solarus.spawnedCount}x spawned {(solarus.willDrop ? "and will drop" : "and won't drop")}");
			sb.AppendLine($"Soulshard Winged Horror: {wingedHorror.droppedCount}x dropped {wingedHorror.spawnedCount}x spawned {(wingedHorror.willDrop ? "and will drop" : "and won't drop")}");
			sb.AppendLine($"Soulshard Dracula: {dracula.droppedCount}x dropped {dracula.spawnedCount}x spawned {(dracula.willDrop ? "and will drop" : "and won't drop")}");
			ctx.Reply(sb.ToString());
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



