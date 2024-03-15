using System;
using System.Text;
using VampireCommandFramework;

namespace KindredCommands.Commands;

public class AnnounceCommands
{
	[Command("time", description: "Reports the server time.")]
    public static void GetTimeCommand(ChatCommandContext ctx)
    {
        var serverTime = DateTime.Now;
        ctx.Reply("Server time is: " + serverTime);
    }


	[CommandGroup("announce")]
	internal class Announcements
	{
		[Command("add", "a", description: "Announce something to the server at a set time in server time.", adminOnly: true)] // Set the time for the announcement
		public static void AnnounceCommand(ChatCommandContext ctx, string name, string message, string time, bool oneTime = false)
		{
			var serverTime = DateTime.Now;

			if (!DateTime.TryParse(time, out var announcementTime))
			{
				ctx.Reply($"Invalid time format for '{time}'. Please use HH:mm AM/PM for daily repeating times and for one time messages can specify exact dates with month name, day, and time");
				return;
			}

			if (!oneTime && announcementTime < serverTime)
				announcementTime = announcementTime.AddDays(1);

			if(!Core.AnnouncementsService.AddAnnouncement(name, message, time, oneTime))
			{
				ctx.Reply($"Announcement message with the name {name} already exists.");
				return;
			}

			string timeUntilStr = GetTimeUntil(announcementTime);
			if (!oneTime)
				ctx.Reply($"Announcement of \"{message}\" will be made at: {announcementTime:hh:mm tt}. Time until then: {timeUntilStr}");
			else
				ctx.Reply($"One time announcement of \"{message}\" will be made at: {announcementTime}. Time until then: {timeUntilStr}");
		}

		private static string GetTimeUntil(DateTime time)
		{
			var timeUntil = time - DateTime.Now;
			return timeUntil.TotalHours > 0 ? $"{timeUntil:hh} hours, {timeUntil:mm} minutes, and {timeUntil:ss} seconds" :
				timeUntil.Minutes > 0 ? $"{timeUntil:mm} minutes and {timeUntil:ss} seconds" :
				$"{timeUntil:ss} seconds";
		}

		[Command("change", "c", description: "Change an existing announcement.", adminOnly: true)]
		public static void ChangeAnnouncementCommand(ChatCommandContext ctx, string name, string message, string time, bool oneTime = false)
		{
			Core.AnnouncementsService.RemoveAnnouncement(name);
			AnnounceCommand(ctx, name, message, time, oneTime);
		}

		[Command("list", "l", description: "List all announcements.", adminOnly: true)]
		public static void ListAnnouncementsCommand(ChatCommandContext ctx)
		{
			var serverTime = DateTime.Now;

			var sb = new StringBuilder();
			sb.AppendLine("Announcements:");
			var noAnnouncements = true;
			foreach (var announcement in Core.AnnouncementsService.GetAnnouncements())
			{
				if (!DateTime.TryParse(announcement.Time, out var announcementTime)) continue;

				noAnnouncements = false;

				if (!announcement.OneTime && announcementTime < serverTime)
					announcementTime = announcementTime.AddDays(1);

				var appendString = $"{announcement.Name} with message \"{announcement.Message}\" @ {announcement.Time}{(announcement.OneTime ? "" : " daily")}";
				if (sb.Length + appendString.Length > Core.MAX_REPLY_LENGTH)
				{
					ctx.Reply(sb.ToString());
					sb.Clear();
				}
				sb.AppendLine(appendString);
			}

			if(noAnnouncements)
				sb.AppendLine("No scheduled announcements");

			ctx.Reply(sb.ToString());
		}

		[Command("remove", "r", description: "Remove an announcement.", adminOnly: true)]
		public static void RemoveAnnouncementCommand(ChatCommandContext ctx, string name)
		{
			if(Core.AnnouncementsService.RemoveAnnouncement(name))
				ctx.Reply($"Announcement {name} removed.");
			else
				ctx.Reply($"Announcement {name} not found.");
		}
	}
}
