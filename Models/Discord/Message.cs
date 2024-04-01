using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KindredCommands.Models.Discord;
public class Message
{
	[JsonProperty("username")]
	public string username { get; set; }
	[JsonProperty("avatar_url")]
	public string avatar_url { get; set; }
	[JsonProperty("content")]
	public string content { get; set; }
	[JsonProperty("embeds")]
	public List<Embed> embeds { get; set; }

	public Message(string _username, List<ContentHelper> _content)
	{
		username = _username;
		embeds = new List<Embed>
		{
			new Embed()
			{
				title = "Log de Comando",
				color = 16711680,
				description = "Log de comandos Administrativos utilizados através do KindredCommands",
				timestamp = string.Empty,
				url = string.Empty,
				author = new Dictionary<string, string>() {
					{
						"name", username
					},
					{
						"url", ""
					},
					{
						"icon_url", "https://media.discordapp.net/attachments/1214344314264616970/1214477760618307594/V.png?ex=66027c0e&is=65f0070e&hm=5dab3883612562a75a1cee8a311000d692a54429421623e129c79912210526a9&="
					}
				},
				image = new Dictionary<string, string>()
				{
					{
						"url", "https://media.discordapp.net/attachments/1214344314264616970/1214426399423856680/Beginners_Guide-V_Rising_Promo7.png?ex=66024c38&is=65efd738&hm=4965a5b1e1280a9752211a605d809dc9971ebdfeb7cde2d3df945f9a0242530a&="
					}
				},
				thumbnail = new Dictionary<string, string>()
				{
					{
						"url", "https://media.discordapp.net/attachments/1214344314264616970/1214426399423856680/Beginners_Guide-V_Rising_Promo7.png?ex=66024c38&is=65efd738&hm=4965a5b1e1280a9752211a605d809dc9971ebdfeb7cde2d3df945f9a0242530a&="
					}
				},
				footer = new Dictionary<string, string>()
				{
					{
						"text", "Blood Pact PvP - Servidor Dedicado de V Rising"
					}
				},
				fields = generateField(_content)
			}
		};
	}

	public List<Field> generateField(List<ContentHelper> content)
	{
		if (!content.Any())
			return new List<Field>();

		List<Field> fields = new();
		content.ForEach(c =>
		{
			fields.Add(new Field()
			{
				name = c.Title,
				value = c.Content,
				inline = true
				
			});
		});

		return fields;

	}
}
