using System.Runtime.CompilerServices;
using BepInEx.Logging;
using KindredCommands.Services;
using Unity.Entities;

namespace KindredCommands;

internal static class Core
{
	public static World Server { get; } = GetWorld("Server") ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");

	public static EntityManager EntityManager { get; } = Server.EntityManager;

	public static ManualLogSource Log { get; } = Plugin.PluginLog;
	public static AnnouncementsService AnnouncementsService { get; internal set; }

	public static PlayerService Players { get; internal set; }

	public static UnitSpawnerService UnitSpawner { get; internal set; }

	public static PrefabService Prefabs { get; internal set; }

	public static StealthAdminService StealthAdminService { get; internal set; }

	public static ConfigSettingsService ConfigSettings { get; internal set; }

	public const int MAX_REPLY_LENGTH = 509;

	public static void LogException(System.Exception e, [CallerMemberName] string caller = null)
	{
		Core.Log.LogError($"Failure in {caller}\nMessage: {e.Message} Inner:{e.InnerException?.Message}\n\nStack: {e.StackTrace}\nInner Stack: {e.InnerException?.StackTrace}");
	}


	internal static void InitializeAfterLoaded()
	{
		if (_hasInitialized) return;

		Players = new();
		UnitSpawner = new();
		Prefabs = new();
		AnnouncementsService = new();
		StealthAdminService = new();
		ConfigSettings = new();
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
