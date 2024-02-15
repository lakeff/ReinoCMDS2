using KindredCommands.Commands.Converters;
using ProjectM;
using ProjectM.CastleBuilding;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using VampireCommandFramework;

namespace KindredCommands.Commands;

internal class CastleCommands
{
	[Command("claim", description: "Claims the Castle Heart you are standing next to for a specified player", adminOnly: true)]
	public static void CastleClaim(ChatCommandContext ctx, OnlinePlayer player = null)
	{
		Entity newOwnerUser = player?.Value.UserEntity ?? ctx.Event.SenderUserEntity;

		var castleHearts = Helper.GetEntitiesByComponentType<CastleHeart>();
		var playerPos = ctx.Event.SenderCharacterEntity.Read<LocalToWorld>().Position;
		foreach (var castleHeart in castleHearts)
		{
			var castleHeartPos = castleHeart.Read<LocalToWorld>().Position;

			if (Vector3.Distance(playerPos, castleHeartPos) > 5f)
			{
				continue;
			}

			var name = player?.Value.CharacterName.ToString() ?? ctx.Name;

			ctx.Reply($"Assigning castle heart to {name}");

			TeamUtility.ClaimCastle(Core.EntityManager, newOwnerUser, castleHeart);
			return;
		}
		ctx.Reply("Not close enough to a castle heart");
	}
}
