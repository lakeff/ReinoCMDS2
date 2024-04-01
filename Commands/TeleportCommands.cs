using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KindredCommands.Commands.Converters;
using ProjectM.Behaviours;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredCommands.Commands;
internal class TeleportCommands
{
	
	[Command("tptp", "ttp", description: "Teleporta quem executou o comando para a posição do player.", adminOnly: false)]
	public static void TeleportToPlayer(ChatCommandContext ctx, FoundPlayer player)
	{
		var modEntity = ctx.Event.SenderCharacterEntity;
		var playerEntity = player.Value.CharEntity;
		if (Helper.VerifyAdminLevel(ProjectM.AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			if (player is not null)
			{
				var pos = Helper.GetPlayerPosition(playerEntity);


				var entity = Core.EntityManager.CreateEntity(
					ComponentType.ReadWrite<FromCharacter>(),
					ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
				);


				Core.EntityManager.SetComponentData<FromCharacter>(entity, new()
				{
					User = ctx.Event.SenderUserEntity,
					Character = ctx.Event.SenderCharacterEntity
				});

				Core.EntityManager.SetComponentData<PlayerTeleportDebugEvent>(entity, new()
				{
					Position = new float3(pos.x + 0.5f, pos.y + 0.5f, pos.z),
					Target = PlayerTeleportDebugEvent.TeleportTarget.Self
				});


				ctx.Reply($"Você foi teleportado ao player {player.Value.CharacterName}");
			}
			else
			{
				ctx.Reply($"Player não encontrado ou está offline");
			}
		}
		else
		{
			return;
		}
	}

	[Command("tptm", "tpm", description: "Teleporta o player para a posição de quem executou o comando.", adminOnly: false)]
	public static void TeleportPlayerToMe(ChatCommandContext ctx, FoundPlayer player = null)
	{
		var modEntity = ctx.Event.SenderCharacterEntity;
		if (Helper.VerifyAdminLevel(ProjectM.AdminLevel.Moderator, ctx.Event.SenderUserEntity))
		{
			if (player is not null)
			{
				var pos = Helper.GetPlayerPosition(modEntity);

				var entity = Core.EntityManager.CreateEntity(
					ComponentType.ReadWrite<FromCharacter>(),
					ComponentType.ReadWrite<PlayerTeleportDebugEvent>()
				);

				Core.EntityManager.SetComponentData<FromCharacter>(entity, new()
				{
					User = player.Value.UserEntity,
					Character = ctx.Event.SenderCharacterEntity
				});

				Core.EntityManager.SetComponentData<PlayerTeleportDebugEvent>(entity, new()
				{
					Position = new float3(pos.x - 0.5f, pos.y - 0.5f, pos.z),
					Target = PlayerTeleportDebugEvent.TeleportTarget.Self

				});

				ctx.Reply($"Player {player.Value.CharacterName} teleportado até você.");
			}
			else
			{
				ctx.Reply($"Player não encontrado ou está offline");
			}
		}
		else
		{
			return;
		}
	}
}
