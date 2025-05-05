using System;
using System.Diagnostics;
using Sandbox;
using Sandbox.Utility;
using TycoonTerrain.Game.World.Data;
using TycoonTerrain.Utils;

namespace TycoonTerrain.Game.World;

public class WorldHeightManager : SingletonComponent<WorldHeightManager>
{
	private byte[,] HeightMap { get; set; }
	
	public void GenerateWorldHeight()
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		
		var worldSizeInCell = WorldSettings.Instance.WorldSizeInCell();
		HeightMap = new byte[ worldSizeInCell.x + 1, worldSizeInCell.y + 1 ];

		var falloffMap = GenerateFallOffMap();

		for ( var x = 0; x <= worldSizeInCell.x; x++ )
		{
			for ( var y = 0; y <= worldSizeInCell.y; y++ )
			{
				var noise = Noise.Perlin( x * 0.4f, y * 0.4f + WorldSettings.Instance.Seed ) * 10f;
				var noiseWithFalloff = noise - falloffMap[x, y];

				var finalHeight = System.Math.Clamp( noiseWithFalloff, 0, 100 );
				HeightMap[x, y] = (byte) finalHeight;
			}
		}
		
		var heightMapSizeBytes = ByteSizeUtils.ConvertBytesToKilobytesRounded( sizeof(byte) * HeightMap.Length );
		Log.Info( $"Generated Heightmap in {stopwatch.ElapsedMilliseconds}ms. Total memory usage: {heightMapSizeBytes}kb." );
	}

	public byte GetSimulationHeight( WorldHeightCoordinate worldHeightCoordinate )
	{
		return HeightMap[worldHeightCoordinate.Xy.x, worldHeightCoordinate.Xy.y];
	}

	public float GetPhysicalHeight( byte simulationHeight )
	{
		return WorldSettings.Instance.HeightLevel * simulationHeight;
	}
	
	private byte[,] GenerateFallOffMap()
	{
		var worldSizeInCell = WorldSettings.Instance.WorldSizeInCell();
		byte[,] falloffMap = new byte[ worldSizeInCell.x + 1, worldSizeInCell.y + 1 ];

		for ( var x = 0; x <= worldSizeInCell.x; x++ )
		{
			for ( var y = 0; y <= worldSizeInCell.y; y++ )
			{
				var xv = x / (float) worldSizeInCell.x * 2 - 1;
				var yv = y / (float) worldSizeInCell.y * 2 - 1;
				var v = MathF.Max( MathF.Abs( xv ), MathF.Abs( yv ) );
				
				var result = MathF.Pow( v, 3f ) / (MathF.Pow( v, 3f ) + MathF.Pow( WorldSettings.Instance.FallOffStrength - WorldSettings.Instance.FallOffStrength * v, 3f ));
				falloffMap[x, y] = (byte) result.Remap( 0, 1, 0, 5 );
			}
		}
		
		return falloffMap;
	}
}
