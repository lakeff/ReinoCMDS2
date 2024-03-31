using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using KindredCommands.Models.Discord;
using Il2CppSystem.IO;
using System.IO;
using Unity.Collections;
using System.Net.Http;
using System.Net.Http.Json;
using UnityEngine.Networking.Match;
using static ProjectM.BannedEvent;


namespace KindredCommands.Services;
internal class DiscordService
{
	private static readonly HttpClient sharedClient = new HttpClient();
	public static async void SendWebhook(FixedString64 usuario, List<ContentHelper> content)
	{
		var username = usuario.ToString();
		var message = new Message(username, content);

		HttpResponseMessage response = await PostMessageAsync(message);
		if (response.StatusCode != HttpStatusCode.OK)
		{
			Console.WriteLine($"{response.StatusCode}-{response.RequestMessage}");
		}
		
	}

	public static async Task<HttpResponseMessage> PostMessageAsync(Message message)
	{
		var response = await sharedClient.PostAsJsonAsync(
			"https://discord.com/api/webhooks/1217930859290558506/Eboje_7Tjk1oNKuFOcl_T2RC8P_EoqqQdn8YyMYSWzXyc99F2wB0ccoxyzkJ48T3B75b",
			message);

		return response;
	}
}
