using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredCommands.Commands;

[CommandGroup("dropitems", "Commands for managing dropped items.")]
internal class DroppedItemCommands
{
	//reduce the time a dropped item stays on the ground
	[Command("lifetime", "lt", ".dropitems lifetime <seconds>", "Sets the lifetime of dropped items in seconds.", adminOnly: true)]
	public static void SetDroppedItemLifetime(ChatCommandContext ctx, int seconds)
	{
		if (seconds < 0)
		{
			throw ctx.Error("Lifetime must be a positive number.");
		}
		Core.DropItem.SetDroppedItemLifetime(seconds);
		ctx.Reply($"Dropped item lifetime set to {seconds} seconds.");
	}

	[Command("removelifetime", "rlt", ".dropitems removelifetime", "Removes the lifetime of dropped items.", adminOnly: true)]
	public static void RemoveDroppedItemLifetime(ChatCommandContext ctx)
	{
		Core.DropItem.RemoveDroppedItemLifetime();
		ctx.Reply("Dropped item lifetime removed.");
	}

	[Command("lifetimewhendisabled", "ltwd", ".dropitems lifetimewhendisabled <seconds=300>", "Sets the lifetime of dropped items when disabled in seconds.", adminOnly: true)]
	public static void SetDroppedItemLifetimeWhenDisabled(ChatCommandContext ctx, int seconds=300)
	{
		if (seconds < 0)
		{
			throw ctx.Error("Lifetime must be a positive number.");
		}
		Core.DropItem.SetDroppedItemLifetimeWhenDisabled(seconds);
		ctx.Reply($"Dropped item lifetime when disabled set to {seconds} seconds.");
	}

	//remove dropped items around the player in a radius
	[Command("clear", "c", ".dropitems clear <radius>", "Clears all dropped items within a radius of the player.", adminOnly: true)]
	public static void ClearDroppedItems(ChatCommandContext ctx, float radius)
	{
		var pos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value;
		var cleared = Core.DropItem.ClearDropItemsInRadius(pos, radius);
		ctx.Reply($"Cleared {cleared}x dropped items within a radius of {radius}.");
	}

	[Command("clearall", "ca", ".dropitems clearall", "Clears all dropped items in the world.", adminOnly: true)]
	public static void ClearAllDroppedItems(ChatCommandContext ctx)
	{
		var cleared = Core.DropItem.ClearDropItems();
		ctx.Reply($"Cleared all {cleared} dropped items in the world.");
	}

	[Command("clearshards", "cs", ".dropitems clearshards <radius>", "Clears all dropped shards within a radius of the player.", adminOnly: true)]
	public static void ClearDroppedShards(ChatCommandContext ctx, float radius)
	{
		var pos = ctx.Event.SenderCharacterEntity.Read<Translation>().Value;
		var cleared = Core.DropItem.ClearDropShardsInRadius(pos, radius);
		ctx.Reply($"Cleared {cleared} dropped shards within a radius of {radius}.");
	}

	[Command("clearallshards", "cas", ".dropitems clearallshards", "Clears all dropped shards in the world.", adminOnly: true)]
	public static void ClearAllDroppedShards(ChatCommandContext ctx)
	{
		var cleared = Core.DropItem.ClearDropShards();
		ctx.Reply($"Cleared all {cleared} dropped shards in the world.");
	}
}
