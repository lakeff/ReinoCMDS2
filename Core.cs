using System.Runtime.CompilerServices;
using BepInEx.Logging;
using KindredCommands.Services;
using ProjectM;
using ProjectM.Scripting;
using Unity.Entities;
using static ProjectM.DropItemThrowSystem;

namespace KindredCommands;

internal static class Core
{
	public static World Server { get; } = GetWorld("Server") ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");

	public static EntityManager EntityManager { get; } = Server.EntityManager;
	public static GameDataSystem GameDataSystem { get; } = Server.GetExistingSystemManaged<GameDataSystem>();
	public static PrefabCollectionSystem PrefabCollectionSystem { get; internal set; }
	public static ServerScriptMapper ServerScriptMapper { get; internal set; }
	public static double ServerTime => ServerGameManager.ServerTime;
	public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager();

	public static ServerGameSettingsSystem ServerGameSettingsSystem { get; internal set; }

	public static ManualLogSource Log { get; } = Plugin.PluginLog;
	public static AnnouncementsService AnnouncementsService { get; internal set; }
	public static BoostedPlayerService BoostedPlayerService { get; internal set; }
	public static BossService Boss { get; internal set; }
	public static CastleTerritoryService CastleTerritory { get; private set; }
	public static ConfigSettingsService ConfigSettings { get; internal set; }
	public static DropItemService DropItem { get; internal set; }
	public static GearService GearService { get; internal set; }
	public static PlayerService Players { get; internal set; }
	public static PrefabService Prefabs { get; internal set; }
	public static RegionService Regions { get; internal set; }
	public static StealthAdminService StealthAdminService { get; internal set; }
	public static UnitSpawnerService UnitSpawner { get; internal set; }

	public const int MAX_REPLY_LENGTH = 509;

	public static void LogException(System.Exception e, [CallerMemberName] string caller = null)
	{
		Core.Log.LogError($"Failure in {caller}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
	}


	internal static void InitializeAfterLoaded()
	{
		if (_hasInitialized) return;

		PrefabCollectionSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();
		ServerGameSettingsSystem = Server.GetExistingSystemManaged<ServerGameSettingsSystem>();
		ServerScriptMapper = Server.GetExistingSystemManaged<ServerScriptMapper>();

		Players = new();
		Prefabs = new();
		ConfigSettings = new();

		AnnouncementsService = new();
		BoostedPlayerService = new();
		Boss = new();
		CastleTerritory = new();
		DropItem = new();
		GearService = new();
		Regions = new();
		StealthAdminService = new();
		UnitSpawner = new();

		Data.Character.Populate();

		_hasInitialized = true;
		Log.LogInfo($"{nameof(InitializeAfterLoaded)} completed");
	}
	private static bool _hasInitialized = false;

	private static World GetWorld(string name)
	{
		foreach (var world in World.s_AllWorlds)
		{
			if (world.Name == name)
			{
				return world;
			}
		}

		return null;
	}
}
