using System.Linq;
using KindredCommands.Models;
using VampireCommandFramework;

namespace KindredCommands.Commands.Converters;
public record IntArray(int[] Value);

internal class IntArrayConverter : CommandArgumentConverter<IntArray>
{
	public override IntArray Parse(ICommandContext ctx, string input)
	{
		// Use the built-in TryParse method to convert the input string to an integer array
		if (input.Split(',').Select(x => int.TryParse(x, out var r)).All(x => x))
		{
			var player = input.Split(',').Select(int.Parse).ToArray();
			return new IntArray(player);
		}
		throw ctx.Error($"Invalid input. Expected a comma-separated list of integers.");
	}
}
