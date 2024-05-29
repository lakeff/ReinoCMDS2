using System;
using System.Linq;
using ProjectM.Shared;
using VampireCommandFramework;

namespace KindredCommands.Commands.Converters;

public class RelicTypeConverter : CommandArgumentConverter<RelicType>
{
	public override RelicType Parse(ICommandContext ctx, string input)
	{
		var relicTypes = Enum.GetValues(typeof(RelicType)).Cast<RelicType>();
		var relicType = relicTypes.FirstOrDefault(x => x.ToString().Equals(input, StringComparison.OrdinalIgnoreCase));

		if (relicType != RelicType.None)
			return relicType;

		input = input.ToLowerInvariant();

		if(input=="adam")
			return RelicType.TheMonster;

		if (input=="all")
			return RelicType.None;

		var search = relicTypes.Where(x => x.ToString().ToLowerInvariant().Contains(input)).ToList();

		if (search.Count == 1)
			return search.First();

		if (search.Count > 1)
			throw ctx.Error($"Multiple Shard Types found matching {input}. Please be more specific.\n" + string.Join("\n", search.Select(x => x.ToString())));
		throw ctx.Error("Could not find Shard Type.  Possible Options TheMonster, Solarus, WingedHorror, Dracula, or All");
	}
}
