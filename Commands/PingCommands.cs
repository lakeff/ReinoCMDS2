using System;
using ProjectM.Network;
using VampireCommandFramework;

namespace KindredCommands.Commands;
internal static class PingCommands
{
	[Command("ping", shortHand: "p", description: "Mostra o seu ping atual no servidor.")]
	public static void PingCommand(ChatCommandContext ctx, string mode = "")
	{
		if (mode is null)
		{
			throw new ArgumentNullException(nameof(mode));
		}

		var ping = ctx.Event.SenderCharacterEntity.Read<Latency>().Value * 1000;
		ctx.Reply($"Ta liso! Seu ping é: <color=#FFFE0F>{ping:0}</color> ms");
	}
}
