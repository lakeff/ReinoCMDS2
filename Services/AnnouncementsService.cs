using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ProjectM;
using ProjectM.Physics;
using UnityEngine;

namespace KindredCommands.Services;
class AnnouncementsService
{
	private static readonly string CONFIG_PATH = Path.Combine(BepInEx.Paths.ConfigPath, MyPluginInfo.PLUGIN_NAME);
	private static readonly string ANNOUNCEMENTS_PATH = Path.Combine(CONFIG_PATH, "announcements.json");
	
	readonly List<Announcement> announcements = [];

	public struct Announcement
	{
		public string Name { get; set; }
		public string Time { get; set; }
		public string Message { get; set; }
		public bool OneTime { get; set; }
	}

	Dictionary<string, Coroutine> announcementCoroutines = [];

	GameObject announcementSvcGameObject;
	IgnorePhysicsDebugSystem announcementMonoBehaviour;


	public AnnouncementsService()
	{
		announcementSvcGameObject = new GameObject("AnnouncementService");
		announcementMonoBehaviour = announcementSvcGameObject.AddComponent<IgnorePhysicsDebugSystem>();

		LoadAnnoucements();
	}

	void ScheduleAnnouncement(Announcement announcement)
	{
		var coroutine = announcementMonoBehaviour.StartCoroutine(MessageCoroutine(announcement).WrapToIl2Cpp());
		if(coroutine != null)
			announcementCoroutines[announcement.Name] = coroutine;
	}

	IEnumerator MessageCoroutine(Announcement announcement)
	{
		if (!DateTime.TryParse(announcement.Time, out var announcementTime))
		{
			Core.Log.LogError($"Failed to parse time for announcement {announcement.Name} with time {announcement.Time} and message \"{announcement.Message}\"");
			yield break;
		}

		if (!announcement.OneTime && announcementTime < DateTime.Now)
			announcementTime = announcementTime.AddDays(1);

		do
		{
			yield return new WaitForSecondsRealtime((float)(announcementTime - DateTime.Now).TotalSeconds);
			ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, announcement.Message);

			announcementTime = announcementTime.AddDays(1);
			SortAnnouncements();
		}
		while (!announcement.OneTime);

		announcements.Remove(announcement);
		announcementCoroutines.Remove(announcement.Name);
		SaveAnnoucements();
	}

	public bool AddAnnouncement(string name, string message, string time, bool oneTime)
	{
		var nameLower = name.ToLowerInvariant();
		if (announcements.Where(a => a.Name.ToLowerInvariant() == nameLower).Any())
			return false;

		var announcement = new Announcement { Name = name, Time = time, Message = message, OneTime = oneTime };
		announcements.Add(announcement);
		ScheduleAnnouncement(announcement);
		SortAnnouncements();
		SaveAnnoucements();
		return true;
	}

	private void SortAnnouncements()
	{
		int AnnouncementCompare(Announcement x, Announcement y)
		{
			if (!DateTime.TryParse(x.Time, out var xTime)) return -1;
			if (!DateTime.TryParse(y.Time, out var yTime)) return 1;

			var curTime = DateTime.Now;
			if (xTime < curTime) xTime = xTime.AddDays(1);
			if (yTime < curTime) yTime = yTime.AddDays(1);

			var result = DateTime.Compare(xTime, yTime);
			if (result == 0) result = string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
			return result;
		}
		announcements.Sort(AnnouncementCompare);
	}

	public bool RemoveAnnouncement(string name)
	{
		var nameLower = name.ToLowerInvariant();
		var names = announcements.Where(a => a.Name.ToLowerInvariant() == nameLower).Select(x => x.Name) ;
		if(!names.Any()) return false;

		name = names.First();
		announcements.RemoveAll(a => a.Name == name);

		if (!announcementCoroutines.TryGetValue(name, out var coroutine))
		{
			SaveAnnoucements();
			return true;
		}
		
		announcementMonoBehaviour.StopCoroutine(coroutine);
		announcementCoroutines.Remove(name);
		SaveAnnoucements();
		return  true;
	}

	public IEnumerable<Announcement> GetAnnouncements()
	{
		return announcements;
	}

	void LoadAnnoucements()
	{
		if (File.Exists(ANNOUNCEMENTS_PATH))
		{
			var json = File.ReadAllText(ANNOUNCEMENTS_PATH);
			announcements.Clear();
			announcements.AddRange(JsonSerializer.Deserialize<Announcement[]>(json));
			SortAnnouncements();

			foreach (var coroutine in announcementCoroutines.Values)
			{
				announcementMonoBehaviour.StopCoroutine(coroutine);
			}
			announcementCoroutines.Clear();
			
			foreach(var announcement in announcements)
			{
				ScheduleAnnouncement(announcement);
			}
		}
		else
		{
			SaveAnnoucements();
		}
	}

	void SaveAnnoucements()
	{
		if (!Directory.Exists(CONFIG_PATH)) Directory.CreateDirectory(CONFIG_PATH);

		var options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		var json = JsonSerializer.Serialize(announcements, options);
		File.WriteAllText(ANNOUNCEMENTS_PATH, json);
	}

}
