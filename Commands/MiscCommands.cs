using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Transforms;
using VampireCommandFramework;

namespace KindredCommands.Commands
{
	internal class MiscCommands
	{
		[Command("forcerespawn", "fr", description: "Sets the chain transition time for nearby spawn chains to now to force them to respawn if they can", adminOnly: true)]
		public static void ChainTransition(ChatCommandContext ctx, float range = 10)
		{
			var charEntity = ctx.Event.SenderCharacterEntity;
			var time = Core.ServerTime;
			foreach (var chainEntity in Helper.GetAllEntitiesInRadius<AutoChainInstanceData>(charEntity.Read<Translation>().Value.xz, range))
			{
				chainEntity.Write(new AutoChainInstanceData() { NextTransitionAttempt = time });
			}
		}

		[Command("settime", "st", description: "Sets the game time to the day and hour", adminOnly: true)]
		public static void SetTime(ChatCommandContext ctx, int day, int hour)
		{
			var st = Core.EntityManager.CreateEntity(new ComponentType[1] { ComponentType.ReadOnly<SetTimeOfDayEvent>() });
			st.Write(new SetTimeOfDayEvent()
			{
				Day = day,
				Hour = hour,
				Type = SetTimeOfDayEvent.SetTimeType.Set
			});
		}
	}
}
