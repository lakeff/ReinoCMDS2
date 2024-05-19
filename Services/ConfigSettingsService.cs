using System.IO;
using System.Text.Json;

namespace KindredCommands.Services;
internal class ConfigSettingsService
{
	private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
	private static readonly string SETTINGS_PATH = Path.Combine(CONFIG_PATH, "settings.json");

	public bool RevealMapToAll {
		get {
			return config.RevealMapToAll;

		}
		set { 
			config.RevealMapToAll = value; 
			SaveConfig();
		}
	}

	public bool HeadgearBloodbound
	{
		get
		{
			return config.HeadgearBloodbound;
		}
		set
		{
			config.HeadgearBloodbound = value;
			SaveConfig();
		}
	}

	public int ItemDropLifetime
	{
		get
		{
			return config.ItemDropLifetime;
		}
		set
		{
			config.ItemDropLifetime = value;
			SaveConfig();
		}
	}

	public int ItemDropLifetimeWhenDisabled
	{
		get
		{
			return config.ItemDropLifetimeWhenDisabled;
		}
		set
		{
			config.ItemDropLifetimeWhenDisabled = value;
			SaveConfig();
		}
	}

	public int ShardDropLifetimeWhenDisabled
	{
		get
		{
			return config.ShardDropLifetimeWhenDisabled;
		}
		set
		{
			config.ShardDropLifetimeWhenDisabled = value;
			SaveConfig();
		}
	}

	struct Config
	{
		public Config()
		{
			ItemDropLifetimeWhenDisabled = 300;
		}

		public bool RevealMapToAll { get; set; }
		public bool HeadgearBloodbound { get; set; }
		public int ItemDropLifetime { get; set; }
		public int ItemDropLifetimeWhenDisabled { get; set; }
		public int ShardDropLifetimeWhenDisabled { get; set; }
	}

	Config config;

	public ConfigSettingsService()
	{
		LoadConfig();

		// Log out current settings
		Core.Log.LogInfo("Current settings");
		Core.Log.LogInfo($"RevealMapToAll: {RevealMapToAll}");
		Core.Log.LogInfo($"HeadgearBloodbound: {HeadgearBloodbound}");
		Core.Log.LogInfo($"ItemDropLifetime: {ItemDropLifetime}");
		Core.Log.LogInfo($"ItemDropLifetimeWhenDisabled: {ItemDropLifetimeWhenDisabled}");
		Core.Log.LogInfo($"ShardDropLifetimeWhenDisabled: {ShardDropLifetimeWhenDisabled}");
	}

	void LoadConfig()
	{
		if (!File.Exists(SETTINGS_PATH))
		{
			config = new Config();
			SaveConfig();
			return;
		}

		var json = File.ReadAllText(SETTINGS_PATH);
		config = JsonSerializer.Deserialize<Config>(json);
	}

	void SaveConfig()
	{
		if(!Directory.Exists(CONFIG_PATH))
			Directory.CreateDirectory(CONFIG_PATH);
		var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true });
		File.WriteAllText(SETTINGS_PATH, json);
	}
}
