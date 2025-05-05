using System;
using System.Collections.Generic;

namespace TycoonTerrain.Utils;

public static class RandomUtil
{
	
	public static float RandomFloat( float min, float max )
	{
		return (float) Random.Shared.NextDouble() * (max - min) + min;
	}
	
}
