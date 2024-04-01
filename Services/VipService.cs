using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using KindredCommands.Models;

namespace KindredCommands.Services;
public class VipService
{
	//private static int lastHour;
	//public static void InitTimer()
	//{
	//	var aTimer = new System.Timers.Timer(1000);
	//	lastHour = DateTime.Now.Minute;

	//	aTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimerVip);

	//	aTimer.Start();
	//}
	//private static void TimerVip(object source, ElapsedEventArgs e)
	//{
	//	if (lastHour < DateTime.Now.Minute || (lastHour == 1 && DateTime.Now.Minute == 0))
	//	{
	//		lastHour = DateTime.Now.Minute;
	//		VerifyVipExpireDate();
	//	}
	//}


	public static bool VerifyVipPermission(string steamId)
	{
		Dictionary<string, Dictionary<string, string>> vipList = Database.GetVip();
		string playerVip = vipList[steamId].ToString();

		if (string.IsNullOrEmpty(playerVip))
		{
			return false;
		}

		return true;
	}

	public static bool VerifyVipExpireDate(Dictionary<string, Dictionary<string, string>> vipList)
	{
		if (vipList.Any())
		{
			foreach (var vip in vipList)
			{
				var expireDate = System.DateTime.Parse(vip.Value["ExpireDate"]);
				if (DateTime.Compare(expireDate, Helper.GetVipDate()) <= 0)
				{
					Database.RemoveVip(vip.Key.ToString());
				}
			}
			return true;
		}

		return false;
	}
}
