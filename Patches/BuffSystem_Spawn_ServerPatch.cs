using System.Collections.Generic;
using KindredCommands.Commands;
using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity;

namespace KindredCommands.Patches;

[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
public static class BuffSystem_Spawn_ServerPatch
{
	#region GodMode & Other Buff
	private static ModifyUnitStatBuff_DOTS Cooldown = new()
	{
		StatType = UnitStatType.CooldownModifier,
		Value = 0,
		ModificationType = ModificationType.Set,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS SunCharge = new()
	{
		StatType = UnitStatType.SunChargeTime,
		Value = 50000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS Hazard = new()
	{
		StatType = UnitStatType.ImmuneToHazards,
		Value = 1,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS SunResist = new()
	{
		StatType = UnitStatType.SunResistance,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS Speed = new()
	{
		StatType = UnitStatType.MovementSpeed,
		Value = 15,
		ModificationType = ModificationType.Set,
		Id = ModificationId.NewId(4)
	};

	private static ModifyUnitStatBuff_DOTS PResist = new()
	{
		StatType = UnitStatType.PhysicalResistance,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS FResist = new()
	{
		StatType = UnitStatType.FireResistance,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS HResist = new()
	{
		StatType = UnitStatType.HolyResistance,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS SResist = new()
	{
		StatType = UnitStatType.SilverResistance,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS GResist = new()
	{
		StatType = UnitStatType.GarlicResistance,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS SPResist = new()
	{
		StatType = UnitStatType.SpellResistance,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS PPower = new()
	{
		StatType = UnitStatType.PhysicalPower,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS SiegePower = new()
	{
		StatType = UnitStatType.SiegePower,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS RPower = new()
	{
		StatType = UnitStatType.ResourcePower,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS SPPower = new()
	{
		StatType = UnitStatType.SpellPower,
		Value = 10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS PHRegen = new()
	{
		StatType = UnitStatType.PassiveHealthRegen,
		Value = 100000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS HRecovery = new()
	{
		StatType = UnitStatType.HealthRecovery,
		Value = 100000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS MaxHP = new()
	{
		StatType = UnitStatType.MaxHealth,
		Value = 100000,
		ModificationType = ModificationType.Set,
		Id = ModificationId.NewId(5)
	};

	private static ModifyUnitStatBuff_DOTS MaxYield = new()
	{
		StatType = UnitStatType.ResourceYield,
		Value = 10,
		ModificationType = ModificationType.Multiply,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS DurabilityLoss = new()
	{
		StatType = UnitStatType.ReducedResourceDurabilityLoss,
		Value = -10000,
		ModificationType = ModificationType.Add,
		Id = ModificationId.NewId(0)
	};

	private static ModifyUnitStatBuff_DOTS AttackSpeed = new()
	{
		StatType = UnitStatType.AttackSpeed,
		Value = 5,
		ModificationType = ModificationType.Multiply,
		Id = ModificationId.NewId(0)
	};
	#endregion

	public readonly static List<ModifyUnitStatBuff_DOTS> godBuffs =
	[
		PResist,
		FResist,
		HResist,
		SResist,
		SunResist,
		GResist,
		SPResist,
		Hazard,
		SunCharge,
		Cooldown,
		Speed,
		MaxHP,
		AttackSpeed,
		PPower,
		RPower,
		SPPower,
		SiegePower
	];

	public static void Prefix(BuffSystem_Spawn_Server __instance)
	{
		EntityManager entityManager = __instance.EntityManager;
		NativeArray<Entity> entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);

		foreach (var entity in entities)
		{
			PrefabGUID GUID = entity.Read<PrefabGUID>();
			Entity Owner = entity.Read<EntityOwner>().Owner;
			if (!Owner.Has<PlayerCharacter>()) continue;
			if (!GodCommands.GodPlayers.Contains(Owner)) continue;
			if (GUID == Data.Prefabs.CustomBuff)
			{
				var Buffer = entityManager.AddBuffer<ModifyUnitStatBuff_DOTS>(entity);
				Buffer.Clear();

				foreach (var buff in godBuffs)
				{
					if (buff.Id.Id == Speed.Id.Id && GodCommands.PlayerSpeeds.ContainsKey(Owner))
					{
						var modifiedBuff = buff;
						modifiedBuff.Value = GodCommands.PlayerSpeeds[Owner];
						Buffer.Add(modifiedBuff);
					}
					else if (buff.Id.Id == MaxHP.Id.Id && GodCommands.PlayerHps.ContainsKey(Owner) && GodCommands.PlayerHps[Owner] != 0)
					{
						var modifiedBuff = buff;
						modifiedBuff.Value = GodCommands.PlayerHps[Owner];
						Buffer.Add(modifiedBuff);
					}
					else
					{
						Buffer.Add(buff);
					}
				}

				entity.Add<ModifyBloodDrainBuff>();
				var modifyBloodDrainBuff = new ModifyBloodDrainBuff()
				{
					AffectBloodValue = true,
					AffectIdleBloodValue = true,
					BloodValue = 0,
					BloodIdleValue = 0,

					ModificationId = new ModificationId(),
					ModificationIdleId = new ModificationId(),
					IgnoreIdleDrainModId = new ModificationId(),

					ModificationPriority = 1000,
					ModificationIdlePriority = 1000,

					ModificationType = ModificationType.Set,
					ModificationIdleType = ModificationType.Set,

					IgnoreIdleDrainWhileActive = true,
				};
				entity.Write(modifyBloodDrainBuff);
				entity.Add<DisableAggroBuff>();
				entity.Write(new DisableAggroBuff
				{
					Mode = DisableAggroBuffMode.OthersDontAttackTarget
				});

				entity.Add<BuffModificationFlagData>();
				var buffModificationFlagData = new BuffModificationFlagData()
				{
					ModificationTypes = (long)(BuffModificationTypes.DisableDynamicCollision | BuffModificationTypes.DisableMapCollision | BuffModificationTypes.Invulnerable | BuffModificationTypes.ImmuneToHazards | BuffModificationTypes.ImmuneToSun | BuffModificationTypes.CannotBeDisconnectDragged | BuffModificationTypes.DisableUnitVisibility | BuffModificationTypes.IsVbloodGhost),
					ModificationId = ModificationId.NewId(0),
				};
				entity.Write(buffModificationFlagData);
			}
			else if (GUID == Data.Prefabs.Buff_InCombat_PvPVampire)
			{
				if (BuffUtility.TryGetBuff(Core.EntityManager, Owner, Data.Prefabs.Buff_InCombat_PvPVampire, out var buffEntity))
				{
					DestroyUtility.Destroy(Core.EntityManager, buffEntity, DestroyDebugReason.TryRemoveBuff);
				}
			}
		}
	}
}
