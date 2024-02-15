using Unity.Transforms;
using VampireCommandFramework;

namespace KindredCommands.Commands;
internal class InfoCommands
{
	[Command("whereami", "wai", description: "Gives your current position", adminOnly: true)]
	public static void WhereAmI(ChatCommandContext ctx)
	{
		var pos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
		ctx.Reply($"You are at {pos.x}, {pos.y}, {pos.z}");
	}
}
