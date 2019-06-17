using System;

namespace TorchEconomy
{
	public static class RandomExtensions
	{
		public static double NextRange(this Random random, double minValue, double maxValue)
		{
			var next = random.NextDouble();

			return minValue + (next * (maxValue - minValue));
		}
	}
}