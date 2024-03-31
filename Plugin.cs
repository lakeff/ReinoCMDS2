using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Bloodstone.API;
using KindredCommands.Models;
using HarmonyLib;
using ProjectM;
using VampireCommandFramework;
using KindredCommands.Services;
using System.Linq;

namespace KindredCommands;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.Bloodstone")]
[BepInDependency("gg.deca.VampireCommandFramework")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin, IRunOnInitialized
{
	internal static Harmony Harmony;
	internal static ManualLogSource PluginLog;
	public static ManualLogSource LogInstance { get; private set; }

	public override async void Load()
	{
		PluginLog = Log;
		// Plugin startup logic
		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
		LogInstance = Log;
		Database.InitConfig();
		// Harmony patching
		Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		Harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

		// Register all commands in the assembly with VCF
		CommandRegistry.RegisterAll();

		// Verify VIPs expire dates
		var vipList = Database.GetVip();
		if (vipList.Any())
			VipService.VerifyVipExpireDate(vipList);

	}

	public override bool Unload()
	{
		CommandRegistry.UnregisterAssembly();
		Harmony?.UnpatchSelf();
		return true;
	}


	public void OnGameInitialized()
	{
		if (!HasLoaded())
		{
			Log.LogDebug("Attempt to initialize before everything has loaded.");
			return;
		}

		Core.InitializeAfterLoaded();
	}

	private static bool HasLoaded()
	{
		// Hack, check to make sure that entities loaded enough because this function
		// will be called when the plugin is first loaded, when this will return 0
		// but also during reload when there is data to initialize with.
		var collectionSystem = Core.Server.GetExistingSystem<PrefabCollectionSystem>();
		return collectionSystem?.SpawnableNameToPrefabGuidDictionary.Count > 0;
	}
}
