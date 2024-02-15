using KindredCommands.Commands.Converters;
using ProjectM;
using VampireCommandFramework;
using static KindredCommands.Commands.BuffCommands;

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
	}
}
