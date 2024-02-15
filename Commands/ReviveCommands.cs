using KindredCommands.Commands.Converters;
using ProjectM.Network;
using VampireCommandFramework;

namespace KindredCommands.Commands;

internal class ReviveCommands
{
	[Command("revive", adminOnly: true)]
	public static void ReviveCommand(ChatCommandContext ctx, FoundPlayer player = null)
	{
		var character = player?.Value.CharEntity ?? ctx.Event.SenderCharacterEntity;
		var user = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;

		Helper.ReviveCharacter(character, user);

		ctx.Reply($"Revived {user.Read<User>().CharacterName}");
	}
}
