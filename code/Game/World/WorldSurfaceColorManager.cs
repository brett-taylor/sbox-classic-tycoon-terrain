using System;
using TycoonTerrain.Game.World.Data;
using TycoonTerrain.Utils;

namespace TycoonTerrain.Game.World;

public class WorldSurfaceColorManager : SingletonComponent<WorldSurfaceColorManager>
{
	public Color GetSurfaceColorAt( WorldCell worldCell )
	{
		var groundColor = Vector3.Zero;

		var total = 0f;
		for ( var y = -WorldSettings.Instance.BiomeFuzzinessRange; y <= WorldSettings.Instance.BiomeFuzzinessRange; y++ )
		{
			for ( var x = -WorldSettings.Instance.BiomeFuzzinessRange; x <= WorldSettings.Instance.BiomeFuzzinessRange; x++ )
			{
				var otherWorldCell = WorldCell.FromAbsoluteXy( worldCell.Xy.x + x, worldCell.Xy.y + y );
				var squaredDistance = worldCell.SquaredDistance( otherWorldCell );
				
				var dropOff = 0f;
				if ( squaredDistance != 0 )
					dropOff = 1f / MathF.Pow( squaredDistance, WorldSettings.Instance.BiomeFuzzinessDropOff);
				
				total += dropOff;
				var otherWorldCellColour = GenerateRandomBiomeColour( otherWorldCell );
				groundColor += dropOff * new Vector3( otherWorldCellColour.r, otherWorldCellColour.g, otherWorldCellColour.b );
			}
		}

		var finalColour = groundColor / total;
		return new Color( finalColour.x, finalColour.y, finalColour.z );
	}

	private Color GenerateRandomBiomeColour( WorldCell worldCell )
	{
		var biome = BiomeManager.Instance.GetBiomeAt( worldCell );
		var groundColor = biome.GroundColor;
		
		var randomOffset = RandomUtil.RandomFloat( biome.GroundColorOffsetMin, biome.GroundColorOffsetMax );
		return new Color( groundColor.r + randomOffset, groundColor.g + randomOffset, groundColor.b + randomOffset );
	}
}
